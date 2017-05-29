using ListExt;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JPEG
{
    public partial class Form1 : Form
    {
        byte[,] Y;     //values in Y form
        byte[,] Cr;    //values in Cr form
        byte[,] Cb;    //values in Cb form

        byte[,] Ydif;     //values in Y form
        byte[,] Crdif;    //values in Cr form
        byte[,] Cbdif;    //values in Cb form

        List<int> yVec = new List<int>();
        List<int> crVec = new List<int>();
        List<int> cbVec = new List<int>();
        List<int> differences = new List<int>(); //list of differences

        int width;     //width of the image
        int height;    //height of the image

        Bitmap bitmap; //original bitmap
        Bitmap bitmap2;

        static ArrayList yList = new ArrayList();  //list of RLE y bytes
        static ArrayList crList = new ArrayList(); //list of RLE cr bytes
        static ArrayList cbList = new ArrayList(); //list of RLE cb bytes
        

        public Form1() {
            InitializeComponent();
        }

        /**Converts YcrCb values back to RGB values.
           @param width - width of the bitmap
           @param height - height of the bitmap
           @param bm - bitmap to alter
        */
        protected void UpdateBitmap(byte[,] Y, byte[,] Cr, byte[,] Cb, int width, int height, Bitmap bm) {
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int luma = Y[x, y];
                    int cb = Cb[x, y];
                    int cr = Cr[x, y];

                    int r = (int)(1.164 * (luma - 16) + 1.596 * (cr - 128));
                    int g = (int)(1.164 * (luma - 16) - 0.813 * (cr - 128) - 0.392 * (cb - 128));
                    int b = (int)(1.164 * (luma - 16) + 2.017 * (cb - 128));

                    r = Math.Max(0, Math.Min(255, r));
                    g = Math.Max(0, Math.Min(255, g));
                    b = Math.Max(0, Math.Min(255, b));
                    bm.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
        }

        /**Converts RGB values to YCrCb values.
           @param width - width of the bitmap
           @param height - height of the bitmap
        */
        protected void ChangeColourSpace(ref byte[,] lum, ref byte[,] Cr, ref byte[,] Cb, Bitmap bm) {
            lum = new byte[width, height];
            Cb = new byte[width, height];
            Cr = new byte[width, height];

            BitmapData bmdata = bm.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, bm.PixelFormat);
            byte[] data = new byte[bmdata.Height * bmdata.Stride];
            Marshal.Copy(bmdata.Scan0, data, 0, bmdata.Height * bmdata.Stride);

            // Convert to YCbCr
            for (int y = 0; y < bmdata.Height; y++) {
                for (int x = 0; x < width; x++) {
                    byte[] curr = new byte[3];
                    Array.Copy(data, y * bmdata.Stride + x * 3, curr, 0, 3);

                    float b = curr[0];
                    float g = curr[1];
                    float r = curr[2];

                    lum[x, y] = (byte)((0.257 * r) + (0.504 * g) + (0.0988 * b) + 16);
                    Cb[x, y] = (byte)(128 - (0.148 * r) - (0.2916 * g) + (0.4398 * b));
                    Cr[x, y] = (byte)(128 + (0.439 * r) - (0.368 * g) - (0.0718 * b));
                }
            }
            bm.UnlockBits(bmdata);
        }

        /**Loads an image from the users harddrive.*/
        private void loadButton_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPG files (*.jpg)|*.jpg";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            bitmap = new Bitmap(dialog.FileName);
            pictureBox1.Image = bitmap;
            width = bitmap.Width;
            height = bitmap.Height;
            ChangeColourSpace(ref Y, ref Cr, ref Cb, bitmap); //first image we pass in Y, Cr, Cb and bitmap1
            statusLabel.Text = "File opened";
        }

        private void save() {
            FileStream stream = new FileStream("./file.enc", FileMode.Create); //write file to same folder as executable

            stream.Write(BitConverter.GetBytes(width), 0, 4); //write in width and height
            stream.Write(BitConverter.GetBytes(height), 0, 4);

            yVec = yVec.RLE(); //RLE all the lists
            crVec = crVec.RLE();
            cbVec = cbVec.RLE();

            stream.Write(BitConverter.GetBytes(yVec.Count), 0, 4); //write in the length of each motion vector array
            stream.Write(BitConverter.GetBytes(crVec.Count), 0, 4);
            stream.Write(BitConverter.GetBytes(cbVec.Count), 0, 4);
            stream.Write(BitConverter.GetBytes(differences.Count), 0, 4);

            foreach (int b in yVec)
                stream.Write(BitConverter.GetBytes(b), 0, 4); //values can be bigger than bytes
            foreach (int b in crVec)
                stream.Write(BitConverter.GetBytes(b), 0, 4);
            foreach (int b in cbVec)
                stream.Write(BitConverter.GetBytes(b), 0, 4);

            foreach (int b in differences) //write difference blocks
                stream.WriteByte((byte)b);

            foreach (int b in yList) //write bytes for JPEG
                stream.WriteByte((byte)b);
            foreach (int b in crList)
                stream.WriteByte((byte)b);
            foreach (int b in cbList)
                stream.WriteByte((byte)b);
            stream.Flush();
            stream.Close();
        }

        /**Subsamples the image by removing every 2nd row and column.*/
        private void subsampleButton_Click(object sender, EventArgs e) {
            byte[,] Crt = new byte[(int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0)]; //first image
            byte[,] Cbt = new byte[(int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0)];

            byte[,] Crt2 = new byte[(int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0)]; //second image
            byte[,] Cbt2 = new byte[(int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0)];

            for (int x = 0, xin = 0; x < width; x += 2, xin++) {
                for (int y = 0, yin = 0; y < height; y += 2, yin++) {
                    Crt[xin, yin] = Cr[x, y];
                    Cbt[xin, yin] = Cb[x, y];

                    Crt2[xin, yin] = Crdif[x, y];
                    Cbt2[xin, yin] = Cbdif[x, y];
                }
            }
            Cb = Cbt;
            Cr = Crt;

            Cbdif = Cbt2;
            Crdif = Crt2; //each image should be subsampled

            block(width, height); //converts to blocks and performs quantization and such

            MAD(width, height, Y, Ydif, yVec, 10); //perform mad on each block
            MAD(width / 2, height / 2, Cr, Crdif, crVec, 10);
            MAD(width / 2, height / 2, Cb, Cbdif, cbVec, 10);

            save();
            bitmap.Save("./bitmap.bmp"); //saves JPEG bitmap to executable folder
            statusLabel.Text = "Data subsampled and saved";
        }

        /**Super samples both images and prompts the user to save the image to their harddrive.*/
        private void saveButton_Click(object sender, EventArgs e) {
            byte[,] Crt = new byte[width, height];
            byte[,] Cbt = new byte[width, height];

            byte[,] Crt2 = new byte[width, height];
            byte[,] Cbt2 = new byte[width, height];

            int xin = 0;
            int yin = 0;
            for (int x = 0; x < width; x+=2) {
                for (int y = 0; y < height; y+=2) {
                    Crt[x, y] = Cr[xin, yin];
                    Crt2[x, y] = Crdif[xin, yin];
                    if (x < width - 1) {
                        Crt[x + 1, y] = Cr[xin, yin];
                        Crt2[x + 1, y] = Crdif[xin, yin];
                    }
                    if (y < height - 1) {
                        Crt[x, y + 1] = Cr[xin, yin];
                        Crt2[x, y + 1] = Crdif[xin, yin];
                    }
                    if (x < width - 1 && y < height - 1) {
                        Crt[x + 1, y + 1] = Cr[xin, yin];
                        Crt2[x + 1, y + 1] = Crdif[xin, yin];
                    }

                    Cbt[x, y] = Cb[xin, yin];
                    Cbt2[x, y] = Cbdif[xin, yin];

                    if (x < width - 1) {
                        Cbt[x + 1, y] = Cb[xin, yin];
                        Cbt2[x + 1, y] = Cbdif[xin, yin];
                    }
                    if (y < height - 1) {
                        Cbt[x, y + 1] = Cb[xin, yin];
                        Cbt2[x, y + 1] = Cbdif[xin, yin];
                    }
                    if (x < width - 1 && y < height - 1) {
                        Cbt[x + 1, y + 1] = Cb[xin, yin];
                        Cbt2[x + 1, y + 1] = Cbdif[xin, yin++];
                    }
                }
                xin++;
                yin = 0;
            }
            Cb = Cbt;
            Cr = Crt;

            Cbdif = Cbt2;
            Crdif = Crt2;

            UpdateBitmap(Y, Cr, Cb, width, height, bitmap);
            UpdateBitmap(Ydif, Crdif, Cbdif, width, height, bitmap2);

            pictureBox1.Image = bitmap;
            pictureBox2.Image = bitmap2;
        }

        /**Converts a 2D array into 8x8 blocks to be processed.
           @param width - width of the array. (divide by 2 for cr and cb)
           @param height - height of the array. (divide by 2 for cr and cb)
           @param channel - 2D byte array to be split.
           @param which - 1 for Y, 2 for Cr, 3 for Cb.
        */
        private void convertBlocks(int width, int height, byte[,] channel, int which, ArrayList values) {
            for (int u = 0; u < width; u += 8) {
                int[,] tempblock = new int[8, 8];
                int tempvalue = 0;
                for (int v = 0; v < height; v += 8) {
                    for (int i = u; i < u + 8; i++) {
                        for (int j = v; j < v + 8; j++) {
                            if (i < width && j < height)
                                tempvalue = channel[i, j];
                            tempblock[i-u, j-v] = tempvalue;
                        }
                    }
                    // block ready
                    Block block = new Block(tempblock, which);
                    for(int i=0;i<block.run.Count;i++) { //Store in list
                        values.Add(block.run[i]);
                    }
                }
            }
        }

        /**Calls functions to split each channel into blocks.
           @param width - width of the bitmap.
           @param height - height of the bitmap.
        */
        private void block(int width, int height) {
            convertBlocks(width, height, Y, 1, yList);
            convertBlocks((int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0), Cr, 2, crList);
            convertBlocks((int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0), Cb, 3, cbList);
        }


        /**Converts bytes read in from file to a usable format.
           @param channel - list of bytes from 1 channel in file.
           @param which - 1 for Y, 2 for Cr, 3 for Cb.
           @param width - width of array (divide by 2 for cr and cb)
           @param height - height of array (divide by 2 for cr and cb)
        */
        private void rebuild(ArrayList channel, int which, int width, int height, byte[,] values) {
            int[] tempblock = new int[64];
            int[] source = (int[])channel.ToArray(typeof(int));
            int startx = 0;
            int starty = 0;
            for (int i = 0; i < channel.Count; i+=64) {
                Array.Copy(source, i, tempblock, 0, 64);
                Block block = new Block(tempblock, which); //remove which later

                for (int x = startx, blockindexx = 0; x < startx + 8; x++, blockindexx++) {
                    for (int y = starty, blockindexy = 0; y < starty + 8; y++, blockindexy++) {
                        if (x < width && y < height) {
                            values[x, y] = (byte)block.block[blockindexx, blockindexy];
                        }     
                    }
                }
                starty += 8;
                if (starty >= height) {
                    startx += 8;
                    starty = 0;
                }
            }
            channel.Clear();
        }

        /**Load ENC button click.*/
        private void loadENCButton_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "ENC files (*.enc)|*.enc";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            byte[] file = File.ReadAllBytes(dialog.FileName);

            width = BitConverter.ToInt32(file, 0);
            height = BitConverter.ToInt32(file, 4);

            int yLength = (int)(Math.Ceiling(width / 8.0) * Math.Ceiling(height / 8.0) * 64);
            int crLength = (int)(Math.Ceiling(width / 16.0) * Math.Ceiling(height / 16.0) * 64);
            int cbLength = (int)(Math.Ceiling(width / 16.0) * Math.Ceiling(height / 16.0) * 64);

            int yVecCount = BitConverter.ToInt32(file, 8) * 4;
            int crVecCount = BitConverter.ToInt32(file, 12) * 4;
            int cbVecCount = BitConverter.ToInt32(file, 16) * 4;
            int difCount = BitConverter.ToInt32(file, 20);

            yVec.Clear();
            crVec.Clear();
            cbVec.Clear();
            differences.Clear();

            byte[] nohead = new byte[file.Length - 24];
            for (int i = 24; i < file.Length; i++)
                nohead[i - 24] = file[i];
            file = nohead;

            for (int i = 0; i < yVecCount; i += 4)
                yVec.Add(BitConverter.ToInt32(file, i));

            for (int i = yVecCount; i < yVecCount + crVecCount; i += 4) {
                crVec.Add(BitConverter.ToInt32(file, i));
            }
            for (int i = yVecCount + crVecCount; i < yVecCount + crVecCount + cbVecCount; i += 4) {
                cbVec.Add(BitConverter.ToInt32(file, i));
            }
            for (int i = yVecCount + crVecCount + cbVecCount; i < yVecCount + crVecCount + cbVecCount + difCount; i++) {
                if (file[i] > 127)
                    differences.Add((int)(file[i] - 256));
                else
                    differences.Add((int)file[i]);
            }

            yVec = yVec.ReverseRLE();
            crVec = crVec.ReverseRLE();
            cbVec = cbVec.ReverseRLE();
            differences = differences.ReverseRLE();

            List<int> strip = new List<int>();
            for (int i = yVecCount + crVecCount + cbVecCount + difCount; i < file.Length; i++)
                if (file[i] > 127)
                    strip.Add((int)(file[i] - 256));
                else
                    strip.Add((int)file[i]);

            strip = strip.ReverseRLE();

            ArrayList temp = new ArrayList();

            bitmap = new Bitmap(width, height);

            Y = new byte[width, height];
            Cr = new byte[(int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0)];
            Cb = new byte[(int)Math.Ceiling(width / 2.0), (int)Math.Ceiling(height / 2.0)];

            for (int i = 0; i < yLength; i++)
                temp.Add(strip[i]);

            rebuild(temp, 1, width, height, Y);

            for (int i = yLength; i < yLength + crLength; i++)
                temp.Add(strip[i]);

            rebuild(temp, 2, (int)Math.Ceiling(width / 2.0), (int)Math.Ceiling((height / 2.0)), Cr);

            for (int i = yLength + crLength; i < yLength + crLength + cbLength; i++)
                temp.Add(strip[i]);

            rebuild(temp, 3, (int)Math.Ceiling(width / 2.0), (int)Math.Ceiling((height / 2.0)), Cb);

            bitmap2 = new Bitmap(width, height);

            Ydif = new byte[width, height];
            Crdif = new byte[width / 2, height / 2];
            Cbdif = new byte[width / 2, height / 2];

            int[,] diffY = new int[width, height];
            int[,] diffCr = new int[width / 2, height / 2];
            int[,] diffCb = new int[width / 2, height / 2];

            int arrayIndex = 0;

            GenerateFrame(width, height, yVec, ref arrayIndex, Y, Ydif);
            GenerateFrame(width/2, height/2, crVec, ref arrayIndex, Cr, Crdif);
            GenerateFrame(width/2, height/2, cbVec, ref arrayIndex, Cb, Cbdif);

            saveButton_Click(null, null);
        }

        private void MAD(int width, int height, byte[,] channel, byte[,] channel2, List<int> list, int p) {
            int minimum = int.MaxValue; //minimum value
            Point vector = new Point(); //motion vector

            for (int u = 0; u < width; u += 8) {
                for (int v = 0; v < height; v += 8) {
                    int[,] tempblock = new int[8, 8]; //block to compare with
                    int[,] tempblock2 = new int[8, 8]; //block to compare with
                    minimum = int.MaxValue;
                    
                    for (int i = u; i < u + 8; i++) {
                        for (int j = v; j < v + 8; j++) {
                            if (i < width && j < height) {
                                tempblock[i - u, j - v] = channel[i, j]; //block from left image
                                tempblock2[i - u, j - v] = channel2[i, j]; //block from right image
                            }
                        }
                    }

                    //first see if images are the same
                    Boolean same = true;
                    for (int x = 0; x < 8; x++) {
                        for (int y = 0; y < 8; y++) {
                            if (tempblock[x, y] != tempblock2[x, y]) {
                                same = false;
                                x = 8;
                                y = 8;
                            }
                        }
                    }
                    if(same) {
                        vector = new Point(0, 0);
                    }

                            else { //if blocks are different we need to find best match
                                tempblock2 = new int[8, 8]; //reset this block
                                for (int x = u - p; x < u + p; x++) { //if they're not the same check the range 2p around that block
                                    for (int y = v - p; y < v + p; y++) {
                                        for (int i = x; i < x + 8; i++) {
                                            for (int j = y; j < y + 8; j++) {
                                                if (i < width && j < height && x >= 0 && x < width && y >= 0 && y < height)
                                                    tempblock2[i - x, j - y] = channel2[i, j];
                                            }
                                        }

                                        //we have 2 blocks
                                        int sum = 0;
                                        for (int a = 0; a < 8; a++) {
                                            for (int b = 0; b < 8; b++) {
                                                sum += Math.Abs(tempblock[a, b] - tempblock2[a, b]); //calculate the mad
                                            }
                                        }
                                        sum /= 64; //divide by whatever because the book says so
                                        if (sum < minimum) {
                                            minimum = sum;
                                            vector.X = x;
                                            vector.Y = y;
                                        }
                                    }
                                }
                            }
                    list.Add(vector.X);
                    list.Add(vector.Y);
                    tempblock2 = new int[8, 8];
                    if (!same) {
                        int indexx = 0; //tempblock contain
                        int indexy = 0;
                        for (int x = vector.X; x < vector.X + 8; x++) {
                            for (int y = vector.Y; y < vector.Y + 8; y++) {
                                if(x < width && y < height)
                                    tempblock2[indexx, indexy] = channel2[x, y] - tempblock[indexx, indexy++] ;
                            }
                            indexy = 0;
                            indexx++;
                        }
                    }
                    Block block = new Block(tempblock2);
                    for (int i = 0; i < block.run.Count; i++)
                        differences.Add((int)block.run[i]);
                }
            }
        }

        /**Load in the second bitmap*/
        private void loadButton2_Click(object sender, EventArgs e) {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            bitmap2 = new Bitmap(dialog.FileName);
            pictureBox2.Image = bitmap2;
            width = bitmap.Width;
            height = bitmap.Height;
            ChangeColourSpace(ref Ydif, ref Crdif, ref Cbdif, bitmap2); //second image we pass in differences and bitmap2
            statusLabel.Text = "File opened";
        }

        /**Draws a grid showing the block locations*/
        private void button2_Click(object sender, EventArgs e) { //draws grid
            if (bitmap == null)
                return;
            Pen pen = new Pen(Color.Black, 1);
            for (int x=8;x< width;x+=8) {
                for(int y=8;y<height;y+=8) {
                    using (var graphics = Graphics.FromImage(bitmap)) {
                        graphics.DrawLine(pen, x, 0, x, height);
                        graphics.DrawLine(pen, 0, y, width, y);
                    }
                }
            }
            pictureBox1.Invalidate();
        }

        /**Given a motion vector list and the original channel values, creates the new frame*/
        private void GenerateFrame(int width, int height, List<int> vectors, ref int arrayIndex, byte[,] initial, byte[,] difference) {
            int index = 0;
            for (int x = 0; x < width; x += 8) {
                for (int y = 0; y < height; y += 8) {
                    if (vectors[index] == 0 && vectors[index + 1] == 0) { //if block stays the same
                        for (int a = x; a < x + 8; a++) {
                            for (int b = y; b < y + 8; b++) {
                                if (a < width && b < height)
                                    difference[a, b] = initial[a, b]; //set it to the initial values
                            }
                        }
                    }
                    else {
                        int[] differenceBlock = new int[64];
                        for (int i = arrayIndex * 64; i < arrayIndex * 64 + 64; i++)
                            differenceBlock[i - arrayIndex * 64] = differences[i];
                        Block block = new Block(differenceBlock); //difference block

                        for (int a = x, c = 0; a < x + 8; a++, c++) {
                            for (int b = y, d = 0; b < y + 8; b++, d++) {
                                difference[a, b] = (byte)(initial[a, b] + block.block[c, d]);
                            }
                        }
                    }
                    index += 2;
                    arrayIndex++;
                }
            }
        }
    }
}

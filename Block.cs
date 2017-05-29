using System;
using System.Collections;

namespace JPEG
{
    class Block
    {

        int[,] zigzag = new int[,] {{0, 1, 5, 6, 14, 15, 27, 28},
                                    {2, 4, 7, 13, 16, 26, 29, 42},
                                    {3, 8, 12, 17, 25, 30, 41, 43},
                                    {9, 11, 18, 24, 31, 40, 44, 53},
                                    {10, 19, 23, 32, 39, 45, 52, 54},
                                    {20, 22, 33, 38, 46, 51, 55, 60},
                                    {21, 34, 37, 47, 50, 56, 59, 61 },
                                    {35, 36, 48, 49, 57, 58, 62, 63}};

        byte[,] luminance = {
            { 16, 11, 10, 16, 24, 40, 51, 61 },
            { 12, 12, 14, 19, 26, 58, 60, 55 },
            { 14, 13, 16, 24, 40, 57, 69, 56 },
            { 14, 17, 22, 29, 51, 87, 80, 62 },
            { 18, 22, 37, 56, 68, 109, 103, 77 },
            { 24, 35, 55, 64, 81, 104, 113, 92 },
            { 49, 64, 78, 87, 103, 121, 120, 101 },
            { 72, 92, 95, 98, 112, 100, 103, 99 }};

        byte[,] chrominance = {
            { 17, 18, 24, 27, 47, 99, 99, 99 },
            { 18, 21, 26, 66, 99, 99, 99, 99 },
            { 24, 26, 56, 99, 99, 99, 99, 99 },
            { 47, 66, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 }};

        public int[,] block;
        public ArrayList run = new ArrayList();

        /**Normal block constructor*/
        public Block(int[,] pass, int which) {
            this.block = pass;
            block = DCT(block);
            Quantize(which);
            RLE(ZigZag());
        }

        /**Difference block constructor*/
        public Block(int[,] pass) {
            this.block = pass;
            block = DCT(block);
            MotionQuantize();
            RLE(ZigZag());
        }

        /**Decode difference block constructor*/
        public Block(int[] pass) {
            this.block = reverseZigZag(pass);
            ReverseMotionQuantize();
            block = IDCT(block);
        }

        /**Decode normal block constructor*/
        public Block(int[] pass, int which) {
            this.block = reverseZigZag(pass);
            reverseQuantize(which);
            block = IDCT(block);
        }

        /**Discrete Cosine Transform.
           @param dct - 2D array to be DCT'd.
           @param which - 1 for Y, 2 for Cr, 3 for Cb.
        */
        private int[,] DCT(int[,] pass) {
            int[,] block = new int[8, 8];
            for (int u = 0; u < 8; u++) {
                for (int v = 0; v < 8; v++) {
                    double cu = u == 0 ? Math.Sqrt(2) / 2 : 1;
                    double cv = v == 0 ? Math.Sqrt(2) / 2 : 1;
                    double temp = 0;
                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 8; j++) {
                            var r = Math.Cos(((2 * i + 1) * u * Math.PI) / 16);
                            var e = Math.Cos(((2 * j + 1) * v * Math.PI) / 16);
                            temp += (r * e * pass[i, j]);
                        }
                    }
                    temp *= cu * cv / 4;
                    if (temp > 0)
                        block[u, v] = (int)Math.Round(temp);
                    else if (temp < 0)
                        block[u, v] = (int)Math.Floor(temp);
                }
            }
            return block;
        }

        /**Quantizes difference blocks*/
        private void MotionQuantize() {
            block[0, 0] = (int)Math.Round((double)block[0, 0] / 8.0);
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    if (!(x == 0 && y == 0)) {
                        block[x, y] = (int)Math.Floor((double)block[x, y] / 4.0);
                    }
                }
            }
        }

        /**Reverse quantizes difference blocks*/
        private void ReverseMotionQuantize() {
            block[0, 0] = (int)Math.Round((double)block[0, 0] * 8.0);
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    if (!(x == 0 && y == 0)) {
                        block[x, y] = (int)Math.Floor((double)block[x, y] * 4.0);
                    }
                }
            }
        }

        /**Inverse Discrete Cosine Transform.
           @param dct - 2D array that's been DCT'd.
        */
        private int[,] IDCT(int[,] pass) {
            int[,] block = new int[8, 8];
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    double temp = 0;
                    for (int u = 0; u < 8; u++) {
                        for (int v = 0; v < 8; v++) {
                            double cu = u == 0 ? Math.Sqrt(2) / 2 : 1;
                            double cv = v == 0 ? Math.Sqrt(2) / 2 : 1;
                            double num = cu * cv / 4;
                            var r = Math.Cos(((2 * i + 1) * u * Math.PI) / 16);
                            var e = Math.Cos(((2 * j + 1) * v * Math.PI) / 16);
                            temp += (r * e * pass[u, v] * num);
                        }
                    }

                    if (temp > 0)
                        block[i, j] = (int)Math.Round(temp);
                    else if (temp < 0)
                        block[i, j] = (int)Math.Floor(temp);
                }
            }
            return block;
        }

        /**Quantizes an 8x8 block that's been DCT'd.
           @param block - 8x8 DCT'd block.
           @param which - 1 for Y, 2 for Cr, 3 for Cb.
        */
        private void Quantize(int which) {
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    if (which == 1) {
                        block[x, y] = (int)Math.Round(((double)block[x, y] / (double)luminance[x, y]));
                    }
                    else
                        block[x, y] = (int)Math.Round((double)(block[x, y] / (double)chrominance[x, y]));
                }
            }
        }

        /**Unquantizes an 8x8 block that's been DCT'd.
           @param block - 8x8 quantized DCT block.
           @param which - 1 for Y, 2 for Cr, 3 for Cb.
        */
        private void reverseQuantize(int which) {
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    if (which == 1)
                        block[x, y] = (int)(block[x, y] * luminance[x, y]);
                    else
                        block[x, y] = (int)(block[x, y] * chrominance[x, y]);
                }
            }
        }

        /**Converts a 2D array into a 1D zigzagged array.
           @param n - size of block (8)
           @param block - 8x8 quantized DCT block.
        */
        public int[] ZigZag() {
            int[] result = new int[64];
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    result[zigzag[x, y]] = block[x, y];
                }
            }
            return result;
        }

        /**Converts a 1D array into 2D unzigzagged array.
           @param n - size of block (8)
           @param block - size 64 array
        */
        public int[,] reverseZigZag(int[] block) {
            int[,] result = new int[8, 8];
            for (int x = 0; x < 8; x++) {
                for (int y = 0; y < 8; y++) {
                    result[x, y] = block[zigzag[x, y]];
                }
            }
            return result;
        }

        /**Converts a 1D zigzag array into RLE.
           @param block - size 64 array that's been zigzagged.
        */
        private void RLE(int[] block) {
            for (int i = 0; i < block.Length; i++) {
                int count = 1;
                while (i < block.Length - 1 && block[i] == block[i + 1]) {
                    i++;
                    count++;
                }
                if (count > 3 || block[i] == 127) { //run
                    run.Add(127);
                    run.Add(block[i]);
                    run.Add(count);
                }
                else {
                    for (int x = 0; x < count; x++)
                        run.Add(block[i]);
                }
            }
        }
    }
}

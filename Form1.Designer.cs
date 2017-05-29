namespace JPEG
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.loadButton1 = new System.Windows.Forms.Button();
            this.subsampleButton = new System.Windows.Forms.Button();
            this.loadENCButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.loadButton2 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(582, 516);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // loadButton1
            // 
            this.loadButton1.Location = new System.Drawing.Point(12, 534);
            this.loadButton1.Name = "loadButton1";
            this.loadButton1.Size = new System.Drawing.Size(81, 23);
            this.loadButton1.TabIndex = 5;
            this.loadButton1.Text = "Load Image1";
            this.loadButton1.UseVisualStyleBackColor = true;
            this.loadButton1.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // subsampleButton
            // 
            this.subsampleButton.Location = new System.Drawing.Point(180, 534);
            this.subsampleButton.Name = "subsampleButton";
            this.subsampleButton.Size = new System.Drawing.Size(75, 23);
            this.subsampleButton.TabIndex = 6;
            this.subsampleButton.Text = "Subsample";
            this.subsampleButton.UseVisualStyleBackColor = true;
            this.subsampleButton.Click += new System.EventHandler(this.subsampleButton_Click);
            // 
            // loadENCButton
            // 
            this.loadENCButton.Location = new System.Drawing.Point(99, 534);
            this.loadENCButton.Name = "loadENCButton";
            this.loadENCButton.Size = new System.Drawing.Size(75, 23);
            this.loadENCButton.TabIndex = 10;
            this.loadENCButton.Text = "Load ENC";
            this.loadENCButton.UseVisualStyleBackColor = true;
            this.loadENCButton.Click += new System.EventHandler(this.loadENCButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(341, 9);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 13);
            this.statusLabel.TabIndex = 12;
            // 
            // pictureBox2
            // 
            this.pictureBox2.InitialImage = null;
            this.pictureBox2.Location = new System.Drawing.Point(600, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(621, 516);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 13;
            this.pictureBox2.TabStop = false;
            // 
            // loadButton2
            // 
            this.loadButton2.Location = new System.Drawing.Point(1140, 534);
            this.loadButton2.Name = "loadButton2";
            this.loadButton2.Size = new System.Drawing.Size(81, 23);
            this.loadButton2.TabIndex = 14;
            this.loadButton2.Text = "Load Image2";
            this.loadButton2.UseVisualStyleBackColor = true;
            this.loadButton2.Click += new System.EventHandler(this.loadButton2_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(565, 534);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 15;
            this.button2.Text = "Draw grid";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1233, 569);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.loadButton2);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.loadENCButton);
            this.Controls.Add(this.subsampleButton);
            this.Controls.Add(this.loadButton1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button loadButton1;
        private System.Windows.Forms.Button subsampleButton;
        private System.Windows.Forms.Button loadENCButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button loadButton2;
        private System.Windows.Forms.Button button2;
    }
}


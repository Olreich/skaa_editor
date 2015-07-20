﻿namespace SkaaEditor
{
    partial class SkaaEditorMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnLoadSPR = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.multiplePictureBox1 = new MultiplePictureBox.MultiplePictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnLoadSPR
            // 
            this.btnLoadSPR.Location = new System.Drawing.Point(938, 695);
            this.btnLoadSPR.Name = "btnLoadSPR";
            this.btnLoadSPR.Size = new System.Drawing.Size(78, 51);
            this.btnLoadSPR.TabIndex = 0;
            this.btnLoadSPR.Text = "Load SPR";
            this.btnLoadSPR.UseVisualStyleBackColor = true;
            this.btnLoadSPR.Click += new System.EventHandler(this.button1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(348, 312);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(668, 377);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // multiplePictureBox1
            // 
            this.multiplePictureBox1.Location = new System.Drawing.Point(12, 12);
            this.multiplePictureBox1.Name = "multiplePictureBox1";
            this.multiplePictureBox1.Size = new System.Drawing.Size(187, 240);
            this.multiplePictureBox1.TabIndex = 2;
            // 
            // SkaaEditorMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 758);
            this.Controls.Add(this.multiplePictureBox1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnLoadSPR);
            this.Name = "SkaaEditorMainForm";
            this.Text = "Skaa Editor for 7KAA";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoadSPR;
        private System.Windows.Forms.PictureBox pictureBox1;
        private MultiplePictureBox.MultiplePictureBox multiplePictureBox1;
    }
}


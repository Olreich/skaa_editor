﻿/***************************************************************************
*   This file is part of SkaaEditor, a binary file editor for 7KAA.
*
*   Copyright (C) 2015  Steven Lavoie  steven.lavoiejr@gmail.com
*
*   This program is free software; you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation; either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with this program; if not, write to the Free Software Foundation,
*   Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
*
*   SkaaEditor is capable of viewing and/or editing binary files from 
*   Enlight Software's Seven Kingdoms: Ancient Adversaries (7KAA). All code
*  	is licensed under GPLv3, including any code from Enlight Software. For
*  	information on 7KAA, visit http://www.7kfans.com.
***************************************************************************/

using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using SkaaColorChooser;
using System.Drawing.Imaging;
using SkaaGameDataLib;
using System.Collections.Generic;

namespace SkaaEditor
{
    public partial class SkaaEditorMainForm : Form
    {
        public event EventHandler AnimateChanged;
        protected virtual void OnAnimateChanged(EventArgs e)
        {
            EventHandler handler = AnimateChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private Sprite activeSprite;
        private SpriteFrame activeFrame;
        private bool _animate = false;

        public bool Animate
        {
            get
            {
                return this._animate;
            }
            set
            {
                if(this._animate != value)
                {
                    this._animate = value;
                    OnAnimateChanged(null);
                }
            }
        }
        public SpriteFrame ActiveFrame
        {
            get
            {
                return this.activeFrame;
            }
            set
            {
                if(this.activeFrame != value)
                {
                    this.activeFrame = value;
                    this.skaaImageBox1.Image = ActiveFrame.ImageBmp;
                }
            }
        }

        public SkaaEditorMainForm()
        {
            InitializeComponent();

            //disallow opening sprites until a palette is loaded
            if (this.skaaColorChooser1.Palette == null)
                this.openToolStripMenuItem.Enabled = false;

            //disable export until a sprite is loaded
            if (this.skaaImageBox1.Image == null)
                this.exportAsToolStripMenuItem.Enabled = false;

            //set up initial UI
            this.SetupUI();
            //this.exportAsToolStripMenuItem.Enabled = false;
            //this.skaaImageBox1.Text = "Edit >> Load Palette\nFile >> Open >> Choose an SPR file.\nReport bugs to steven.lavoiejr@gmail.com";
            //this.saveToolStripMenuItem.Enabled = false;

            //event subscriptions
            this.skaaColorChooser1.ActiveColorChanged += this.skaaColorChooser1_ActiveColorChanged;
            this.skaaImageBox1.ImageChanged += skaaImageBox1_ImageChanged;
            this.skaaImageBox1.ImageUpdated += skaaImageBox1_ImageUpdated;
            this.AnimateChanged += SkaaEditorMainForm_AnimateChanged;
            //this.skaaFrameViewer1.ActiveFrameChanged += skaaFrameViewer1_ActiveFrameChanged;
            this.timelineControl1.ActiveFrameChanged += skaaFrameViewer1_ActiveFrameChanged;
            
        }
        /// <summary>
        /// Sets up the UI based on the state of the program (sprite loaded or not, etc).
        /// </summary>
        private void SetupUI()
        {
            this.exportAsToolStripMenuItem.Enabled = (this.skaaImageBox1.Image == null) ? false : true;
            this.saveToolStripMenuItem.Enabled = (this.skaaImageBox1.Image == null) ? false : true;
            this.skaaImageBox1.Text = (this.skaaImageBox1.Image == null) ? "Edit >> Load Palette\nFile >> Open >> Choose an SPR file.\nReport bugs to steven.lavoiejr@gmail.com" : null;
            this.showGridToolStripMenuItem.Checked = this.skaaImageBox1.ShowPixelGrid;
            this.timelineControl1.SetSliderEnable((this.skaaImageBox1.Image == null) ? false : true);
        }

        private void skaaEditorMainForm_Load(object sender, EventArgs e) { }
        private void skaaColorChooser1_ActiveColorChanged(object sender, EventArgs e)
        {
            this.skaaImageBox1.ActiveColor = (e as ActiveColorChangedEventArgs).NewColor;
        }
        private void skaaFrameViewer1_ActiveFrameChanged(object sender, EventArgs e)
        {
            //todo: save/cache changes to current frame
            //this.ActiveFrame = skaaFrameViewer1.ActiveFrame;
            this.ActiveFrame = timelineControl1.ActiveFrame;
        }        
        private void skaaImageBox1_ImageChanged(object sender, EventArgs e)
        {
           SetupUI();
        }
        private void SkaaEditorMainForm_AnimateChanged(object sender, EventArgs e) { } //todo: start/stop animating
        private void skaaImageBox1_ImageUpdated(object sender, EventArgs e) { } //todo: update timeline with edits

        //UI item events
        private void cbEdit_CheckedChanged(object sender, EventArgs e)
        {
            this.skaaImageBox1.EditMode = !this.skaaImageBox1.EditMode;
        }
        private void showGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.skaaImageBox1.ShowPixelGrid = !this.skaaImageBox1.ShowPixelGrid;
            (sender as ToolStripMenuItem).Checked = this.skaaImageBox1.ShowPixelGrid;
        }

        //Menu item events
        private void loadPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "pal_std.res";
            dlg.DefaultExt = ".res";
            dlg.SupportMultiDottedExtensions = true;

            if (dlg.ShowDialog() == DialogResult.OK)
                this.activeSprite = new Sprite(this.skaaColorChooser1.LoadPalette(dlg.FileName));

            this.openToolStripMenuItem.Enabled = true;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.skaaColorChooser1.Palette == null)
                return;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".spr";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream spritestream = File.OpenRead(dlg.FileName);
                activeSprite = new Sprite(this.skaaColorChooser1.Palette);

                var x = spritestream.Length;

                while (spritestream.Position < spritestream.Length)
                {
                    byte[] frame_size_bytes = new byte[8];

                    spritestream.Read(frame_size_bytes, 0, 8);

                    int size = BitConverter.ToInt32(frame_size_bytes, 0);
                    short width = BitConverter.ToInt16(frame_size_bytes, 4);
                    short height = BitConverter.ToInt16(frame_size_bytes, 6);

                    SpriteFrame frame = new SpriteFrame(size, width, height, this.skaaColorChooser1.Palette);

                    frame.GetPixels(spritestream);

                    //debugging: gives an ASCII representation of image 
                    //(add \n after every 62d character). Verified alignment of pixels as read.
                    //var hex = BitConverter.ToString(frame.FrameData);

                    frame.BuildBitmap32bpp();

                    activeSprite.Frames.Add(frame);
                }

                this.exportAsToolStripMenuItem.Enabled = true;
                spritestream.Close();

                ActiveFrame = activeSprite.Frames[0];

                timelineControl1.ActiveSprite = this.activeSprite;
                timelineControl1.ActiveFrame = this.activeFrame;
                this.timelineControl1.SetMaxFrames(this.activeSprite.Frames.Count - 1); //-1 for 0-index
            }
        }
        private void exportBmp32bppToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                //todo: Verify the user wants to save this
                this.activeFrame.ImageBmp = (this.skaaImageBox1.Image as Bitmap);

                FileStream fs = new FileStream(dlg.FileName, FileMode.OpenOrCreate);
                this.skaaImageBox1.Image.Save(fs, ImageFormat.Bmp);
                fs.Close();
            }
        }
        private void saveFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.skaaImageBox1.Image == null)
                return;

            //todo: need to overwrite entire file if file exists
            //      right now, it just overwrites by byte so if the
            //      original is larger than what I'm writing, its
            //      data will still be there.
            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(dlg.FileName, FileMode.OpenOrCreate);
                    byte[] raw_pixels = ActiveFrame.BuildBitmap8bppIndexed();

                    /*
                     * BitConverter is required (rather than Convert.ToByte()) so we can get
                     * the full 16- or 32-bit representation. This is also why ActiveFrame's
                     * Height and Width are both cast to short, to ensure we get a 16-bit
                     * representation to match the binary's file format of:
                     * | 4 byte Size | 2 byte Height | 2 byte Width |
                     */
                    byte[] size = BitConverter.GetBytes(ActiveFrame.Size);
                    byte[] height = BitConverter.GetBytes((short) ActiveFrame.Height);
                    byte[] width = BitConverter.GetBytes((short) ActiveFrame.Width);
                    byte[] save = new byte[size.Length + height.Length + width.Length + raw_pixels.Length];

                    int seek_pos = 0; //makes the below lines easier to follow and edit

                    Buffer.BlockCopy(size, 0, save, seek_pos, size.Length);
                    seek_pos += size.Length;
                    Buffer.BlockCopy(width, 0, save, seek_pos, width.Length);
                    seek_pos += width.Length;
                    Buffer.BlockCopy(height, 0, save, seek_pos, height.Length);
                    seek_pos += height.Length;
                    Buffer.BlockCopy(raw_pixels, 0, save, seek_pos, raw_pixels.Length);
                    seek_pos += raw_pixels.Length;

                    fs.Write(save, 0, Buffer.ByteLength(save));
                    fs.Close();
                }
                catch (IOException ioex)
                {
                    throw new Exception(ioex.ToString());
                }
            }
        }
        private void saveAllFramesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.skaaImageBox1.Image == null)
                return;

            SaveFileDialog dlg = new SaveFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.OpenOrCreate);
                byte[] save = activeSprite.BuildSPR();
                fs.Write(save, 0, Buffer.ByteLength(save));
                fs.Close();
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm abt = new AboutForm();
            abt.Show();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            SkaaSAVEditorTest savEditor = new SkaaSAVEditorTest();
            savEditor.Show();
        }

       

        private void timelineControl1_Load(object sender, EventArgs e)
        {

        }

        private void timelineControl1_DoubleClick(object sender, EventArgs e)
        {

        }
    }
}

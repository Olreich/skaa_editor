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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SkaaGameDataLib;

namespace Timeline
{
    public partial class TimelineControl : UserControl
    {
        public event EventHandler ActiveFrameChanged;
        protected virtual void OnActiveFrameChanged(EventArgs e)
        {
            EventHandler handler = ActiveFrameChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        Sprite _activeSprite;
        SpriteFrame _activeFrame;
        int _activeFrameIndex;
        int _preAnimateActiveFrameIndex;


        public Sprite ActiveSprite
        {
            get
            {
                return this._activeSprite;
            }
            set
            {
                if(this._activeSprite != value)
                {
                    this._activeSprite = value;
                }
            }
        }
        public SpriteFrame ActiveFrame
        {
            get
            {
                if (!this.animationTimer.Enabled)
                    return this._activeFrame;
                else
                    return this._activeSprite.Frames[_preAnimateActiveFrameIndex];
            }
            set
            {
                if (this._activeFrame != value)
                {
                    this._activeFrame = value;
                    this._activeFrameIndex = this._activeSprite.Frames.FindIndex(0, (f => f == _activeFrame));
                    this.picBoxFrame.Image = this._activeFrame.ImageBmp;
                    this.frameSlider.Value = this._activeFrameIndex;

                    if(!this.animationTimer.Enabled)
                        this.OnActiveFrameChanged(null);
                }
            }
        }

        public TimelineControl()
        {
            InitializeComponent();

            this.picBoxFrame.SizeMode = PictureBoxSizeMode.CenterImage;
            this.SetSliderEnable(false);
            this.animationTimer.Enabled = false;
            this.animationTimer.Tick += AnimationTimer_Tick;
            this.animationTimer.Interval = 150;
        }

        public void SetMaxFrames(int frameCount)
        {
            this.frameSlider.Maximum = frameCount;
        }
        public int GetMaxFrames(int frameCount)
        {
            return this.frameSlider.Maximum;
        }
        public void SetSliderEnable(bool enabled = true)
        {
            this.frameSlider.Enabled = enabled;
        }

        private void picBoxFrame_Click(object sender, MouseEventArgs e) 
        {
            if (ActiveFrame == null)
                return;

            //if (!this.animationTimer.Enabled && e.Button == MouseButtons.Left)
            //{
            //    _activeFrameIndex++;
            //    _activeFrameIndex %= (ActiveSprite.Frames.Count - 1);
            //    this.ActiveFrame = this.ActiveSprite.Frames[_activeFrameIndex];
            //    //picBoxFrame.Image = ActiveSprite.Frames[_activeFrameIndex].ImageBmp;
            //}
            //else if (!this.animationTimer.Enabled && e.Button == MouseButtons.Right)
            //{
            //    _activeFrameIndex--;
            //    _activeFrameIndex = (_activeFrameIndex % (ActiveSprite.Frames.Count - 1) + (ActiveSprite.Frames.Count - 1)) % (ActiveSprite.Frames.Count - 1);
            //    // special mod() function above to actually cycle negative numbers around. Turns out % isn't a real mod() function, just remainder.

            //    this.ActiveFrame = this.ActiveSprite.Frames[_activeFrameIndex];
            //}
            //else 
            if (e.Button == MouseButtons.Middle)
            {
                if (picBoxFrame.SizeMode == PictureBoxSizeMode.CenterImage)
                    picBoxFrame.SizeMode = PictureBoxSizeMode.Zoom;
                else
                    picBoxFrame.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void frameSlider_ValueChanged(object sender, EventArgs e)
        {
            _activeFrameIndex = (sender as TrackBar).Value;
            this.ActiveFrame = (this.ActiveSprite == null) ? null : this.ActiveSprite.Frames[_activeFrameIndex];
        }
        
        private void picBoxFrame_DoubleClick(object sender, EventArgs e)
        {
            if (ActiveFrame == null)
                return;

            if (this.animationTimer.Enabled) //currently animating
            {
                this.animationTimer.Stop();
                this.frameSlider.Enabled = true;

                //reset to the currently-displayed frame
                this._activeFrameIndex = this._preAnimateActiveFrameIndex;
                this._preAnimateActiveFrameIndex = 0;
                this.ActiveFrame = this.ActiveSprite.Frames[_activeFrameIndex];
            }
            else //start animating
            {
                this._preAnimateActiveFrameIndex = this._activeFrameIndex;
                this.animationTimer.Start(); 
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            _activeFrameIndex++;
            _activeFrameIndex %= (ActiveSprite.Frames.Count - 1);
            this.ActiveFrame = this.ActiveSprite.Frames[_activeFrameIndex];
        }
    }
}

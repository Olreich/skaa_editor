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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SkaaGameDataLib
{
    public class SpriteFrame
    {
        public int Height
        {
            get;
            set;
        }
        public int Width
        {
            get;
            set;
        }
        public Byte[] FrameData
        {
            get;
            set;
        }
        public List<Bitmap> Images
        {
            get;
            set;
        }
        public Bitmap Image
        {
            get;
            set;
        }
        public ColorPalette Palette
        {
            get;
            set;
        }
        
        public SpriteFrame()
        {

        }
        public SpriteFrame(int width, int height, ColorPalette palette)
        {
            this.Height = height;
            this.Width = width;
            this.FrameData = new Byte[height * width];
            this.Palette = palette;
        }

        public void GetPixels(FileStream stream)
        {
            int pixelsToSkip = 0;
            Byte pixel;

            for (int y = 0; y < this.Height; ++y)
            {
                for (int x = 0; x < this.Width; ++x)
                {
                    if (pixelsToSkip != 0)
                    {
                        if (pixelsToSkip >= this.Width - x)
                        {
                            pixelsToSkip -= (this.Width - x); // skip to next line
                            break;
                        }

                        x += pixelsToSkip;
                        pixelsToSkip = 0;
                    }

                    try { pixel = Convert.ToByte(stream.ReadByte()); }
                    catch { return; /*got -1 for EOF*/ }

                    if (pixel < 0xf8)//MIN_TRANSPARENT_CODE) //normal pixel
                    {
                        this.FrameData[this.Width * y + x] = pixel;
                    }
                    else if (pixel == 0xf8)//MANY_TRANSPARENT_CODE)
                    {
                        pixelsToSkip = stream.ReadByte() - 1;
                    }
                    else //f9,fa,fb,fc,fd,fe,ff
                    {
                        pixelsToSkip = 256 - pixel - 1;	// skip (neg al) pixels
                    }
                }//end inner for
            }//end outer for
        }//end GetPixels()

        public void BuildBitmap32bpp()
        {
            Bitmap bmp = new Bitmap(this.Width, this.Height);

            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    Color pixel = this.Palette.Entries[FrameData[y * this.Width + x]];
                    bmp.SetPixel(x, y, pixel);
                    bmp.SetPixel(x, y, Color.FromArgb(255, pixel));
                }
            }

            this.Image = bmp;
        }

        public void SaveChanges(Bitmap bmp32bppToWrite, ColorPalette indexedPallet)
        {
            Byte palColorByte;
            Bitmap bmp = bmp32bppToWrite;
            //SpriteFrame sf = new SpriteFrame(bmp.Width, bmp.Height, indexedPallet);
            
            byte transparentByte = 0xf8;
            int transparentByteCount = 0;
            int realOffset = 0;

            this.Height = bmp.Height;
            this.Width = bmp.Width;
            this.FrameData = new Byte[this.Height * this.Width];

            //todo: should probably just convert this to a List at the source
            List<Color> Palette = new List<Color>();
            foreach (Color c in indexedPallet.Entries)
            {
                Palette.Add(c);
            }

            //the below is pretty much the same as GetPixel() but reversed(ish)
            for (int y = 0; y < bmp.Height; ++y)
            {
                for (int x = 0; x < bmp.Width; ++x)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    
                    palColorByte = Convert.ToByte(Palette.FindIndex(c => c == Color.FromArgb(255, pixel)));

                    if (palColorByte == 0)
                    {
                        transparentByteCount++;
                    }
                    else
                    {
                        if (transparentByteCount > 0)
                        {
                            this.FrameData[realOffset] = transparentByte;
                            realOffset++;
                            this.FrameData[realOffset] = Convert.ToByte(transparentByteCount);
                            realOffset++;
                            this.FrameData[realOffset] = palColorByte;
                            realOffset++;
                            transparentByteCount = 0;
                        }
                        else
                        {
                            this.FrameData[realOffset] = palColorByte;
                            realOffset++;
                        }
                    }
                }//end inner for
            }//end outer for
        }
    }
}

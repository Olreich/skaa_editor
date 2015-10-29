﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SkaaGameDataLib;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Data;
using System.Drawing.Imaging;
using System.Drawing;

namespace SkaaEditor
{
    [Serializable]
    public class Project
    {
        [field: NonSerialized]
        private EventHandler _activeFrameChanged;
        public event EventHandler ActiveFrameChanged
        {
            add
            {
                if (_activeFrameChanged == null || !_activeFrameChanged.GetInvocationList().Contains(value))
                {
                    _activeFrameChanged += value;
                }
            }
            remove
            {
                _activeFrameChanged -= value;
            }
        }
        protected virtual void OnActiveFrameChanged(EventArgs e)
        {
            EventHandler handler = _activeFrameChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }
        [field: NonSerialized]
        private EventHandler _paletteChanged;
        public event EventHandler PaletteChanged
        {
            add
            {
                if (_paletteChanged == null || !_paletteChanged.GetInvocationList().Contains(value))
                {
                    _paletteChanged += value;
                }
            }
            remove
            {
                _paletteChanged -= value;
            }
        }
        protected virtual void OnPaletteChanged(EventArgs e)
        {
            EventHandler handler = _paletteChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }
        [field: NonSerialized]
        private EventHandler _activeSpriteChanged;
        public event EventHandler ActiveSpriteChanged
        {
            add
            {
                if (_activeSpriteChanged == null || !_activeSpriteChanged.GetInvocationList().Contains(value))
                {
                    _activeSpriteChanged += value;
                }
            }
            remove
            {
                _activeSpriteChanged -= value;
            }
        }
        protected virtual void OnActiveSpriteChanged(EventArgs e)
        {
            EventHandler handler = _activeSpriteChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private string _workingFolder;
        private SpriteFrame _activeFrame;
        private DataSet _spriteTablesDataSet = new DataSet("sprites");
        public DataSet SpriteTablesDataSet
        {
            get
            {
                return this._spriteTablesDataSet;
            }
            set
            {
                if (this._spriteTablesDataSet != value)
                    this._spriteTablesDataSet = value;
            }
        }
        private GameSet _activeGameSet;

        [NonSerialized]
        public SuperPalette PalStruct;
        //[NonSerialized]
        //public SuperGameSet SuperSet;
        [NonSerialized]
        public SuperSprite SuperSpr;
        public Sprite ActiveSprite
        {
            get
            {
                return this.SuperSpr.ActiveSprite;
            }
            set
            {
                if (this.SuperSpr.ActiveSprite != value)
                {
                    this.SuperSpr.ActiveSprite = value;
                    OnActiveSpriteChanged(null);
                }
            }
        }
        public SpriteFrame ActiveFrame
        {
            get
            {
                return this._activeFrame;
            }
            set
            {
                if (this._activeFrame != value)
                {
                    this._activeFrame = value;
                    OnActiveFrameChanged(null);
                }
            }
        }
        public ColorPalette ActivePalette
        {
            get
            {
                return this.PalStruct.ActivePalette;
            }
            set
            {
                if (this.PalStruct.ActivePalette != value)
                {
                    this.PalStruct.ActivePalette = value;
                    OnPaletteChanged(null);
                }
            }
        }
        public GameSet ActiveGameSet
        {
            get
            {
                return this._activeGameSet;
            }
            set
            {
                if (this._activeGameSet != value)
                {
                    this._activeGameSet = value;
                }
            }
        }

        public Project ()
        {
        }
        public Project(string path, bool loadDefaults)
        {
            this._workingFolder = Path.GetDirectoryName(path);
            this.ActiveSpriteChanged += Project_ActiveSpriteChanged;
            this.ActiveFrameChanged += Project_ActiveFrameChanged;

            if (loadDefaults)
            {
                LoadPalette(_workingFolder);
                LoadGameSet(_workingFolder);
            }
        }

        private void Project_ActiveFrameChanged(object sender, EventArgs e)
        {
            
        }

        private void Project_ActiveSpriteChanged(object sender, EventArgs e)
        {
            //changing the ActiveFrame property fires its event before 
            //this event goes on to the MainForm and the TimeLine 
            //control ends up not having an ActiveSprite before it tries
            //to set its own ActiveFrame

            //let the UI's event handler set the active frame

            //if (this.ActiveSprite == null || this.ActiveSprite.Frames.Count < 1)
            //    this.ActiveFrame = null;
            //else
            //    this.ActiveFrame = this.ActiveSprite.Frames[0];
        }

        /// <summary>
        /// This function will open a file containing multiple dBase III databases, like 7KAA's std.set. 
        /// </summary>
        /// <param name="filepath"></param>
        public void LoadGameSet(string filepath = null)
        {
            if (filepath == null)
                filepath = this._workingFolder;

            string filename;
            // If a set is chosen by the user, we'll get a full file path. The 'connex' string in the can't have
            // a file name, just a path. This is because the path is considered the 'database' and the file is
            // a 'table' as far as OLEDB/Jet is concerned.
            FileAttributes attr = File.GetAttributes(filepath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                filename = "std.set";
                filepath = filepath + '\\' + filename;
            }
            else
            {
                filename = Path.GetFileName(filepath);
                //filepath = Path.GetDirectoryName(filepath);
            }

            this.ActiveGameSet = new GameSet(filepath);
            //this.SuperSet.GameSetFileMemoryStream = this.ActiveGameSet.GetRawDataStream() as MemoryStream;
            //this.SuperSet.GameSetFileName = filename;
            this.SpriteTablesDataSet = this.ActiveGameSet.GetSpriteTablesInDataSet();
        }
        public ColorPalette LoadPalette(string filepath = null)
        {
            
            if (filepath == null)
                filepath = this._workingFolder;

            FileAttributes attr = File.GetAttributes(filepath);
            //string filename;
            if (attr.HasFlag(FileAttributes.Directory))
                this.PalStruct.PaletteFileName = "pal_std.res";
            else
            {
                this.PalStruct.PaletteFileName = Path.GetFileName(filepath);
                filepath = Path.GetDirectoryName(filepath);
            }


            this.ActivePalette = new Bitmap(50, 50, PixelFormat.Format8bppIndexed).Palette;          

            using (FileStream fs = File.OpenRead(filepath + '\\' + this.PalStruct.PaletteFileName))
            { 
                fs.Seek(8, SeekOrigin.Begin);

                for (int i = 0; i < 256; i++)
                {
                    int r = fs.ReadByte();
                    int g = fs.ReadByte();
                    int b = fs.ReadByte();

                    if (i < 0xf9) //0xf9 is the lowest transparent color byte
                        this.ActivePalette.Entries[i] = Color.FromArgb(255, r, g, b);
                    else          //0xf9 - 0xff
                        this.ActivePalette.Entries[i] = Color.FromArgb(0, r, g, b);
                }
                this.PalStruct.PaletteFileMemoryStream = new MemoryStream();//FileStream(filepath + '\\' + this.PaletteFileName, FileMode.Open, FileAccess.Read);
                fs.Position = 0;
                fs.CopyTo(this.PalStruct.PaletteFileMemoryStream);
            }

            return this.ActivePalette;
        }

        public Sprite LoadSprite(string filepath)
        {
            if (this.ActivePalette == null)
                return null;

            SuperSprite ssp = new SuperSprite();
            Sprite spr = new Sprite(this.ActivePalette);

            using (FileStream spritestream = File.OpenRead(filepath))
            {
                long x = spritestream.Length;

                while (spritestream.Position < spritestream.Length)
                {
                    byte[] frame_size_bytes = new byte[8];

                    spritestream.Read(frame_size_bytes, 0, 8);

                    int size = BitConverter.ToInt32(frame_size_bytes, 0);
                    short width = BitConverter.ToInt16(frame_size_bytes, 4);
                    short height = BitConverter.ToInt16(frame_size_bytes, 6);

                    SpriteFrame frame = new SpriteFrame(size, width, height, spr);

                    frame.SprBitmapOffset = (uint?)spritestream.Position - 8;

                    frame.SetPixels(spritestream);
                    frame.BuildBitmap32bpp();
                    spr.Frames.Add(frame);
                }

                ssp.ActiveSprite = spr;
                ssp.SpriteFileName = Path.GetFileName(filepath);
                ssp.SpriteFileMemoryStream = new MemoryStream();
                spritestream.Position = 0;
                spritestream.CopyTo(ssp.SpriteFileMemoryStream);
                spritestream.Position = 0;
                this.SuperSpr = ssp;

                //this.SprStruct.SpriteFileMemoryStream = new MemoryStream();
                //spritestream.Position = 0;
                //spritestream.CopyTo(this.SprStruct.SpriteFileMemoryStream);
                //this.SprStruct.SpriteFileName = Path.GetFileName(filepath);
            }

            spr.SpriteId = Path.GetFileNameWithoutExtension(filepath);
            spr.GetTable(this.ActiveGameSet.GetSpriteTablesInDataSet());
            spr.MatchFrameOffsets();

            return spr;
        }
        /// <summary>
        /// Serializes the project with a BinaryFormatter
        /// </summary>
        /// <returns>A MemoryStream containing the serialized project data</returns>
        public Stream SaveProject()
        {
            return Serialization.Serialize(this);
        }
        public void SaveProject(string filepath)
        {
            if(filepath == null)
                ProjectZipper.ZipProject(this, this._workingFolder + '\\' + "new_project.skp");
            else
                ProjectZipper.ZipProject(this, filepath);
        }

        public static Project LoadProject(Stream str)
        {
            return (Project) Serialization.Deserialize(str);
        }
        public static Project LoadProject(string filePath)
        {
            return ProjectZipper.LoadZipProject(filePath);
        }
    }
}

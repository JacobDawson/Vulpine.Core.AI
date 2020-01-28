using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using Vulpine.Core.Draw;

using VColor = Vulpine.Core.Draw.Color;
using SColor = System.Drawing.Color;

namespace ArtEvolver
{
    public class ImageSys : Vulpine.Core.Draw.Image
    {
        //stores the internal bitmap
        private Bitmap bmp;

        public ImageSys(Bitmap bmp)
        {
            this.bmp = bmp;
        }

        public ImageSys(int width, int height)
        {
            this.bmp = new Bitmap(width, height);
        }

        public override int Width
        {
            get { return bmp.Width; }
        }

        public override int Height
        {
            get { return bmp.Height; }
        }

        public Bitmap BMP
        {
            get { return bmp; }
        }

        protected override VColor GetPixelInit(int col, int row)
        {
            SColor sc = bmp.GetPixel(col, row);

            double r = sc.R / 255.0;
            double g = sc.G / 255.0;
            double b = sc.B / 255.0;

            return VColor.FromRGB(r, g, b);
        }

        protected override void SetPixelInit(int col, int row, VColor vc)
        {
            byte r = (byte)Math.Floor((vc.Red * 255.0) + 0.5);
            byte g = (byte)Math.Floor((vc.Green * 255.0) + 0.5);
            byte b = (byte)Math.Floor((vc.Blue * 255.0) + 0.5);


            SColor sc = SColor.FromArgb(r, g, b);
            bmp.SetPixel(col, row, sc);
        }

        public override object GetInternalData()
        {
            return bmp;
        }

        public static implicit operator ImageSys(Bitmap bmp)
        { return new ImageSys(bmp); }

        public static explicit operator Bitmap(ImageSys img)
        { return img.bmp; }
    }
}


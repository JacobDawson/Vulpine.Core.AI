using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Draw;

namespace ArtEvolverConsole
{
    public struct SubPixel
    {
        private double x;
        private double y;

        private Color c;

        public SubPixel(double x, double y, Color c)
        {
            this.x = x;
            this.y = y;
            this.c = c;
        }

        public double PosX
        {
            get { return x; }
        }

        public double PosY
        {
            get { return y; }
        }

        public Color Color
        {
            get { return c; }
        }
    }
}

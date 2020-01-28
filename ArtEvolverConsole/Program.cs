using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;

using ArtEvolver;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.Matrices;

namespace ArtEvolverConsole
{
    class Program
    {
        public const string FILE_IN = @"S:\Animation\_Source\12171.jpg";

        public const string FILE_OUT = @"S:\Animation\";

        public const int MAX_GEN = 4000;


        static void Main(string[] args)
        {
            var bmp = new System.Drawing.Bitmap(FILE_IN);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.Nearest);

            var fitness = BuildFitness(txt);
            EvolMonogen<CPPN> evolver = new EvolMonogen<CPPN>(10, 0.1, fitness);
            CPPN best = new CPPN();
            evolver.Initialise(best);

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Aproximating Image: " + FILE_IN);

            for (int i = 0; i < MAX_GEN; i++)
            {
                evolver.Evolve();
                evolver.GetTopSpec(best);

                ren.Render(best, output);
                string index = i.ToString("0000");
                output.BMP.Save(FILE_OUT + index + ".png");

                int nodes = best.NumNurons;
                int axons = best.NumAxons;
                double fit = evolver.TopFitness;
                

                Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons + " " + fit);
            }
        }


        public static Fitness<CPPN> BuildFitness(Texture target)
        {
            return delegate(CPPN genome)
            {
                double total = 0.0;
                Console.Write("*");

                for (int i = -32; i <= 32; i++)
                {
                    for (int j = -32; j <= 32; j++)
                    {
                        double u = i / 127;
                        double v = j / 127;

                        Color c1 = genome.Sample(u, v);
                        Color c2 = target.Sample(u, v);

                        double dist = c1.ToRGB().Dist(c2.ToRGB());
                        total += dist;
                    }
                }

                return (4096.0 / (total + 1.0));
            };
        }
    }
}

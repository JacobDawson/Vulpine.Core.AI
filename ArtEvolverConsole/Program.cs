﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//using System.Drawing;

using ArtEvolver;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.Matrices;
using Vulpine.Core.Calc.RandGen;

namespace ArtEvolverConsole
{
    class Program
    {
        public const string FILE_IN = @"S:\Animation\_Source\12171.jpg";

        public const string FILE_OUT = @"S:\Animation\";

        public const int MAX_GEN = 1024;


        static void Main(string[] args)
        {
            RunEvolution2();
            //GeneratePictures();
            //TestGennetics3();
            //SortTest();

            Console.WriteLine("Press Any Key To Continue... ");
            Console.ReadKey(true);
        }

        #region Evolution Simulation...

        public static void RunEvolution()
        {
            var bmp = new System.Drawing.Bitmap(FILE_IN);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.Nearest);

            var fitness = BuildFitness(txt);
            EvolMonogen<CPPN> evolver = new EvolMonogen<CPPN>(50, 0.1, fitness);
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
                        double u = i / 32.0;
                        double v = j / 32.0;

                        Color c1 = genome.Sample(u, v);
                        Color c2 = target.Sample(u, v);

                        double dist = c1.ToRGB().Dist(c2.ToRGB());
                        total += dist;
                    }
                }

                //return (4096.0 / (total + 1.0));
                //return (4096.0 - total) / 4096.0;
                //return -total;

                total = (4096.0 - total) / 4096.0;
                if (total < 0.0001) total = 0.0001;
                return total;
            };
        }

        public static void RunEvolution2()
        {
            var bmp = new System.Drawing.Bitmap(FILE_IN);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.Nearest);

            var fitness = BuildFitness(txt);
            EvolSpecies<CPPN> evolver = new EvolSpecies<CPPN>(50, 0.1, fitness);
            CPPN best = new CPPN();
            

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Aproximating Image: " + FILE_IN);
            evolver.Initialise(best);

            for (int i = 0; i < MAX_GEN; i++)
            {               
                ren.Render(best, output);
                string index = i.ToString("0000");
                output.BMP.Save(FILE_OUT + index + ".png");

                int nodes = best.NumNurons;
                int axons = best.NumAxons;
                double fit = evolver.TopFitness;


                Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons + " " + fit);
                Console.WriteLine("Num Species: " + evolver.NumSpecies);

                evolver.Evolve();
                evolver.GetTopSpec(best);
            }
        }

        #endregion //////////////////////////////////////////////////////////////////////

        #region Picture Generation...


        public static void GeneratePictures()
        {
            var bmp = new System.Drawing.Bitmap(FILE_IN);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.Nearest);
            var fitness = BuildFitness(txt);

            CPPN network = new CPPN();
            VRandom rng = new RandMT();

            network.Randomize(rng);

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Aproximating Image: " + FILE_IN);

            //NOTE: Make it so we don't accept mutations that
            //result in NaN fitness!

            for (int i = 0; i < MAX_GEN; i++)
            {
                network.Mutate(rng, 0.05); //0.1

                ren.Render(network, output);
                string file = i.ToString("0000") + ".png";

                output.BMP.Save(FILE_OUT + file);

                int nodes = network.NumNurons;
                int axons = network.NumAxons;
                double fit = fitness(network);

                //Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons + " " + fit);
            }

        }

        #endregion //////////////////////////////////////////////////////////////////////

        #region Testing Genetic Algorythims...

        public static void TestGennetics()
        {
            GenString target = new GenString(
                "Twas brillig, and the slithy toves did gyer and gimble in the wabe.");
            GenString best = new GenString("Hello");

            Console.WriteLine("Target: " + target);

            Fitness<GenString> fit = delegate(GenString gs) {
                double dist = gs.Distance(target);
                //return 1.0 / (dist + 1.0);
                return -dist;
            };

            EvolMonogen<GenString> evol = new EvolMonogen<GenString>(100, 0.1, fit);
            evol.Initialise(best);

            for (int i = 0; i < MAX_GEN; i++)
            {
                evol.Evolve();
                evol.GetTopSpec(best);

                double fitness = evol.TopFitness;

                Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + fitness);
                Console.WriteLine(best);
                
                //Thread.Sleep(500);
            }
        }

        public static void TestGennetics2()
        {
            GenString2 target = new GenString2(
                "Twas brillig, and the slithy toves did gyer and gimble in the wabe.");
            GenString2 best = new GenString2("Hello");

            Console.WriteLine("Target: " + target);

            Fitness<GenString2> fit = delegate(GenString2 gs)
            {
                double dist = gs.Distance(target);
                //return 1.0 / (dist + 1.0);
                return -dist;
            };

            EvolMonogen<GenString2> evol = new EvolMonogen<GenString2>(100, 0.1, fit);
            evol.Initialise(best);

            for (int i = 0; i < MAX_GEN; i++)
            {
                evol.Evolve();
                evol.GetTopSpec(best);

                double fitness = evol.TopFitness;

                Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + fitness);
                Console.WriteLine(best);

                Thread.Sleep(500);
            }
        }

        public static void TestGennetics3()
        {
            GenString target = new GenString(
                "Twas brillig, and the slithy toves did gyer and gimble in the wabe.");
            GenString best = new GenString("Hello");

            Console.WriteLine("Target: " + target);

            Fitness<GenString> fit = delegate(GenString gs)
            {
                double dist = gs.Distance(target);
                //return 1.0 / (dist + 1.0);
                //return -dist;

                dist = 100.0 - dist;
                if (dist < 0.1) dist = 0.1;
                return dist;
            };

            EvolSpecies<GenString> evol = new EvolSpecies<GenString>(100, 0.1, fit);
            evol.Initialise(best);

            for (int i = 0; i < MAX_GEN; i++)
            {
                evol.Evolve();
                evol.GetTopSpec(best);

                double fitness = evol.TopFitness;
                int species = evol.NumSpecies;

                Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + fitness);
                Console.WriteLine("Num Species: " + species);
                Console.WriteLine(best);

                //Thread.Sleep(500);
            }
        }

        #endregion //////////////////////////////////////////////////////////////////////

        public static void SortTest()
        {
            List<Int32> numbers = new List<Int32>
            { 
                3, 13, 20, 14, 6, 12, 10, 5, 19, 8, 18, 1, 9, 15, 2, 16, 17, 7, 4, 11
            };

            Console.WriteLine("Original Sequence... ");
            foreach (int n in numbers)
            {
                Console.Write(n + ", ");
            }

            Console.WriteLine();

            numbers.Sort((x, y) => -x.CompareTo(y));

            Console.WriteLine("Sorted Sequence... ");
            foreach (int n in numbers)
            {
                Console.Write(n + ", ");
            }

            Console.WriteLine();
        }
    }
}

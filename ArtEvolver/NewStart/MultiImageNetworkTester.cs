using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using System.Drawing.Imaging;

using ArtEvolver;

//using ArtEvolverConsole.NetworkTests;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.Matrices;
using Vulpine.Core.Calc.RandGen;

namespace ArtEvolver.NewStart
{
    public static class MultiImageNetworkTester
    {
        public const string Path = @"S:\Animation\_Source\MultiImageNetworkTester\";

        private static string[] files = { };



        public const string Out_Format = @"S:\Animation\_Multi\Run_{0:000}\{0:000}_{1:000}_{2}.{3}";

        public const string Dir_Format = @"S:\Animation\_Multi\Run_{0:000}";

        public const int START_POINT = 8;

        //public const int NUM_MUTATIONS = 50;

        public const int PICS_PER_NETWORK = 500;




        public static void RunTest01()
        {
            VRandom rng = new RandMT();
            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            ////extracts the full path names for each of the images
            //string[] full_names = new string[files.Length];
            //for (int i = 0; i < files.Length; i++)
            //    full_names[i] = Path + files[i];

            //NOTE: Need To Verify This!!!
            string[] full_names = Directory.GetFiles(Path);

            //creates the Image Cube used for interpolations
            ImageCube imgcube = ImageCube.CreateFromFiles(full_names);
            

            for (int i = START_POINT; i < 1000; i++)
            {
                //creates a new network with the given image cube
                MultiImageNetwork2 network = new MultiImageNetwork2(imgcube);

                Console.WriteLine("Building Network {0}:", i);

                int net_size = rng.RandInt(100, 400);

                for (int n = 0; n < net_size; n++)
                {
                    network.Mutate(rng, 1.0);
                }

                Console.WriteLine("Mutations: {0}, Nodes: {1}, Axons: {2}", 
                    net_size, network.NumNurons, network.NumAxons);

                //we must create the directory before we can save to it!
                string net_dir = String.Format(Dir_Format, i);
                Console.WriteLine(net_dir);
                Directory.CreateDirectory(net_dir);


                for (int j = 0; j < PICS_PER_NETWORK; j++)
                {
                    network.Randomize(rng);

                    Console.Write("    Rendering Network # {0:000}  ", j);

                    string outA = String.Format(Out_Format, i, j, "A", "png");
                    string outB = String.Format(Out_Format, i, j, "B", "png");

                    //renders only the first half of the network
                    network.FirstOnly = true;
                    ren.Render(network, output);
                    output.BMP.Save(outA, ImageFormat.Png);
                    Console.Write(".");

                    //renders the entire network
                    network.FirstOnly = false;
                    ren.Render(network, output);
                    output.BMP.Save(outB, ImageFormat.Png);
                    Console.Write(".");

                    string first =  String.Format(Out_Format, i, j, "n1", "txt");
                    string second = String.Format(Out_Format, i, j, "n2", "txt");

                    //writes the network weights to a text file
                    network.WriteFirst(first);
                    network.WriteSecond(second);
                    Console.Write(".");

                    Console.WriteLine();
                }
            }


        }

    }
}

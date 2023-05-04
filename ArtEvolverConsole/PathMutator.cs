using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using ArtEvolver;

using ArtEvolverConsole.NetworkTests;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.Matrices;
using Vulpine.Core.Calc.RandGen;

namespace ArtEvolverConsole
{
    class PathMutator
    {
        public const string FILE_IN = @"S:\Animation\_Source\line.jpg";

        public const string FILE_OUT = @"S:\Animation\";

        public const int MAX_GEN = 40960;

        public static void RandomMutaitons()
        {

            PathCPPN network = new PathCPPN(true);
            VRandom rng = new RandMT();

            network.Randomize(rng);

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Building Network...");

            for (int i = 0; i < 1000; i++)
            {
                network.Mutate(rng, 1.0);
            }


            for (int i = 0; i < MAX_GEN; i++)
            {
                //network.Mutate(rng, 1.0); //0.1
                network.Randomize(rng);

                
                string index = i.ToString("0000"); // +".png";
                Console.Write(".");

                //renders 0 degrees
                network.SetPoint(0.0, 1.0);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "A.png");
                Console.Write(".");

                //renders 60 degrees
                network.SetPoint(0.866, 0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "B.png");
                Console.Write(".");

                //renders 120 degrees
                network.SetPoint(0.866, -0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "C.png");
                Console.Write(".");

                //renders 180 degrees
                network.SetPoint(0.0, -1.0);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "D.png");
                Console.Write(".");

                //renders 240 degrees
                network.SetPoint(-0.866, -0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "E.png");
                Console.Write(".");

                //renders 300 degrees
                network.SetPoint(-0.866, 0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "F.png");
                Console.Write(".");

                //writes out a text representation of the network
                network.WriteFile(FILE_OUT + index + ".txt");
                Console.WriteLine();

                int nodes = network.NumNurons;
                int axons = network.NumAxons;
                //double fit = fitness(network);

                //Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons);
            }

        }



        private const int POOL_SIZE = 10;

        private const int MPG = 10;


        public static void RandomMutaitons2()
        {
            int[] ages = new int[POOL_SIZE];
            PathCPPN[] pool = new PathCPPN[POOL_SIZE];
            VRandom rng = new RandMT();

            Console.WriteLine("Creating Initial Population:");
            for (int i = 0; i < POOL_SIZE; i++)
            {
                PathCPPN temp = new PathCPPN(true);

                for (int j = 0; j < MPG; j++)
                temp.Mutate(rng, 1.0);

                temp.Randomize(rng);
                pool[i] = temp;
                ages[i] = 0;
                Console.Write(".");
            }

            Console.WriteLine();
            Console.WriteLine();

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(128, 128);

            //used as a working space
            PathCPPN network = new PathCPPN(true);
            int age = 0;


            for (int i = 0; i < MAX_GEN; i++)
            {

                

                //copies the values into our working network
                int inx = rng.RandInt(POOL_SIZE);
                network.Overwrite(pool[inx]);
                age = ages[inx] + 1;

                //selects an out x value
                int outx = rng.RandInt(POOL_SIZE);
                int outage = ages[outx];
                int trycount = 0;

                //makes shure the network we are replacing is younger
                while (age <= outage && trycount < 100)
                {
                    outx = rng.RandInt(POOL_SIZE);
                    outage = ages[outx];
                    trycount++;
                }

                //preforms the desired number of mutations
                for (int j = 0; j < MPG; j++)
                    network.Mutate(rng, 1.0);

                //randomizes the values after mutating
                network.Randomize(rng);


                string index = i.ToString("0000"); // +".png";
                Console.Write(".");

                //renders 0 degrees
                network.SetPoint(0.0, 1.0);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "A.png");
                Console.Write(".");

                //renders 60 degrees
                network.SetPoint(0.866, 0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "B.png");
                Console.Write(".");

                //renders 120 degrees
                network.SetPoint(0.866, -0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "C.png");
                Console.Write(".");

                //renders 180 degrees
                network.SetPoint(0.0, -1.0);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "D.png");
                Console.Write(".");

                //renders 240 degrees
                network.SetPoint(-0.866, -0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "E.png");
                Console.Write(".");

                //renders 300 degrees
                network.SetPoint(-0.866, 0.5);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "F.png");
                Console.Write(".");

                //writes out a text representation of the network
                network.WriteFile(FILE_OUT + index + ".txt");
                Console.WriteLine();

                int nodes = network.NumNurons;
                int axons = network.NumAxons;
                //double fit = fitness(network);

                //Console.WriteLine();
                //Console.WriteLine("Generation " + i + ": " + nodes + " " + axons);

                Console.WriteLine("Generation {0}: ({1} -> {2}), {3}, {4}",
                    age,
                    inx,
                    outx,
                    nodes,
                    axons);

                pool[outx].Overwrite(network);
                ages[outx] = age;

            }

        }



        public static void RandomMutaitons3()
        {
            var bmp = new System.Drawing.Bitmap(FILE_IN);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.BiLiniar);

            CombCPPN network = new CombCPPN(txt);
            VRandom rng = new RandMT();

            network.Randomize(rng);

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Building Network...");

            for (int i = 0; i < 1000; i++)
            {
                network.Mutate(rng, 1.0);
            }


            for (int i = 0; i < MAX_GEN; i++)
            {
                //network.Mutate(rng, 1.0); //0.1
                network.Randomize(rng);


                string index = i.ToString("0000"); // +".png";
                Console.Write(".");

                //renders 0 degrees
                network.SetPoint(0.0, 1.0);
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "A.png");
                Console.Write(".");

                ////renders 60 degrees
                //network.SetPoint(0.866, 0.5);
                //ren.Render(network, output);
                //output.BMP.Save(FILE_OUT + index + "B.png");
                //Console.Write(".");

                ////renders 120 degrees
                //network.SetPoint(0.866, -0.5);
                //ren.Render(network, output);
                //output.BMP.Save(FILE_OUT + index + "C.png");
                //Console.Write(".");

                ////renders 180 degrees
                //network.SetPoint(0.0, -1.0);
                //ren.Render(network, output);
                //output.BMP.Save(FILE_OUT + index + "D.png");
                //Console.Write(".");

                ////renders 240 degrees
                //network.SetPoint(-0.866, -0.5);
                //ren.Render(network, output);
                //output.BMP.Save(FILE_OUT + index + "E.png");
                //Console.Write(".");

                ////renders 300 degrees
                //network.SetPoint(-0.866, 0.5);
                //ren.Render(network, output);
                //output.BMP.Save(FILE_OUT + index + "F.png");
                //Console.Write(".");

                //writes out a text representation of the network
                network.WriteFile(FILE_OUT + index + ".txt");
                Console.WriteLine();

                int nodes = network.NumNurons;
                int axons = network.NumAxons;
                //double fit = fitness(network);

                //Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons);
            }

        }





        public static void RandomMutaitons4()
        {
            var bmp = new System.Drawing.Bitmap(FILE_IN);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.BiLiniar);

            CombCPPN2 network = new CombCPPN2(txt);
            VRandom rng = new RandMT();

            network.Randomize(rng);

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Building Network...");

            for (int i = 0; i < 200; i++)
            {
                network.Mutate(rng, 1.0);
            }


            for (int i = 0; i < MAX_GEN; i++)
            {
                //network.Mutate(rng, 1.0); //0.1
                network.Randomize(rng);


                string index = i.ToString("0000"); // +".png";
                Console.Write(".");

                //renders 0 degrees
                network.Mode = 1;
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "A.png");
                Console.Write(".");

                network.Mode = 2;
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "B.png");
                Console.Write(".");

                network.Mode = 3;
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "C.png");
                Console.Write(".");

                //writes out a text representation of the network
                //network.WriteFile(FILE_OUT + index + ".txt");
                Console.WriteLine();

                int nodes = network.NumNurons;
                int axons = network.NumAxons;
                //double fit = fitness(network);

                //Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons);
            }

        }


        public const string FILE_1 = @"S:\Animation\_Source\cupcake.jpg";
        public const string FILE_2 = @"S:\Animation\_Source\bath.jpg";
        public const string FILE_3 = @"S:\Animation\_Source\hairbrush.jpg";
        public const string FILE_4 = @"S:\Animation\_Source\fan.jpg";


        public static void RandomMutaitons5()
        {
            var bmp1 = new System.Drawing.Bitmap(FILE_1);
            ImageSys img1 = new ImageSys(bmp1);
            Texture txt1 = new Interpolent(img1, Intpol.BiLiniar);

            var bmp2 = new System.Drawing.Bitmap(FILE_2);
            ImageSys img2 = new ImageSys(bmp2);
            Texture txt2 = new Interpolent(img2, Intpol.BiLiniar);

            var bmp3 = new System.Drawing.Bitmap(FILE_3);
            ImageSys img3 = new ImageSys(bmp3);
            Texture txt3 = new Interpolent(img3, Intpol.BiLiniar);

            var bmp4 = new System.Drawing.Bitmap(FILE_4);
            ImageSys img4 = new ImageSys(bmp4);
            Texture txt4 = new Interpolent(img4, Intpol.BiLiniar);

            CombCPPN3 network = new CombCPPN3(txt1, txt2, txt3, txt4);
            VRandom rng = new RandMT();

            network.Randomize(rng);

            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);

            //img.BMP.Save(FILE_OUT + "test.png");

            Console.WriteLine("Building Network...");

            for (int i = 0; i < 200; i++)
            {
                network.Mutate(rng, 1.0);
            }


            for (int i = 0; i < MAX_GEN; i++)
            {
                //network.Mutate(rng, 1.0); //0.1
                network.Randomize(rng);


                string index = i.ToString("0000"); // +".png";
                Console.Write(".");

                //renders 0 degrees
                network.Mode = 1;
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "A.png");
                Console.Write(".");

                network.Mode = 2;
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "B.png");
                Console.Write(".");

                network.Mode = 3;
                ren.Render(network, output);
                output.BMP.Save(FILE_OUT + index + "C.png");
                Console.Write(".");

                //writes out a text representation of the network
                //network.WriteFile(FILE_OUT + index + ".txt");
                Console.WriteLine();

                int nodes = network.NumNurons;
                int axons = network.NumAxons;
                //double fit = fitness(network);

                //Console.WriteLine();
                Console.WriteLine("Generation " + i + ": " + nodes + " " + axons);
            }

        }


        #region The Big One...


        public const string Path = @"S:\Animation\_Source\TheBigOne\";

        private static string[] files = { };


        public const string Out_Format = @"S:\Animation\_Big\{0:0000}_{1:000}_{2}.{3}";

        public const int START_POINT = 52;

        public static void TheBigOne()
        {
            VRandom rng = new RandMT();
            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);
            ImageSys small = new ImageSys(256, 256);

            //NOTE: Need To Verify This !!!
            files = Directory.GetFiles(Path);


            for (int i = START_POINT; i < 9999; i++)
            {
                //int index = i % files.Length;
                int index = rng.RandInt(files.Length);
                string file_in = Path + files[index];

                var bmp = new System.Drawing.Bitmap(file_in);
                ImageSys img = new ImageSys(bmp);
                Texture txt = new Interpolent(img, Intpol.BiLiniar);

                CombCPPN2 network = new CombCPPN2(txt);
                network.Randomize(rng);

                

                //img.BMP.Save(FILE_OUT + "test.png");

                Console.WriteLine("Building Network {0}:", i);

                for (int n = 0; n < 200; n++)
                {
                    network.Mutate(rng, 1.0);
                }

                Console.WriteLine("Nodes: {0}, Axons: {1}", network.NumNurons, network.NumAxons);

                for (int j = 0; j < 200; j++)
                {
                    network.Randomize(rng);

                    Console.Write("    Rendering Network # {0:000}  ", j);


                    string outA = String.Format(Out_Format, i, j, "A", "png");
                    string outB = String.Format(Out_Format, i, j, "B", "png");
                    string outC = String.Format(Out_Format, i, j, "C", "png");


                    //renders 0 degrees
                    network.Mode = 1;
                    ren.Render(network, small);
                    small.BMP.Save(outA);
                    Console.Write(".");

                    network.Mode = 2;
                    ren.Render(network, small);
                    small.BMP.Save(outB);
                    Console.Write(".");

                    network.Mode = 3;
                    ren.Render(network, output);
                    output.BMP.Save(outC);
                    Console.Write(".");

                    string first = String.Format(Out_Format, i, j, "n1", "txt");
                    string second = String.Format(Out_Format, i, j, "n2", "txt");

                    network.WriteFirst(first);
                    network.WriteSecond(second);
                    Console.Write(".");


                    //writes out a text representation of the network
                    //network.WriteFile(FILE_OUT + index + ".txt");
                    Console.WriteLine();
                }
            }

        }

        public const string Out_Format_Alt = @"S:\Animation\_Big2\Run_{0:000}\{0:000}_{1:000}_{2}_{3}.{4}";

        public const int START_POINT_Alt = 1;


        public static void TheBigOne_Alt()
        {
            VRandom rng = new RandMT();
            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);
            ImageSys small = new ImageSys(256, 256);


            //NOTE: Need To Verify This !!!
            files = Directory.GetFiles(Path);


            Texture[] textures = new Texture[files.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                string file_in = Path + files[i];

                var bmp = new System.Drawing.Bitmap(file_in);
                ImageSys img = new ImageSys(bmp);
                textures[i] = new Interpolent(img, Intpol.BiLiniar);
            }


            for (int i = START_POINT_Alt; i < 999; i++)
            {
                ////int index = i % files.Length;
                //int index = rng.RandInt(files.Length);
                //string file_in = Path + files[index];

                //var bmp = new System.Drawing.Bitmap(file_in);
                //ImageSys img = new ImageSys(bmp);
                //Texture txt = new Interpolent(img, Intpol.BiLiniar);

                string net_dir = String.Format(@"S:\Animation\_Big2\Run_{0:000}", i);
                Console.WriteLine(net_dir);
                Directory.CreateDirectory(net_dir);


                CombCPPN2 network = new CombCPPN2(textures[0]);
                network.Randomize(rng);



                //img.BMP.Save(FILE_OUT + "test.png");

                Console.WriteLine("Building Network {0}:", i);

                int net_size = rng.RandInt(100, 600);


                for (int n = 0; n < net_size; n++)
                {
                    network.Mutate(rng, 1.0);
                }

                int nurons = network.NumNurons;
                int axons = network.NumAxons;
                int hidden = nurons - 17;

                Console.WriteLine("Mutations: {0}, Nodes: {1}, Axons: {2}, Hidden: {3}", net_size, nurons, axons, hidden);

                for (int j = 0; j < 500; j++)
                {
                    network.Randomize(rng);

                    Console.Write("    Rendering Network # {0:000}  ", j);

                    int image = rng.RandInt(textures.Length);
                    Texture tex = textures[image];
                    int dot = files[image].IndexOf('.');
                    string name = files[image].Substring(0, dot);


                    //selects a random texture
                    network.SetTexture(tex);


                    string outA = String.Format(Out_Format_Alt, i, j, name, "A", "png");
                    string outB = String.Format(Out_Format_Alt, i, j, name, "B", "png");
                    string outC = String.Format(Out_Format_Alt, i, j, name, "C", "png");


                    //renders 0 degrees
                    network.Mode = 1;
                    ren.Render(network, small);
                    small.BMP.Save(outA);
                    Console.Write(".");

                    network.Mode = 2;
                    ren.Render(network, small);
                    small.BMP.Save(outB);
                    Console.Write(".");

                    network.Mode = 3;
                    ren.Render(network, output);
                    output.BMP.Save(outC);
                    //Console.Write(".");
                  

                    string first = String.Format(Out_Format_Alt, i, j, name, "n1", "txt");
                    string second = String.Format(Out_Format_Alt, i, j, name, "n2", "txt");

                    network.WriteFirst(first);
                    network.WriteSecond(second);
                    Console.Write(".");


                    //writes out a text representation of the network
                    //network.WriteFile(FILE_OUT + index + ".txt");
                    Console.WriteLine();
                }
            }

        }

        #endregion


        public const string NetPath = @"S:\Animation\_Big\_NetSave\";
        public const string AniPath = @"S:\Animation\_Big\_Animation\Test04\";


        public const string Ani_Image = @"S:\Animation\_Source\cupcake.jpg";

        public const string Ani_N1_A = NetPath + @"0004_124n1.txt";
        public const string Ani_N1_B = NetPath + @"0004_124n2.txt";

        public const string Ani_N2_A = NetPath + @"0004_184n1.txt";
        public const string Ani_N2_B = NetPath + @"0004_184n2.txt";

        public const int Ani_Frames = 1800;

        public static void AniTest()
        {
            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);
            ImageSys small = new ImageSys(256, 256);

            var bmp = new System.Drawing.Bitmap(Ani_Image);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.BiLiniar);

            CombCPPN2 start_net = new CombCPPN2(txt, Ani_N1_A, Ani_N1_B);
            CombCPPN2 end_net = new CombCPPN2(txt, Ani_N2_A, Ani_N2_B);

            CombCPPN2 build_net = start_net.Clone();


            Console.WriteLine("Drawing Starting Network...");
            ren.Render(start_net, small);
            small.BMP.Save(AniPath + @"_start.png");

            Console.WriteLine("Drawing Ending Network...");
            ren.Render(end_net, small);
            small.BMP.Save(AniPath + @"_end.png");


            for (int i = 0; i <= Ani_Frames; i++)
            {
                double t = (double)i / Ani_Frames;
                String id = i.ToString("0000");

                build_net.Lerp(start_net, end_net, t);
                ren.Render(build_net, small);
                small.BMP.Save(AniPath + id + ".png");

                Console.WriteLine(id + "\t" + t);
            }


            Console.WriteLine();
            Console.WriteLine("Finished!!");

        }



        private static readonly string[] networks = 
        {
            "0004_003",
            "0004_011",
            "0004_015",
            "0004_022",
            "0004_024",
            "0004_047",
            "0004_050",
            "0004_055",
            "0004_059",
            "0004_065",
            "0004_073",
            "0004_091",
            "0004_106",
            "0004_108",
            "0004_117",
            "0004_119",
            "0004_124",
            "0004_134",
            "0004_140",
            "0004_142",
            "0004_146",
            "0004_151",
            "0004_154",
            "0004_166",
            "0004_167",
            "0004_176",
            "0004_179",
            "0004_184",
            "0004_185",
            "0004_189",
            "0004_190",
            "0004_195",





        };


        private const int Frames_Per_Transition = 5 * 30;


        public static void AniMultTest()
        {
            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);
            //ImageSys small = new ImageSys(256, 256);

            var bmp = new System.Drawing.Bitmap(Ani_Image);
            ImageSys img = new ImageSys(bmp);
            Texture txt = new Interpolent(img, Intpol.BiLiniar);

            CombCPPN2 start_net = null; // = new CombCPPN2(txt, Ani_N1_A, Ani_N1_B);
            CombCPPN2 end_net = null; // = new CombCPPN2(txt, Ani_N2_A, Ani_N2_B);

            CombCPPN2 build_net = null; // = start_net.Clone();


            //Console.WriteLine("Drawing Starting Network...");
            //ren.Render(start_net, small);
            //small.BMP.Save(AniPath + @"_start.png");

            //Console.WriteLine("Drawing Ending Network...");
            //ren.Render(end_net, small);
            //small.BMP.Save(AniPath + @"_end.png");


            for (int i = 0; i < networks.Length; i++)
            {
                int j = (i + 1) % networks.Length;
                //if (i + 1 >= networks.Length) i = 0;

                string f1a = NetPath + networks[i] + "n1.txt";
                string f1b = NetPath + networks[i] + "n2.txt";
                string f2a = NetPath + networks[j] + "n1.txt";
                string f2b = NetPath + networks[j] + "n2.txt";

                Console.WriteLine();
                Console.WriteLine(f1a);
                Console.WriteLine(f1b);
                Console.WriteLine(f2a);
                Console.WriteLine(f2a);
                Console.WriteLine();


                if (start_net != null) start_net.Dispose();
                if (end_net != null) end_net.Dispose();
                if (build_net != null) build_net.Dispose();

                start_net = new CombCPPN2(txt, f1a, f1b);
                end_net = new CombCPPN2(txt, f2a, f2b);
                build_net = start_net.Clone();

                String iid = i.ToString("000");


                for (int k = 0; k < Frames_Per_Transition; k++)
                {
                    double t = (double)k / Frames_Per_Transition;
                    String kid = k.ToString("000");

                    String fid = (k + i * Frames_Per_Transition).ToString("00000");

                    build_net.Lerp(start_net, end_net, t);
                    ren.Render(build_net, output);
                    output.BMP.Save(AniPath + fid + ".png");

                    Console.WriteLine(fid + "\t" + iid + "\t" + kid);
                }



            }


            //for (int i = 0; i <= Ani_Frames; i++)
            //{
            //    double t = (double)i / Ani_Frames;
            //    String id = i.ToString("0000");

            //    build_net.Lerp(start_net, end_net, t);
            //    ren.Render(build_net, small);
            //    small.BMP.Save(AniPath + id + ".png");

            //    Console.WriteLine(id + "\t" + t);
            //}


            Console.WriteLine();
            Console.WriteLine("Finished!!");

        }



        #region The Huge One...


        public const string Source_Path = @"S:\Animation\_Source\TheHugeOne\";

        private static string[] huge_files = { };

        public const string Huge_Out_Format = @"S:\Animation\_Huge\{0:000}_{1:0000}_{2}.{3}";

        public const int HUGE_START_POINT = 6;

        public const int HUGE_Mutations = 10000;

        public const int HUGE_Epoch = 100;

        public const int HUGE_Singles = 3;
        public const int HUGE_Doubles = 2;

        public static void TheHugeOne()
        {
            VRandom rng = new RandMT();
            Renderor ren = new Renderor();
            ImageSys output = new ImageSys(256, 256);
            //ImageSys small = new ImageSys(256, 256);


            //NOTE: Need To Verify This !!!
            huge_files = Directory.GetFiles(Source_Path);



            int hf = huge_files.Length;


            for (int i = HUGE_START_POINT; i < 999; i++)
            {
                ////int index = i % files.Length;
                //int index = rng.RandInt(files.Length);
                //string file_in = Path + files[index];

                //var bmp = new System.Drawing.Bitmap(file_in);
                //ImageSys img = new ImageSys(bmp);
                //Texture txt = new Interpolent(img, Intpol.BiLiniar);

                //CombCPPN2 network = new CombCPPN2(txt);
                //network.Randomize(rng);


                Console.WriteLine("Building Network {0}:", i);

                //adds all the files to the network
                CombCPPN_Multi3 network = new CombCPPN_Multi3(huge_files.Length);
                for (int k = 0; k < huge_files.Length; k++)
                {
                    string file_in = Source_Path + huge_files[k];
                    var bmp = new System.Drawing.Bitmap(file_in);
                    ImageSys img = new ImageSys(bmp);
                    Texture txt = new Interpolent(img, Intpol.BiLiniar);
                    network.AddTexture(k, txt);
                }


                for (int n = 0; n < HUGE_Mutations; n++)
                {
                    network.Mutate(rng, 1.0);
                }

                Console.Write("Network Built !! ");
                Console.WriteLine("Nodes: {0}, Axons: {1}", network.NumNurons, network.NumAxons);
                Console.WriteLine();

                

                for (int j = 0; j < HUGE_Epoch; j++)
                {
                    network.Randomize(rng);

                    Console.Write("    Rendering Network # {0:0000}  ", j);


                    //string outA = String.Format(Huge_Out_Format, i, j, "A", "png");
                    //string outB = String.Format(Huge_Out_Format, i, j, "B", "png");
                    //string outC = String.Format(Huge_Out_Format, i, j, "C", "png");


                    //network.Mode = 0;
                    //ren.Render(network, output);
                    //output.BMP.Save(outA);
                    //Console.Write(".");

                    ////network.Mode = 1;
                    ////ren.Render(network, output);
                    ////output.BMP.Save(outB);
                    ////Console.Write(".");

                    ////network.Mode = 2;
                    ////ren.Render(network, output);
                    ////output.BMP.Save(outC);
                    ////Console.Write(".");


                    string first = String.Format(Huge_Out_Format, i, j, "n1", "txt");
                    string second = String.Format(Huge_Out_Format, i, j, "n2", "txt");

                    network.WriteFirst(first);
                    network.WriteSecond(second);
                    Console.Write(".");

                    

                    for (int k = 0; k < HUGE_Singles; k++)
                    {
                        int s1 = rng.RandInt(hf);
                        Vector sel = SelectOne(s1, hf);
                        string final = s1.ToString("00");
                        string outs = String.Format(Huge_Out_Format, i, j, final, "png"); 

                        network.SetSelector(sel);
                        ren.Render(network, output);
                        output.BMP.Save(outs);
                        Console.Write(".");

                    }

                    for (int k = 0; k < HUGE_Doubles; k++)
                    {
                        int s1 = rng.RandInt(hf);
                        int s2 = rng.RandInt(hf);
                        Vector sel = SelectTwo(s1, s2, hf);
                        string final = s1.ToString("00") + "_" + s2.ToString();
                        string outs = String.Format(Huge_Out_Format, i, j, final, "png");

                        network.SetSelector(sel);
                        ren.Render(network, output);
                        output.BMP.Save(outs);
                        Console.Write(".");

                    }

                    


                    //writes out a text representation of the network
                    //network.WriteFile(FILE_OUT + index + ".txt");
                    Console.WriteLine();
                }
            }

        }

        private static Vector SelectOne(int s1, int size)
        {
            Vector sel = new Vector(size);

            for (int i = 0; i < size; i++)
            {
                if (i == s1) sel[i] = 1.0;
                else sel[i] = 0.0;
            }

            return sel;
        }

        private static Vector SelectTwo(int s1, int s2, int size)
        {
            Vector sel = new Vector(size);

            for (int i = 0; i < size; i++)
            {
                if (i == s1) sel[i] = 1.0;
                else if (i == s2) sel[i] = 1.0;
                else sel[i] = 0.0;
            }

            return sel;
        }

        #endregion

    }
}

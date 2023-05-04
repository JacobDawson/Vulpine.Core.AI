using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Nural;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace ArtEvolver
{
    public class CombCPPN3 : Genetic<CombCPPN3>, Texture
    {
        private const int DEPTH = 32;

        //Net1: [x, y, d, 1] -> [xp, yp, s1, s2, r1, r2]
        //Net2: [y, u, v, r1, r2, 1] -> [yp, up, vp]

        private NetworkAuto net1;
        private NetworkAuto net2;

        private Texture tex1;
        private Texture tex2;
        private Texture tex3;
        private Texture tex4;

        //stores the x, y cordinates for the current texture
        private double x;
        private double y;

        private int sample_mode;

        public CombCPPN3(Texture tex1, Texture tex2, Texture tex3, Texture tex4)
        {
            net1 = new NetworkAuto(4, 6, false);
            net2 = new NetworkAuto(6, 3, false);

            this.tex1 = tex1;
            this.tex2 = tex2;
            this.tex3 = tex3;
            this.tex4 = tex4;

            x = 0.0;
            y = 0.0;
        }

        internal CombCPPN3(NetworkAuto net1, NetworkAuto net2)
        {
            this.net1 = net1;
            this.net2 = net2;
            x = 0.0;
            y = 0.0;
        }

        //public CombCPPN(string file)
        //{
        //    network = new NetworkAuto(file);
        //    x = 0.0;
        //    y = 0.0;
        //}

        //public void WriteFile(string file)
        //{
        //    net1.WriteToFile(file);
        //}

        public int NumNurons
        {
            get { return net1.NumNurons + net2.NumNurons; }
        }

        public int NumAxons
        {
            get { return net1.NumAxons + net2.NumAxons; }
        }

        public int Mode
        {
            get { return sample_mode; }
            set { sample_mode = value; }
        }


        public void SetPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Color Sample(double u, double v)
        {
            switch (sample_mode % 3)
            {
                case 1: return SampleFirst(u, v);
                case 2: return SampleLast(u, v);
                default: return SampleBoth(u, v);
            }
        }

        private const double SF = 0.5;


        public Color SampleBoth(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();

            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            //sets the input to the network 1
            Vector input = new Vector(u, v, dist, 1.0);
            net1.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net1.Propergate();

            //obtains the output from net 1
            Vector out1 = net1.ReadOutput();
            double xp = out1[0] * SF;
            double yp = out1[1] * SF;
            double s1 = out1[2];
            double s2 = out1[3];
            double r1 = out1[4];
            double r2 = out1[5];

            //Color pix;
            //Vector yuv;


            ////samples each of the four textures
            //Color p1 = tex1.Sample(xp, yp);
            //Color p2 = tex2.Sample(xp, yp);
            //Color p3 = tex3.Sample(xp, yp);
            //Color p4 = tex4.Sample(xp, yp);

            ////interpolates the textures on a cube
            //Color p5 = Color.Lerp(p1, p2, s1);
            //Color p6 = Color.Lerp(p3, p4, s1);
            //Color p7 = Color.Lerp(p5, p6, s2);

            Color pix;

            if (s1 > 0.0)
            {
                if (s2 > 0.0) pix = tex1.Sample(xp, yp);
                else pix = tex2.Sample(xp, yp);
            }
            else
            {
                if (s2 > 0.0) pix = tex3.Sample(xp, yp);
                else pix = tex4.Sample(xp, yp);
            }

            //converts the color data
            Vector yuv = pix.ToYUV();
            double yi = (yuv[0] - 0.5) * 2.0;
            double ai = yuv[0] * 2.0;
            double bi = yuv[1] * 2.0;

            //sets the input to the network 2
            Vector input2 = new Vector(yi, ai, bi, r1, r2, 1.0);
            net2.SetInput(input2);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net2.Propergate();


            //constructs the color from the output
            Vector output2 = net2.ReadOutput();
            //double y = VMath.Sigmoid(output[0]);
            //double a = Math.Tanh(output[1]) * 0.5;
            //double b = Math.Tanh(output[2]) * 0.5;
            double w = (output2[0] + 1.0) * 0.5;
            double a = output2[1] * 0.5;
            double b = output2[2] * 0.5;
            return Color.FromYUV(w, a, b);
        }


        public Color SampleFirst(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();

            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            //sets the input to the network 1
            Vector input = new Vector(u, v, dist, 1.0);
            net1.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net1.Propergate();

            //obtains the output from net 1
            Vector out1 = net1.ReadOutput();
            double xp = out1[0] * SF;
            double yp = out1[1] * SF;
            double s1 = out1[2];
            double s2 = out1[3];

            ////samples the color data from the texture
            //Color pix = tex1.Sample(xp, yp);
            //return pix;


            Color pix;

            if (s1 > 0.0)
            {
                if (s2 > 0.0) pix = tex1.Sample(xp, yp);
                else pix = tex2.Sample(xp, yp);
            }
            else
            {
                if (s2 > 0.0) pix = tex3.Sample(xp, yp);
                else pix = tex4.Sample(xp, yp);
            }

            return pix;

            ////samples each of the four textures
            //Color p1 = tex1.Sample(xp, yp);
            //Color p2 = tex2.Sample(xp, yp);
            //Color p3 = tex3.Sample(xp, yp);
            //Color p4 = tex4.Sample(xp, yp);

            ////interpolates the textures on a cube
            //Color p5 = Color.Lerp(p1, p2, s1);
            //Color p6 = Color.Lerp(p3, p4, s1);
            //Color p7 = Color.Lerp(p5, p6, s2);

            //return p7;
        }



        public Color SampleLast(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();

            //samples the color data from the texture
            Color pix = tex1.Sample(u, v);
            Vector yuv = pix.ToYUV();
            double yi = (yuv[0] - 0.5) * 2.0;
            double ai = yuv[0] * 2.0;
            double bi = yuv[1] * 2.0;

            //sets the input to the network 2
            Vector input2 = new Vector(yi, ai, bi, u, v, 1.0);
            net2.SetInput(input2);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net2.Propergate();

            //constructs the color from the output
            Vector output2 = net2.ReadOutput();
            //double y = VMath.Sigmoid(output[0]);
            //double a = Math.Tanh(output[1]) * 0.5;
            //double b = Math.Tanh(output[2]) * 0.5;
            double w = (output2[0] + 1.0) * 0.5;
            double a = output2[1] * 0.5;
            double b = output2[2] * 0.5;
            return Color.FromYUV(w, a, b);
        }




        public void Overwrite(CombCPPN3 genome)
        {
            net1.Overwrite(genome.net1);
            net2.Overwrite(genome.net2);
        }

        public void Mutate(VRandom rng, double rate)
        {
            net1.Mutate(rng, rate);
            net2.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, CombCPPN3 genome)
        {
            net1.Crossover(rng, genome.net1);
            net2.Crossover(rng, genome.net2);
        }

        public double Compare(CombCPPN3 genome)
        {
            double d1 = net1.Compare(genome.net1);
            double d2 = net2.Compare(genome.net2);

            return d1 + d2;
        }

        //public CPPN SpawnRandom(VRandom rng)
        //{
        //    //throw new NotImplementedException();

        //    var next = network.SpawnRandom(rng);
        //    return new CPPN(next);
        //}

        public void Randomize(VRandom rng)
        {
            net1.Randomize(rng);
            net2.Randomize(rng);
        }

        public CombCPPN3 Clone()
        {
            NetworkAuto c1 = net1.Clone();
            NetworkAuto c2 = net2.Clone();

            return new CombCPPN3(c1, c2);
        }

        public void Dispose()
        {
            net1.Dispose();
        }

    }
}

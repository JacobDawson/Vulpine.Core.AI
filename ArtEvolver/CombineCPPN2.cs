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
    public class CombCPPN2 : Genetic<CombCPPN2>, Texture
    {
        private const int DEPTH = 16;

        //Net1: [x, y, d, 1] -> [xp, yp, r1, r2]
        //Net2: [y, u, v, r1, r2, 1] -> [yp, up, vp]

        private NetworkAuto net1;
        private NetworkAuto net2;
        private Texture texture;

        //stores the x, y cordinates for the current texture
        private double x;
        private double y;

        private int sample_mode;

        public CombCPPN2(Texture tex)
        {
            net1 = new NetworkAuto(4, 4, false);
            net2 = new NetworkAuto(6, 3, false);
            texture = tex;

            x = 0.0;
            y = 0.0;
        }

        public CombCPPN2(Texture tex, String nf1, String nf2)
        {
            net1 = new NetworkAuto(nf1);
            net2 = new NetworkAuto(nf2);
            texture = tex;

            x = 0.0;
            y = 0.0; 
        }

        internal CombCPPN2(Texture tex, NetworkAuto net1, NetworkAuto net2)
        {          
            this.net1 = net1;
            this.net2 = net2;
            texture = tex;
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

        public void WriteFirst(string file)
        {
            net1.WriteToFile(file);
        }

        public void WriteSecond(string file)
        {
            net2.WriteToFile(file);
        }

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

        public void SetTexture(Texture tex)
        {
            this.texture = tex;
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
            double r1 = out1[2];
            double r2 = out1[3];

            //samples the color data from the texture
            Color pix = texture.Sample(xp, yp);
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

            //samples the color data from the texture
            Color pix = texture.Sample(xp, yp);
            return pix;
        }



        public Color SampleLast(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();

            //samples the color data from the texture
            Color pix = texture.Sample(u, v);
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




        public void Overwrite(CombCPPN2 genome)
        {
            net1.Overwrite(genome.net1);
            net2.Overwrite(genome.net2);
        }

        public void Mutate(VRandom rng, double rate)
        {
            net1.Mutate(rng, rate);
            net2.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, CombCPPN2 genome)
        {
            net1.Crossover(rng, genome.net1);
            net2.Crossover(rng, genome.net2);
        }

        public double Compare(CombCPPN2 genome)
        {
            double d1 = net1.Compare(genome.net1);
            double d2 = net2.Compare(genome.net2);

            return d1 + d2;
        }

        public void Lerp(CombCPPN2 genome, double amount)
        {
            net1.Lerp(genome.net1, amount);
            net2.Lerp(genome.net2, amount);
        }

        public void Lerp(CombCPPN2 start, CombCPPN2 target, double amount)
        {
            //overwrites both networks with the starting configurations
            net1.Overwrite(start.net1);
            net2.Overwrite(start.net2);

            //preforms the interpolation with the target networks
            net1.Lerp(target.net1, amount);
            net2.Lerp(target.net2, amount);
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

        public CombCPPN2 Clone()
        {
            //NOTE: Should I clone the texture as well???

            NetworkAuto c1 = net1.Clone();
            NetworkAuto c2 = net2.Clone();

            return new CombCPPN2(texture, c1, c2);
        }

        public void Dispose()
        {
            net1.Dispose();
        }

    }
}

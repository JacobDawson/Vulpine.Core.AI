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
    public class CombCPPN : Genetic<CombCPPN>, Texture
    {
        private const int DEPTH = 32;

        private NetworkAuto network;
        private Texture texture;

        //stores the x, y cordinates for the current texture
        private double x;
        private double y;

        public CombCPPN(Texture tex)
        {
            network = new NetworkAuto(7, 5, true);
            texture = tex;
            x = 0.0;
            y = 0.0;
        }

        internal CombCPPN(NetworkAuto network)
        {
            this.network = network;
            x = 0.0;
            y = 0.0;
        }

        //public CombCPPN(string file)
        //{
        //    network = new NetworkAuto(file);
        //    x = 0.0;
        //    y = 0.0;
        //}

        public void WriteFile(string file)
        {
            network.WriteToFile(file);
        }

        public int NumNurons
        {
            get { return network.NumNurons; }
        }

        public int NumAxons
        {
            get { return network.NumAxons; }
        }


        public void SetPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }


        public Color Sample(double u, double v)
        {
            //clears the network for each sample
            network.ResetNetwork();

            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            ////sets the input to the network
            //Vector input = new Vector(u, v, dist, x, y, 1.0);
            //network.SetInput(input);

            ////propagates until the maximum depth
            //for (int i = 0; i < DEPTH; i++) 
            //network.Propergate();


            Vector input, output, yuv;
            double yi = 0.0;
            double ui = 0.0;
            double vi = 0.0;
            double xp, yp;
            Color pix;

            //itterates the network, sampeling the texture each time
            for (int i = 0; i < DEPTH; i++)
            {
                input = new Vector(u, v, dist, yi, ui, vi, 1.0);
                network.SetInput(input);
                network.Propergate();

                output = network.ReadOutput();
                xp = output[0];
                yp = output[1];

                pix = texture.Sample(xp, yp);
                yuv = pix.ToYUV();

                yi = (yuv[0] - 0.5) * 2.0;
                ui = yuv[0] * 2.0;
                vi = yuv[1] * 2.0;
            }

            //constructs the color from the output
            output = network.ReadOutput();
            //double y = VMath.Sigmoid(output[0]);
            //double a = Math.Tanh(output[1]) * 0.5;
            //double b = Math.Tanh(output[2]) * 0.5;
            double w = (output[2] + 1.0) * 0.5;
            double a = output[3] * 0.5;
            double b = output[4] * 0.5;
            return Color.FromYUV(w, a, b);
        }

        public void Overwrite(CombCPPN genome)
        {
            network.Overwrite(genome.network);
        }

        public void Mutate(VRandom rng, double rate)
        {
            network.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, CombCPPN genome)
        {
            network.Crossover(rng, genome.network);
        }

        public double Compare(CombCPPN genome)
        {
            return network.Compare(genome.network);
        }

        //public CPPN SpawnRandom(VRandom rng)
        //{
        //    //throw new NotImplementedException();

        //    var next = network.SpawnRandom(rng);
        //    return new CPPN(next);
        //}

        public void Randomize(VRandom rng)
        {
            network.Randomize(rng);
        }

        public CombCPPN Clone()
        {
            NetworkAuto copy = network.Clone();
            return new CombCPPN(copy);
        }

        public void Dispose()
        {
            network.Dispose();
        }

    }
}

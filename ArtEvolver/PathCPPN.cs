using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Nural;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Draw;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace ArtEvolver
{
    public class PathCPPN : Genetic<PathCPPN>, Texture
    {
        private const int DEPTH = 32;

        private NetworkAuto network;

        //stores the x, y cordinates for the current texture
        private double x;
        private double y;

        public PathCPPN(bool recurent = false)
        {
            network = new NetworkAuto(6, 3, recurent);
            x = 0.0;
            y = 0.0;
        }

        internal PathCPPN(NetworkAuto network)
        {
            this.network = network;
            x = 0.0;
            y = 0.0;
        }

        public PathCPPN(string file)
        {
            network = new NetworkAuto(file);
            x = 0.0;
            y = 0.0;
        }

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

            //sets the input to the network
            Vector input = new Vector(u, v, dist, x, y, 1.0);
            network.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++) 
            network.Propergate();

            //constructs the color from the output
            Vector output = network.ReadOutput();
            //double y = VMath.Sigmoid(output[0]);
            //double a = Math.Tanh(output[1]) * 0.5;
            //double b = Math.Tanh(output[2]) * 0.5;
            double w = (output[0] + 1.0) * 0.5;
            double a = output[1] * 0.5;
            double b = output[2] * 0.5;
            return Color.FromYUV(w, a, b);
        }

        public void Overwrite(PathCPPN genome)
        {
            network.Overwrite(genome.network);
        }

        public void Mutate(VRandom rng, double rate)
        {
            network.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, PathCPPN genome)
        {
            network.Crossover(rng, genome.network);
        }

        public double Compare(PathCPPN genome)
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

        public PathCPPN Clone()
        {
            NetworkAuto copy = network.Clone();
            return new PathCPPN(copy);
        }

        public void Dispose()
        {
            network.Dispose();
        }

    }
}

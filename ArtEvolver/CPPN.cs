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
    public class CPPN : Genetic<CPPN>, Texture
    {
        private const int DEPTH = 32;

        private NetworkAuto network;

        public CPPN()
        {
            network = new NetworkAuto(4, 3);
        }

        internal CPPN(NetworkAuto network)
        {
            this.network = network;
        }

        public int NumNurons
        {
            get { return network.NumNurons; }
        }

        public int NumAxons
        {
            get { return network.NumAxons; }
        }


        public Color Sample(double u, double v)
        {
            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            //sets the input to the network
            Vector input = new Vector(u, v, dist, 1.0);
            network.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++) 
            network.Propergate();

            //constructs the color from the output
            Vector output = network.ReadOutput();
            //double y = VMath.Sigmoid(output[0]);
            //double a = Math.Tanh(output[1]) * 0.5;
            //double b = Math.Tanh(output[2]) * 0.5;
            double y = (output[0] + 1.0) * 0.5;
            double a = output[1] * 0.5;
            double b = output[2] * 0.5;
            return Color.FromYUV(y, a, b);
        }

        public void Overwrite(CPPN genome)
        {
            network.Overwrite(genome.network);
        }

        public void Mutate(VRandom rng, double rate)
        {
            network.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, CPPN genome)
        {
            network.Crossover(rng, genome.network);
        }

        public double Compare(CPPN genome)
        {
            return network.Compare(genome.network);
        }

        public CPPN SpawnRandom(VRandom rng)
        {
            //throw new NotImplementedException();

            var next = network.SpawnRandom(rng);
            return new CPPN(next);
        }

        public void Dispose()
        {
            network.Dispose();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Nural;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace ArtEvolverConsole.NetworkTests
{
    public class TestXor : Genetic<TestXor>
    {
        private const int DEPTH = 16;

        private static List<Vector> tests;

        private NetworkAuto network;

        public TestXor()
        {
            network = new NetworkAuto(3, 1);
        }

        internal TestXor(NetworkAuto network)
        {
            this.network = network;
        }

        public double Evaluate(double x, double y)
        {
            //we must reset the network with each invocation
            network.ResetNetwork();

            //sets the input to the network
            Vector input = new Vector(x, y, 1.0);
            network.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
            network.Propergate();

            //obtains the output from the network
            Vector output = network.ReadOutput();
            return output[0];
        }

        public static void InitialiseTests()
        {
            //check if we are already initailised
            if (tests != null) return;

            tests = new List<Vector>
            {
                //new Vector(1.0, 1.0, -1.0),
                //new Vector(1.0, -1.0, 1.0),
                //new Vector(-1.0, 1.0, 1.0),
                //new Vector(-1.0, -1.0, -1.0),

                new Vector(1.0, 1.0, 0.0),
                new Vector(1.0, 0.0, 1.0),
                new Vector(0.0, 1.0, 1.0),
                new Vector(0.0, 0.0, 0.0),
            };
        }

        public double RunTests()
        {
            InitialiseTests();

            double total = 0.0;

            for (int i = 0; i < tests.Count; i++)
            {
                Vector test = tests[i];
                double a = test[0];
                double b = test[1];
                double c = test[2];

                double cp = Evaluate(a, b);
                double e = cp - c;
                e = e * e;

                total += e;
            }

            total = (10.0 - total) / 10.0;
            if (total < 0.0001) total = 0.0001;

            return total;
        }


        #region Genetic Implementaiton...

        public int NumNurons
        {
            get { return network.NumNurons; }
        }

        public int NumAxons
        {
            get { return network.NumAxons; }
        }

        public void ReduceNetwork()
        {
            network.ReduceNetwork();
        }

        public void Overwrite(TestXor genome)
        {
            network.Overwrite(genome.network);
        }

        public void Mutate(VRandom rng, double rate)
        {
            network.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, TestXor genome)
        {
            network.Crossover(rng, genome.network);
        }

        public double Compare(TestXor genome)
        {
            return network.Compare(genome.network);
        }

        public void Randomize(VRandom rng)
        {
            network.Randomize(rng);
        }

        public TestXor Clone()
        {
            NetworkAuto copy = network.Clone();
            return new TestXor(copy);
        }

        public void Dispose()
        {
            network.Dispose();
        }

        #endregion
    }
}

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
    public class TestAdder : Genetic<TestAdder>
    {
        private const int DEPTH = 16;

        private static List<Vector> tests;

        private NetworkAuto network;

        public TestAdder()
        {
            network = new NetworkAuto(4, 2);
        }

        internal TestAdder(NetworkAuto network)
        {
            this.network = network;
        }

        public Vector Evaluate(double a, double b, double ci)
        {
            //we must reset the network with each invocation
            network.ResetNetwork();

            //sets the input to the network
            Vector input = new Vector(a, b, ci, 1.0);
            network.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                network.Propergate();

            //obtains the output from the network
            return network.ReadOutput();
        }

        public static void InitialiseTests()
        {
            //check if we are already initailised
            if (tests != null) return;

            tests = new List<Vector>
            {
                //new Vector(1.0, 1.0, 1.0, 1.0, 1.0),
                //new Vector(1.0, 1.0, 0.0, 1.0, 0.0),
                //new Vector(1.0, 0.0, 1.0, 1.0, 0.0),
                //new Vector(1.0, 0.0, 0.0, 0.0, 1.0),

                //new Vector(0.0, 1.0, 1.0, 1.0, 0.0),
                //new Vector(0.0, 1.0, 0.0, 0.0, 1.0),
                //new Vector(0.0, 0.0, 1.0, 0.0, 1.0),
                //new Vector(0.0, 0.0, 0.0, 0.0, 0.0),

                new Vector(1.0, 1.0, 1.0, 1.0, 1.0),
                new Vector(1.0, 1.0, -1.0, 1.0, -1.0),
                new Vector(1.0, -1.0, 1.0, 1.0, -1.0),
                new Vector(1.0, -1.0, -1.0, -1.0, 1.0),

                new Vector(-1.0, 1.0, 1.0, 1.0, -1.0),
                new Vector(-1.0, 1.0, -1.0, -1.0, 1.0),
                new Vector(-1.0, -1.0, 1.0, -1.0, 1.0),
                new Vector(-1.0, -1.0, -1.0, -1.0, -1.0),
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
                double ci = test[2];
                double co = test[3];
                double s = test[4];

                //double cp = Evaluate(a, b);
                //double e = cp - c;
                //e = e * e;

                Vector output = Evaluate(a, b, ci);
                double cd = co - output[0];
                double sd = s - output[1];

                total += (cd * cd);
                total += (sd * sd);
            }

            total = (16.0 - total) / 16.0;
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

        public void Overwrite(TestAdder genome)
        {
            network.Overwrite(genome.network);
        }

        public void Mutate(VRandom rng, double rate)
        {
            network.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, TestAdder genome)
        {
            network.Crossover(rng, genome.network);
        }

        public double Compare(TestAdder genome)
        {
            return network.Compare(genome.network);
        }

        public void Randomize(VRandom rng)
        {
            network.Randomize(rng);
        }

        public TestAdder Clone()
        {
            NetworkAuto copy = network.Clone();
            return new TestAdder(copy);
        }

        public void Dispose()
        {
            network.Dispose();
        }

        #endregion
    }
}

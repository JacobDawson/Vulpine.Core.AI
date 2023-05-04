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
    public class TestLatchSR : Genetic<TestLatchSR>
    {
        private const int DEPTH = 16;

        private NetworkAuto network;

        public TestLatchSR()
        {
            network = new NetworkAuto(3, 1, true);
        }

        internal TestLatchSR(NetworkAuto network)
        {
            this.network = network;
        }

        public Vector Evaluate(double s, double r)
        {
            //sets the input to the network
            Vector input = new Vector(s, r, 1.0);
            network.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                network.Propergate();

            //obtains the output from the network
            return network.ReadOutput();
        }

        public double TestNetwork(VRandom rng)
        {
            bool latch = false;
            bool is_set = false;

            int count = 0;
            double total = 0.0;

            while (count < 100)
            {
                bool set = rng.RandBool();
                bool reset = rng.RandBool();

                //skips invalid input
                if (set && reset) continue;

                if (set)
                {
                    latch = true;
                    is_set = true;

                    Vector output = Evaluate(1.0, -1.0);
                    double diff = 1.0 - output[0];
                    total += diff * diff;                   
                }
                else if (reset)
                {
                    latch = false;
                    is_set = true;

                    Vector output = Evaluate(-1.0, 1.0);
                    double diff = -1.0 - output[0];
                    total += diff * diff;    
                }
                else
                {
                    double expected = 0.0;
                    if (is_set)
                    {
                        if (latch) expected = 1.0;
                        else expected = -1.0;
                    }

                    Vector output = Evaluate(-1.0, -1.0);
                    double diff = expected - output[0];
                    total += diff * diff;   
                    
                }

                count++;
            }

            total = (50.0 - total) / 50.0;
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

        public void Overwrite(TestLatchSR genome)
        {
            network.Overwrite(genome.network);
        }

        public void Mutate(VRandom rng, double rate)
        {
            network.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, TestLatchSR genome)
        {
            network.Crossover(rng, genome.network);
        }

        public double Compare(TestLatchSR genome)
        {
            return network.Compare(genome.network);
        }

        public void Randomize(VRandom rng)
        {
            network.Randomize(rng);
        }

        public TestLatchSR Clone()
        {
            NetworkAuto copy = network.Clone();
            return new TestLatchSR(copy);
        }

        public void Dispose()
        {
            network.Dispose();
        }

        #endregion
    }
}

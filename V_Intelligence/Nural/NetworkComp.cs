using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI.Nural
{
    public class NetworkComp : Genetic<NetworkComp>
    {
        //global paramaters that determin how networks evolve
        private const double C0 = 2.0;
        private const double C1 = 1.0;

        //stores the nurons and axons of the network directly!
        private NuronComp[] nurons;
        private AxonComp[] axons;

        public NetworkComp(int size)
        {
            //NOTE: The maximum number of axons we could possably have is (n^2)
            //though for pragmatic reasons we should set the maximum to be a
            //value less than this, otherwise we could just use a connectivity matrix

            //computes the total number of posable axons;
            int atol = size * size;

            nurons = new NuronComp[size];
            axons = new AxonComp[atol];
        }

        public int NuronCount
        {
            get { return nurons.Length; }
        }

        public int AxonCount
        {
            get { return axons.Length; }
        }


        #region Genetic Implementaiton...

        public NetworkComp SpawnRandom(VRandom rng)
        {
            throw new NotImplementedException();
        }

        public void Overwrite(NetworkComp genome)
        {
            //copies all the nurons that it can, and ignors the rest
            for (int i = 0; i < nurons.Length; i++)
            {
                if (i < genome.NuronCount) 
                    nurons[i] = genome.nurons[i];
            }

            //copies all the axions, and disables missing ones
            for (int i = 0; i < axons.Length; i++)
            {
                if (i < genome.AxonCount) 
                    axons[i] = genome.axons[i];
                else axons[i].Enabled = false;
            }
        }

        public void Mutate(VRandom rng, double rate)
        {
            throw new NotImplementedException();
        }

        public void Crossover(VRandom rng, NetworkComp genome)
        {
            //determins the rate of crossover
            double rate = rng.RandDouble(0.25, 0.75);

            //calculates the maximum number of nurons and axons
            //this is nessary incase the genomes have diffrent numbers
            int nmax = Math.Min(this.NuronCount, genome.NuronCount);
            int amax = Math.Min(this.AxonCount, genome.AxonCount);

            for (int i = 0; i < nmax; i++)
            {
                if (rng.NextDouble() > rate) continue;

                NuronComp copy = genome.nurons[i];
                nurons[i].Funciton = copy.Funciton;
            }

            for (int i = 0; i < amax; i++)
            {
                if (rng.NextDouble() > rate) continue;

                //NOTE: should we copy the entier axon or just the weight?

                AxonComp copy = genome.axons[i];
                axons[i].Weight = copy.Weight;
                axons[i].Enabled = copy.Enabled;

                axons[i].Input = copy.Input;
                axons[i].Output = copy.Output;
                
            }

            throw new NotImplementedException();
        }

        public double Compare(NetworkComp genome)
        {
            //calculates the maximum number of nurons and axons
            //this is nessary incase the genomes have diffrent numbers
            int nmax = Math.Min(this.NuronCount, genome.NuronCount);
            int amax = Math.Min(this.AxonCount, genome.AxonCount);

            //used in computing the distance
            int disjoint = 0;
            double wbar = 0.0;

            for (int i = 0; i < amax; i++)
            {
                AxonComp source = this.axons[i];
                AxonComp target = genome.axons[i];

                if (source.IsSimilar(target))
                {
                    //computes the distance between the weights
                    double w1 = source.Weight;
                    double w2 = target.Weight;

                    wbar += Math.Abs(w1 - w2);
                }
                else
                {
                    //counts the number of disjoint axons
                    disjoint++;
                }
            }

            //computes the number of matches
            double match = amax - disjoint;

            //couputes the distance for specisation
            double dist = (C0 * disjoint) / (double)amax;
            dist += C1 * (wbar / (double)match);

            return dist;
        }

        //NOTE; The above method works best when whe asume that the axons are
        //already in sorted order. Which means we should probably sort the axons
        //if we preform a mutaiton or crossover where their order would change

        #endregion //////////////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            //Shoudl probaly do somehting important here
        }


        public void Initialise(VRandom rng)
        {
            throw new NotImplementedException();
        }

        public NetworkComp FromPrototype()
        {
            throw new NotImplementedException();
        }
    }
}

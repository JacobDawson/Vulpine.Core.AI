using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI.Genetics
{
    public class EvolMonogen<T> where T : Genetic<T>
    {
        //uses a random number generator to propell evolution
        private VRandom rng;

        //uses a fitness funciton to define the goal of evolution
        private Fitness<T> fit;

        //stores the population as an array
        private T[] pop;

        ////stores the single genome used to create a generation
        //private T genitor;

        //the rate of mutaiton
        private double rate;

        //keeps a running count of the generations
        private int gencount;

        //keeps a refrence to the best fit species and it's fitness
        private double topfit;
        private T topspec;


        //NOTE: Add an OnEvaluate event that trigers whenever the fitness funciton
        //is called. That way we display the progress between generations.


        public EvolMonogen(int popsize, double rate, Fitness<T> fitness)
        {
            //uses the Mersin Twister for RNG
            this.rng = new RandMT();

            this.pop = new T[popsize];
            this.rate = rate;
            this.fit = fitness;
        }


        public int Generaton
        {
            get { return gencount; }
        }

        public double TopFitness
        {
            get { return topfit; }
        }


        public void Initialise(T prototype)
        {
            topfit = Double.NegativeInfinity;
            gencount = 0;

            //uses the prototype as the current top species
            if (topspec != null) topspec.Dispose();
            topspec = prototype.Clone();    

            for (int i = 0; i < pop.Length; i++)
            {
                //fills the populaiton with clones of the prototype
                if (pop[i] != null) pop[i].Dispose();
                pop[i] = prototype.Clone();

                //randomizes the population
                pop[i].Randomize(rng);

                //adds a couple of mutaitons for greater divercity
                pop[i].Mutate(rng, rate);
                pop[i].Mutate(rng, rate);
            }           
        }

        public void Evolve()
        {
            ////stores the caninidate genitor for the next generation
            //genitor.Overwrite(pop[0]);
            //double f1 = fit(genitor);

            //stores the canidate genitor for the next generation
            T genitor = topspec;
            double f1 = topfit;

            for (int i = 0; i < pop.Length; i++)
            {
                //checks the fitness of each member
                double f2 = fit(pop[i]);
                if (f1 > f2 || f2.IsNaN()) continue;

                //updates the canidate if a more fit genome is found
                genitor = pop[i];
                f1 = f2;
            }

            //sets population-0 to be the genitor
            pop[0].Overwrite(genitor);
            genitor = pop[0];

            for (int i = 1; i < pop.Length; i++)
            {
                //copies the genitor over the rest of the populaton and mutates them
                pop[i].Overwrite(genitor);
                pop[i].Mutate(rng, rate);
            }

            ////Sanity check (the fitness should always increase!)
            //if (topfit > f1 || f1.IsNaN()) throw new Exception
            //("Excpeted fitness greator than " + topfit + " but was " + f1);

            gencount++;
            
            //topspec = genitor;
            //SetTopSpec(genitor);
            topspec.Overwrite(genitor);

            //topfit = f1;
            topfit = fit(topspec);
            Console.Write("\n" + topfit + " : " + f1);
        }

        public void GetTopSpec(T container)
        {
            //copies the top species genome into the container
            if (topspec == null) return;
            container.Overwrite(topspec);
        }

        //private void SetTopSpec(T genome)
        //{
        //    if (topspec == null) topspec = genome.SpawnRandom(rng);
        //    topspec.Overwrite(genome);
        //}


    }
}

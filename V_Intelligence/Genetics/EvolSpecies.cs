using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI.Genetics
{
    public class EvolSpecies<T> where T : Genetic<T>
    {
        //Determins how many creatures die in each generation (0.25)
        public const double DeathRate = 0.5; 


        //uses a random number generator to propell evolution
        private VRandom rng;

        //uses a fitness funciton to define the goal of evolution
        private Fitness<T> fitf;

        //keeps an array of individuals as the populaiton
        private Organism<T>[] pop;

        //keeps all the species in a list
        private List<Species<T>> species;

        //keeps track of the organisms that have died
        private Stack<Organism<T>> deadpool;

        //keeps track of new organims that have been born
        private Stack<Organism<T>> babies;

        //stores the index of the best preforming individual
        private int champ;

        //stores the current generation number
        private int generation;

        //the rate of mutation used in breading
        private double rate;

        //determins if crossover is allowed
        private bool crossover = false;

        //threshold for inclusion in a species (6.0) (6.86)
        private double compat_tresh = 6.0;
        private double compat_mod = 0.3;

        private int species_target = 10;

        public EvolSpecies(int popsize, double rate, Fitness<T> fitf)
        {        
            this.rng = new RandMT();

            this.fitf = fitf;
            this.rate = rate;

            this.pop = new Organism<T>[popsize];
            this.species = new List<Species<T>>(popsize / 2);

            int death = (int)(popsize * DeathRate) + 1;

            this.deadpool = new Stack<Organism<T>>(death);
            this.babies = new Stack<Organism<T>>(death);

            generation = 0;
            champ = 0;
        }

        #region Evolution Implementation...

        public int Generaton
        {
            get { return generation; }
        }

        public double TopFitness
        {
            get 
            {
                Organism<T> best = pop[champ];
                return best.TrueFit; 
            }
        }

        public int NumSpecies
        {
            get { return species.Count; }
        }


        public void Initialise(T prototype)
        {
            //throw new NotImplementedException();

            for (int i = 0; i < pop.Length; i++)
            {
                //disposes of the old population
                if (pop[i] != null) pop[i].Dispose();

                //fills the population by mutating the prototpye
                T genome = prototype.Clone();
                genome.Mutate(rng, rate);
                genome.Mutate(rng, rate);

                //creates a new organism with the measured fitness
                double fitness = fitf(genome);
                pop[i] = new Organism<T>(genome, i, fitness);
                babies.Push(pop[i]);

                //updates the champ if this new creature is better
                if (fitness > pop[champ].TrueFit) champ = i;
            }

            //sorts the initial populaiton into species
            Speciate();
        }

        public void GetTopSpec(T container)
        {
            if (pop[champ] == null) return;

            T genome = pop[champ].Genome;
            container.Overwrite(genome);
        }

        public void Evolve()
        {
            //throw new NotImplementedException();

            //FIRST: should determin if any species should go extenct
            //and add their members to the deadpool

            //QUESTION: how should we handel species going exteict?
            //how can we award their reproduction slots to other species?


            UpdateFitness();
            EleminateSpecies();
            CalculateChildren();
            CreateOffspring();
            Speciate();
            AdjustThreshold();

            generation++;



        }

        #endregion /////////////////////////////////////////////////////////////////////////

        #region Evolution Stages...

        private void UpdateFitness()
        {
            //calculates the fitness values for each species
            foreach (var spec in species)
                spec.UpdateFitness();

            //sorts the species by their avgrage fitness
            species.Sort();

            double rem = 0.0;

            //update the fitness of each creature based on its species
            foreach (var spec in species)
            {
                spec.ShareFitness();
                rem = spec.Decay(DeathRate, rem);
            }
        }

        private void EleminateSpecies()
        {
            var trash = new Stack<Species<T>>();

            foreach (var spec in species)
            {
                //determins if the species should go extinct
                bool ext = spec.Extention();
                if (!ext) continue;

                ////QUESTION: How do I remove the species???
                //species.Remove(spec);

                trash.Push(spec);

                Console.WriteLine("Species Extention!");
            }

            while (trash.Count > 0)
            {
                //empties the trash!
                var spec = trash.Pop();
                species.Remove(spec);
            }
        }

        private void CalculateChildren()
        {
            double total = 0.0;
            //double fitavg = 0.0;
            //double navg = 0.0;
            double skim = 0.0;

            double aug = 0.0;

            int nalive = 0;
            int expected = 0;

            //calculate the average fitness of the (living) population
            for (int i = 0; i < pop.Length; i++)
            {
                if (pop[i].Marked) continue;
                total += pop[i].Fitness;
                nalive++;
            }

            //fitavg = total / nalive;
            //navg = (pop.Length - nalive) / nalive; 

            aug = (double)(pop.Length - nalive) / total;

            //determin how many children each creature should get
            for (int i = 0; i < pop.Length; i++)
            {
                double nc = pop[i].Fitness;
                //nc = (nc / fitavg) * navg;
                //pop[i].NumChildren = nc;

                pop[i].NumChildren = nc * aug;
            }          

            //now determin how many children each species should get
            foreach (var spec in species)
            {
                skim = spec.CountOffspring(skim);
                expected += spec.NumChildren;
            }

            int actual = pop.Length - nalive;
            Console.WriteLine("Expected: {0}, Actual: {1}", expected, actual);
            Console.WriteLine("Offest: " + (expected -  actual));
        }

        private void CreateOffspring()
        {
            //obtains a working genome to be used as scratch
            T best = pop[champ].Genome;
            T scratch = best.Clone();

            //now we can start creating offspring
            foreach (var spec in species)
            {
                //there are no more dead to be reborn
                if (deadpool.Count == 0) break;
                int nchild = spec.NumChildren;

                for (int i = 0; i < nchild; i++)
                {
                    //obtains a dead creature to be reborn
                    if (deadpool.Count == 0) break;
                    Organism<T> rep = deadpool.Pop();

                    //obtains a pair of random genomes for mating
                    T mom = spec.GetRandMember(rng).Genome;
                    T dad = spec.GetRandMember(rng).Genome;

                    //NOTE: I feel like this could be better

                    //creates a new offspring in scratch
                    scratch.Overwrite(mom);
                    if (crossover) scratch.Crossover(rng, dad);
                    scratch.Mutate(rng, rate);

                    //calculates fitness and updates the organism
                    double fitness = fitf(scratch);
                    rep.Update(scratch, fitness);

                    //updates the champ if this new creature is better
                    if (fitness > pop[champ].TrueFit)
                        champ = rep.Index;

                    //the organisim is born again
                    babies.Push(rep);
                }
            }

            //we need to take care of the rest of the dead
            while (deadpool.Count > 0)
            {
                //obtains a dead creature to be reborn
                Organism<T> rep = deadpool.Pop();

                //creates a new random creature
                scratch.Overwrite(best);
                scratch.Randomize(rng);

                //calculates fitness and updates the organism
                double fitness = fitf(scratch);
                rep.Update(scratch, fitness);

                //the organisim is born again
                babies.Push(rep);
            }
        }

        private void Speciate()
        {
            while (babies.Count > 0)
            {
                Organism<T> baby = babies.Pop();
                bool found = false;

                foreach (var spec in species)
                {
                    //obtains the genome for baby and the prototype
                    T bg = baby.Genome;
                    T pt = spec.Prototype.Genome;

                    //determins if this baby belongs to this species
                    double dist = bg.Compare(pt);
                    if (dist > compat_tresh) continue;

                    //adds the baby to the species and quits searching
                    spec.AddGenome(baby.Index, baby.TrueFit);
                    found = true;
                    break;
                }

                if (!found)
                {
                    //creates a brand new species for the baby
                    var myspec = new Species<T>(this, baby);
                    species.Add(myspec);

                    Console.WriteLine("New Species Created!");
                }
            }
        }

        private void AdjustThreshold()
        {
            //keep the threshhold stable for first few itterations
            if (generation < 2) return;

            //adjust threshold as needed to reach target
            if (species.Count < species_target) 
                compat_tresh -= compat_mod;
            else if (species.Count > species_target) 
                compat_tresh += compat_mod;
        }

        #endregion /////////////////////////////////////////////////////////////////////////

        internal Organism<T> GetMember(int index)
        {
            return pop[index];
        }

        internal void MarkForDeath(int index)
        {
            pop[index].Marked = true;
            deadpool.Push(pop[index]);
        }
    }
}

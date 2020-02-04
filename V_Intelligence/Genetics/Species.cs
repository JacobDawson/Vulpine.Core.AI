using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI.Genetics
{
    public class Species<T> : IComparable<Species<T>> where T : Genetic<T>
    {
        //referes back to the species parent population
        private EvolSpecies<T> pop;

        //stores a reference to the prototype individual
        private int proto;

        //stores references to the members of the species
        private List<Int32> members;

        //stores the maximum and average fitness of its members
        private double max_fit;
        private double avg_fit;

        //stores the age and the last point of improovment
        private int age;
        private int invo;

        //stores the number of children its allowed to procude
        private int nchild;

        //stores the cut-off ages for young and old species
        public const int YOUNG = 10;
        public const int OLD = 10;

        //NOTE: The fitness sharing code assumes that fitness is always positive. 


        internal Species(EvolSpecies<T> pop, Organism<T> proto)
        {
            this.proto = proto.Index;
            this.pop = pop;

            members = new List<Int32>();
            members.Add(proto.Index);

            max_fit = proto.TrueFit;
            avg_fit = max_fit;

            age = 0;
            invo = 0;
            nchild = 0;
        }

        public int CompareTo(Species<T> other)
        {
            //sorts by fitness in decending order
            return -avg_fit.CompareTo(other.avg_fit);
        }


        public Organism<T> Prototype
        {
            get { return pop.GetMember(proto); }
        }

        public double MaxFitness
        {
            get { return max_fit; }
        }

        public double AvgFitness
        {
            get { return avg_fit; }
        }

        public int Age
        {
            get { return age; }
        }

        public int LastImp
        {
            get { return invo; }
        }

        public int NumChildren
        {
            get { return nchild; }
        }



        internal void UpdateFitness()
        {
            double max = 0.0;
            double total = 0.0;

            //calculates the max and total fitness in one pass
            foreach (int index in members)
            {
                Organism<T> member = pop.GetMember(index);
                double fit = member.Fitness;

                if (fit > max) max = fit;
                total += fit;
            }

            //updates the average and max fitness
            avg_fit = total / members.Count;
            max_fit = max;

            //increments the age
            age = age + 1;
        }

        internal void ShareFitness()
        {
            double count = (double)members.Count;
            double bonus = (age < YOUNG) ? 1.3 : 1.0;
            double penilty = ((age - invo) > OLD) ? 0.7 : 1.0; 

            foreach (int index in members)
            {
                Organism<T> member = pop.GetMember(index);
                double fit = member.Fitness;

                fit = (fit / count) * bonus * penilty;
                member.Fitness = fit;
            }
        }

        internal void AddGenome(int index, double fit)
        {
            //updates the prototype if the new creature is
            //better than any other member in the species
            if (fit > max_fit)
            {          
                proto = index;
                max_fit = fit;
            }

            //adds the member to the species
            members.Add(index);
        }

        internal double CountOffspring(double skim)
        {
            double x1 = 0.0;
            double x2 = skim;

            int n1 = 0;
            int n2 = 0;

            foreach (int index in members)
            {
                Organism<T> member = pop.GetMember(index);
                x1 = member.NumChildren;

                //updates the interger and fractional running counts
                n1 = (int)x1.Floor();
                x1 = x1.Frac();
                n2 = n2 + n1;
                x2 = x2 + x1;

                //if the fractional part exceeds one, we rebalance
                if (x2 >= 1.0)
                {
                    n2 = n2 + 1;
                    x2 = x2 - 1.0;
                }
            }

            //sets the number of children and returns the remainder
            nchild = n2;
            return x2;
        }

        internal double Decay(double drate, double rem)
        {
            //defines the sorting order for the species members
            Comparison<Int32> comp = delegate(int x, int y)
            {
                Organism<T> m1 = pop.GetMember(x);
                Organism<T> m2 = pop.GetMember(y);
                return m1.CompareTo(m2);
            };

            //sorts the members by fitness
            members.Sort(comp);

            ////determins the number of creatures left alive
            //int nalive = (int)Math.Floor(drate * members.Count);
            //nalive = members.Count - nalive;

            //determins the number of creatures that should die
            double dead = drate * members.Count;
            int ndead = (int)dead.Floor();
            rem = rem + dead.Frac();

            //updates the count if the remainder is more than one
            if (rem >= 1.0)
            {
                ndead = ndead + 1;
                rem = rem - 1.0;
            }

            //uses the dead count to determin how many are left alive
            int nalive = members.Count - ndead;
            
            //removes the members whose fitness is too low
            for (int i = members.Count - 1; i >= nalive; i--)
            {
                int index = members[i];
                pop.MarkForDeath(index);
                members.RemoveAt(i);
            }

            return rem;
        }

        internal bool Extention()
        {
            //if the species is still young or inovating (keep alive)
            if (age < YOUNG) return false;
            if ((age - invo) < OLD) return false;

            //if the species has assigned children (keep alive)
            if (nchild > 0) return false;

            //marks all remaining members for death
            foreach (int index in members)
                pop.MarkForDeath(index);

            //cleaers the member list and the pop refference
            members.Clear();
            pop = null;

            return true;
        }



        //internal Organism<T> GetMember(int index)
        //{
        //    //retreives the organism from the greater populaiton
        //    return pop.GetMember(index);
        //}

        internal Organism<T> GetRandMember(VRandom rng)
        {
            int index = rng.RandInt(members.Count);
            int id = members[index];
            return pop.GetMember(id);

            //IDEA: weight the members by their respective fitness
            //so that more fit members are selected more often
        }





    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Genetics
{
    public class Organism<T> : IDisposable, IComparable<Organism<T>> where T : Genetic<T>
    {
        //stores the genome for the organism
        private T genome;

        //stores the index of the organism
        private int index;  

        //refrences the organisim's fitness value
        private double fit;

        //the true fitness value that dose not change
        private double truefit;

        //number of offspring allocated to this individual
        private double nchild;

        //determins the age of the organism
        private int age;

        //indicates if this organim has been marked for death
        private bool mark;


        internal Organism(T genome, int index, double fit)
        {
            //clones the initial genome
            this.genome = genome.Clone();

            //sets the index
            this.index = index;
            
            //sets the internal data
            this.fit = fit;
            this.truefit = fit;
            this.nchild = 0.0;
            this.mark = false;
            this.age = 0;
        }

        public int CompareTo(Organism<T> other)
        {
            //sorts by fitness in decending order
            return -fit.CompareTo(other.fit);
        }

        public T Genome
        {
            get { return genome; }
        }

        public int Index
        {
            get { return index; }
        }

        public double Fitness
        {
            get { return fit; }
            set { fit = value; }
        }

        public double TrueFit
        {
            get { return truefit; }
        }

        public double NumChildren
        {
            get { return nchild; }
            set { nchild = value; }
        }

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        public bool Marked
        {
            get { return mark; }
            set { mark = value; }
        }



        internal void Update(T genome, double fit)
        {
            //overwrites the genome with the new genome
            this.genome.Overwrite(genome);

            //resets the internal fitness values
            this.fit = fit;
            this.truefit = fit;

            //is no longer marked for death
            this.mark = false;
            this.age = 0;
        }

        public void Dispose()
        {
            genome.Dispose();
        }
    }
}

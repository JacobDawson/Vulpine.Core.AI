using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Genetics
{
    public class EvolEventArgs : EventArgs
    {
        //references the current generation
        private int gnumber;

        //stores the curent percent completed
        private double percent;

        //stores the last fitness calculated
        private double fitness;

        //NOTE: Consider adding a halting mechanism, taken from StepEventArgs
        //in Vulpine.Core.Calc.Algorythims

        internal EvolEventArgs(int gnum, double per, double fitness)
        {
            this.gnumber = gnum;
            this.percent = per;
            this.fitness = fitness;
        }

        public int Generation
        {
            get { return gnumber; }
        }

        public double Percent
        {
            get { return percent; }
        }

        public double Fitness
        {
            get { return fitness; }
        }



    }
}

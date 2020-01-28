using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Nural
{
    internal struct AxonComp
    {
        private int in_node;
        private int out_node;
        private double weight;
        private bool enabled;

        internal AxonComp(int in_node, int out_node, double weight)
        {
            this.in_node = in_node;
            this.out_node = out_node;
            this.weight = weight;
            this.enabled = true;
        }

        public int Input
        {
            get { return in_node; }
            set { in_node = value; }
        }

        public int Output
        {
            get { return out_node; }
            set { out_node = value; }
        }

        public double Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Determins if two axons share similar topology. That is, if the axons have
        /// the same inputs and outputs. It dose not consider the weight of the axons.
        /// </summary>
        /// <param name="other">An axon to compare</param>
        /// <returns>True if the axons share the same topology</returns>
        public bool IsSimilar(AxonComp other)
        {
            if (this.Input != other.Input) return false;
            if (this.Output != other.Output) return false;

            return true;
        }
    }
}

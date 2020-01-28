using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Nural
{
    public struct NuronComp
    {
        //used in propergating the network
        private double value;

        //determins the neurons activation function
        private ActFunc func;

        internal NuronComp(ActFunc func)
        {
            this.func = func;
            this.value = 0.0;
        }

        public ActFunc Funciton
        {
            get { return func; }
            set { func = value; }
        }

        public Double Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public void Reset()
        {
            //sets the internaly stored value to zero
            this.value = 0.0;
        }
    }
}

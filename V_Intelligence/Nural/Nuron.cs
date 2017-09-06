/**
 *  This file is an integral part of the Vulpine Core Library: 
 *  Copyright (c) 2016-2017 Benjamin Jacob Dawson. 
 *
 *      http://www.jakesden.com/corelibrary.html
 *
 *  This file is licensed under the Apache License, Version 2.0 (the "License"); 
 *  you may not use this file except in compliance with the License. You may 
 *  obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.    
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Data;
using Vulpine.Core.Data.Lists;

namespace Vulpine.Core.AI.Nural
{
    public sealed class Nuron : IComparable<Nuron>
    {
        //stores the ID number for the nuron
        private int node;

        //used in propergating the network
        private double value;

        //determins the neurons activation function
        private ActFunc func;

        //keeps a list of conecting axonx
        private VList<Int32> inputs;

        //referes to the parent network
        private NetworkCPP network;


        internal Nuron(NetworkCPP network, ActFunc func, int node)
        {
            this.network = network;
            this.func = func;
            this.node = node;

            inputs = new VListArray<Int32>();
            value = 0.0;
        }


        /// <summary>
        /// Creates a copy of a given nuron for a given network. All of the
        /// refrences to axons in the old network, now point to axons in
        /// the new network. All other values remain the same. 
        /// </summary>
        /// <param name="network">The network containing this nuron</param>
        /// <param name="other">Another neuron to copy its values</param>
        internal Nuron(NetworkCPP network, Nuron other)
        {
            //sets the network refrence to the new network
            this.network = network;

            //copies the other vlaues
            func = other.func;
            node = other.node;
            value = other.value;

            //makes a deep copy of the list of inputs
            inputs = new VListArray<Int32>(other.inputs);
        }

        public int CompareTo(Nuron other)
        {
            //compares the indices of the nodes
            return node.CompareTo(other.node);
        }



        public int ID
        {
            get { return node; }
        }

        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public ActFunc Func
        {
            get { return func; }
            set { func = value; }
        }



        /// <summary>
        /// Lists all the axons that input their value into the given nuron.
        /// It atomaticly skips over any brocken connections, should they
        /// exist, although disabled axons are left intact.
        /// </summary>
        /// <returns>An interation of all input axons</returns>
        public IEnumerable<Axon> ListAxons()
        {
            foreach (int index in inputs)
            {
                Axon ax = network.GetAxonByID(index);
                if (ax == null) continue;

                yield return ax;
            }
        }




        internal IEnumerable<Int32> ListInputs()
        {
            //loops over the list of edge numbers
            return inputs.AsEnumerable();
        }

        internal void AddInput(Axon input)
        {
            //adds the inovation number to our list
            inputs.Add(input.InvNo);
        }


        internal Axon GetAxon(Nuron other)
        {
            if (other.node > this.node)
            {
                //makes shure the nodes are in the right order
                return other.GetAxon(this);
            }

            foreach (int invno in inputs)
            {

                Axon ax = network.GetAxonByID(invno);
                if (ax != null && ax.Input == other.ID) return ax;
            }

            return null;

        }
    }
}

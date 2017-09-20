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
    public sealed class Nuron : IComparable<Nuron>, Node<Nuron>
    {
        //stores the index of the nuron
        private int index;

        //indicates the level containing this node
        private int level;

        //used in propergating the network
        private double value;

        //determins the neurons activation function
        private ActFunc func;

        //keeps a list of conecting axonx
        private VList<Axon> inputs;

        //referes to the parent network
        private NetworkCPP network;


        internal Nuron(NetworkCPP network, ActFunc func, int level, int index)
        {
            this.network = network;
            this.func = func;
            this.level = level;
            this.index = index;

            inputs = new VListArray<Axon>();
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
            level = other.level;
            value = other.value;

            //makes a deep copy of the list of inputs
            inputs = new VListArray<Axon>(other.inputs);
        }

        public int CompareTo(Nuron other)
        {
            //compares the levels of the nodes
            return level.CompareTo(other.level);        
        }



        public int Level
        {
            get { return level; }
        }

        public int Index
        {
            get { return index; }
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
            //checkes that we have not been disposed
            if (network == null) yield break;

            //simply itterates over the list
            foreach (Axon ax in inputs) yield return ax;
        }

        public IEnumerable<Nuron> ListInputs()
        {
            //checkes that we have not been disposed
            if (network == null) yield break;

            foreach (Axon ax in inputs)
            {
                //returns the nurons that have matching axons
                Nuron temp = network.GetNuronByID(ax.Source);
                if (temp != null) yield return temp;            
            }
        }


        //internal IEnumerable<Int32> ListInputs()
        //{
        //    //loops over the list of edge numbers
        //    return inputs.AsEnumerable();
        //}



        ////THIS METHOD IS CRITICAL !!!
        //internal void AddInput(Axon input)
        //{
        //    //adds the inovation number to our list
        //    inputs.Add(input.Index);
        //}




        /// <summary>
        /// Obtains the axon that connects to this nuron from another given
        /// nuron. Note that the order of the nurons is important. It returns
        /// null in no sutch connection exists.
        /// </summary>
        /// <param name="other">Another nuron</param>
        /// <returns>The axon conecting the nurons</returns>
        public Axon GetAxon(Nuron other)
        {
            //if (other.level > this.level)
            //{
            //    //makes shure the nodes are in the right order
            //    return other.GetAxon(this);
            //}

            //foreach (int invno in inputs)
            //{
            //    Axon ax = network.GetAxonByID(invno);
            //    if (ax == null) continue;

            //    //determins if we find a match
            //    if (ax.Source == other.Index) return ax;
            //}

            ////we faild to find the axon
            //return null;

            //checkes that we have not been disposed
            if (network == null) return null;

            foreach (Axon ax in inputs)
            {
                //checks each axon for a match
                if (ax.Source == other.Index) return ax;
            }

            //we faild to find the axon
            return null;
        }

        internal Axon AddAxon(Nuron input, double weight)
        {
            ////enshures the nurons belong to the same network
            //if (network != target.network)
            //throw new InvalidOperationException();

            //int index = -1; // network.RandIndex();
            //if (index < 0) return false;

            //Axon ax = new Axon(index, target.Index, weight);
            //inputs.Add(index);
            ////network.AddAxon(ax);

            //enshures the nurons belong to the same network
            if (network == null || network != input.network)
                throw new InvalidOperationException();

            int source = this.Index;
            int target = input.Index;

            //creates the axon and adds it to our list
            Axon ax = new Axon(0, source, target, weight);
            inputs.Add(ax);

            return ax;

        }

        internal void ClearData()
        {
            //desposes of the internal data sturctor
            if (inputs != null) inputs.Clear();

            //deletes the network to avoid cyclicl refrences 
            this.network = null;
            this.inputs = null;
        }


        Nuron Node<Nuron>.Data
        {
            get { return this; } //refrence to ourself
            set { throw new InvalidOperationException(); }
        }

        //simply calls the non generic method
        IEnumerable<Node<Nuron>> Node<Nuron>.ListChildren()
        { return ListInputs(); }

        //calling the public despose method dose nothing
        void IDisposable.Dispose() { }
    }
}

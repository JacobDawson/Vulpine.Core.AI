/**
 *  This file is an integral part of the Vulpine Core Library
 *  Copyright (c) 2016-2018 Benjamin Jacob Dawson
 *
 *      http://www.jakesden.com/corelibrary.html
 *
 *  The Vulpine Core Library is free software; you can redistribute it 
 *  and/or modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  The Vulpine Core Library is distributed in the hope that it will 
 *  be useful, but WITHOUT ANY WARRANTY; without even the implied 
 *  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *  See the GNU Lesser General Public License for more details.
 *
 *      https://www.gnu.org/licenses/lgpl-2.1.html
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Data;
using Vulpine.Core.Data.Lists;

namespace Vulpine.Core.AI.Nural.Old
{
    public sealed class NuronOld :  Node<NuronOld>, IComparable<NuronOld>
    {
        //NOTE: Write a ToString method once we have determined what
        //types of activation functions should be included.

        //NOTE: I still need to decide weather I should use diffrent Nurons
        //for recurent and feed-forward networks, respectivly.

        #region Class Definitons...

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

        /// <summary>
        /// Creates a new nuron on a given network, with a given activation 
        /// function and level depth. An index is required to be able to 
        /// refrence the nuron from outside the network. 
        /// </summary>
        /// <param name="network">Network on wich to create the nuron</param>
        /// <param name="func">Activation funciton of the nuron</param>
        /// <param name="level">Level of the nuron</param>
        /// <param name="index">Index of the nuron</param>
        internal NuronOld(NetworkCPP network, ActFunc func, int level, int index)
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
        internal NuronOld(NetworkCPP network, NuronOld other)
        {
            //sets the network refrence to the new network
            this.network = network;

            //copies the other vlaues
            func = other.func;
            level = other.level;
            value = other.value;

            inputs = new VListArray<Axon>(other.inputs.Count);

            //makes a deep copy of the list of inputs
            foreach (Axon ax in other.inputs)
            {
                Axon clone = new Axon(ax);
                this.inputs.Add(clone);
            }
        }

        /// <summary>
        /// Determins if this nuron is equal to another nuron. Two nurons
        /// are the same if they exist on the same network and share the
        /// same index. 
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if the objects are identical</returns>
        public override bool Equals(object obj)
        {
            //makes sure the object is another nuron
            var other = obj as NuronOld;
            if (other == null) return false;

            //compares both the network and the index
            if (network != other.network) return false;
            if (index != other.index) return false;

            return true;
        }

        /// <summary>
        /// Generates a sudo-unique hashcode that can be used to identify
        /// the current nurron. This is just the nuron's index.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            //just use our index as our hash
            return index;
        }

        /// <summary>
        /// Compares one nuron to another, sorting the nurons first by
        /// index and then by level. It returns a negative number if the 
        /// current nuron comes first, a positive number if it comes second, 
        /// and zero if the nurons are equal
        /// </summary>
        /// <param name="other">Nuron to compare</param>
        /// <returns>The resluts of comparison</returns>
        public int CompareTo(NuronOld other)
        {
            //sorts the nurons first by assending level
            int test = level.CompareTo(other.level);
            if (test != 0) return test;

            //then compares the nurons by their index
            return index.CompareTo(other.index);
        }

        #endregion ////////////////////////////////////////////////////////////////////

        #region Class Properties...

        /// <summary>
        /// The index of the curent nuron, used for looking up individual
        /// nurons inisde a network.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Indecates the level on which this nuron resides. Forward axons
        /// always connect lower levels to higher levels.
        /// </summary>
        public int Level
        {
            get { return level; }
        }

        /// <summary>
        /// The current value of the nurron
        /// </summary>
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }

        /// <summary>
        /// The activation function of this nuron. It is applied to the
        /// nuron's output before determining its value.
        /// </summary>
        public ActFunc Func
        {
            get { return func; }
            set { func = value; }
        }

        /// <summary>
        /// Returns a refrence to the current nurron, nessary for
        /// implementing the Node interface.
        /// </summary>
        public NuronOld Data
        {
            get { return this; }
            set { throw new InvalidOperationException(); }
        }

        #endregion ////////////////////////////////////////////////////////////////////

        #region Axon Operations...

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

        /// <summary>
        /// Lists all the nurons that connect into this nuron. It atomaticly 
        /// skips over any brocken connections, should they exist, although 
        /// nurons connedted by disabled axons are included.
        /// </summary>
        /// <returns>An interation of all input nurons</returns>
        public IEnumerable<NuronOld> ListInputs()
        {
            //checkes that we have not been disposed
            if (network == null) yield break;

            foreach (Axon ax in inputs)
            {
                //returns the nurons that have matching axons
                NuronOld temp = network.GetNuronByID(ax.Source);
                if (temp != null) yield return temp;            
            }
        }

        /// <summary>
        /// Obtains the axon that connects to this nuron from another given
        /// nuron. Note that the order of the nurons is important. It returns
        /// null if no sutch connection exists.
        /// </summary>
        /// <param name="index">Index of another nuron</param>
        /// <returns>The axon conecting the nurons</returns>
        public Axon GetAxon(int index)
        {
            //checkes that we have not been disposed
            if (network == null) return null;

            foreach (Axon ax in inputs)
            {
                //checks each axon for a match
                if (ax.Source == index) return ax;
            }

            //we faild to find the axon
            return null;
        }

        /// <summary>
        /// Obtains the axon that connects to this nuron from another given
        /// nuron. Note that the order of the nurons is important. It returns
        /// null if no sutch connection exists.
        /// </summary>
        /// <param name="other">Another nuron</param>
        /// <returns>The axon conecting the nurons</returns>
        public Axon GetAxon(NuronOld other)
        {
            //checks that both nurons belong to the same network
            if (network != other.network) return null;

            //obtains the axon by the other's index
            return GetAxon(other.Index);
        }

        //public Axon AddAxon(Nuron input, double weight)
        //{
        //    if (input.Level > this.Level)
        //    {
        //        //swaps the nurons if the level is swaped
        //        return input.AddAxon(this, weight);
        //    }

        //    //enshures that the nurons are on diffrent levels
        //    if (input.Level == this.Level) throw new
        //    InvalidOperationException();

        //    //enshures that the nurons belong to the same network
        //    if (input.network != this.network) throw new
        //    InvalidOperationException();

        //    //calls upon the internal method
        //    return AddAxonInit(input, weight);
        //}

        #endregion ////////////////////////////////////////////////////////////////////

        #region Internal Methods...

        /// <summary>
        /// Connects the given input nuron to the current nuron with the
        /// given weight. Note that it dose not check the topology of the
        /// network before adding the axon, so care must be taken to avoid
        /// redundent connections. If a connection already exists between
        /// the two nurons, it returns that axon instead.
        /// </summary>
        /// <param name="input">Nuron to conect</param>
        /// <param name="weight">Weight of the connection</param>
        /// <returns>The axon between the nurons</returns>
        internal Axon AddAxonInit(NuronOld input, double weight)
        {
            ////enshures the nurons belong to the same network
            //if (network == null || network != input.network)
            //    throw new InvalidOperationException();

            //makes shure our parent network hasn't been disposed
            if (network == null) throw new InvalidOperationException();

            //sees if we alreay contain a link to the input
            Axon ax = GetAxon(input.Index);

            if (ax == null)
            {
                //obtains an index to the sorce and target
                int source = this.Index;
                int target = input.Index;

                //creates the axon and adds it to our list
                ax = new Axon(source, target, weight);
                inputs.Add(ax);
            }
            else
            {
                //sets the axons weight to our new weight
                ax.Weight = weight;
            }

            return ax;

        }

        /// <summary>
        /// Clears all the data contained in this node. It is nessary to
        /// call this when disposing of the parent network in order to
        /// avoid cyclical refrences and potential memory leaks.
        /// </summary>
        internal void ClearData()
        {
            //desposes of the internal data sturctor
            if (inputs != null) inputs.Clear();

            //deletes the network to avoid cyclic refrences 
            this.network = null;
            this.inputs = null;
        }

        #endregion ////////////////////////////////////////////////////////////////////

        //simply calls the non-generic method
        IEnumerable<Node<NuronOld>> Node<NuronOld>.ListChildren()
        { return ListInputs(); }

        //calling the public despose method dose nothing
        void IDisposable.Dispose() { }
    }
}

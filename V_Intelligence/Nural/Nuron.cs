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

using Vulpine.Core.Calc;

namespace Vulpine.Core.AI.Nural
{
    public sealed class Nuron :  Node<Nuron>, IComparable<Nuron>
    {
        //NOTE: Write a ToString method once we have determined what
        //types of activation functions should be included.

        #region Class Definitons...

        //stores the index of the nuron
        private int index;

        //used in propergating the network
        private double value;
        private double vprev;

        //determins the neurons activation function
        private ActFunc func;

        //referes to the parent network
        private NetworkAuto network;

        /// <summary>
        /// Creates a new nuron on a given network, with a given activation 
        /// function and level depth. An index is required to be able to 
        /// refrence the nuron from outside the network. 
        /// </summary>
        /// <param name="network">Network on wich to create the nuron</param>
        /// <param name="func">Activation funciton of the nuron</param>
        /// <param name="level">Level of the nuron</param>
        /// <param name="index">Index of the nuron</param>
        internal Nuron(NetworkAuto network, ActFunc func, int index)
        {
            this.network = network;
            this.func = func;
            this.index = index;

            this.value = 0.0;
            this.vprev = 0.0;
        }

        internal Nuron(NetworkAuto network, Nuron other)
        {
            this.network = network;
            this.func = other.func;
            this.index = other.index;

            this.value = 0.0;
            this.vprev = 0.0;
        }

        /// <summary>
        /// Determins if this nuron is equal to another nuron. Two nurons
        /// are the same if they share the same index, even if they belong
        /// to diffrent networks.
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if the objects are identical</returns>
        public override bool Equals(object obj)
        {
            //makes sure the object is another nuron
            var other = obj as Nuron;
            if (other == null) return false;

            //compares the index
            return (index != other.index);
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
        /// level and then by index. It returns a negative number if the 
        /// current nuron comes first, a positive number if it comes second, 
        /// and zero if the nurons are equal
        /// </summary>
        /// <param name="other">Nuron to compare</param>
        /// <returns>The resluts of comparison</returns>
        public int CompareTo(Nuron other)
        {
            ////sorts the nurons first by assending level
            //int test = level.CompareTo(other.level);
            //if (test != 0) return test;

            //compares the nurons by their index
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

        ///// <summary>
        ///// Indecates the level on which this nuron resides. Forward axons
        ///// always connect lower levels to higher levels.
        ///// </summary>
        //public int Level
        //{
        //    get { return level; }
        //}

        /// <summary>
        /// Determins if the current nuron is an input node.
        /// </summary>
        public bool IsInput
        {
            get { return (func == ActFunc.Input); }
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
        /// The value of the nuron in the last itteration
        /// </summary>
        public double Prev
        {
            get { return vprev; }
            set { vprev = value; }
        }

        /// <summary>
        /// The activation function of this nuron. It is applied to the
        /// nuron's output before determining its value.
        /// </summary>
        public ActFunc Func
        {
            get { return func; }
            //set { func = value; }
        }

        /// <summary>
        /// Returns a refrence to the current nurron, nessary for
        /// implementing the Node interface.
        /// </summary>
        public Nuron Data
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

            var axons = network.ListAxons();

            foreach (Axon ax in axons)
            {
                if (ax.Target != index) continue;
                yield return ax;
            }
        }

        /// <summary>
        /// Lists all the nurons that connect into this nuron. It atomaticly 
        /// skips over any brocken connections, should they exist, although 
        /// nurons connedted by disabled axons are included.
        /// </summary>
        /// <returns>An interation of all input nurons</returns>
        public IEnumerable<Nuron> ListInputs()
        {
            //checkes that we have not been disposed
            if (network == null) yield break;

            var axons = network.ListAxons();

            foreach (Axon ax in axons)
            {
                if (ax.Target != index) continue;
                Nuron nuron = network.GetNuron(ax.Source);
                yield return nuron;
            }
        }

        ///// <summary>
        ///// Obtains the axon that connects to this nuron from another given
        ///// nuron. Note that the order of the nurons is important. It returns
        ///// null if no sutch connection exists.
        ///// </summary>
        ///// <param name="index">Index of another nuron</param>
        ///// <returns>The axon conecting the nurons</returns>
        //public Axon GetAxon(int index)
        //{
        //    //checkes that we have not been disposed
        //    if (network == null) return null;

        //    foreach (int aid in inputs)
        //    {
        //        //checks each axon for a match
        //        if (aid == index) return network.GetAxon(index);
        //    }

        //    //we faild to find the axon
        //    return null;
        //}

        ///// <summary>
        ///// Obtains the axon that connects to this nuron from another given
        ///// nuron. Note that the order of the nurons is important. It returns
        ///// null if no sutch connection exists.
        ///// </summary>
        ///// <param name="other">Another nuron</param>
        ///// <returns>The axon conecting the nurons</returns>
        //public Axon GetAxon(Nuron other)
        //{
        //    //checks that both nurons belong to the same network
        //    if (network != other.network) return null;

        //    //obtains the axon by the other's index
        //    return GetAxon(other.Index);
        //}

        ////public Axon AddAxon(Nuron input, double weight)
        ////{
        ////    if (input.Level > this.Level)
        ////    {
        ////        //swaps the nurons if the level is swaped
        ////        return input.AddAxon(this, weight);
        ////    }

        ////    //enshures that the nurons are on diffrent levels
        ////    if (input.Level == this.Level) throw new
        ////    InvalidOperationException();

        ////    //enshures that the nurons belong to the same network
        ////    if (input.network != this.network) throw new
        ////    InvalidOperationException();

        ////    //calls upon the internal method
        ////    return AddAxonInit(input, weight);
        ////}

        #endregion ////////////////////////////////////////////////////////////////////

        #region Internal Methods...

        ///// <summary>
        ///// Connects the given input nuron to the current nuron with the
        ///// given weight. Note that it dose not check the topology of the
        ///// network before adding the axon, so care must be taken to avoid
        ///// redundent connections. If a connection already exists between
        ///// the two nurons, it returns that axon instead.
        ///// </summary>
        ///// <param name="input">Nuron to conect</param>
        ///// <param name="weight">Weight of the connection</param>
        ///// <returns>The axon between the nurons</returns>
        //internal Axon AddAxonInit(Nuron input, double weight)
        //{
        //    //makes shure our parent network hasn't been disposed
        //    if (network == null) throw new InvalidOperationException();

        //    //sees if we alreay contain a link to the input
        //    Axon ax = GetAxon(input.Index);

        //    if (ax == null)
        //    {
        //        //obtains an index to the sorce and target
        //        int source = this.Index;
        //        int target = input.Index;

        //        //creates the axon and adds it to our list
        //        ax = new Axon(source, target, weight);
        //        inputs.Add(ax);
        //    }
        //    else
        //    {
        //        //sets the axons weight to our new weight
        //        ax.Weight = weight;
        //    }

        //    return ax;

        //}

        /// <summary>
        /// Clears all the data contained in this node. It is nessary to
        /// call this when disposing of the parent network in order to
        /// avoid cyclical refrences and potential memory leaks.
        /// </summary>
        internal void ClearData()
        {
            //deletes the network to avoid cyclic refrences 
            this.network = null;
        }

        /// <summary>
        /// Applies the activation funciton to the curent value.
        /// </summary>
        internal void Activate()
        {
            switch (func)
            {
                case ActFunc.Sine:
                    value = Math.Sin(value); break;
                case ActFunc.Cosine:
                    value = Math.Cos(value); break;
                case ActFunc.Gaussian:
                    value = VMath.Gauss(value); break;
                case ActFunc.Sigmoid:
                    value = Math.Tanh(value); break;
                case ActFunc.Sinc:
                    value = VMath.Sinc(value); break;
            }

            //prevents propagating NaNs
            if (value.IsNaN()) value = 0.0;
        }

        #endregion ////////////////////////////////////////////////////////////////////

        //simply calls the non-generic method
        IEnumerable<Node<Nuron>> Node<Nuron>.ListChildren()
        { return ListInputs(); }

        //calling the public despose method dose nothing
        void IDisposable.Dispose() { }
    }
}

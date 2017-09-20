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
using Vulpine.Core.Data.Tables;
using Vulpine.Core.Data.Lists;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI.Nural
{
    /// <summary>
    /// A Compositional Pattern Procduncing Network (CPPN) is a sepcial type of nural 
    /// network typicaly used to generate hyper-dimentional image data, although they 
    /// may also be used in-place of more traditional nural networks. Internaly, they
    /// consist of a directed acyclic graph (DAG) of nurons and axons. Each nuron can
    /// have its own activation function, which determins the types of patterns the 
    /// network can produce. All CPPNs start with miminal structor and use the Nural 
    /// Evolution of Augmenting Topoligies (NEAT) algorythim to evolve the structor
    /// nessary to produce the desired patterns.
    /// </summary>
    public sealed class NetworkCPP : Genetic<NetworkCPP>
    {
        
        //indicates the maximum number of levels
        private const int MAX_LV = 1024;

        //indicates the maximum number of tries for random probing
        private const int MAX_TRY = 64;

        //global paramaters that determin how networks evolve
        private const double C0 = 2.0;
        private const double C1 = 1.0;
        private const double SDN = 1.0;
        private const double SDS = 0.2;
        private const double P_Linear = 0.5;
        private const double P_Node = 0.1;

        //uses a random number genrator to evolve the network
        private VRandom rng;

        //stores a table of nurons by inovation number
        //private Table<Int32, Axon> axons;
        private Table<Int32, Nuron> nurons;


        private NetworkCPP(VRandom rng, NetworkCPP other)
        {
            //copies the RNG by refrence
            this.rng = rng;

            //int size_n = other.nurons.Buckets;
            //int size_a = other.nurons.Buckets;

            //nurons = new TableClosed<Int32, Nuron>(size_n);
            //axons = new TableClosed<Int32, Axon>(size_a);

            int size = other.nurons.Buckets;
            nurons = new TableOpen<Int32, Nuron>(size);

            foreach (var pair in other.nurons)
            {
                Nuron copy = new Nuron(this, pair.Item);
                nurons.Add(pair.Key, copy);
            }

            //foreach (var pair in other.axons)
            //{
            //    Axon copy = new Axon(pair.Item);
            //    axons.Add(pair.Key, copy);
            //}
        }

        /// <summary>
        /// The total number of axons in the network, including
        /// any disabled axons.
        /// </summary>
        public int Axons
        {
            get { return -1; } //NOTE: Add a count variable
        }

        /// <summary>
        /// The total number of nurons in the network, including
        /// any disconected nurons.
        /// </summary>
        public int Nurons
        {
            get { return nurons.Count; }
        }




        ///// <summary>
        ///// Adds a nuron to the network at the given level and with the given 
        ///// activation function. It returns null if it was unable to add the nuron.
        ///// </summary>
        ///// <param name="func">Activation function of the nuron</param>
        ///// <param name="level">Level of the nuron</param>
        ///// <returns>The inserted nuron</returns>
        //public Nuron AddNuron(ActFunc func, int level)
        //{
        //    //clamps the level to be within range
        //    if (level > MAX_LV) level = MAX_LV;
        //    if (level < 0) level = 0;

        //    //tries to generate a random index
        //    int index = RandIndex();
        //    if (index < 0) return null;

        //    //creates the node and adds it
        //    Nuron n = new Nuron(this, func, level, index);
        //    nurons.Add(index, n);

        //    return n;
        //}


        //public bool AddAxon(Nuron source, Nuron target, double weight)
        //{
        //    bool pass = true;

        //    //confirms that the source and target belong to this network
        //    pass &= nurons.HasKey(source.Index);
        //    pass &= nurons.HasKey(target.Index);
        //    if (!pass) throw new InvalidOperationException();

        //    //tries to generate a random index
        //    int index = RandIndex();
        //    if (index < 0) return false;

        //    //creates the axon, adding it to the target and network
        //    Axon ax = new Axon(index, source.Index, weight);
        //    target.AddInput(ax);
        //    axons.Add(index, ax);

        //    return true;
        //}




        //internal Axon GetAxonByID(int id)
        //{
        //    return axons.GetValue(id);
        //}

        internal Nuron GetNuronByID(int id)
        {
            return nurons.GetValue(id);
        }





        #region Genetic Implementaiton...


        /// <summary>
        /// Compares the current genotype to the genotype of a diffrent
        /// nural net. The result is a positive real value that indicates
        /// how siimilar the genotypes are. This is tipicaly used to
        /// seperate a population of individules into species.
        /// </summary>
        /// <param name="other">Nural net for comparison</param>
        /// <returns>Mesure of similarity</returns>
        public double Compare(NetworkCPP other)
        {
            //used in computing the distance
            int match = 0;
            int disjoint = 0;    
            double wbar = 0.0;

            foreach (Axon ax1 in this.ListAxons())
            {
                //tries to find the matching axon
                Axon ax2 = other.FindMatch(ax1);

                if (ax2 != null)
                {
                    //computes the distance between the weights
                    double w1 = ax1.Weight;
                    double w2 = ax2.Weight;

                    wbar += Math.Abs(w1 - w2);
                    match += 1;
                }
                else
                {
                    //counts the disjoint nodes
                    disjoint += 1;
                }
            }

            foreach (Axon ax1 in other.ListAxons())
            {               
                //only counts the missing disjoint edges
                Axon ax2 = other.FindMatch(ax1);
                if (ax2 == null) disjoint += 1;
            }

            //determins the size of the larger network
            int size = Math.Max(this.Axons, other.Axons);
            size = (size > 20) ? size - 20 : 1;

            //couputes the distance for specisation
            double dist = (C0 * disjoint) / (double)size;
            dist += C1 * (wbar / (double)match);

            return dist;
        }


        /// <summary>
        /// Combines the genes of the curent nural net with the genes of
        /// another network to create a brand new offspring. The idea is
        /// that the child network will possess trates from both its
        /// parents, similar to sexual reproduction.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="mate">Mate of the curent network</param>
        /// <returns>The child of both networks</returns>
        public NetworkCPP Combine(VRandom rng, NetworkCPP mate)
        {
            //makes a clone of the dominate parent
            NetworkCPP child = new NetworkCPP(rng, this);

            //determins weather or not to do liniar crossover
            bool liniar = rng.RandBool(P_Linear);
            double a = rng.NextDouble();

            foreach (Axon ax in mate.ListAxons())
            {
                //obtains the matching child axon
                Axon axc = child.FindMatch(ax);
                if (axc == null) continue;

                if (liniar)
                {
                    //chooses a value between the two weights
                    double weight = axc.Weight * (1.0 - a);
                    axc.Weight = weight + (ax.Weight * a);
                }
                else
                {
                    //determins the new weight based on crossover
                    bool cross = rng.RandBool();
                    if (cross) axc.Weight = ax.Weight;
                }

                //has a chance of enabling if either are disabled
                bool en = ax.Enabled && axc.Enabled;
                axc.Enabled = en || rng.RandBool(0.25);
            }

            return child;
        }


        /// <summary>
        /// Clones the current nural net with some random mutation of its
        /// genotpye. The rate of mutaiton determins how many of the network
        /// connections are preturbed. For exampe, a mutation rate of 0.5
        /// indicates that half the weights will be perturbed.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="rate">Rate of mutation</param>
        /// <returns>A mutated network</returns>
        public NetworkCPP Mutate(VRandom rng, double rate)
        {
            //clones a child and then mutates it
            var child = new NetworkCPP(rng, this);
            child.MutateSelf(rate);
            return child;
        }


        /// <summary>
        /// Preterbs the current neural net by some random amount without
        /// creating a clone. The rate of mutaiton determins how many of the 
        /// network connections are preturbed. For exampe, a mutation rate 
        /// of 0.5 indicates that half the weights will be perturbed.
        /// </summary>
        /// <param name="rate">Rate of mutation</param>
        public void MutateSelf(double rate)
        {
            //clamps the rate to be between zero and one
            rate = VMath.Clamp(rate);

            if (rng.RandBool(P_Node))
            {
                //changes the activation funciton of a single node
                Nuron node = RandNuron();
                node.Func = RandFunc();
                return;
            }

            foreach (Axon ax in ListAxons())
            {
                //mutates weights based on the rate of mutation
                if (rng.RandBool(1.0 - rate)) continue;

                if (ax.Enabled)
                {
                    //permutes the weight by a small amount
                    double delta = rng.RandGauss() * SDS;
                    ax.Weight = ax.Weight + delta;
                }
                else
                {
                    //resets the neuron to a small weight
                    ax.Weight = rng.RandGauss() * SDS;
                    ax.Enabled = true;
                }
            }
        }


        /// <summary>
        /// Creates a new nural net with more genes than its parent. This is 
        /// diffrent from regular mutaiton, as the genotype becomes bigger, 
        /// increasing the search space and opening new opertunites for 
        /// diversification and improvment.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <returns>An organism with an expanded genotype</returns>
        public NetworkCPP Expand(VRandom rng)
        {
            //clones a child and then expands it
            var child = new NetworkCPP(rng, this);
            child.ExpandSelf();
            return child;
        }


        /// <summary>
        /// Expands the curent nural net, in place, by either inserting a
        /// new node along a pre-existing edge or creating a new edge. The
        /// probibility of either event occoring is determined by the
        /// existing topology.
        /// </summary>
        public void ExpandSelf()
        {
            Nuron n1, n2;

            //generates a pair of random nurons
            bool test = RandPair(out n1, out n2);
            Axon target = n2.GetAxon(n1);

            if (test && target == null)
            {
                ////creates a new axon between the nurons
                //double weight = rng.RandGauss() * SDN;
                //AddAxonInit(n1, n2, weight);

                //creates a new axon between the nurons
                double weight = rng.RandGauss() * SDN;
                n2.AddAxon(n1, weight);

            }
            else if (test && !target.Enabled)
            {
                //reinitilises the axon with a new weight
                target.Weight = rng.RandGauss() * SDN;
                target.Enabled = true;
            }
            else if ((n2.Level - n1.Level) > 3)
            {
                //disables the target edge
                double weight = target.Weight;
                target.Enabled = false;

                //creates a new node with a random funciton
                int index = RandIndex();
                int level = (n2.Level + n1.Level) / 2;
                ActFunc func = RandFunc();

                //aborts the operation if we fail to insert
                if (index < 0) return;

                //inserts the node into our data-structor
                Nuron nx = new Nuron(this, func, level, index);
                nurons.Add(index, nx);

                ////adds axons to replace the missing axon
                //AddAxonInit(n1, nx, 1.0);
                //AddAxonInit(nx, n2, weight);

                //adds axons to replace the missing axon
                nx.AddAxon(n1, 1.0);
                n2.AddAxon(nx, weight);
            }
        }


        #endregion ////////////////////////////////////////////////////////////////////

        #region Helper Methods...

        ///// <summary>
        ///// Helper method: Adds an axon connection to the network, leading
        ///// from the source nuron to the target nuron. It dose not check the 
        ///// topology of the network before adding the axon, so care must be 
        ///// taken to avoid redunent or recurent connections. It returns null 
        ///// if it is unable to add the axon.
        ///// </summary>
        ///// <param name="source">Source nuron</param>
        ///// <param name="target">Target nuron</param>
        ///// <param name="weight">Weight of axon</param>
        ///// <returns>The added axon</returns>
        //private Axon AddAxonInit(Nuron source, Nuron target, double weight)
        //{
        //    int a1 = source.Index;
        //    int a2 = target.Index;

        //    //computes a hash of the endpoints as an index
        //    int index = unchecked((a1 * 907) ^ a2);
        //    index = index & Int32.MaxValue;

        //    //we must generate an index if we have that one
        //    if (axons.HasKey(index))
        //    {
        //        index = RandIndex();
        //        if (index < 0) return null;
        //    }

        //    //adds the axon to the target and our network
        //    Axon ax = new Axon(index, source.Index, weight);
        //    target.AddInput(ax);
        //    axons.Add(index, ax);

        //    return ax;
        //}


        /// <summary>
        /// Helper method: Enumerates all of the axons in the current network.
        /// </summary>
        /// <returns>An enumeration of all axons</returns>
        private IEnumerable<Axon> ListAxons()
        {
            //loops over all the nurons in the network
            foreach (Nuron n in nurons.ListItems())
            {
                //loops over all the axons conected to the nuron
                foreach (Axon ax in n.ListAxons()) yield return ax;
            }

            ////simply lists the axons in the table
            //return axons.ListItems();
        }

        /// <summary>
        /// Helper method: Finds an axon in the curent network that matches a
        /// target axon from another network. If no match can be found, it
        /// returns null. 
        /// </summary>
        /// <param name="other">Target axon</param>
        /// <returns>A matching axon</returns>
        private Axon FindMatch(Axon other)
        {
            //first atempts to find the target nuron
            Nuron n1 = nurons.GetValue(other.Target);
            if (n1 == null) return null;

            //then atempts to find the source nuron
            Nuron n2 = nurons.GetValue(other.Source);
            if (n2 == null) return null;

            //obtains the axon from the target
            return n1.GetAxon(n2);


            ////tries to find an axon with a matching ID
            //return axons.GetValue(other.Index);
        }


        #endregion ////////////////////////////////////////////////////////////////////

        #region Random Methods...

        /// <summary>
        /// Helper method: Chooses a random activation function, out 
        /// of the subset of allowed activation functions for this 
        /// type of network.
        /// </summary>
        /// <returns>Random activation funciton</returns>
        private ActFunc RandFunc()
        {
            //NOTE: Expand this later
            return ActFunc.Sigmoid;
        }

        /// <summary>
        /// Helper method: Selects a single nuron from the network at random.
        /// </summary>
        /// <returns>A randomly selected nuron</returns>
        private Nuron RandNuron()
        {
            //generates a random nuron in O(n)
            int index = rng.RandInt(nurons.Count);
            return nurons.ElementAt(index).Item;
        }

        ///// <summary>
        ///// Helper method: Selects a single axon from the network at random.
        ///// </summary>
        ///// <returns>A randomly selected axon</returns>
        //private Axon RandAxon()
        //{
        //    //generates a random axon in O(n)
        //    int index = rng.RandInt(axons.Count);
        //    return axons.ElementAt(index).Item;
        //}

        /// <summary>
        /// Helper method: generates a random index for refering to
        /// nurons or axons, garenteed to be unique. It returns negative
        /// if no sutch index can be found in a timely fassion.
        /// </summary>
        /// <returns>A random unique index</returns>
        private int RandIndex()
        {
            int index = -1;
            int count = 0;

            while (index < 0 && count < MAX_TRY)
            {
                //chooses a ranom positive interger
                index = rng.NextInt() & Int32.MaxValue;

                //invalidates the index if we have a collision
                //if (axons.HasKey(index)) index = -1;
                if (nurons.HasKey(index)) index = -1;

                count++;
            }

            return index;
        }

        /// <summary>
        /// Helper mehtod: Selects a pair of nurons at random from diffrent 
        /// levels of the network. The nurons are always listed in order. It
        /// returns false if it was ubale to generate a valid pair.
        /// </summary>
        /// <param name="n1">The lower level nuron</param>
        /// <param name="n2">The upper level nuron</param>
        private bool RandPair(out Nuron n1, out Nuron n2)
        {
            //generates two random nurons
            n1 = RandNuron();
            n2 = RandNuron();

            int count = 0;

            //keeps searching while the nurons are on the same level
            while (n1.Level == n2.Level && count < MAX_TRY)
            {
                Nuron n3 = RandNuron();
                n1 = n2;
                n2 = n3;

                count++;
            }

            //swaps the nurons if they are out of order
            if (n1.Level > n2.Level)
            {
                Nuron n3 = n1;
                n1 = n2;
                n2 = n3;
            }

            //indicates if we found a valid pair
            return (count < MAX_TRY);
        }

        #endregion ////////////////////////////////////////////////////////////////////

    }
}

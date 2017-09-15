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
    public sealed class NetworkCPP
    {
        //indicates the maximum number of tries for random probing
        private const int MAX_TRY = 64;

        //indicates the maximum number of levels
        private const int MAX_LV = 1024;

        //uses a random number genrator to evolve the network
        private VRandom rng;

        //stores a table of axons by inovation number
        private Table<Int32, Axon> axons;
        private Table<Int32, Nuron> nurons;


        private NetworkCPP(NetworkCPP other)
        {
            //copies the RNG by refrence
            rng = other.rng;

            int size_n = other.nurons.Buckets;
            int size_a = other.nurons.Buckets;

            nurons = new TableClosed<Int32, Nuron>(size_n);
            axons = new TableClosed<Int32, Axon>(size_a);

            foreach (var pair in other.nurons)
            {
                Nuron copy = new Nuron(this, pair.Item);
                nurons.Add(pair.Key, copy);
            }

            foreach (var pair in other.axons)
            {
                Axon copy = new Axon(pair.Item);
                axons.Add(pair.Key, copy);
            }
        }




        /// <summary>
        /// Adds a nuron to the network at the given level and with the given 
        /// activation function. It returns null if it was unable to add the nuron.
        /// </summary>
        /// <param name="func">Activation function of the nuron</param>
        /// <param name="level">Level of the nuron</param>
        /// <returns>The inserted nuron</returns>
        public Nuron AddNuron(ActFunc func, int level)
        {
            //clamps the level to be within range
            if (level > MAX_LV) level = MAX_LV;
            if (level < 0) level = 0;

            //tries to generate a random index
            int index = RandIndex();
            if (index < 0) return null;

            //creates the node and adds it
            Nuron n = new Nuron(this, func, level, index);
            nurons.Add(index, n);

            return n;
        }


        public bool AddAxon(Nuron source, Nuron target, double weight)
        {
            bool pass = true;

            //confirms that the source and target belong to this network
            pass &= nurons.HasKey(source.Index);
            pass &= nurons.HasKey(target.Index);
            if (!pass) throw new InvalidOperationException();

            //tries to generate a random index
            int index = RandIndex();
            if (index < 0) return false;

            //creates the axon, adding it to the target and network
            Axon ax = new Axon(index, source.Index, weight);
            target.AddInput(ax);
            axons.Add(index, ax);

            return true;
        }


        





        internal Axon GetAxonByID(int id)
        {
            return axons.GetValue(id);
        }

        internal Nuron GetNuronByID(int id)
        {
            return nurons.GetValue(id);
        }


        //NOTE: Could we make the paramaters static?

        public double Compare(NetworkCPP other, params double[] c)
        {
            //makes certain that we have enough paramaters
            if (c.Length < 2) throw new ArgumentException();

            //refrences the edge tables
            var net1 = this.axons;
            var net2 = other.axons;

            //used in computing the distance
            int match = 0;
            int disjoint = 0;    
            double wbar = 0.0;

            foreach (Axon ax in net1.ListItems())
            {
                if (net2.HasKey(ax.Index))
                {
                    //computes the distance between the weights
                    double w1 = ax.Weight;
                    double w2 = net2[ax.Index].Weight;

                    wbar += Math.Abs(w1 - w2);
                    match += 1;
                }
                else
                {
                    //counts the disjoint nodes
                    disjoint += 1;
                }
            }

            foreach (Axon ax in net2.ListItems())
            {
                //only counts the missing disjoint edges
                if (!net1.HasKey(ax.Index)) disjoint += 1;
            }

            //determins the size of the larger network
            int size = Math.Max(net1.Count, net2.Count);
            size = (size > 20) ? size - 20 : 1;

            //couputes the distance for specisation
            double dist = (c[0] * disjoint) / (double)size;
            dist += c[1] * (wbar / (double)match);

            return dist;
        }


        //NOTE: What should happen to disabled nodes during crossover?


        public NetworkCPP Combine(NetworkCPP mate, VRandom rng, double rate)
        {
            //makes a clone of the dominate parent
            NetworkCPP child = new NetworkCPP(this);

            //refrences the edge tables
            var net1 = mate.axons;
            var net2 = child.axons;

            foreach (Axon ax in net1.ListItems())
            {
                //obtains the child axon
                Axon axc = net2.GetValue(ax.Index);
                if (axc == null) continue;

                //determins the new weight based on crossover
                bool cross = rng.RandBool(rate);
                axc.Weight = cross ? ax.Weight : axc.Weight;

                //has a chance of enabling if either are disabled
                bool en = ax.Enabled && axc.Enabled;
                axc.Enabled = en || rng.RandBool(0.25);
            }

            return child;
        }



        //public void Expand()
        //{
        //    Nuron n1, n2;

        //    RandPair(out n1, out n2);
        //    Axon target = n2.GetAxon(n1);

        //    if (target == null)
        //    {
        //        //creates a new axon betwen the neurons
        //        int index = RandIndex();
        //        double weight = rng.RandGauss();

        //        ////we drop the axon in case of collision
        //        //if (index < 0) return;

        //        //adds the values to our data-structor
        //        target = new Axon(index, n1.Index, weight);
        //        n2.AddInput(target);
        //        axons.Add(index, target);
        //    }
        //    else if ((n2.Level - n1.Level) > 3)
        //    {
        //        //disables the target edge
        //        double weight = target.Weight;
        //        target.Enabled = false;

        //        //creates a new node
        //        int index = RandIndex();
        //        int level = (n2.Level + n1.Level) / 2;
        //        ActFunc func = RandFunc();

        //        //inserts the node into our data-structor
        //        Nuron node = new Nuron(this, func, level, index);
        //        nurons.Add(index, node);

        //        //adds an edge between n1 and x with weight 1.0
        //        index = RandIndex();
        //        target = new Axon(index, n1.Index, 1.0);
        //        node.AddInput(target);
        //        axons.Add(index, target);

        //        //adds an edge between x and n2 with target weight
        //        index = RandIndex();
        //        target = new Axon(index, node.Index, weight);
        //        n2.AddInput(target);
        //        axons.Add(index, target);
        //    }
            
        //}


        public void Expand()
        {
            Nuron n1, n2;

            //generates a pair of random nurons
            bool test = RandPair(out n1, out n2);
            Axon target = n2.GetAxon(n1);

            if (test && target == null)
            {
                //creates a new axon between the nurons
                double weight = rng.RandGauss();
                AddAxonInit(n1, n2, weight);

            }
            else if (test && !target.Enabled)
            {
                //reinitilises the axon with a new weight
                target.Weight = rng.RandGauss();
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

                //adds axons to replace the missing axon
                AddAxonInit(n1, nx, 1.0);
                AddAxonInit(nx, n2, weight);
            }
        }


        public void MutateSingle(double power)
        {
            //selects a random axon
            int index = rng.RandInt(axons.Count);
            Axon ax = axons.ElementAt(index).Item;

            //permutes the weight by a small amount
            double delta = rng.RandGauss(0.0, power);
            ax.Weight = ax.Weight + delta;

            //has a chance of renabling disabled axons
            ax.Enabled |= rng.RandBool(0.1);
        }


        public void Mutate(double rate, double power)
        {
            foreach (Axon ax in axons.ListItems())
            {
                //mutates weights based on the rate of mutation
                if (rng.RandBool(1.0 - rate)) continue;

                //permutes the weight by a small amount
                double delta = rng.RandGauss() * power;
                ax.Weight = ax.Weight + delta;
            }
        }


        /// <summary>
        /// Helper method: Adds an axon connection to the network, leading
        /// from the source nuron to the target nuron. It dose not check the 
        /// topology of the network before adding the axon, so care must be 
        /// taken to avoid redunent or recurent connections. It returns null 
        /// if it is unable to add the axon.
        /// </summary>
        /// <param name="source">Source nuron</param>
        /// <param name="target">Target nuron</param>
        /// <param name="weight">Weight of axon</param>
        /// <returns>The added axon</returns>
        private Axon AddAxonInit(Nuron source, Nuron target, double weight)
        {
            int index = RandIndex();
            if (index < 0) return null;

            Axon ax = new Axon(index, source.Index, weight);
            target.AddInput(ax);
            axons.Add(index, ax);

            return ax;
        }



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

        /// <summary>
        /// Helper method: Selects a single axon from the network at random.
        /// </summary>
        /// <returns>A randomly selected axon</returns>
        private Axon RandAxon()
        {
            //generates a random axon in O(n)
            int index = rng.RandInt(axons.Count);
            return axons.ElementAt(index).Item;
        }

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
                if (axons.HasKey(index)) index = -1;
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



    }
}

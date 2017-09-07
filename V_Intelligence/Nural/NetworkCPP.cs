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
    /// A Compositional Pattern Producing Network, or CPP Network, is a special type of
    /// Nural Net used to generate repeating and concentric patterns. Typicaly the entire
    /// spectrum of inputs is fed through the network to generate a hyper-dimentonal image,
    /// although the structor can be used for genraral tasks as well. The structor of the
    /// network consists of a directed acyclic graph (DAG) where each of the nurons can 
    /// have it's own evaluation funciton. The exact functions used determin the types
    /// of paterns the network can create. All CPPNs start mininaly and utilise nural
    /// evolution in order to evolve the desired structor.
    /// 
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
        /// activation function. It returns true if the nuron was sucesfully 
        /// added to the network.
        /// </summary>
        /// <param name="func">Activation function of the nuron</param>
        /// <param name="level">Level of the nuron</param>
        /// <returns>True on success</returns>
        public bool AddNuron(ActFunc func, int level)
        {
            //clamps the level to be within range
            if (level > MAX_LV) level = MAX_LV;
            if (level < 0) level = 0;

            //tries to generate a random index
            int index = RandIndex();
            if (index < 0) return false;

            //creates the node and adds it
            Nuron n = new Nuron(this, func, level, index);
            nurons.Add(index, n);

            return true;
        }


        public bool AddAxon(Nuron source, Nuron target, double weight)
        {
            //Note: confirm that source and target belong to this network


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

            return false;
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
            var net1 = this.axons;
            var net2 = mate.axons;
            var netC = child.axons;

            //foreach (Axon ax in net1.ListItems())
            //{
            //    //copies each axon from the dominate parent
            //    child.AddAxon(ax);
            //}

            foreach (Axon ax in net2.ListItems())
            {
                //we are only intrested in acvitve crossovers
                if (!netC.HasKey(ax.Index)) continue;
                if (!ax.Enabled) continue;

                //obtains the child axon
                Axon axc = netC.GetValue(ax.Index);

                if (axc.Enabled)
                {
                    //copies the weight based on crossover
                    if (rng.NextDouble() < rate)
                        axc.Weight = ax.Weight;
                }
                else
                {
                    //copies the weight and enables the axon
                    axc.Weight = ax.Weight;
                    axc.Enabled = true;
                }


                //if (netC.HasKey(ax.InvNo))
                //{
                //    //copies the weight based on crossover
                //    if (rng.NextDouble() > rate) continue;

                //    Axon axc = netC[ax.InvNo];
                //    axc.Weight = ax.Weight;

                //}
            }

            return child;
        }



        public void Expand(VRandom rng)
        {
            Nuron n1, n2;

            RandPair(out n1, out n2);
            Axon target = n2.GetAxon(n1);

            if (target == null)
            {
                //creates a new axon betwen the neurons
                int index = rng.NextInt() & Int32.MaxValue;
                double weight = rng.RandGauss(0.0, 1.0);

                //we drop the axon in case of collision
                if (axons.HasKey(index)) return;

                //adds the values to our data-structor
                target = new Axon(index, n1.Index, weight);
                n2.AddInput(target);
                axons.Add(index, target);
            }
            else
            {
                //disables the target edge
                target.Enabled = false;

                //creates a new node
                int index = rng.NextInt() & Int32.MaxValue;
                int level = (n2.Level + n1.Level) / 2;
                ActFunc func = RandFunc();

                //we drop the node in case of collision
                if (nurons.HasKey(index)) return;

                //NOTE: Need to add edges!!
            }
            
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
        /// Helper method: generates a random index for refering to
        /// nurons or axons, garenteed to be unique. It returns negative
        /// if no sutch index can be found in a timely fassion.
        /// </summary>
        /// <returns>A random unique index</returns>
        internal int RandIndex()
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
        /// Helper mehtod: Selects a pair of nurons at random from diffrent 
        /// levels of the network. The nurons are always listed in order.
        /// </summary>
        /// <param name="n1">The lower level nuron</param>
        /// <param name="n2">The upper level nuron</param>
        private void RandPair(out Nuron n1, out Nuron n2)
        {
            //generates two random nurons
            n1 = RandNuron();
            n2 = RandNuron();

            //keeps searching while the nurons are on the same level
            while (n1.Level == n2.Level)
            {
                Nuron n3 = RandNuron();
                n1 = n2;
                n2 = n3;
            }

            //swaps the nurons if they are out of order
            if (n1.Level > n2.Level)
            {
                Nuron n3 = n1;
                n1 = n2;
                n2 = n3;
            }
        }



    }
}

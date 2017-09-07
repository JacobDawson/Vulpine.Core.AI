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
        //stores all the nurons in sorted order
        private SortList<Nuron> nurons;

        //stores a table of axons by inovation number
        private Table<Int32, Axon> axons;


        private Table<Int32, Nuron> nurons2;


        private NetworkCPP(NetworkCPP other)
        {
            int max_n = other.nurons.Count;
            nurons = new SortListArray<Nuron>(max_n);
            axons = new TableClosed<Int32, Axon>();

            foreach (Nuron n in other.nurons)
            {
                Nuron copy = new Nuron(this, n);
                nurons.Add(copy);
            }

            foreach (Axon ax in other.axons.ListItems())
            {
                Axon copy = new Axon(ax);
                axons.Add(ax.Index, copy);
            }
        }


        public void AddNuron(VRandom rng, ActFunc func, int level)
        {
            int index = rng.NextInt() & Int32.MaxValue;

            while (nurons2.HasKey(index))
            {
                index = rng.NextInt() & Int32.MaxValue;
            }

            Nuron n = new Nuron(this, func, level, index);
        }


        private int GetIndex(VRandom rng)
        {
            int index = -1;

            while (index < 0)
            {
                index = rng.NextInt() & Int32.MaxValue;
                if (axons.HasKey(index)) index = -1;
                if (nurons2.HasKey(index)) index = -1;
            }

            return index;
        }




        public void AddAxon(Axon ax)
        {
            
        }





        internal Axon GetAxonByID(int id)
        {
            return axons.GetValue(id);
        }

        internal Nuron GetNuronByID(int id)
        {
            return nurons2.GetValue(id);
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
            ////starts with a clone of the curent network
            //NetworkCPP child = new NetworkCPP(this);

            //used in obtaining two random nodes
            int max = nurons.Count;
            int r1 = rng.RandInt(max);
            int r2 = rng.RandInt(max);

            //obtains two random nurons
            Nuron n1 = nurons.GetItem(r1);
            Nuron n2 = nurons.GetItem(r2);
            Axon target = null;

            //abort if we happen to chose identical nodes
            if (n1.Level == n2.Level) return;

            //swaps the neurons if reversed
            if (n1.Level < n2.Level)
            {
                var temp = n1;
                n1 = n2;
                n2 = temp;
            }

            //scans the input axons for a match
            foreach (Axon ax in n1.ListAxons())
            {
                if (ax.Input == n2.Level)
                {
                    target = ax;
                    break;
                }
            }



            if (target == null)
            {
                //creates a new axon betwen the neurons
                int invno = rng.NextInt() & Int32.MaxValue;
                double weight = rng.RandGauss(); //fix paramaters!!!

                //we drop the axon in case of collision
                if (axons.HasKey(invno)) return;

                //adds the values to our data-structor
                target = new Axon(invno, n2.Level, weight);
                n1.AddInput(target);
                axons.Add(invno, target);
            }
            else if (n1.Level - n2.Level > 4)
            {
                //disables the target edge
                target.Enabled = false;

                int level = rng.RandInt(n2.Level + 1, n1.Level);
                ActFunc func = GenFunc(rng);
                //Nuron n = new Nuron(this, func, level);
                //nurons.Add(n);


            }



            //return null;
        }



        public void Expand2(VRandom rng)
        {
            Nuron n1, n2;

            RandPair(rng, out n1, out n2);
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
                ActFunc func = GenFunc(rng);

                //we drop the node in case of collision
                if (nurons2.HasKey(index)) return;
            }
            
        }





        /// <summary>
        /// Chooses a random activation function, out of the subset of
        /// allowed activation functions for this type of network.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <returns>Random activation funciton</returns>
        private static ActFunc GenFunc(VRandom rng)
        {
            //NOTE: Expand this later
            return ActFunc.Sigmoid;
        }



        private Axon GetConnection(Nuron n1, Nuron n2)
        {
            //makes shure we scan the latter node
            if (n1.Level < n2.Level) return GetConnection(n2, n1);

            //scans the input axons for a match
            foreach (Axon ax in n1.ListAxons())
                if (ax.Input == n2.Level) return ax;

            //we failed to find a match
            return null;
        }


        /// <summary>
        /// Helper method: selects a single nuron from the network at random.
        /// </summary>
        /// <param name="rng">An RNG to select the nuron</param>
        /// <returns>A randomly selected nuron</returns>
        private Nuron RandNuron(VRandom rng)
        {
            //generates a random nuron in O(n)
            int index = rng.RandInt(nurons2.Count);
            return nurons2.ElementAt(index).Item;
        }

        /// <summary>
        /// Helper mehtod: selects a pair of nurons at random from diffrent 
        /// levels of the network. The nurons are always listed in order.
        /// </summary>
        /// <param name="rng">An RNG to select the nurons</param>
        /// <param name="n1">The lower level nuron</param>
        /// <param name="n2">The upper level nuron</param>
        private void RandPair(VRandom rng, out Nuron n1, out Nuron n2)
        {
            //generates two random nurons
            n1 = RandNuron(rng);
            n2 = RandNuron(rng);

            //keeps searching while the nurons are on the same level
            while (n1.Level == n2.Level)
            {
                Nuron n3 = RandNuron(rng);
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

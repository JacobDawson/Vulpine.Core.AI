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
    public sealed class NetworkCPP
    {
        //stores all the nurons in sorted order
        private SortList<Nuron> nurons;

        //stores a table of axons by inovation number
        private Table<Int32, Axon> axons;


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
                axons.Add(ax.InvNo, copy);
            }
        }


        public void AddAxon(Axon ax)
        {
            
        }





        internal Axon GetAxonByID(int id)
        {
            return axons.GetValue(id);
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
                if (net2.HasKey(ax.InvNo))
                {
                    //computes the distance between the weights
                    double w1 = ax.Weight;
                    double w2 = net2[ax.InvNo].Weight;

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
                if (!net1.HasKey(ax.InvNo)) disjoint += 1;
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
                if (!netC.HasKey(ax.InvNo)) continue;
                if (!ax.Enabled) continue;

                //obtains the child axon
                Axon axc = netC.GetValue(ax.InvNo);

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
            if (n1.ID == n2.ID) return;

            //swaps the neurons if reversed
            if (n1.ID < n2.ID)
            {
                var temp = n1;
                n1 = n2;
                n2 = temp;
            }

            //scans the input axons for a match
            foreach (Axon ax in n1.ListAxons())
            {
                if (ax.Input == n2.ID)
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
                target = new Axon(invno, n2.ID, weight);
                n1.AddInput(target);
                axons.Add(invno, target);
            }
            else if (n1.ID - n2.ID > 4)
            {
                //disables the target edge
                target.Enabled = false;

                int level = rng.RandInt(n2.ID + 1, n1.ID);
                ActFunc func = GenFunc(rng);
                Nuron n = new Nuron(this, func, level);
                nurons.Add(n);


            }



            //return null;
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






        //private Axon GetConnection(Nuron n1, Nuron n2)
        //{
        //    if (n1.ID > n2.ID)
        //    {
        //        return GetConnection(n2, n1);
        //    }


        //    foreach (int id in n1.ListInputs())
        //    {
        //        Axon ax = axons.GetValue(id);
        //        if (ax != null && ax.Input == n2.ID) return ax;
        //    }

        //    return null;         
        //}

        private Axon GetConnection(Nuron n1, Nuron n2)
        {
            //makes shure we scan the latter node
            if (n1.ID < n2.ID) return GetConnection(n2, n1);

            //scans the input axons for a match
            foreach (Axon ax in n1.ListAxons())
                if (ax.Input == n2.ID) return ax;

            //we failed to find a match
            return null;
        }




    }
}

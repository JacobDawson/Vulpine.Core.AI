using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Data;
using Vulpine.Core.Data.Tables;
using Vulpine.Core.Data.Lists;
using Vulpine.Core.Data.Queues;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace Vulpine.Core.AI.Nural
{
    public class NetworkAuto : Genetic<NetworkAuto>
    {
        //the standard deviation for new nodes and shifted nodes
        private const double SD_NEW = 2.0;
        private const double SD_SHIFT = 0.5;

        //what is the probablity that the network will expand when mutated
        private const double P_EXPAND = 0.2;

        //what is the probablity that a node will change activation 
        //functions when the network is mutated
        private const double P_NODE = 0.2;

        //indicates the maximum number of tries for random probing
        private const int MAX_TRY = 64;
        private const int MAX_ID = 64000; 

        //stores the nurons and axons in independent tables
        private Table<Int32, Nuron> nurons;
        private Table<Int32, Axon> axons;

        //stores the indicies of the input and output nurons
        private int[] inputs;
        private int[] outputs;

        /// <summary>
        /// Creates a prototype network, with the given number of inputs and
        /// outputs. No connections exist in the network, and the Initialise
        /// mehtod must be called in order to generate a minimily connected
        /// network. This way, diffrent starting networks can be generated
        /// from the same prototype.
        /// </summary>
        /// <param name="inputs">Number of input nurons</param>
        /// <param name="outputs">Number of output nurons</param>
        public NetworkAuto(int inputs, int outputs)
        {
            //initialises the tables for the nurons and axons
            nurons = new TableOpen<Int32, Nuron>(256);
            axons = new TableOpen<Int32, Axon>(1024);

            //creates arrays to remember the inputs and outputs
            this.inputs = new int[inputs];
            this.outputs = new int[outputs];

            for (int i = 0; i < inputs; i++)
            {
                Nuron n = new Nuron(this, ActFunc.Input, i);
                nurons.Add(n.Index, n);
                this.inputs[i] = n.Index; //i
            }

            for (int i = 0; i < outputs; i++)
            {
                Nuron n = new Nuron(this, ActFunc.Sigmoid, MAX_ID - i);
                nurons.Add(n.Index, n);
                this.outputs[i] = n.Index; //MAX_ID - i
            }

            //IDEA: Why not just store the nurons in a list, with an incrementing index?
            //the nuron's ID is then just it's index into the list. Could this make
            //things more complicated when we want to compare nurons between networks?
        }



        public int InSize
        {
            get { return inputs.Length; }
        }

        public int OutSize
        {
            get { return outputs.Length; }
        }

        public int NumNurons
        {
            get { return nurons.Count; }
        }

        public int NumAxons
        {
            get { return axons.Count; }
        }



        //internal Axon GetAxon(int index)
        //{
        //    return axons.GetValue(index);
        //}

        internal Nuron GetNuron(int index)
        {
            return nurons.GetValue(index);
        }

        internal IEnumerable<Axon> ListAxons()
        {
            return axons.ListItems();
        }





        #region Network Opperations...


        public void SetInput(Vector data)
        {           
            //fills each of the input nurons with the vector data
            for (int i = 0; i < inputs.Length; i++)
            {
                Nuron n = nurons.GetValue(inputs[i]);
                n.Value = data.GetExtended(i);
                n.Prev = n.Value;
            }
        }

        public void Propergate()
        {
            var ittr0 = nurons.ListItems();
            foreach (Nuron nx in ittr0)
            {
                //Skips input nurons
                if (nx.IsInput) continue;

                //clears the value and updates the previous value
                nx.Prev = nx.Value;
                nx.Value = 0.0;
            }

            var ittr1 = axons.ListItems();
            foreach (Axon ax in ittr1)
            {
                //skips disabled nurons
                if (!ax.Enabled) continue;

                //obtains both nurons from the axon
                Nuron n1 = nurons.GetValue(ax.Source);
                Nuron n2 = nurons.GetValue(ax.Target);

                //updates the target based on the input
                n2.Value += (n1.Prev * ax.Weight);
            }

            var ittr2 = nurons.ListItems();
            foreach (Nuron nx in ittr2)
            {
                //activates each of the nurons
                nx.Activate();
            }
        }

        public Vector ReadOutput()
        {
            //creates a new vector to hold the result
            Vector result = new Vector(outputs.Length);

            //copies the output nurons into the result vector
            for (int i = 0; i < outputs.Length; i++)
            {
                Nuron n = nurons.GetValue(outputs[i]);
                result[i] = n.Value;
            }

            //returns the result
            return result;
        }

        public void WriteOutputTo(Vector result)
        {
            //enshures that the result vector can hold the output
            if (result.Length != outputs.Length) throw new ArgumentException
                ("Reslut vector dose not match network topology.", "result");

            //copies the output nurons into the result vector
            for (int i = 0; i < outputs.Length; i++)
            {
                Nuron n = nurons.GetValue(outputs[i]);
                result[i] = n.Value;
            }
        }

        public void ResetNetwork()
        {
            //uses to itterate the nurons
            var ittr = nurons.ListItems();

            //simply sets the value of each nuron to zero
            foreach (Nuron nx in ittr)
            {
                nx.Value = 0.0;
                nx.Prev = 0.0;
            }
        }


        #endregion /////////////////////////////////////////////////////////////////////////


        #region Genetic Opperations...

        public void Initialise(VRandom rng)
        {
            //creates axons betwen all input-output pairs
            //and randomizes the coneciton weights

            for (int i = 0; i < inputs.Length; i++)
            {
                for (int j = 0; j < outputs.Length; j++)
                {
                    double w = rng.RandGauss() * SD_NEW;
                    Axon ax = new Axon(inputs[i], outputs[j], w);
                    axons.Overwrite(ax.Index, ax);

                    //We need to use the overwrite method here, incase Initialse 
                    //has been called before, allowing for reinitializaiton
                }
            }
        }

        public NetworkAuto SpawnRandom(VRandom rng)
        {
            int ni = this.InSize;
            int no = this.OutSize;

            //creates a new network with the same number of ins and outs
            NetworkAuto network = new NetworkAuto(ni, no);

            //initalises the new network to a radom state
            network.Initialise(rng);

            return network;
        }

        public void Overwrite(NetworkAuto genome)
        {
            //NOTE: Need to assert that the inputs and outputs match

            this.axons.Clear();
            this.nurons.Clear();

            //used in itterating all the axons and nurons
            var g_axons = genome.axons.ListItems();
            var g_nurons = genome.nurons.ListItems();

            foreach (Nuron n1 in g_nurons)
            {
                nurons.Overwrite(n1.Index, n1);
            }

            foreach (Axon a1 in g_axons)
            {
                axons.Overwrite(a1.Index, a1);
            }
        }

        public void OverwriteAlt(NetworkAuto genome)
        {
            //used in itterating all the axons and nurons
            var g_axons = genome.axons.ListItems();
            var g_nurons = genome.nurons.ListItems();

            foreach (Nuron n1 in g_nurons)
            {
                //tries to find the matching nuron in the current network
                Nuron n0 = nurons.GetValue(n1.Index);

                if (n0 != null)
                {
                    //copies the genome data if it finds a match
                    n0.Func = n1.Func;
                }
                else
                {
                    //creates a copy of the first nurons and adds it
                    n0 = new Nuron(this, n1);
                    nurons.Add(n0.Index, n0);
                }
            }

            foreach (Axon a1 in g_axons)
            {
                //tries to find the matching axon in the current network
                Axon a0 = axons.GetValue(a1.Index);

                if (a0 != null)
                {
                    //copies the genome data if it finds a match
                    a0.Weight = a1.Weight;
                    a0.Enabled = a1.Enabled;          
                }
                else
                {
                    //creates a copy of the first axon and adds it
                    a0 = new Axon(a1);
                    axons.Add(a0.Index, a0);
                }
            }

            //uses a queue to mark certain nurons and axons for deletion
            int tn = Math.Abs(nurons.Count - genome.nurons.Count);
            int ta = Math.Abs(axons.Count - genome.axons.Count);

            var ntrash = new DequeArray<Nuron>(tn);
            var atrash = new DequeArray<Axon>(ta);

            var t_axons = axons.ListItems();
            var t_nurons = nurons.ListItems();

            //marks the excess axons and nurons for deletion
            foreach (Axon a1 in t_axons)
            {
                if (genome.axons.HasKey(a1.Index)) continue;
                else atrash.PushFront(a1);
            }

            foreach (Nuron n1 in t_nurons)
            {
                if (genome.nurons.HasKey(n1.Index)) continue;
                else ntrash.PushFront(n1);
            }

            //removes the excess axons and nurons
            while (!atrash.Empty)
            {
                Axon del = atrash.PopFront();
                axons.Remove(del.Index);
            }

            while (!ntrash.Empty)
            {
                Nuron del = ntrash.PopFront();
                del.ClearData();
                nurons.Remove(del.Index); 
            }
        }

        public void Mutate(VRandom rng, double rate)
        {
            //clamps the rate to be between zero and one
            rate = VMath.Clamp(rate);

            //expands the size of the network if nessary
            if (rng.RandBool(P_EXPAND)) Expand(rng);

            //changes the activation funciton of a single node
            if (rng.RandBool(P_NODE))
            {              
                Nuron node = GetRandomNuron(rng);
                if (node.IsInput) return;
                node.Func = GetRandomActivation(rng);
                return;
            }

            var ittr = axons.ListItems();

            foreach (Axon ax in ittr)
            {
                //mutates weights based on the rate of mutation
                if (!rng.RandBool(rate)) continue;

                if (ax.Enabled)
                {
                    //permutes the weight by a small amount
                    double delta = rng.RandGauss() * SD_SHIFT;
                    ax.Weight = ax.Weight + delta;
                }
                else
                {
                    //resets the neuron to a small weight
                    ax.Weight = rng.RandGauss() * SD_SHIFT;
                    ax.Enabled = true;
                }
            }
        }

        public void Crossover(VRandom rng, NetworkAuto genome)
        {
            throw new NotImplementedException();
        }

        public double Compare(NetworkAuto genome)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //clears the data from all nurons
            var ittr = nurons.ListItems();
            foreach (Nuron n in ittr) n.ClearData();

            //cleans up the tables of nurons and axons
            nurons.Dispose();
            axons.Dispose();
        }

        #endregion /////////////////////////////////////////////////////////////////////////

        #region Mutation Operations...

        private void Expand(VRandom rng)
        {
            //obtains a pair of nurons completly at random
            Nuron n1 = GetRandomNuron(rng);
            Nuron n2 = GetRandomNuron(rng);

            //IMPORTANT: What if n2 is an input nuron, what if both
            //nurons are input nurons? This should occor about 1/4
            //of the time with new netorks!!!

            if (n2.IsInput)
            {
                //this is an Ad-Hoc solution to reduce the number of
                //node-to-input connections, it still won't prevent
                //input-input connecitons however.

                if (n1.IsInput) return;

                Nuron nx = n1;
                n1 = n2;
                n2 = nx;
            }

            //creates a temporary axon between the two objects
            double w = rng.RandGauss() * SD_NEW;
            Axon temp = new Axon(n1.Index, n2.Index, w);

            //atempts to gain the actual axon if it exists
            Axon actual = axons.GetValue(temp.Index);

            //NOTE: It is possable to have a node connect to itself
            //this is not a failing, but a feature of the system

            if (actual == null)
            {
                //adds the new axon into the network
                axons.Add(temp.Index, temp);
            }
            else if (actual.Enabled == false)
            {
                //reneables the axon and updates it weight
                actual.Enabled = true;
                actual.Weight = temp.Weight;
            }
            else
            {
                //determins if we are able to insert a new node
                int nid1 = GetRandomIndex(rng);
                if (nid1 < 0) return;

                //adds a new node to the network
                ActFunc act = GetRandomActivation(rng);
                Nuron n3 = new Nuron(this, act, nid1);
                nurons.Add(n3.Index, n3);

                //inserts two new axons where the old one used to be
                int nid0 = actual.Source;
                int nid2 = actual.Target;

                Axon ax1 = new Axon(nid0, nid1, actual.Weight);
                Axon ax2 = new Axon(nid1, nid2, temp.Weight);

                //we use the overwitre comand because there may be hash collisions
                axons.Overwrite(ax1.Index, ax1);
                axons.Overwrite(ax2.Index, ax2);

                //deactivates the old axon
                actual.Enabled = false;
            }
        }


        #endregion /////////////////////////////////////////////////////////////////////////

        #region Helper Funcitons...


        private Nuron GetRandomNuron(VRandom rng)
        {
            //uses the RNG to select a random nuron
            var list = nurons.ListItems();
            return rng.RandElement(list);
        }

        private ActFunc GetRandomActivation(VRandom rng)
        {
            //generates a random nuber to select the fuciton
            int test = rng.RandInt(6);

            switch (test)
            {
                case 0: return ActFunc.Identity;
                case 1: return ActFunc.Sine;
                case 2: return ActFunc.Cosine;
                case 3: return ActFunc.Gaussian;
                case 4: return ActFunc.Sigmoid;
                case 5: return ActFunc.Sinc;
            }

            //we should never reach this point
            throw new NotImplementedException();
        }

        private int GetRandomIndex(VRandom rng)
        {
            int index = -1;
            int count = 0;

            while (index < 0 && count < MAX_TRY)
            {
                //chooses a ranom positive interger
                index = rng.RandInt(MAX_ID);

                //invalidates the index if we have a collision
                if (nurons.HasKey(index)) index = -1;

                count++;
            }

            return index;
        }

        #endregion /////////////////////////////////////////////////////////////////////////



    }
}

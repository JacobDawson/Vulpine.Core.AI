using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using Vulpine.Core.Data;
using Vulpine.Core.Data.Tables;
using Vulpine.Core.Data.Lists;
using Vulpine.Core.Data.Queues;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace Vulpine.Core.AI.Nural
{
    public class NetworkAuto : Network<NetworkAuto>
    {
        //the standard deviation for new nodes and shifted nodes
        private const double SD_NEW = 2.0;
        private const double SD_SHIFT = 0.5;

        //what is the probablity that the network will expand when mutated
        private const double P_EXPAND = 0.01; //0.1;

        //the probability that we toggel an axon during mutation
        public const double P_TOGGEL = P_EXPAND + 0.1; //0.2;

        //the probablity that we prefrom liniar crossover
        public const double P_Linear = 0.25;

        //inidcates the chance that a network conneciton is
        //made recurent, assuming recurent connecions are allowed
        public const double P_Recur = 0.1;

        //constants used in comparing networks
        public const double C0 = 2.0;
        public const double C1 = 1.0;

        //indicates the maximum number of tries for random probing
        private const int MAX_TRY = 64;
        private const int MAX_ID = 64000;
        private const int MAX_LV = 4096;

        //stores the nurons and axons in independent tables
        private Table<Int32, Nuron> nurons;
        private Table<Int32, Axon> axons;

        //stores the indicies of the input and output nurons
        private int[] inputs;
        private int[] outputs;

        //indicates if recurent connections are allowed
        private bool recurent = false;

        /// <summary>
        /// Creates a prototype network, with the given number of inputs and
        /// outputs. The network starts out maximily connected with axons between
        /// each pair of input and output nurons. There are no hidden nurons.
        /// The weight of each axon is initialy set to one.
        /// </summary>
        /// <param name="inputs">Number of input nurons</param>
        /// <param name="outputs">Number of output nurons</param>
        public NetworkAuto(int inputs, int outputs, bool recurent = false)
        {
            this.recurent = recurent;

            ////initialises the tables for the nurons and axons
            //nurons = new TableOpen<Int32, Nuron>(256);
            //axons = new TableOpen<Int32, Axon>(1024);

            //initialises the tables for the nurons and axons
            nurons = new TableSystem<Int32, Nuron>(256);
            axons = new TableSystem<Int32, Axon>(1024);

            //creates arrays to remember the inputs and outputs
            this.inputs = new int[inputs];
            this.outputs = new int[outputs];

            //creates all the input nurons
            for (int i = 0; i < inputs; i++)
            {
                Nuron n = new Nuron(this, ActFunc.Input, i, 0);
                nurons.Add(n.Index, n);
                this.inputs[i] = n.Index; //i
            }

            //creates all the output nurons
            for (int i = 0; i < outputs; i++)
            {
                Nuron n = new Nuron(this, ActFunc.Sigmoid, MAX_ID - i, MAX_LV);
                nurons.Add(n.Index, n);
                this.outputs[i] = n.Index; //MAX_ID - i
            }

            //adds conecitons between all the nodes
            Initialise();
        }


        public NetworkAuto(NetworkAuto other)
        {  
            //copies the global settings
            this.recurent = other.recurent;

            //obtains the raw statistics of the opposing network
            int ninputs = other.inputs.Length;
            int noutputs = other.outputs.Length;
            int nnurons = other.nurons.Count;
            int naxons = other.axons.Count;

            //makes tables large enough to hold the nurons and axons
            nurons = new TableSystem<Int32, Nuron>(nnurons);
            axons = new TableSystem<Int32, Axon>(naxons);

            //dose the same for the inputs and outputs
            inputs = new int[ninputs];
            outputs = new int[noutputs];

            //makes copies of all the nurons in the original network
            var ittr1 = other.nurons.ListItems();
            foreach (Nuron n in ittr1)
            {
                Nuron temp = new Nuron(this, n);
                nurons.Add(temp.Index, temp);
            }

            //makes copies of all the axons in the original network 
            var ittr2 = other.axons.ListItems();
            foreach (Axon ax in ittr2)
            {
                Axon temp = new Axon(ax);
                axons.Add(temp.Index, temp);
            }

            //copies the input and output indicies
            for (int i = 0; i < inputs.Length; i++)
                this.inputs[i] = other.inputs[i];

            for (int i = 0; i < outputs.Length; i++)
                this.outputs[i] = other.outputs[i];
        }


        public NetworkAuto(string file)
        {
            StreamReader sr = new StreamReader(file);
            string line;
            string[] pram;
            
            //skips the first two lines
            sr.ReadLine();
            sr.ReadLine();

            //copies the global settings
            line = sr.ReadLine();
            this.recurent = Boolean.Parse(line);

            //obtains the raw statistics of the opposing network
            line = sr.ReadLine();
            pram = line.Split(',');
            int ninputs = Int32.Parse(pram[0]);
            int noutputs = Int32.Parse(pram[1]);
            int nnurons = Int32.Parse(pram[2]);
            int naxons = Int32.Parse(pram[3]);

            //makes tables large enough to hold the nurons and axons
            nurons = new TableSystem<Int32, Nuron>(nnurons);
            axons = new TableSystem<Int32, Axon>(naxons);

            //dose the same for the inputs and outputs
            inputs = new int[ninputs];
            outputs = new int[noutputs];

            ////makes copies of all the nurons in the original network
            //var ittr1 = other.nurons.ListItems();
            //foreach (Nuron n in ittr1)
            //{
            //    Nuron temp = new Nuron(this, n);
            //    nurons.Add(temp.Index, temp);
            //}

            for (int i = 0; i < nnurons; i++)
            {
                int funcID, index, level;
                ActFunc func = ActFunc.SoftPlus;

                line = sr.ReadLine();
                pram = line.Split(',');
                funcID = Int32.Parse(pram[0]);
                index = Int32.Parse(pram[1]);
                level = Int32.Parse(pram[2]);

                switch (funcID)
                {
                    case 0: func = ActFunc.Input; break;
                    case 1: func = ActFunc.Identity; break;
                    case 2: func = ActFunc.Sine; break;
                    case 3: func = ActFunc.Cosine; break;
                    case 4: func = ActFunc.Gaussian; break;
                    case 5: func = ActFunc.Sigmoid; break;
                    case 6: func = ActFunc.SoftPlus; break;
                    case 7: func = ActFunc.Sinc; break;
                }

                Nuron temp = new Nuron(this, func, index, level);
                nurons.Add(temp.Index, temp);
            }

            ////makes copies of all the axons in the original network 
            //var ittr2 = other.axons.ListItems();
            //foreach (Axon ax in ittr2)
            //{
            //    Axon temp = new Axon(ax);
            //    axons.Add(temp.Index, temp);
            //}

            for (int i = 0; i < naxons; i++)
            {
                int source, target;
                double weight;

                line = sr.ReadLine();
                pram = line.Split(',');
                source = Int32.Parse(pram[0]);
                target = Int32.Parse(pram[1]);
                weight = Double.Parse(pram[2]);
                
                Axon temp = new Axon(source, target, weight);
                axons.Add(temp.Index, temp);
            }

            ////copies the input and output indicies
            //for (int i = 0; i < inputs.Length; i++)
            //    this.inputs[i] = other.inputs[i];

            //for (int i = 0; i < outputs.Length; i++)
            //    this.outputs[i] = other.outputs[i];

            for (int i = 0; i < ninputs; i++)
            {
                line = sr.ReadLine();
                this.inputs[i] = Int32.Parse(line);
            }

            for (int i = 0; i < noutputs; i++)
            {
                line = sr.ReadLine();
                this.outputs[i] = Int32.Parse(line);
            }

            sr.Close();

        }

        public void WriteToFile(string file)
        {
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine("FoxieIO Adaptive Nural Network, v1.0.0");
            sw.WriteLine("-----");

            sw.WriteLine(recurent);
            sw.WriteLine("{0},{1},{2},{3}", 
                inputs.Length, 
                outputs.Length, 
                nurons.Count, 
                axons.Count);


            //makes copies of all the nurons in the original network
            var ittr1 = nurons.ListItems();
            foreach (Nuron n in ittr1)
            {
                int funcID, index, level;
                ActFunc func;

                func = n.Func;
                index = n.Index;
                level = n.Level;
                funcID = 6;

                switch (func)
                {
                    case ActFunc.Input: funcID = 0; break;
                    case ActFunc.Identity: funcID = 1; break;
                    case ActFunc.Sine: funcID = 2; break;
                    case ActFunc.Cosine: funcID = 3; break;
                    case ActFunc.Gaussian: funcID = 4; break;
                    case ActFunc.Sigmoid: funcID = 5; break;
                    case ActFunc.SoftPlus: funcID = 6; break;
                    case ActFunc.Sinc: funcID = 7; break;
                }

                sw.WriteLine("{0},{1},{2}",
                    funcID,
                    index,
                    level);
            }

            //makes copies of all the axons in the original network 
            var ittr2 = axons.ListItems();
            foreach (Axon ax in ittr2)
            {
                int source, target;
                double weight;

                source = ax.Source;
                target = ax.Target;
                weight = ax.Weight;

                if (weight.IsNaN()) weight = 0.0;

                sw.WriteLine("{0},{1},{2:R}",
                    source,
                    target,
                    weight);
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                int index = this.inputs[i];
                sw.WriteLine(index);
            }

            for (int i = 0; i < outputs.Length; i++)
            {
                int index = this.outputs[i];
                sw.WriteLine(index);
            }

            sw.Flush();
            sw.Close();
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

        /// <summary>
        /// Removes unused Nurons and Axons from the network, reducing the overall
        /// size of the network without effecting it's functionality.
        /// </summary>
        public void ReduceNetwork()
        {
            var ittr1 = axons.ListItems();
            var trash1 = new Stack<Axon>();

            foreach (Axon ax in ittr1)
            {
                if (ax.Enabled) continue;
                else trash1.Push(ax);
            }

            while (trash1.Count > 0)
            {
                Axon ax = trash1.Pop();
                axons.Remove(ax.Index);
            }

            var ittr2 = nurons.ListItems();
            var trash2 = new Stack<Nuron>();

            foreach (Nuron n in ittr2)
            {
                var ittr3 = axons.ListItems();
                bool clear = false;

                foreach (Axon ax in ittr3)
                {
                    if (ax.Source == n.Index || ax.Target == n.Index)
                    {
                        clear = true;
                        break;
                    }
                }

                if (clear) continue;
                else trash2.Push(n);
            }

            while (trash2.Count > 0)
            {
                Nuron n = trash2.Pop();
                nurons.Remove(n.Index);
            }
        }


        #endregion /////////////////////////////////////////////////////////////////////////


        #region Genetic Opperations...


        /// <summary>
        /// Initalizes the network, adding axons between all the input and output
        /// nurons. Usualy this is done when the network is first instanciated.
        /// At first, the weights of all connections will be one. In order to get
        /// the network into a random state, you will need to call the Randomize
        /// function.
        /// </summary>
        private void Initialise()
        {
            //creates axons betwen all input-output pairs
            //and randomizes the coneciton weights

            for (int i = 0; i < inputs.Length; i++)
            {
                for (int j = 0; j < outputs.Length; j++)
                {
                    Axon ax = new Axon(inputs[i], outputs[j], 1.0);
                    axons.Overwrite(ax.Index, ax);

                    //We need to use the overwrite method here, incase Initialse 
                    //has been called before, allowing for reinitializaiton
                }
            }
        }

        public NetworkAuto Clone()
        {
            //calls the copy constructor, passing itself
            return new NetworkAuto(this);
        }

        public void Randomize(VRandom rng)
        {
            //used to list all the axons
            var ittr = axons.ListItems();

            //sets the weight of each axon to a random value
            foreach (Axon ax in ittr)
            {
                if (!ax.Enabled) continue;
                ax.Weight = rng.RandGauss() * SD_NEW;
            }
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
                //nurons.Overwrite(n1.Index, n1);
                Nuron temp = new Nuron(this, n1);
                nurons.Add(temp.Index, temp);
            }

            foreach (Axon a1 in g_axons)
            {
                //axons.Overwrite(a1.Index, a1);
                Axon temp = new Axon(a1);
                axons.Add(temp.Index, temp);
            }
        }

        public void Overwrite2(NetworkAuto genome)
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
                    ////copies the genome data if it finds a match
                    //n0.Func = n1.Func;
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


            ////////////////////////////////////////////////////////////////////


            //used to clean up the remaining Axons
            int count1 = genome.axons.Count - axons.Count;
            count1 = Math.Min(count1, 0) + 8;
            var trash1 = new DequeArray<Axon>(count1);
            var ittr1 = axons.ListItems();

            //marks missing Axons for deletion
            foreach (Axon a in ittr1)
            {
                if (genome.axons.HasKey(a.Index)) continue;
                else trash1.PushFront(a);
            }

            //deletes the missing Axons
            while (!trash1.Empty)
            {
                Axon del = trash1.PopFront();
                axons.Remove(del.Index);
            }

            //used to clean up the remaining Nurons
            int count2 = genome.nurons.Count - nurons.Count;
            count2 = Math.Min(count2, 0) + 8;
            var trash2 = new DequeArray<Nuron>(count2);
            var ittr2 = nurons.ListItems();

            //marks missing Nurons for deletion
            foreach (Nuron n in ittr2)
            {
                if (genome.nurons.HasKey(n.Index)) continue;
                else trash2.PushFront(n);
            }

            //deletes the missing Nurons
            while (!trash2.Empty)
            {
                Nuron del = trash2.PopFront();
                nurons.Remove(del.Index);
                del.ClearData();
            }

            trash1.Dispose();
            trash2.Dispose();

        }

        //In this method we assume a standard mutation rate of 1.0;
        public void Mutate(VRandom rng, double rate)
        {
            //makes shure the rate is positive
            rate = Math.Abs(rate);

            double selector = rng.NextDouble();


            if (selector < P_EXPAND)
            {
                //expands the network
                Expand(rng);
            }
            else if (selector < P_TOGGEL)
            {
                Axon ax = GetRandomAxon(rng);

                if (ax.Enabled)
                {
                    //outright disables the nuron
                    ax.Enabled = false;
                }
                else
                {
                    //resets the neuron to a large weight
                    ax.Weight = rng.RandGauss() * SD_NEW;
                    ax.Enabled = true;
                }
            }
            else
            {
                Axon ax = GetRandomAxon(rng);

                if (ax.Enabled)
                {
                    //permutes the weight by a small amount
                    double delta = rng.RandGauss() * SD_SHIFT;
                    ax.Weight = ax.Weight + delta;
                }
                else
                {
                    //resets the nuron to a small weight
                    double delta = rng.RandGauss() * SD_SHIFT;
                    ax.Weight = delta;
                    ax.Enabled = true;
                }
            }

        }


        //This is the (Old) One. It assumes a rate between 0.0 and 1.0
        public void MutateAlt(VRandom rng, double rate)
        {
            //clamps the rate to be between zero and one
            rate = VMath.Clamp(rate);

            //expands the size of the network at random
            //if (rng.RandBool(P_EXPAND)) Expand(rng);
            if (rng.RandBool(rate)) Expand(rng);

            //used in itterating the structure
            var ittr1 = nurons.ListItems();
            var ittr2 = axons.ListItems();

            //foreach (Nuron n in ittr1)
            //{
            //    //skips over input nurons
            //    if (n.IsInput) continue;

            //    //mutates nodes based on the augmented mutation rate
            //    if (!rng.RandBool(P_NODE * rate)) continue;

            //    //updates the activation funciton
            //    n.Func = GetRandomActivation(rng);
            //}

            foreach (Axon ax in ittr2)
            {
                if (rng.RandBool(P_TOGGEL * rate))
                {
                    //toggeles the enabled state
                    ax.Enabled = !ax.Enabled;
                    if (ax.Enabled) ax.Weight = 0.0;
                }

                if (ax.Enabled)
                {
                    //permutes the weight by a small amount
                    double delta = rng.RandGauss() * SD_NEW;
                    ax.Weight = ax.Weight + (delta * rate);
                }
            }
        }

        //IDEA: Get Rid of Liniar Crossover, as it is not what NEAT uses
        //instead use the default form of crossover.

        public void Crossover(VRandom rng, NetworkAuto genome)
        {
            //throw new NotImplementedException();

            //determins weather or not to do liniar crossover
            bool liniar = rng.RandBool(P_Linear);
            double a = rng.NextDouble();

            //lists all the axons and nurons in the mate
            var ittr1 = genome.nurons.ListItems();
            var ittr2 = genome.axons.ListItems();

            foreach (Nuron n in ittr1)
            {
                //obtains the matching child nuron
                Nuron nc = nurons.GetValue(n.Index);

                if (nc == null)
                {
                    //adds the missing nuron
                    nc = new Nuron(this, n);
                    nurons.Add(nc.Index, nc);
                }
                //else
                //{
                //    //determins the activation based on crossover
                //    bool cross = rng.RandBool();
                //    if (cross) nc.Func = n.Func;
                //}
            }

            foreach (Axon ax in ittr2)
            {
                //obtains the matching child axon
                Axon axc = axons.GetValue(ax.Index);

                if (axc == null)
                {
                    //adds the missing axon, but disabled
                    axc = new Axon(ax);
                    axc.Enabled = false;
                    axons.Add(axc.Index, axc);
                }
                else
                {
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

                    //if the axon is present in both networks, it has a
                    //strong chance of becoming enabled
                    bool en = ax.Enabled && axc.Enabled;
                    axc.Enabled = en || rng.RandBool(0.25);
                }
            }
        }


        public void Lerp(NetworkAuto genome, double amount)
        {
            //clip ammount to be in range 0 to 1
            double x = amount;
            if (x > 1.0) x = 1.0;
            if (x < 0.0) x = 0.0;


            var ittr = genome.axons.ListItems();

            foreach (Axon ax in ittr)
            {
                //obtains the matching axon
                Axon axc = axons.GetValue(ax.Index);

                //ignores axons not shared in common
                if (axc == null) continue;

                double w1 = axc.Weight;
                double w2 = ax.Weight;

                //preforms the linear interpolation
                axc.Weight = (1.0 - x) * w1 + x * w2;
            }

            
            //throw new NotImplementedException();
        }


        public double Compare(NetworkAuto genome)
        {
            //used in computing the distance
            int match = 0;
            int disjoint = 0;
            double wbar = 0.0;

            //lists the axons in each of the networks
            var ittr1 = this.axons.ListItems();
            var ittr2 = genome.axons.ListItems();

            foreach (Axon ax1 in ittr1)
            {
                //tries to find the matching axon
                Axon ax2 = genome.axons.GetValue(ax1.Index);

                if (ax2 != null)
                {
                    //computes the distance bteween the weights
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

            foreach (Axon ax1 in ittr2)
            {
                //counts the disjoint edges from the other network
                bool test = this.axons.HasKey(ax1.Index);
                if (!test) disjoint += 1;
            }

            //determins the size of the larger network
            int size = Math.Max(this.NumAxons, genome.NumAxons);
            //size = (size > 20) ? size - 20 : 1;

            //couputes the distance for specisation
            double dist = (C0 * disjoint) / (double)size;
            dist += C1 * (wbar / (double)match);

            return dist;
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
            Nuron n1, n2;

            //generates a pair of random nurons on diffrent levels
            bool pass = GetRandomPair(rng, out n1, out n2);
            if (!pass) return;

            //determins if this should be a recurent conneciton
            bool testrec = recurent;
            testrec = testrec && !n1.IsInput;
            testrec = testrec && rng.RandBool(P_Recur);

            if (testrec)
            {
                Nuron nx = n1;
                n1 = n2;
                n2 = nx;
            }

            //creates a temporary axon between the two nurons
            double w = rng.RandGauss() * SD_NEW;
            Axon temp = new Axon(n1.Index, n2.Index, w);

            //atempts to gain the actual axon if it exists
            Axon actual = axons.GetValue(temp.Index);

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

                //sets the level of the new node in the middle
                if (Math.Abs(n1.Level - n2.Level) < 3) return;
                int level = (n1.Level + n2.Level) / 2; 

                //adds a new node to the network
                ActFunc act = GetRandomActivation(rng);
                Nuron n3 = new Nuron(this, act, nid1, level);
                nurons.Add(n3.Index, n3);

                //inserts two new axons where the old one used to be
                int nid0 = actual.Source;
                int nid2 = actual.Target;

                //Axon ax1 = new Axon(nid0, nid1, actual.Weight);
                //Axon ax2 = new Axon(nid1, nid2, temp.Weight);

                Axon ax1 = new Axon(nid0, nid1, actual.Weight);
                Axon ax2 = new Axon(nid1, nid2, 1.0);

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

        private Axon GetRandomAxon(VRandom rng)
        {
            //uses the RNG to select a random axon
            var list = axons.ListItems();
            return rng.RandElement(list);
        }

        private ActFunc GetRandomActivation(VRandom rng)
        {
            //generates a random nuber to select the fuciton
            int test = rng.RandInt(7);

            switch (test)
            {
                case 0: return ActFunc.Identity;
                case 1: return ActFunc.Sine;
                case 2: return ActFunc.Cosine;
                case 3: return ActFunc.Gaussian;
                case 4: return ActFunc.Sigmoid;
                case 5: return ActFunc.Sinc;
                case 6: return ActFunc.SoftPlus;
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

        /// <summary>
        /// Helper mehtod: Selects a pair of nurons at random from diffrent 
        /// levels of the network. The nurons are always listed in order. It
        /// returns false if it was ubale to generate a valid pair.
        /// </summary>
        /// <param name="rng">A random number generator</param>
        /// <param name="n1">The lower level nuron</param>
        /// <param name="n2">The upper level nuron</param>
        /// <returns>True if a vaild pair was generated</returns>
        private bool GetRandomPair(VRandom rng, out Nuron n1, out Nuron n2)
        {
            //generates two random nurons
            n1 = GetRandomNuron(rng);
            n2 = GetRandomNuron(rng);

            int count = 0;
            Nuron n3 = null;

            //keeps searching while the nurons are on the same level
            while (n1.Level == n2.Level && count < MAX_TRY)
            {
                n3 = GetRandomNuron(rng);
                n1 = n2;
                n2 = n3;
                count++;
            }

            //swaps the nurons if they are out of order
            if (n1.Level > n2.Level)
            {
                n3 = n1;
                n1 = n2;
                n2 = n3;
            }

            //indicates if we found a valid pair
            return (n1.Level != n2.Level);
        }

        #endregion /////////////////////////////////////////////////////////////////////////



    }
}

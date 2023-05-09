using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace Vulpine.Core.AI.Nural
{
    public class NetworkMeta : Network<NetworkMeta>
    {
        //stores the conectivity matrix
        private Matrix con_matrix;

        //stores the value of each node in a vector
        private Vector nurons;

        private int num_inputs;
        private int num_ouputs;
        private int num_hidden;

        public NetworkMeta(int inputs, int outputs, int hidden)
        {
            num_inputs = inputs;
            num_ouputs = outputs;
            num_hidden = hidden;

            int total = inputs + outputs + hidden;
            con_matrix = new Matrix(total, total);
            nurons = new Vector(total);
        }

        public NetworkMeta(string file)
        {
            throw new NotImplementedException();
        }


        public int InSize
        {
            get { return num_inputs; }
        }

        public int OutSize
        {
            get { return num_ouputs; }
        }

        public int NumNurons
        {
            get { return nurons.Length; }
        }

        public int NumAxons
        {
            get { return con_matrix.NumCells / 2; }
        }

        public void WriteToFile(string file)
        {
            StreamWriter sw = new StreamWriter(file);
            sw.WriteLine("FoxieIO Meta-Adaptive Network, v1.0.0");
            sw.WriteLine("-----");

            for (int i = 0; i < con_matrix.NumRows; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    double value = con_matrix[i, j];
                    sw.WriteLine(value.ToString("G20"));
                }
            }

            sw.Flush();
            sw.Close();
        }

        public void SetInput(Vector data)
        {
            for (int i = 0; i < num_inputs; i++)
            {
                double value = data.GetExtended(i);
                //if (i >= nurons.Length) break;
                nurons.SetElement(i, value);
            }
        }

        public void Propergate()
        {
            double input, weight, total, act;
            Vector next = new Vector(nurons.Length);

            //updates all the neurons for the next itteration
            for (int i = num_inputs; i < nurons.Length; i++)
            {
                ////skips over the input nodes
                //if (i < num_inputs) continue;

                total = 0.0;

                //sums up the weighted inputs
                for (int j = 0; j < i; j++)
                {
                    input = nurons[j];
                    weight = con_matrix[i, j];
                    total += input * weight;

                }

                //applies an arbitray activation function based on positon
                switch ((i - num_inputs) % 6)
                {
                    case 1:
                        act = Math.Sin(total); break;
                    case 2:
                        act = Math.Cos(total); break;
                    case 3:
                        act = VMath.Gauss(total); break;
                    case 4:
                        act = Math.Tanh(total); break;
                    case 5:
                        act = VMath.Sinc(total); break;
                    default:
                        act = total; break;
                }

                //outputs nodes are set to pass-through
                if (i > nurons.Length - num_ouputs) act = total;

                next.SetElement(i, act);
            }

            //overwrites the nurons with the next set of values
            for (int i = num_inputs; i < nurons.Length; i++)
                nurons[i] = next[i];
        }

        public Vector ReadOutput()
        {
            Vector output = new Vector(num_ouputs);
            int offset = nurons.Length - num_ouputs;

            for (int i = 0; i < num_ouputs; i++)
            {
                double value = nurons[i + offset];
                output[i] = value;
            }

            return output;
        }

        public void WriteOutputTo(Vector result)
        {
            int offset = nurons.Length - num_ouputs;

            for (int i = 0; i < num_ouputs; i++)
            {
                double value = nurons[i + offset];
                if (i >= result.Length) break;
                result[i] = value;
            }
        }

        public void ResetNetwork()
        {
            for (int i = 0; i < nurons.Length; i++)
            {
                nurons.SetElement(i, 0.0);
            }
        }

        public void Lerp(NetworkMeta genome, double amount)
        {
            throw new NotImplementedException();
        }

        public void Overwrite(NetworkMeta genome)
        {
            throw new NotImplementedException();
        }

        public void Mutate(Calc.RandGen.VRandom rng, double rate)
        {
            //For Now: Simply re-randomize the network. We should apply
            //more subtle changes if we actualy want to use genetic-algorytims

            this.Randomize(rng);
        }

        public void Crossover(Calc.RandGen.VRandom rng, NetworkMeta genome)
        {
            throw new NotImplementedException();
        }

        public double Compare(NetworkMeta genome)
        {
            throw new NotImplementedException();
        }

        public void Randomize(VRandom rng)
        {
            //fills the matrix with random data
            for (int i = 0; i < con_matrix.NumRows; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    con_matrix[i, j] = rng.RandGauss(0.0, 0.5);
                }
            }


            int min = num_inputs;
            int max = con_matrix.NumRows - num_ouputs;
            int total = max - min;
            int tries = (int)(total * 0.75);

            //disables a certain number of nodes by setting all the inputs to zero
            for (int k = 0; k < tries; k++)
            {
                int index = rng.RandInt(min, max);

                for (int j = 0; j < index; j++)
                {
                    con_matrix[index, j] = 0.0;
                }
            }
        }

        public NetworkMeta Clone()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //Do Nothing
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Nural;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace ArtEvolver
{
    /// <summary>
    /// The idea behind this class is to create a newtork that can sample from multiple pre-existing
    /// textures, to render a brand new image. To do this it will (for every point in the output image)
    /// produce an (x, y) cordinate to sample from the internal images. It will also produce a set of
    /// selection nodes, which will interpolate between the various textures. The textures can be thought
    /// of as occupyling points on a hyppercube, so n selector nodes can interpolate between 2^n diffrent
    /// images. Furthermore, this is done twice, such that the output of one image can influence the input
    /// to another image.
    /// </summary>
    public class CombCPPN_Multi : Genetic<CombCPPN_Multi>, Texture
    {
        private const int DEPTH = 32;

        

        //Net1: [x, y, d, 1] -> [xp, yp, s1 ... sn, r1 ... rn]
        //Net2: [y1, u1, v1, r1 ... rn] -> [xp, yp, s1 ... sn, r1 ... rn]
        //Net3: [y2, u2, v2, r1 ... rn] -> [yp, up, vp] 

        private NetworkAuto net1;
        private NetworkAuto net2;
        //private NetworkAuto net3;

        private Texture texture;

        private Texture[] textures;

        //stores the x, y cordinates for the current texture
        private double x;
        private double y;

        private int sample_mode;

        private int n1, n2, s1, sn; 


        public const int PassThrough = 4;

        public CombCPPN_Multi(int num_tex)
        {
            textures = new Texture[num_tex];
            for (int i = 0; i < textures.Length; i++)
                textures[i] = null;

            sn = (int)Math.Ceiling(VMath.Log2(num_tex));
            s1 = sn * sn;

            n1 = 2 + sn + PassThrough;
            n2 = 3 + PassThrough;
            

            net1 = new NetworkAuto(4, n1, false);
            //net2 = new NetworkAuto(n2, n1, false);
            net2 = new NetworkAuto(n2, 3, false);

            texture = null;
            x = 0.0;
            y = 0.0;
        }

        public CombCPPN_Multi(int num_tex, String nf1, String nf2, String nf3)
        {
            textures = new Texture[num_tex];
            for (int i = 0; i < textures.Length; i++)
                textures[i] = null;

            net1 = new NetworkAuto(nf1);
            net2 = new NetworkAuto(nf2);
            //net3 = new NetworkAuto(nf3);

            texture = null;
            x = 0.0;
            y = 0.0; 
        }

        internal CombCPPN_Multi(Texture tex, NetworkAuto net1, NetworkAuto net2)
        {          
            this.net1 = net1;
            this.net2 = net2;
            texture = tex;
            x = 0.0;
            y = 0.0;
        }

        //public CombCPPN(string file)
        //{
        //    network = new NetworkAuto(file);
        //    x = 0.0;
        //    y = 0.0;
        //}

        //public void WriteFile(string file)
        //{
        //    net1.WriteToFile(file);
        //}

        public void AddTexture(int index, Texture tex)
        {
            textures[index] = tex;
        }

        public void WriteFirst(string file)
        {
            net1.WriteToFile(file);
        }

        public void WriteSecond(string file)
        {
            net2.WriteToFile(file);
        }

        //public void WriteThird(string file)
        //{
        //    net3.WriteToFile(file);
        //}

        public int NumNurons
        {
            get { return net1.NumNurons + net2.NumNurons; } // +net3.NumNurons; }
        }

        public int NumAxons
        {
            get { return net1.NumAxons + net2.NumAxons; } // + net3.NumAxons; }
        }

        public int Mode
        {
            get { return sample_mode; }
            set { sample_mode = value; }
        }


        public void SetPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Color Sample(double u, double v)
        {
            switch (sample_mode % 3)
            {
                case 1: return SampleMap(u, v);
                case 2: return SampleImage(u, v);
                default: return SampleMain(u, v);
            }
        }


        public Color SampleMain(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();
            //net3.ResetNetwork();

            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            //sets the input to the network 1
            Vector input = new Vector(u, v, dist, 1.0);
            net1.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net1.Propergate();

            //obtains the output from net 1
            Vector out1 = net1.ReadOutput();
            double xp = out1[0] * SF;
            double yp = out1[1] * SF;

            
            Color[] samples = new Color[s1];

            for (int i = 0; i < samples.Length; i++)
            {
                if (i >= textures.Length)
                {
                    //missing textures are treated as solid black
                    samples[i] = Color.Black;
                }
                else
                {
                    //samples the texture
                    samples[i] = textures[i].Sample(xp, yp);
                }
            }


            for (int i = 0; i < sn; i++)
            {
                //x = [-1 .. 1] becomes x = [0 .. 1]
                double x = out1[i + 2];
                x = (x + 1.0) * 0.5;
                

                for (int k = 0; k < samples.Length - 1; k += 2)
                {
                    Color c1 = samples[k + 0];
                    Color c2 = samples[k + 1];

                    samples[k / 2] = Color.Lerp(c1, c2, x);
                }

                //Note: The final value is stored in samples[0]
                //Note: We could reduce the K loop by half each time
            }

            Vector in2 = new Vector(net2.InSize);
            Vector luv = samples[0].ToLUV();

            in2[0] = luv[0];
            in2[1] = luv[1];
            in2[2] = luv[2];

            for (int i = 0; i < PassThrough; i++)
            {
                //copies the passthrough values
                in2[3 + i] = out1[2 + sn + i];
            }

            net2.SetInput(in2);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net2.Propergate();

            //obtains the output from net 2
            Vector out2 = net2.ReadOutput();


            double w = (out2[0] + 1.0) * 0.5;
            double a = out2[1] * 0.5;
            double b = out2[2] * 0.5;
            return Color.FromYUV(w, a, b);



            //throw new NotImplementedException();
        }


        public Color SampleMap(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();
            //net3.ResetNetwork();

            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            //sets the input to the network 1
            Vector input = new Vector(u, v, dist, 1.0);
            net1.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net1.Propergate();

            //obtains the output from net 1
            Vector out1 = net1.ReadOutput();
            double xp = out1[0] * SF;
            double yp = out1[1] * SF;

            double s0 = (out1[2] + 1.0) * 0.5;
            double s1 = (out1[3] + 1.0) * 0.5;
            double s2 = (out1[4] + 1.0) * 0.5;

            return Color.FromRGB(s0, s1, s2);

            

            //throw new NotImplementedException();

            
        }


        public Color SampleImage(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();
            //net3.ResetNetwork();

            //computes the distance from the origin
            double dist = (u * u) + (v * v);
            dist = Math.Sqrt(dist);

            //sets the input to the network 1
            Vector input = new Vector(u, v, dist, 1.0);
            net1.SetInput(input);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net1.Propergate();

            //obtains the output from net 1
            Vector out1 = net1.ReadOutput();
            double xp = out1[0] * SF;
            double yp = out1[1] * SF;


            Color[] samples = new Color[s1];

            for (int i = 0; i < samples.Length; i++)
            {
                if (i >= textures.Length)
                {
                    //missing textures are treated as solid black
                    samples[i] = Color.Black;
                }
                else
                {
                    //samples the texture
                    samples[i] = textures[i].Sample(xp, yp);
                }
            }


            for (int i = 0; i < sn; i++)
            {
                //x = [-1 .. 1] becomes x = [0 .. 1]
                double x = out1[i + 2];
                x = (x + 1.0) * 0.5;


                for (int k = 0; k < samples.Length - 1; k += 2)
                {
                    Color c1 = samples[k + 0];
                    Color c2 = samples[k + 1];

                    samples[k / 2] = Color.Lerp(c1, c2, x);
                }

                //Note: The final value is stored in samples[0]
                //Note: We could reduce the K loop by half each time
            }

            return samples[0];



            //throw new NotImplementedException();
        }


        private const double SF = 0.5;


        //public Color SampleBoth(double u, double v)
        //{
        //    //clears the network for each sample
        //    net1.ResetNetwork();
        //    net2.ResetNetwork();

        //    //computes the distance from the origin
        //    double dist = (u * u) + (v * v);
        //    dist = Math.Sqrt(dist);

        //    //sets the input to the network 1
        //    Vector input = new Vector(u, v, dist, 1.0);
        //    net1.SetInput(input);

        //    //propagates until the maximum depth
        //    for (int i = 0; i < DEPTH; i++)
        //        net1.Propergate();

        //    //obtains the output from net 1
        //    Vector out1 = net1.ReadOutput();
        //    double xp = out1[0] * SF;
        //    double yp = out1[1] * SF;
        //    double r1 = out1[2];
        //    double r2 = out1[3];

        //    //samples the color data from the texture
        //    Color pix = texture.Sample(xp, yp);
        //    Vector yuv = pix.ToYUV();
        //    double yi = (yuv[0] - 0.5) * 2.0;
        //    double ai = yuv[0] * 2.0;
        //    double bi = yuv[1] * 2.0;

        //    //sets the input to the network 2
        //    Vector input2 = new Vector(yi, ai, bi, r1, r2, 1.0);
        //    net2.SetInput(input2);

        //    //propagates until the maximum depth
        //    for (int i = 0; i < DEPTH; i++)
        //        net2.Propergate();


        //    //constructs the color from the output
        //    Vector output2 = net2.ReadOutput();
        //    //double y = VMath.Sigmoid(output[0]);
        //    //double a = Math.Tanh(output[1]) * 0.5;
        //    //double b = Math.Tanh(output[2]) * 0.5;
        //    double w = (output2[0] + 1.0) * 0.5;
        //    double a = output2[1] * 0.5;
        //    double b = output2[2] * 0.5;
        //    return Color.FromYUV(w, a, b);
        //}





        public void Overwrite(CombCPPN_Multi genome)
        {
            net1.Overwrite(genome.net1);
            net2.Overwrite(genome.net2);
        }

        public void Mutate(VRandom rng, double rate)
        {
            net1.Mutate(rng, rate);
            net2.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, CombCPPN_Multi genome)
        {
            net1.Crossover(rng, genome.net1);
            net2.Crossover(rng, genome.net2);
        }

        public double Compare(CombCPPN_Multi genome)
        {
            double d1 = net1.Compare(genome.net1);
            double d2 = net2.Compare(genome.net2);

            return d1 + d2;
        }

        public void Lerp(CombCPPN_Multi genome, double amount)
        {
            net1.Lerp(genome.net1, amount);
            net2.Lerp(genome.net2, amount);
        }

        public void Lerp(CombCPPN_Multi start, CombCPPN_Multi target, double amount)
        {
            //overwrites both networks with the starting configurations
            net1.Overwrite(start.net1);
            net2.Overwrite(start.net2);

            //preforms the interpolation with the target networks
            net1.Lerp(target.net1, amount);
            net2.Lerp(target.net2, amount);
        }

        //public CPPN SpawnRandom(VRandom rng)
        //{
        //    //throw new NotImplementedException();

        //    var next = network.SpawnRandom(rng);
        //    return new CPPN(next);
        //}

        public void Randomize(VRandom rng)
        {
            net1.Randomize(rng);
            net2.Randomize(rng);
        }

        public CombCPPN_Multi Clone()
        {
            //NOTE: Should I clone the texture as well???

            NetworkAuto c1 = net1.Clone();
            NetworkAuto c2 = net2.Clone();

            return new CombCPPN_Multi(texture, c1, c2);
        }

        public void Dispose()
        {
            net1.Dispose();
            net2.Dispose();
        }

    }
}

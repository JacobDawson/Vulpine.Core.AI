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
    public class CombCPPN_Multi3 : Genetic<CombCPPN_Multi3>, Texture
    {
        private const int DEPTH = 8; //32;



        private NetworkAuto net1;
        private NetworkAuto net2;

        private Texture texture;

        private Texture[] textures;

        //stores the x, y cordinates for the current texture
        private double x;
        private double y;

        private int sample_mode;

        private int n1, n2; // s1, sn; 


        private Vector selector;


        public const int PassThrough = 4;

        public CombCPPN_Multi3(int num_tex)
        {
            textures = new Texture[num_tex];
            for (int i = 0; i < textures.Length; i++)
                textures[i] = null;

            //sn = (int)Math.Ceiling(VMath.Log2(num_tex));
            //s1 = sn * sn;

            n1 = (num_tex * 2) + PassThrough;
            n2 = (num_tex * 3) + PassThrough;

            selector = new Vector(num_tex);
            

            net1 = new NetworkAuto(4, n1, false);
            net2 = new NetworkAuto(n2, 3, false);

            texture = null;
            x = 0.0;
            y = 0.0;
        }

        public CombCPPN_Multi3(int num_tex, String nf1, String nf2, String nf3)
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

        internal CombCPPN_Multi3(Texture tex, NetworkAuto net1, NetworkAuto net2)
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

        public void SetSelector(Vector sel)
        {
            for (int i = 0; i < selector.Length; i++)
            {
                double val = sel.GetExtended(i);
                selector.SetElement(i, val);
            }
        }


        public void SetPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Color Sample(double u, double v)
        {
            //switch (sample_mode % 3)
            //{
            //    case 1: return SampleMap(u, v);
            //    case 2: return SampleImage(u, v);
            //    default: return SampleMain(u, v);
            //}

            return SampleMain(u, v);
        }


        public Color SampleMain(double u, double v)
        {
            //clears the network for each sample
            net1.ResetNetwork();
            net2.ResetNetwork();    //<<-- Try Toggeling This
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
            Vector in2 = new Vector(net2.InSize);
            //double xp = out1[0] * SF;
            //double yp = out1[1] * SF;


            //reads all the textures, sampled at their own points, and scales the
            //results by the selector vector.
            for (int i = 0; i < textures.Length; i++)
            {
                int k = i * 2;

                double xp = out1[k + 0];
                double yp = out1[k + 1];

                Color sample = textures[i].Sample(xp, yp);
                Vector yuv = sample.ToYUV() * selector[i];

                int j = i * 3;

                in2[j + 0] = yuv[0];
                in2[j + 1] = yuv[1];
                in2[j + 2] = yuv[2];
            }

            int sk = textures.Length * 2;
            int sj = textures.Length * 3;

            for (int i = 0; i < PassThrough; i++)
            {
                //copies the passthrough values
                in2[sj + i] = out1[sk + i];
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





        public void Overwrite(CombCPPN_Multi3 genome)
        {
            net1.Overwrite(genome.net1);
            net2.Overwrite(genome.net2);

            //we must copy the selector vector as well
            selector = new Vector(genome.selector);
        }

        public void Mutate(VRandom rng, double rate)
        {
            net1.Mutate(rng, rate);
            net2.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, CombCPPN_Multi3 genome)
        {
            net1.Crossover(rng, genome.net1);
            net2.Crossover(rng, genome.net2);
        }

        public double Compare(CombCPPN_Multi3 genome)
        {
            double d1 = net1.Compare(genome.net1);
            double d2 = net2.Compare(genome.net2);

            return d1 + d2;
        }

        public void Lerp(CombCPPN_Multi3 genome, double x)
        {
            net1.Lerp(genome.net1, x);
            net2.Lerp(genome.net2, x);

            //we must interpolate the slector vector as well
            selector = (selector * (1.0 - x)) + (genome.selector * x);
        }

        public void Lerp(CombCPPN_Multi3 start, CombCPPN_Multi3 target, double x)
        {
            //overwrites both networks with the starting configurations
            net1.Overwrite(start.net1);
            net2.Overwrite(start.net2);

            //preforms the interpolation with the target networks
            net1.Lerp(target.net1, x);
            net2.Lerp(target.net2, x);

            //we must interpolate the slector vector as well
            selector = (start.selector * (1.0 - x)) + (target.selector * x);
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

        public CombCPPN_Multi3 Clone()
        {
            //NOTE: Should I clone the texture as well???

            NetworkAuto c1 = net1.Clone();
            NetworkAuto c2 = net2.Clone();

            return new CombCPPN_Multi3(texture, c1, c2);
        }

        public void Dispose()
        {
            net1.Dispose();
            net2.Dispose();
        }

    }
}

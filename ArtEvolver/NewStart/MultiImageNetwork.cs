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

namespace ArtEvolver.NewStart
{
    /// <summary>
    /// This network is designed to sample from multiple images (up to 16) in order to generate
    /// random output. 
    /// </summary>
    public class MultiImageNetwork : Genetic<MultiImageNetwork>, Texture
    {

        #region Class Definitions...

        //Representation of the Network Structure:

        //Net1: [x, y, d, 1] -> [u, v, s0, s1, s2, s3, r0, r1]
        //Net2: [c_y, c_u, c_v, r0, r1, 1] -> [out_y, out_u, out_v]

        //The amount of times to propigate each netowrk (Default 32)
        private const int DEPTH = 32;

        //The scale factor for the sampled images (Default 0.5)
        private const double ScaleF = 0.75;

        //This value should determin sharpness of the boundries between each image
        //The larger this number is the sharper the boundries should become
        //A value of 1.0 results in continious interpolation
        private const double IntF = 4.0;


        private bool first_only = false;

        private ImageCube imgcube;

        private NetworkAuto net1;
        private NetworkAuto net2;

        /// <summary>
        /// Creates a blank Multi-Image Network from a given Image Cube
        /// </summary>
        /// <param name="source">An Image Cube to sample from</param>
        public MultiImageNetwork(ImageCube source)
        {
            //simply makes a refrence copy of the source
            imgcube = source;

            //NOTE: we should probably try to clone the source, or else make
            //sure that ImageCube is immutable, but this is good enough for now

            //hard codes the structure of the networks
            net1 = new NetworkAuto(4, 8, false);
            net2 = new NetworkAuto(6, 3, false);
        }

        /// <summary>
        /// Loads an existing Multi-Image Network from a pair of files.
        /// </summary>
        /// <param name="source">An Image Cube to sample from</param>
        /// <param name="nf1">File source for the 1st network</param>
        /// <param name="nf2">File source for the 2nd network</param>
        public MultiImageNetwork(ImageCube source, String nf1, String nf2)
        {
            //simply makes a refrence copy of the source
            imgcube = source;

            //reades the networks in from a file
            net1 = new NetworkAuto(nf1);
            net2 = new NetworkAuto(nf2);

            //NOTE: we should probably check that the input networks actualy have the
            //structure that we expect, but we can skip that step for now.
        }

        internal MultiImageNetwork(ImageCube source, NetworkAuto net1, NetworkAuto net2)
        {          
            this.net1 = net1;
            this.net2 = net2;
            imgcube = source;
        }

        /// <summary>
        /// Writes the first half of the network out to a file
        /// </summary>
        /// <param name="file">Output file destination</param>
        public void WriteFirst(string file)
        {
            net1.WriteToFile(file);
        }

        /// <summary>
        /// Writes the second half of the network out to a file
        /// </summary>
        /// <param name="file">Output file destination</param>
        public void WriteSecond(string file)
        {
            net2.WriteToFile(file);
        }

        public int NumNurons
        {
            get { return net1.NumNurons + net2.NumNurons; } 
        }

        public int NumAxons
        {
            get { return net1.NumAxons + net2.NumAxons; } 
        }

        /// <summary>
        /// Indicated if the sampeling function should sample from only the first
        /// half of the network, or if it should use the entier network.
        /// </summary>
        public bool FirstOnly
        {
            get { return first_only; }
            set { first_only = value; }
        }

        #endregion

        #region Sampeling Functions...

        /// <summary>
        /// This method squashes the input to be within the range [-1 .. 1]
        /// </summary>
        /// <param name="x">Any Real Value</param>
        /// <returns>A value in the range [-1 .. 1]</returns>
        private double Squash(double x)
        {
            //NOTE: The network already takes care of squashing the output for us, so
            //we simply return the value unaltered. If that were not the case, we would
            //need to handel the squashing here.

            return x;
        }

        /// <summary>
        /// Samples a collor from the first half of the network
        /// </summary>
        /// <param name="u">U Sampeling cordinate</param>
        /// <param name="v">V Sampeling cordinate</param>
        /// <param name="r0">The 0th pass-through paramater</param>
        /// <param name="r1">The 1st pass-through paramater</param>
        /// <returns>The sampled color</returns>
        private Color SampleFirst(double u, double v, out double r0, out double r1)
        {
            //clears the network for each sample
            net1.ResetNetwork();

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
            //double xp = out1[0] * ScaleF;
            //double yp = out1[1] * ScaleF;
            //double s0 = out1[2];
            //double s1 = out1[3];
            //double s2 = out1[4];
            //double s3 = out1[5];

            //extracts the sampeling point, squashed and scaled
            double xp = Squash(out1[0]) * ScaleF;
            double yp = Squash(out1[1]) * ScaleF;

            //extracts the interpolation point, squashed and scaled
            double s0 = Squash(out1[3]) * IntF;
            double s1 = Squash(out1[4]) * IntF;
            double s2 = Squash(out1[5]) * IntF;
            double s3 = Squash(out1[6]) * IntF;

            //copies over the recurnet layers for later use
            r0 = out1[6];
            r1 = out1[7];

            //Samples from the 6-dimentional Image Cube
            Color sample = imgcube.Sample(xp, yp, s0, s1, s2, s3);

            return sample;
        }

        /// <summary>
        /// Samples from the entiere network
        /// </summary>
        /// <param name="u">U Sampeling cordinate</param>
        /// <param name="v">V Sampeling cordinate</param>
        /// <returns>The sampled color</returns>
        private Color SampleAll(double u, double v)
        {
            double r0, r1;

            //calles the SampleFirst method in order to propigate the first half       
            Color sample = SampleFirst(u, v, out r0, out r1);

            //converts the color space from RGB to YUV
            Vector yuv = sample.ToYUV();

            //constructs the imput vector for the second network
            Vector in2 = new Vector(
                yuv[0], 
                yuv[1], 
                yuv[2],
                r0, 
                r1, 
                1.0
            );

            net2.ResetNetwork();
            net2.SetInput(in2);

            //propagates until the maximum depth
            for (int i = 0; i < DEPTH; i++)
                net2.Propergate();

            //obtains the output from net 2
            Vector out2 = net2.ReadOutput();
            double w = Squash(out2[0]);
            double a = Squash(out2[1]);
            double b = Squash(out2[2]);

            w = (w + 1.0) * 0.5;
            a = a * 0.5;
            b = b * 0.5;

            return Color.FromYUV(w, a, b);
        }

        public Color Sample(double u, double v)
        {
            if (first_only)
            {
                //samples from only the first half of the network
                double r0, r1;
                return SampleFirst(u, v, out r0, out r1);
            }
            else
            {
                //samples from the entire network
                return SampleAll(u, v);
            }
        }

        #endregion

        #region Nessary Interface Implementation...


        public void Overwrite(MultiImageNetwork genome)
        {
            net1.Overwrite(genome.net1);
            net2.Overwrite(genome.net2);
        }

        public void Mutate(VRandom rng, double rate)
        {
            net1.Mutate(rng, rate);
            net2.Mutate(rng, rate);
        }

        public void Crossover(VRandom rng, MultiImageNetwork genome)
        {
            net1.Crossover(rng, genome.net1);
            net2.Crossover(rng, genome.net2);
        }

        public double Compare(MultiImageNetwork genome)
        {
            double d1 = net1.Compare(genome.net1);
            double d2 = net2.Compare(genome.net2);

            return d1 + d2;
        }

        public void Lerp(MultiImageNetwork genome, double amount)
        {
            net1.Lerp(genome.net1, amount);
            net2.Lerp(genome.net2, amount);
        }

        public void Lerp(MultiImageNetwork start, MultiImageNetwork target, double amount)
        {
            //overwrites both networks with the starting configurations
            net1.Overwrite(start.net1);
            net2.Overwrite(start.net2);

            //preforms the interpolation with the target networks
            net1.Lerp(target.net1, amount);
            net2.Lerp(target.net2, amount);
        }

        public void Randomize(VRandom rng)
        {
            net1.Randomize(rng);
            net2.Randomize(rng);
        }

        public MultiImageNetwork Clone()
        {
            //NOTE: Should I clone the texture as well???

            NetworkAuto c1 = net1.Clone();
            NetworkAuto c2 = net2.Clone();

            return new MultiImageNetwork(imgcube, c1, c2);
        }

        public void Dispose()
        {
            net1.Dispose();
            net2.Dispose();
        }

        #endregion
    }
}

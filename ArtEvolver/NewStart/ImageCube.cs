using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Draw;
using Vulpine.Core.Draw.Textures;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.Matrices;

namespace ArtEvolver.NewStart
{
    /// <summary>
    /// The purpous of this class is to create a space from which multiple images can be sampled.
    /// It works by assinging an image to each corner of a hyper-cube, and then interpolating
    /// the images based on where the input point lies relitive to the hypercube. Points inside
    /// the hypercube will be an interpolation of all images, while points outside will interpolate
    /// only some of the images. Most points in the space will result in selecting a single image.
    /// The hypercube is centered at the origin and has a radius of 1.
    /// </summary>
    public class ImageCube
    {
        //indicates which interpolation method should be prefered when sampling textures
        private const Intpol PreferdIntpol = Intpol.BiLiniar;


        private const int NumTex = 16; //8;
        private const int Dim = 4;

        private Texture[] textures;

        private ImageCube()
        {
            textures = new Texture[NumTex];
        }

        /// <summary>
        /// Constructs an Image Cube from a list of textures
        /// </summary>
        /// <param name="source">Source lIst of texture</param>
        /// <returns>The constructed Image Cube</returns>
        public static ImageCube CreateFromTex(params Texture[] source)
        {
            ImageCube imgcube = new ImageCube();

            for (int i = 0; i < NumTex; i++)
            {
                if (i >= source.Length)
                {
                    //simply sets the texture source to "null"
                    imgcube.textures[i] = null;
                }
                else
                {
                    //simply copys the refrence to the texture
                    imgcube.textures[i] = source[i];

                    //NOTE: we should probably do something more secure here
                    //like actualy copy the texture somehow, but this is good
                    //enough for now.
                }
            }

            return imgcube;
        }

        /// <summary>
        /// Constructs an Image Cube from a list of image files
        /// </summary>
        /// <param name="files">List of image files</param>
        /// <returns>The constructed Image Cube</returns>
        public static ImageCube CreateFromFiles(params string[] files)
        {
            ImageCube imgcube = new ImageCube();

            for (int i = 0; i < NumTex; i++)
            {
                if (i >= files.Length)
                {
                    //simply sets the texture source to "null"
                    imgcube.textures[i] = null;
                    continue;
                }

                var bmp = new System.Drawing.Bitmap(files[i]);
                ImageSys img = new ImageSys(bmp);
                Texture txt = new Interpolent(img, Intpol.BiLiniar);

                imgcube.textures[i] = txt;

            }

            return imgcube;
        }



        /// <summary>
        /// Samples the image cube at the given cordinates
        /// </summary>
        /// <param name="u">U cordinate into the image</param>
        /// <param name="v">V cordinate into the image</param>
        /// <param name="s0">0th cordinate into the interpolation space</param>
        /// <param name="s1">1st cordinate into the interpolation space</param>
        /// <param name="s2">2nd cordinate into the interpolation space</param>
        /// <param name="s3">3rd cordinate into the interpolation space</param>
        /// <returns>The sampled collor</returns>
        public Color Sample(double u, double v, double s0, double s1, double s2, double s3)
        {
            Color[] samples = new Color[NumTex];
            Double[] selector = new Double[Dim];

            selector[0] = s0;
            selector[1] = s1;
            selector[2] = s2;
            selector[3] = s3;

            //samples an image at each corner of the hypercube
            for (int i = 0; i < NumTex; i++)
            {
                if (i >= textures.Length)
                {
                    //missing textures are treated as solid black
                    samples[i] = Color.Black;
                }
                else if (textures[i] == null)
                {
                    //missing textures are treated as solid black
                    samples[i] = Color.Black;
                }
                else
                {
                    //samples the texture
                    samples[i] = textures[i].Sample(u, v);
                }
            }

            //interpolates the samples as nessary, given the selection vector
            for (int i = 0; i < Dim; i++)
            {
                //converts any imput to be in range [0 .. 1]
                double x = selector[i];
                x = ClipValue(x);

                for (int k = 0; k < samples.Length - 1; k += 2)
                {
                    Color c1 = samples[k + 0];
                    Color c2 = samples[k + 1];

                    samples[k / 2] = Color.Lerp(c1, c2, x);
                }

                //Note: The final value is stored in samples[0]
                //Note: We could reduce the K loop by half each time
            }

            //returns the final value
            return samples[0];
        }


        /// <summary>
        /// Maps values in the range of [-1 .. 1] to be in the range of [0 .. 1], values
        /// outside that range are cliped to be 0 or 1, respetivly.
        /// </summary>
        /// <param name="x">Any numeric value</param>
        /// <returns>A value in the range [0 .. 1]</returns>
        private static double ClipValue(double x)
        {
            //clips the outside range
            if (x < -1.0) return 0.0;
            if (x > 1.0) return 1.0;

            //scales the value to be [0 .. 1]
            return (x + 1.0) / 2.0;
        }

    }
}

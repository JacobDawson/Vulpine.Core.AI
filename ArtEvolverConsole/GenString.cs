using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.AI;
using Vulpine.Core.AI.Genetics;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace ArtEvolverConsole
{
    public class GenString : Genetic<GenString>
    {
        public const double P_Insert = 0.5;
        public const double P_Remove = 0.5;
        public const double P_Swap = 0.5;

        private string mystr;

        public GenString(string s)
        {
            mystr = s;
        }

        public override string ToString()
        {
            return "\"" + mystr + "\"";
        }

        /// <summary>
        /// Computes the Levenshtein distance metric between two strings, also known
        /// as the Edit Distance. It tries to do this in a space effecent way, by
        /// only storing two rows of the Levenshtein matrix at a time. The distance
        /// is defined to be the minimum number of edits (insertions, deletions,
        /// or replacements) required to transform one string into the other.
        /// </summary>
        /// <param name="other">A string to messure the distance between</param>
        /// <returns>The Levenshtein Distance</returns>
        public int Distance(GenString other)
        {
            //obtains the strings for comparison
            string s1 = this.mystr;
            string s2 = other.mystr;

            //enshures that s1 is the shorter of the two strings
            if (s1.Length > s2.Length)
            {
                string temp = s1;
                s1 = s2;
                s2 = temp;
            }

            //creates only two rows of the Levenshtein matrix
            int[] v0 = new int[s1.Length + 1]; //(j)
            int[] v1 = new int[s1.Length + 1]; //(j + 1)

            for (int i = 0; i < v1.Length; i++)
            {
                v0[i] = i;
                v1[i] = 0;
            }

            for (int j = 0; j < s2.Length; j++)
            {
                //sets the first entry of the new row
                v1[0] = j + 1;

                for (int i = 0; i < s1.Length; i++)
                {
                    //compares the cost of insertion, deletion, and replacement
                    int val1 = v0[i + 1] + 1;
                    int val2 = v1[i] + 1;
                    int val3 = v0[i];

                    if (s1[i] != s2[j]) val3 += 1;

                    //applies the minimum of the possable edits
                    val1 = Math.Min(val1, val2);
                    val1 = Math.Min(val1, val3);
                    v1[i + 1] = val1;
                }

                for (int i = 0; i < v1.Length; i++)
                {
                    //copies v1 into v0 for next itteraiton
                    v0[i] = v1[i];
                }
            }

            //the last element in v1 is the edit distance
            return v1[v1.Length - 1];
        }

        public double Distance2(GenString other)
        {
            //obtains the strings for comparison
            string s1 = this.mystr;
            string s2 = other.mystr;

            //enshures that s1 is the shorter of the two strings
            if (s1.Length > s2.Length)
            {
                string temp = s1;
                s1 = s2;
                s2 = temp;
            }

            //creates only two rows of the Levenshtein matrix
            double[] v0 = new double[s1.Length + 1]; //(j)
            double[] v1 = new double[s1.Length + 1]; //(j + 1)

            for (int i = 0; i < v1.Length; i++)
            {
                v0[i] = i;
                v1[i] = 0;
            }

            for (int j = 0; j < s2.Length; j++)
            {
                //sets the first entry of the new row
                v1[0] = j + 1;

                for (int i = 0; i < s1.Length; i++)
                {
                    //compares the cost of insertion, deletion, and replacement
                    double val1 = v0[i + 1] + 1;
                    double val2 = v1[i] + 1;
                    double val3 = v0[i];

                    //if (s1[i] != s2[j]) val3 += 1;

                    if (s1[i] != s2[j])
                    {
                        int d = (int)s1[i] - (int)s2[j];
                        val3 += 0.5 + Math.Abs(d / 128.0);
                    }

                    //applies the minimum of the possable edits
                    val1 = Math.Min(val1, val2);
                    val1 = Math.Min(val1, val3);
                    v1[i + 1] = val1;
                }

                for (int i = 0; i < v1.Length; i++)
                {
                    //copies v1 into v0 for next itteraiton
                    v0[i] = v1[i];
                }
            }

            //the last element in v1 is the edit distance
            return v1[v1.Length - 1];
        }



        #region Genetic Implementation...


        public void Overwrite(GenString genome)
        {
            //because strings are immutable, we can just copy the refferece
            this.mystr = genome.mystr;
        }

        public void Mutate(VRandom rng, double rate)
        {
            if (rng.RandBool(P_Insert * rate))
            {
                int index = rng.RandInt(1, mystr.Length);
                int len = mystr.Length;

                //splits the string at the instertion point
                string s1 = mystr.Substring(0, index);
                string s2 = mystr.Substring(index, len - index);

                //selects a random character in the ASCII range
                int c = rng.RandInt(32, 127);
                mystr = s1 + (char)c + s2;
            }

            //converts the string to a char array
            char[] temp = mystr.ToCharArray();

            if (rng.RandBool(P_Swap * rate))
            {
                //select two random indicies
                int i = rng.RandInt(0, temp.Length);
                int j = rng.RandInt(0, temp.Length);

                //swaps the characters at the two incicies
                char c = temp[i];
                temp[i] = temp[j];
                temp[j] = c;
            }

            for (int i = 0; i < temp.Length; i++)
            {
                //skips over characters with the inverse rate
                if (!rng.RandBool(rate)) continue;

                //selects a random character in the ASCII range
                int c = rng.RandInt(32, 127);
                temp[i] = (char)c;
            }

            //sets the new mutated string
            mystr = new String(temp);
        }

        public void Crossover(VRandom rng, GenString genome)
        {
            int len1 = this.mystr.Length;
            int len2 = genome.mystr.Length;

            int min = Math.Min(len1, len2);
            int cp = rng.RandInt(-min, min);

            string s1, s2;

            if (cp < 0)
            {
                //corrects for the negative index
                cp = min + cp;

                s1 = genome.mystr.Substring(0, cp);
                s2 = this.mystr.Substring(cp, len1 - cp);
            }
            else
            {
                s1 = this.mystr.Substring(0, cp);
                s2 = genome.mystr.Substring(cp, len2 - cp);
            }

            //concatinates the two substrings
            mystr = s1 + s2;
        }

        public double Compare(GenString genome)
        {
            ////we compute the Levenshtein distance
            //return Distance(genome);

            //computes the levenshtein distance between the string
            double dist = Distance(genome);
            int s1 = this.mystr.Length;
            int s2 = genome.mystr.Length;

            //normalizes the distance based on the longer string
            s2 = Math.Max(s1, s2);
            return dist / (double)s2;
        }

        public void Randomize(VRandom rng)
        {
            char[] temp = new char[mystr.Length];

            for (int i = 0; i < temp.Length; i++)
            {
                //selects a random character in the ASCII range
                int c = rng.RandInt(32, 127);
                temp[i] = (char)c;
            }

            //sets the new random string
            mystr = new String(temp);
        }

        public GenString Clone()
        {
            //again, we can use the referece becasue strings are immutable
            return new GenString(mystr);
        }

        public void Dispose()
        {
            //nothing to do here...
        }

        #endregion ///////////////////////////////////////////////////////////////////
    }
}

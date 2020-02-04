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
    public class GenString2 : Genetic<GenString2>
    {
        public const double P_Insert = 0.5;
        public const double P_Remove = 0.5;
        public const double P_Swap = 0.5;

        private string mystr;

        public GenString2(string s)
        {
            mystr = s;
        }

        public override string ToString()
        {
            return "\"" + mystr + "\"";
        }

        public int Distance(GenString2 other)
        {
            string s1 = this.mystr;
            string s2 = other.mystr;
            int dist = 0;

            if (s1.Length > s2.Length)
            {
                string temp = s1;
                s1 = s2;
                s2 = temp;
            }

            for (int i = 0; i < s2.Length; i++)
            {
                if (i < s1.Length)
                {
                    //adds the diffrence between the two characters
                    int c = (int)s1[i] - (int)s2[i];
                    dist += Math.Abs(c);
                }
                else
                {
                    //adds the interger value of the character
                    int c = (int)s2[i] - 30;
                    dist += (int)s2[i];
                }
            }

            return dist;
        }



        #region Genetic Implementation...


        public void Overwrite(GenString2 genome)
        {
            //because strings are immutable, we can just copy the refferece
            this.mystr = genome.mystr;
        }

        public void Mutate(VRandom rng, double rate)
        {
            if (rng.RandBool(P_Insert * rate))
            {
                //selects a random character in the ASCII range
                int c = rng.RandInt(32, 127);
                mystr = mystr + (char)c;
            }
            else
            {
                //converts the string to a char array
                char[] temp = mystr.ToCharArray();

                //increments or decrements a random character
                int index = rng.RandInt(0, temp.Length);
                bool add = rng.RandBool();
                int c = (int)temp[index] + (add ? 1 : -1);

                //makes shure the resulting character is valid
                if (c < 32) c = 32;
                if (c > 126) c = 126;
                temp[index] = (char)c;

                //sets the new mutated string
                mystr = new String(temp);
            }
        }

        public void Crossover(VRandom rng, GenString2 genome)
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

        public double Compare(GenString2 genome)
        {
            //we compute the Levenshtein distance
            return Distance(genome);
        }

        public void Randomize(VRandom rng)
        {
            char[] temp = new char[16];

            for (int i = 0; i < temp.Length; i++)
            {
                //selects a random character in the ASCII range
                int c = rng.RandInt(32, 127);
                temp[i] = (char)c;
            }

            //sets the new random string
            mystr = new String(temp);
        }

        public GenString2 Clone()
        {
            //again, we can use the referece becasue strings are immutable
            return new GenString2(mystr);
        }

        public void Dispose()
        {
            //nothing to do here...
        }

        #endregion ///////////////////////////////////////////////////////////////////
    }
}

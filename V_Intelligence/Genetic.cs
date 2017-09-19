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

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI
{
    /// <summary>
    /// This interface represents a class of genetic objects, known as genomes, which 
    /// can be evolved to minimise some arbitary fitness funciton. All such genetic 
    /// objects have a genotype, wheather implicit or explicit, that determins its 
    /// phenotype, or how the organism looks and behaves. By copying, mutating, and 
    /// recombining defrent genes a population may evolve to fit any given criteria. 
    /// Note that the fitness funciton itself is not a part of the genome, as a single 
    /// genome may be evolved to match diffrent or even changing criteria. Various 
    /// algorythims exist to evolve genetic sturctors, and it is not nessary for a
    /// genome to implement all geneitc operations. If a genome cannot preform a given 
    /// operation, it should simply clone the genotype instead. 
    /// </summary>
    /// <typeparam name="T">Genome Type</typeparam>
    /// <remarks>Last Update: 2017-09-19</remarks>
    public interface Genetic<T>
    {
        //T Copy();


        /// <summary>
        /// Compares the current genotype to the genotype of a diffrent
        /// organism. The result is a positive real value that indicates
        /// how siimilar the genotypes are. This is tipicaly used to
        /// seperate a population of individules into species.
        /// </summary>
        /// <param name="other">Organism for comparison</param>
        /// <returns>Mesure of similarity</returns>
        double Compare(T other);

        /// <summary>
        /// Clones the current organism with some random mutation of its
        /// genotpye. The rate of mutaiton determins how dissimilar the
        /// clone is from its parent. The exact meaning of this paramater
        /// is determined by the implementing genome.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="rate">Rate of mutation</param>
        /// <returns>A mutated organism</returns>
        T Mutate(VRandom rng, double rate);

        /// <summary>
        /// Combines the genes of the curent organism with the genes of
        /// another organism to create a brand new offspring. The idea is
        /// that the child organism will possess trates from both its
        /// parents, similar to sexual reproduction.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <param name="mate">Mate of the curent organism</param>
        /// <returns>The child of both organisms</returns>
        T Combine(VRandom rng, T mate);

        /// <summary>
        /// Creates a new organism with more genes than its parent. This is 
        /// diffrent from regular mutaiton, as the genotype becomes bigger, 
        /// increasing the search space and opening new opertunites for 
        /// diversification and improvment.
        /// </summary>
        /// <param name="rng">Random number generator</param>
        /// <returns>An organism with an expanded genotype</returns>
        T Expand(VRandom rng);


        //double Fitness(object target);
    }
}

/**
 *  This file is an integral part of the Vulpine Core Library
 *  Copyright (c) 2020 Benjamin Jacob Dawson
 *
 *      http://www.jakesden.com/corelibrary.html
 *
 *  The Vulpine Core Library is free software; you can redistribute it 
 *  and/or modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  The Vulpine Core Library is distributed in the hope that it will 
 *  be useful, but WITHOUT ANY WARRANTY; without even the implied 
 *  warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 *  See the GNU Lesser General Public License for more details.
 *
 *      https://www.gnu.org/licenses/lgpl-2.1.html
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
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
    /// can be evolved to minimise some arbitary fitness funciton. By copying, mutating, 
    /// and recombining defrent genes a population may evolve to fit any given criteria. 
    /// Note that the fitness funciton itself is not a part of the genome, as a single 
    /// genome may be evolved to match diffrent or even changing criteria. Originaly I
    /// wrote these methods to return new genomes. However, given that many genomes will
    /// be generated over the course of evolution, it is more effectent to update the
    /// genomes in place. To combine two genomes for instance, it is nessary to copy
    /// the first genome, then crossover the copy with the second genome.
    /// </summary>
    /// <typeparam name="T">Genotype</typeparam>
    /// <remarks>Last Update: 2020-01-24</remarks>
    public interface Genetic<T> : IDisposable where T : Genetic<T>
    {   
        /// <summary>
        /// This mehtod overrites the current genome with the given genome, producing
        /// an exact copy. This allows genes to be copied from one generation to the
        /// next, without having to reinstanciate the entire gene-pool. As with all
        /// genetic operations, the parent genome is unaffected.
        /// </summary>
        /// <param name="genome">Parent genome to copy</param>
        void Overwrite(T genome);

        /// <summary>
        /// Causes the curent genome to mutate, becoming a new genome that is similar 
        /// to, but distinct from, the original genome. How mutch the genome changes is
        /// determined by the rate of mutation. A rate of zero should produce a near
        /// identical copy, while a rate of one should generate a near random genome.
        /// </summary>
        /// <param name="rng">Random Number Generator used in preforming mutaitons</param>
        /// <param name="rate">The degree to which the new genome should differ
        /// from the original</param>
        void Mutate(VRandom rng, double rate);

        /// <summary>
        /// Overites part of the current genome with the given genome, but not all,
        /// producing a new genome that is a hybrid of the original genome and the
        /// given genome. Note that this dose not nessarily need to be a 50-50 split.
        /// As with all genetic operaitons, the parent genome is unaffected.
        /// </summary>
        /// <param name="rng">Random Number Generator used in preforming crossings</param>
        /// <param name="genome">Parent genome to crossover</param>
        void Crossover(VRandom rng, T genome);

        /// <summary>
        /// Compares the current genome to another genome. The result is a positive real 
        /// value that indicates how siimilar the genomes are. This is tipicaly used to
        /// seperate a population of individules into species.
        /// </summary>
        /// <param name="genome">Other genome for comparison</param>
        /// <returns>Measure of similarity</returns>
        double Compare(T genome);

        /// <summary>
        /// Spawns a new random genome, using the current genome as a prototype. This is
        /// used to generate the initial populaiton to start the genetic algorythim. It
        /// may also be used, on ocation, to introduce new genetic material into a
        /// population that has stagnated.
        /// </summary>
        /// <param name="rng">Random Number Generater used in spawning new genomes</param>
        /// <returns>A brand new genome, based on the existing prototype</returns>
        T SpawnRandom(VRandom rng);
    }
}

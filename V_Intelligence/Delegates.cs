using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI
{
    /// <summary>
    /// A Fitness Function is used to determin how well a certain genome preforms
    /// and drive the course of evolution. Genetic algorythims seek to maximise
    /// this funciton, as highter values indicate more preferable organisms. Unlike
    /// machine learning, genetic algorythims can be made to optimise any arbitrary 
    /// fitness fcunciton. 
    /// </summary>
    /// <typeparam name="T">Genotype</typeparam>
    /// <param name="genome">Genome to evaluate</param>
    /// <returns>The fitness of the given genome</returns>
    public delegate double Fitness<T>(T genome);
}

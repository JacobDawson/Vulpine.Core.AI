using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;

namespace Vulpine.Core.AI
{
    public interface Genetic<T>
    {
        T Copy();

        T Mutate(VRandom rng);

        T Combine(VRandom rng, T other);

        

        double Fitness(object target);

        double Distance(T other);
    }
}

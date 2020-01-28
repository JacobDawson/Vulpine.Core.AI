using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vulpine.Core.AI.Genetics
{
    public interface Evolution
    {
        int Generation { get; }

        void EvolveStep();
    }
}

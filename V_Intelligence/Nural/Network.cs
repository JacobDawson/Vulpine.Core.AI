using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Vulpine.Core.Calc;
using Vulpine.Core.Calc.RandGen;
using Vulpine.Core.Calc.Matrices;

namespace Vulpine.Core.AI.Nural
{
    public interface Network<T> : Genetic<T> where T : Genetic<T>
    {
        int InSize { get; }

        int OutSize { get; }

        int NumNurons { get; }

        int NumAxons { get; }

        void WriteToFile(string file);

        void SetInput(Vector data);

        void Propergate();

        Vector ReadOutput();

        void WriteOutputTo(Vector result);

        void ResetNetwork();

        void Lerp(T genome, double amount);
    }
}

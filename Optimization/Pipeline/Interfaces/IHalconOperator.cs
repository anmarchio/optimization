using HalconDotNet;
using PRIME.Optimization.EvolutionStrategy.Interfaces;
using PRIME.Optimization.Serialization.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRIME.Optimization.Pipeline.Interfaces
{
    public interface IHalconOperator : IYAMLSerializable
    {
        HObject Execute();
        HObject Execute(HObject input);

        HObject Output { get; }

        void Dispose();

        float NodeName { get; set; }
    }
}

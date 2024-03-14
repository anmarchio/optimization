using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRIME.Optimization.Pipeline.Interfaces
{
    public interface ILeafOperator
    {
        HObject Input { get; set; }
    }
}

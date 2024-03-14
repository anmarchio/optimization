using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace PRIME.Optimization.Pipeline.Interfaces
{
    public interface IParsable
    {
        HalconOperatorNode Parse(IParser parser);
    }
}

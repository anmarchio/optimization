using Optimization.Pipeline.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Pipeline
{
    public class CGPPipelineException : Exception
    {
        public CGPPipelineException(string message, Exception innerException, IPipeline pipeline) : base(message, innerException)
        {
            Pipeline = pipeline;
        }

        public IPipeline Pipeline { get; private set; }

        public string GetDotRepresentation() {  return Pipeline.ToDOTString();  }

        public void UseSerilog()
        {
            var dr = GetDotRepresentation();
            Data["dot-pipeline"] = dr;
            Log.Error("{Exception}{NewLine}{Properties}", this, Environment.NewLine, dr);
        }
    }
}

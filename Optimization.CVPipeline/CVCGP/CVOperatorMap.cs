using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.CartesianGeneticProgramming;
using Optimization.Fitness.OperatorMaps;
using Optimization.HalconPipeline;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.CVPipeline.CVCGP
{
    [Serializable]
    public class CVOperatorMap : ModularOperatorMap
    {

        public CVOperatorMap(IEnumerable<CVNode> nodes, DependencyTree dependencyTree=null) : base(nodes.Union(new List<CVNode>() { new CVInputNode() }).Cast<IParameterInformant>().ToList(), dependencyTree)
        {

        }

        public override bool SerializeBinarySupported
        {
            get
            {
                return false;
            }
        }

        public override bool SerializeXmlSupported
        {
            get
            {
                return false;
            }
        }

        public override void SerializeBinary(string filename)
        {
            throw new NotImplementedException();
        }

        public override void SerializeXml(string filename)
        {
            throw new NotImplementedException();
        }


        public override void Initialize(CGPConfiguration configuration)
        {
            base.Initialize(configuration);
            foreach (var inputId in configuration.ProgramInputIdentifiers)
                InputOperatorTypeMap.Add(inputId, typeof(CVInputNode));
        }

    }
}

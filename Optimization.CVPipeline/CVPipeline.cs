using System;
using System.IO;
using System.Xml.Serialization;
using Emgu.CV;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.HalconPipeline;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.CVPipeline
{
    #region XmlInclude Attributes required to write Xml
    [XmlInclude(typeof(CVNode))]
    [XmlInclude(typeof(CVInputNode))]
    #endregion
    [Serializable]
    public class CVPipeline : CGPPipeline<UMat, UMat, CVNode, CVInputNode, CVNode, object>, IPipeline<CVNode>
    {
        public CVPipeline() : base()
        {

        }

        public CVPipeline(params CVNode[] outputNodes) : base(outputNodes)
        {
        }

        public CVPipeline(FloatVector vector, CGPConfiguration configuration) : base(vector, configuration)
        {  
        }

        public override void SerializeXml(string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                base.SerializeXml();
                var xml = new XmlSerializer(typeof(CVPipeline));
                xml.Serialize(writer, this);
            }
        }

        public static CVPipeline DeserializeXml(string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                var xml = new XmlSerializer(typeof(CVPipeline));
                var pipe = xml.Deserialize(reader) as CVPipeline;
                pipe.DeserializeXml();
                return pipe;
            }
        }

        public override UMat ExecuteSingle(UMat input)
        {
            return base.ExecuteSingle(input) as UMat;
        }

        public override bool SerializeXmlSupported
        {
            get
            {
                return true;
            }
        }

        public override bool SerializeBinarySupported
        {
            get
            {
                return false;
            }
        }


        public void WriteOutputs(UMat input, string directory)
        {
            if (!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            input.WriteImage(Path.Combine(directory, "input.jpg"));
            var disposeTmp = AutoDisposeIntermediateOutputs;
            AutoDisposeIntermediateOutputs = false;
            base.ExecuteSingle(input);
            foreach(var node in Nodes)
            {
                var name = node.GetType().Name + node.NodeID + ".jpg";
                if (node.Output.IsBinary())
                {
                    input.WriteImage(Path.Combine(directory, name), node.Output);
                }
                else
                {
                    node.Output.WriteImage(Path.Combine(directory, name));
                }
            }
            AutoDisposeIntermediateOutputs = disposeTmp;
        }
    }
}


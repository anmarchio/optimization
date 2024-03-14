using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class CloseEdges : HalconOperatorNode
    {
        public CloseEdges() : base() { }

        public CloseEdges(HalconOperatorNode child, int MinAmplitude) : base(child)
        {
            Initialize(MinAmplitude);
        }

        public CloseEdges(IList<HalconOperatorNode> children, int MinAmplitude) : base(children)
        {
            Initialize(MinAmplitude);
        }

        public CloseEdges(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((int)parameters[0]);
        }


        private void Initialize(int minAmplitude)
        {
            MinAmplitude = minAmplitude;
        }

        public int MinAmplitude { get; set; } = 16;

        public override int CGPInputCount
        {
            get
            {
                return 2; // needs Edges HObject and Image HObject as Input => 2
            }
        }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[1];
                    cgpParameterBounds[0] = new List<float>();
                    for (int i = 1; i <= 255; i++) cgpParameterBounds[0].Add(i);
                }
                return cgpParameterBounds;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.EdgeAmpAndRegionToRegion;
            }
        }


        /// <summary>
        /// The multi-input case, especially if there are additional constraints on the type of input (amplitude image and regions in this case)
        /// is somewhat difficult.
        /// We'll check the children if the required operators are present and raise an exception if not.
        /// We should also introduce a special flag that ensures that this behavior can be represented in CGP without too high disruption probability
        /// (i.e. we do not want this operator to often throw the above mentioned exception, just because CGP is unable to provide proper child nodes)
        /// </summary>
        /// <returns></returns>
        public override HObject Execute()
        {
            try
            {
                var input = GatherHObject();
                output = Execute(input);
                return output;
            }catch(Exception ex)
            {
                throw new OperatorException(this, ex);
            }
        }

        private HObject GatherHObject()
        {
            var regionGenerators = Children.FirstOrDefault(x => (x.IsOrOperatorType(OperatorType.ImageToRegion) || x.IsOrOperatorType(OperatorType.ImageAndRegionToRegion) ||
                                                                  x.IsOrOperatorType(OperatorType.RegionToRegion)) && !x.IsOrOperatorType(OperatorType.EdgeAmplitude));
            if (regionGenerators == null) throw new Exception("CloseEdges expects one child to generate edges and not being of type OperatorType.EdgeAmplitude");
            return (regionGenerators as HalconOperatorNode).Output;
        }

        private void GatherEdgeImage()
        {
            var edgeDetector = Children.FirstOrDefault(x => (x.IsOrOperatorType(OperatorType.EdgeAmplitude)));
            if (edgeDetector == null) throw new Exception("CloseEdges expects one child to be of OperatorType.EdgeAmplitude");
            EdgeImage = (edgeDetector as HalconOperatorNode).Output;
        }

        [XmlIgnore]
        public HObject EdgeImage { get; set; }

        public override HObject Execute(HObject input)
        {
            GatherEdgeImage();
            if (!EdgeImage.IsImageType("byte", "uint2", "int4"))
                using (var conv = EdgeImage.ConvertToStandardType())
                {
                    HOperatorSet.CloseEdges(input, conv, out output, MinAmplitude);
                }
            else
                HOperatorSet.CloseEdges(input, EdgeImage, out output, MinAmplitude);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MinAmplitude = (int)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MinAmplitude };
        }

        public override DataTypes[] InputRequirements
        {
            get
            {
                return new DataTypes[] { DataTypes.ROI, DataTypes.EdgeImage };
            }
        }

        /// <summary>
        /// Author:braml
        /// Generates halcon Code by exporting Execute functionality
        /// this specific Operator has 2 or more Input nodes. Leen suggested leaving this implementation empty for now
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            /*
            List<string> lines = new List<string>();

            lines.Add(String.Format("close_edges ({0}, {1}, {2}, {3})",
                Children.First().OutputVariableName, , OutputVariableName, MinAmplitude.ToString()));

            return lines;
            */

            throw new NotImplementedException();
        }
    }
}

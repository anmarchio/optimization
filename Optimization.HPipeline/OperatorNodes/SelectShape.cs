using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class SelectShape : HalconOperatorNode
    {
        private SelectShape(SelectShape copy) : base(copy)
        {

        }
        public SelectShape() : base()
        {
            Initialize();
        }
        public SelectShape(HalconOperatorNode child, int min, int max, SelectShapeFeatureTypes features) : base(new List<HalconOperatorNode>() { child })
        {
            
            Min = min;
            Max = max;
            Features = features;
            Initialize();

        }

        public SelectShape(IList<HalconOperatorNode> children, int min, int max, SelectShapeFeatureTypes features) : base(children)
        {
            Min = min;
            Max = max;
            Features = features;
            Initialize();
        }

        public SelectShape(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

      
        private void Initialize()
        {
        }

        public float Min { get;  set; }
        public float Max { get;  set; }
        [XmlElement("Features")]
        public SelectShapeFeatureTypes Features { get; set; } = SelectShapeFeatureTypes.area;

        public enum SelectShapeFeatureTypes
        {
            area, width, height, compactness, contlength, convexity, rectangularity, ra, rb, anisometry, bulkiness, outer_radius, inner_radius, inner_width, inner_height, dist_mean
        }

        // TODO: should reflect actual selection of SelectShapeFeatureTypes, etc.
        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    var operatorMap = new OperatorMap();
                    cgpParameterBounds = operatorMap.ParameterBounds[5];
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return DecodingMap.selectShape;
            }
        }

        public override HObject Execute(HObject input)
        {
            var lowerThresh = Min;
            var feat = Features.ToString();

            if (feat.Equals("circularity") || feat.Equals("convexity") || feat.Equals("rectangularity"))
            {
                lowerThresh = lowerThresh / 100; // actual value / max value
             
            }
            else if (feat.Equals("compactness") || feat.Equals("anisometry") || feat.Equals("bulkiness"))
            {
                lowerThresh = lowerThresh / 100 * 30; // actual value / max value
            }

            HOperatorSet.SelectShape(input, out output, Features.ToString(), new HTuple("and"), lowerThresh, Max);
            return output;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }


        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)Features, Min };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Features = (SelectShapeFeatureTypes)parameters[0];
            Min = parameters[1];
            Max = 99999;
            Initialize();
        }
        
        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
            }
        }

        /// <summary>
        /// Authors: mara
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();
            lines.Add($"LowerThres := {Min}");
            lines.Add($"Feature := '{Features.ToString()}'");
            lines.Add($"if ((Feature == '{"circularity"}') or (Feature == '{"convexity"}') or (Feature == '{"rectangularity"}') )");
            lines.Add($"LowerThres := LowerThres / 100");
            lines.Add($"elseif ((Feature == '{"compactness"}') or (Feature == '{"anisometry"}') or (Feature == '{"bulkiness"}'))");
            lines.Add($"LowerThres := LowerThres / 100 * 30");
            lines.Add($"endif");
            lines.Add($"select_shape ({Children.First().OutputVariableName}, {OutputVariableName}, {"Feature"}, '{"and"}', {"LowerThres"}, {Max})");
            //lines.Add($"select_shape ({Children.First().OutputVariableName}, {OutputVariableName}, '{SelectShapeFeatureTypes.area}', '{"and"}', {Min}, {Max})");
            
            return lines;
        }
    }
}

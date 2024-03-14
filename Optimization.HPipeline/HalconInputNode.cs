using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HalconDotNet;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline
{
    [Serializable]
    public class HalconInputNode : HalconOperatorNode, IInputNode<HObject>
    {

        public HalconInputNode() : base()
        {
        }
        public HalconInputNode(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {

        }

        [XmlIgnore]
        public HObject Input
        {
            get; set;
        }

        public float ProgramInputIdentifier { get; set; }


        public override float[] ToCGPNodeParameters()
        {
            return new float[] { };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            // no parameters
        }

        public override HObject Execute(HObject input)
        {
            Input = input;
            return Input;
        }

        public override void DisposeOutput()
        {
            // do not actually free the memory, but only remove the pointer
            Input = null;
        }

        public override HObject Output
        {
            get
            {
                return Input;
            }
        }

        public override HObject Execute()
        {
            try
            {
                return Input;
            }catch(Exception ex)
            {
                throw new OperatorException(this, ex);
            }
        }

        public override bool IsInputNode { get { return true; } }

        public override OperatorType OperatorType {get{ return OperatorType.InputNode;} }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[] { };
            }
        }

        public override string OutputVariableName
        {
            get
            {
                return string.Format("Image{0}", Math.Abs(ProgramInputIdentifier));
            }
        }

        public override int CGPInputCount
        {
            get
            {
                return 0;
            }
        }

        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();
            lines.Add(string.Format("read_image ({0}, ''[SUBSTITUDE PATH TO IMAGE HERE]'')", OutputVariableName));
            return lines;            
        }

        public string GetFormattedHalconFunctionCall(string arg)
        {
            return HalconFunctionCall()[0].Replace("'[SUBSTITUDE PATH TO IMAGE HERE]'", arg);
        }
    }
}

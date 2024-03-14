using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.Pipeline.Interfaces;

namespace Optimization.Pipeline
{
    [Serializable]
    public abstract class CGPNode<TOutput> : Node, IParameterInformant, IOutputNode<TOutput>
    {

        public CGPNode()
        {

        }

        public CGPNode(CGPNode<TOutput> child) : base(child)
        {

        }


        public CGPNode(List<CGPNode<TOutput>> children) : base(children.Cast<Node>().ToList())
        {

        }

        public CGPNode(params CGPNode<TOutput>[] children) : base(children)
        {

        }


        public abstract int CGPParameterCount
        {
            get;
        }

        public abstract List<float>[] CGPParameterBounds
        {
            get;
        }


        public abstract int CGPInputCount
        {
            get;
        }

        protected TOutput output;
        public virtual TOutput Output
        {
            get
            {
                if (output == null)
                    output = Execute();
                return output;
            }
        }

        public IEnumerable<float[]> EnumerateParameters()
        {
            if (CGPParameterCount == 0)
            {
                yield break;
            }
            else if (CGPParameterCount == 1)
            {
                for (int i = 0; i < CGPParameterBounds[0].Count; i++)
                {
                    yield return new float[] { CGPParameterBounds[0][i] };
                }
            }
            else if (CGPParameterCount == 2)
            {
                for (int i = 0; i < CGPParameterBounds[0].Count; i++)
                    for (int j = 0; j < CGPParameterBounds[1].Count; j++)
                    {
                        yield return new float[] { CGPParameterBounds[0][i], CGPParameterBounds[1][j] };
                    }
            }
            else if (CGPParameterCount == 3)
            {
                for (int i = 0; i < CGPParameterBounds[0].Count; i++)
                    for (int j = 0; j < CGPParameterBounds[1].Count; j++)
                        for (int k = 0; k < CGPParameterBounds[2].Count; k++)
                        {
                            yield return new float[] { CGPParameterBounds[0][i], CGPParameterBounds[1][j], CGPParameterBounds[2][k] };
                        }
            }
            else if (CGPParameterCount == 4)
            {
                for (int i = 0; i < CGPParameterBounds[0].Count; i++)
                    for (int j = 0; j < CGPParameterBounds[1].Count; j++)
                        for (int k = 0; k < CGPParameterBounds[2].Count; k++)
                            for (int l = 0; l < CGPParameterBounds[3].Count; l++)
                                yield return new float[] { CGPParameterBounds[0][i], CGPParameterBounds[1][j], CGPParameterBounds[2][k], CGPParameterBounds[3][l] };


            }
            else if (CGPParameterCount == 5)
            {
                for (int i = 0; i < CGPParameterBounds[0].Count; i++)
                    for (int j = 0; j < CGPParameterBounds[1].Count; j++)
                        for (int k = 0; k < CGPParameterBounds[2].Count; k++)
                            for (int l = 0; l < CGPParameterBounds[3].Count; l++)
                                for (int m = 0; m < CGPParameterBounds[4].Count; m++)
                                    yield return new float[] { CGPParameterBounds[0][i], CGPParameterBounds[1][j], CGPParameterBounds[2][k], CGPParameterBounds[3][l], CGPParameterBounds[4][m] };
            }
            else if (CGPParameterCount == 6)
            {
                for (int i = 0; i < CGPParameterBounds[0].Count; i++)
                    for (int j = 0; j < CGPParameterBounds[1].Count; j++)
                        for (int k = 0; k < CGPParameterBounds[2].Count; k++)
                            for (int l = 0; l < CGPParameterBounds[3].Count; l++)
                                for (int m = 0; m < CGPParameterBounds[4].Count; m++)
                                    for (int n = 0; n < CGPParameterBounds[5].Count; n++)
                                        yield return new float[] { CGPParameterBounds[0][i], CGPParameterBounds[1][j], CGPParameterBounds[2][k],
                                            CGPParameterBounds[3][l], CGPParameterBounds[4][m], CGPParameterBounds[5][n] };
            }
            else
                throw new NotImplementedException("For more than 6 parameters simply extend as above.");
        }

        public abstract float[] ToCGPNodeParameters();

        public abstract void FromCGPNodeParameters(float[] parameters);

        public abstract void ResetOutput();


        public virtual TOutput Execute()
        {
            if (output == null)
            {
                try
                {
                    var input = (Children.First() as CGPNode<TOutput>).Output;
                    output = Execute(input);
                    if (ReturnsImage(OperatorType))
                        ResizeOutputImage();
                }
                catch (Exception e)
                {
                    throw new OperatorException(this, e);
                }
            }
            return output;
        }

        protected abstract void ResizeOutputImage();

        protected bool ReturnsImage(OperatorType t)
        {
            if (IsOrOperatorType(OperatorType.ImageToImage) || IsOrOperatorType(OperatorType.EdgeAmplitude))
                return true;
            return false;
        }

        public abstract TOutput Execute(TOutput input);


        /// <summary>
        /// Returns the AND input requirements as separate entries in the array and OR requirements as flags combindes by the logical or (|) in each array entry
        /// </summary>
        public virtual DataTypes[] InputRequirements
        {
            get
            {
                if (OperatorType == OperatorType.InputNode)
                {
                    return new DataTypes[] { DataTypes.Image };
                }
                else if (OperatorType == OperatorType.ImageToRegion)
                {
                    return new DataTypes[] { DataTypes.Image };
                }
                else if (OperatorType == OperatorType.RegionToRegion)
                {
                    return new DataTypes[] { DataTypes.ROI };
                }
                else
                {
                    throw new Exception(string.Format("The underlying operatortype: {0} of node {1} is not supported"
                        + "per default and must be implemented manually via override", OperatorType, GetType().Name));
                }
            }
        }

        public virtual DataTypes OutputType
        {
            get
            {
                switch (OperatorType)
                {
                    case OperatorType.ImageToImage: return DataTypes.Image;
                    case OperatorType.ImageToRegion: return DataTypes.ROI;
                    case OperatorType.RegionToRegion: return DataTypes.ROI;
                    case OperatorType.ImageToImage | OperatorType.EdgeAmplitude: return DataTypes.EdgeImage;
                    default:
                        throw new Exception("Apparently the Output for given OperatorType is not specified");
                }

            }
        }
    }
}

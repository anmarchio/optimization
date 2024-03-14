using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;

namespace Optimization.HPipeline
{
    public enum StructElementTypes
    {
        Circle, Rectangle, Ellipse  
    }

    public static class StructureElement
    {
        /// <summary>
        /// Generates a structure element for use in e.g. closing and opening
        /// </summary>
        /// <param name="structType"></param>
        /// <param name="shapeParams">
        /// Rectangle: [0]: width, [1]: height
        /// Circle: [0]: radius
        /// Ellipse: [0]: longer radius, [1]: shorter radius; if [1] > [0], the values are flipped
        /// [2]: phi (if present, else 0)
        /// </param>
        /// <returns></returns>
        public static HObject Generate(StructElementTypes structType, params float[] shapeParams)
        {
            HObject structElement;

            if (structType == StructElementTypes.Rectangle)
            {
                HOperatorSet.GenRectangle1(out structElement, 0, 0, shapeParams[0], shapeParams[1]);
            }
            else if (structType == StructElementTypes.Circle)
            {
                HOperatorSet.GenCircle(out structElement, Math.Ceiling(shapeParams[0] + 1),
                    Math.Ceiling(shapeParams[0] + 1), shapeParams[0]);
            }
            else if (structType == StructElementTypes.Ellipse)
            {
                var longer = shapeParams[0];
                var shorter = shapeParams[1];
                if (shorter > longer)
                {
                    var tmp = shorter; shorter = longer; longer = tmp;
                }

                float phi = 0;
                if (shapeParams.Length > 2)
                    phi = shapeParams[2];
                HOperatorSet.GenEllipse(out structElement, Math.Ceiling(longer + 1), Math.Ceiling(longer + 1), phi, longer, shorter);
            }
            else
            {
                throw new Exception(string.Format("No proper generation method implemented for struct element of type {0}", structType.ToString()));
            }

            return structElement;
        }

        public static string GenerateGrayHalconString(StructElementTypes structType, string structElementName,  string imageType, params float[] shapeParams)
        {
            string structElement = "";
            switch (structType)
            {
                case StructElementTypes.Circle:
                    structElement = string.Format("gen_disc_se ({0}, {1}, {2}, {3}, {4})", structElementName, imageType.ToString(),
                        Math.Ceiling(shapeParams[0]).ToInvariantString(), Math.Ceiling(shapeParams[1]).ToInvariantString(),
                        shapeParams[2].ToInvariantString());
                    return structElement;
                default:
                    throw new Exception("There seems to be no halcon option to generate other gray objects than a disc; for alternatives see read_gray_se");
            }
        }
        public static string GenerateHalconString(StructElementTypes structType, string structElementName, params float[] shapeParams)
        {
            string structElement = "";
            switch (structType)
            {
                case StructElementTypes.Circle:
                    structElement = string.Format("gen_circle ({0}, {1}, {2}, {3})", structElementName,
                        Math.Ceiling(shapeParams[0] + 1).ToInvariantString(), Math.Ceiling(shapeParams[0] + 1).ToInvariantString(),
                        shapeParams[0].ToInvariantString());
                    return structElement;
                case StructElementTypes.Ellipse:
                    var maxRad = Math.Max(shapeParams[0], shapeParams[1]);
                    var longer = shapeParams[0];
                    var shorter = shapeParams[1];
                    if (shorter > longer)
                    {
                        var tmp = shorter; shorter = longer; longer = tmp;
                    }

                    float phi = 0;
                    if (shapeParams.Length > 2)
                        phi = shapeParams[2];
           
                    structElement = string.Format("gen_ellipse ({0}, {1}, {2}, {3}, {4}, {5})",
                        structElementName, Math.Ceiling(maxRad+  1).ToInvariantString(), Math.Ceiling(maxRad + 1).ToInvariantString(),
                        phi, longer, shorter);
                    return structElement;

                case StructElementTypes.Rectangle:
                    structElement = string.Format("gen_rectangle1({0}, {1}, {2}, {3}, {4})",
                        structElementName, 0, 0, shapeParams[0].ToInvariantString(), shapeParams[1].ToInvariantString());
                    return structElement;
                default:
                    throw new NotImplementedException();
            }
        }

        public static HObject GenerateGray(StructElementTypes structType, string imageType, params float[] shapeParams)
        {
            if (shapeParams.Length != 3) throw new Exception("GenDiscSe expects 3 arguments for height, width und max gray value");
            if (structType != StructElementTypes.Circle) throw new Exception("There seems to be no halcon option to generate other gray objects than a disc; for alternatives see read_gray_se");
            HObject se;
            HOperatorSet.GenDiscSe(out se, imageType, shapeParams[0], shapeParams[1], shapeParams[2]);
            return se;
        }

        public static List<float> GetParameterBoundsA()
        {
            return Enumerable.Range(1, 30).Select(x => (float)x).ToList();
        }

        public static List<float> GetParameterBoundsB()
        {
            return Enumerable.Range(1, 30).Select(x => (float)x).ToList();
        }

        /// <summary>
        /// Corresponds to parameter phi for ellipses;
        /// if used for anything else this might have to be reworked
        /// follows suggested values from:
        /// https://www.mvtec.com/doc/halcon/13/en/gen_ellipse.html
        /// </summary>
        /// <returns></returns>
        public static List<float> GetParameterBoundsC()
        {
            return new List<float>
            {
                -1.178097f, -0.785398f, -0.392699f, 0.0f, 0.392699f, 0.785398f, 1.178097f
            };
        }

        public static List<float> GetParameterBoundsGrayValues()
        {
            return new List<float>() { 0, 1, 2, 5, 10, 20, 30, 40 };
        }

        public static List<float> GetParameterBounds()
        {
            return Enum.GetValues(typeof(StructElementTypes)).Cast<StructElementTypes>().ToList().Select(x => (float)x).ToList();
        }
    }
}


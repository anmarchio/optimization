using System;
using System.Collections.Generic;
using HalconDotNet;

namespace Optimization.HPipeline
{
    public static class HObjectExtensions
    {
        public static HTuple Red = new HTuple(255, 0, 0);
        public static HTuple Green = new HTuple(0, 255, 0);
        public static HTuple Yellow = new HTuple(255, 255, 0);


        public static HTuple[] ToHTuple(this float[] arr)
        {
            var tmp = new HTuple[arr.Length];
            for (int i = 0; i < arr.Length; i++) tmp[i] = arr[i];
            return tmp;
        }

        /// <summary>
        /// Print regions into images
        /// </summary>
        /// <param name="image">image to paint the regions into</param>
        /// <param name="reference">the reference labels</param>
        /// <param name="actual">the result of the pipeline</param>
        /// <param name="type">"margin" or "fill"</param>
        /// <param name="intersectionColor">defaults to yellow</param>
        /// <param name="actualColor">defaults to red</param>
        /// <param name="referenceColor">defaults to green</param>
        /// <returns>HObject image containing the colored regions</returns>
        public static HObject Inpaint(this HObject image, HObject reference, HObject actual, string type = "margin", HTuple intersectionColor = null,
            HTuple actualColor = null, HTuple referenceColor = null)
        {
            HTuple numChannels;
            HObject intersection = null, inpaint, difference = null,
                referenceOnly = null, actualOnly = null,
                referenceUnion = null, actualUnion = null;
            if (actualColor == null) actualColor = Red;
            if (intersectionColor == null) intersectionColor = Yellow;
            if (referenceColor == null) referenceColor = Green;

            try
            {
                HOperatorSet.CountChannels(image, out numChannels);
                HOperatorSet.Union1(reference, out referenceUnion);
                HOperatorSet.Union1(actual, out actualUnion);
                reference = referenceUnion;
                actual = actualUnion;

                if (numChannels == 1)
                {
                    inpaint = MultiChannelFromSingle(image);
                }
                else
                {
                    HOperatorSet.CopyImage(image, out inpaint);
                }
                HOperatorSet.Intersection(reference, actual, out intersection);
                HOperatorSet.Difference(reference, actual, out difference);
                HOperatorSet.Difference(reference, intersection, out referenceOnly);
                HOperatorSet.Difference(actual, intersection, out actualOnly);
                HOperatorSet.OverpaintRegion(inpaint, actualOnly, Red, type);
                HOperatorSet.OverpaintRegion(inpaint, referenceOnly, Green, type);
                HOperatorSet.OverpaintRegion(inpaint, intersection, Yellow, type);
                return inpaint;
            }
            finally
            {
                if (intersection != null) intersection.Dispose();
                if (difference != null) difference.Dispose();
                if (referenceUnion != null) referenceUnion.Dispose();
                if (referenceOnly != null) referenceOnly.Dispose();
                if (actualUnion != null) actualUnion.Dispose();
                if (actualOnly != null) actualOnly.Dispose();
            }
        }

        public static int Width(this HObject image)
        {
            HTuple width, height;
            HOperatorSet.GetImageSize(image, out width, out height);
            return width;
        }

        public static int Height(this HObject image)
        {
            HTuple width, height;
            HOperatorSet.GetImageSize(image, out width, out height);
            return height;
        }

        private static HObject MultiChannelFromSingle(HObject image)
        {
            HObject multiChannel;
            HTuple pointer, imgtype, width, height;
            HOperatorSet.GetImagePointer1(image, out pointer, out imgtype, out width, out height);
            HOperatorSet.GenImage3(out multiChannel, "byte", width, height, pointer, pointer, pointer);
            return multiChannel;
        }

        /// <summary>
        /// This creates a new image, if the original image was not single channel.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static HObject SingleChannelFromMulti(this HObject image, int channel = 1)
        {
            if (image.IsSingleChannel()) return image;
            HObject singleChannel;
            HOperatorSet.AccessChannel(image, out singleChannel, channel);
            return singleChannel;
        }

        /// <summary>
        /// author: braml
        /// Helper Function for upper level halcon code export
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="outVariable"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static List<string> SingleChanelFromMultiHalconText(string imageName, string outVariable)
        {
            List<string> lines = new List<string>();

            string tmpVariable = "Tmp_out";

            lines.Add($"{SingleChannelHalconText(imageName, tmpVariable)}");
            lines.Add($"if ({tmpVariable} == 1)");
            lines.Add($"access_channel ({imageName} , {outVariable}, {1})");
            lines.Add($"*This needs to be always the first channel to mimic the behaviour of the .net environment");
            lines.Add($"else");
            lines.Add($"access_channel ({imageName}, {outVariable}, {1})");
            lines.Add($"endif");

            return lines;
        }
        /*if (!output.IsImageType("byte", "int2", "uint2", "real"))
{
using (var tmp = output.ConvertToStandardType())
{
output.Dispose();
HOperatorSet.ZoomImageSize(tmp, out output, width, height, interpolation: "nearest_neighbor");
}*/


        internal static List<string> OutputResizeimageHalconText(string inputVariableName, string zoomOutput, string convOutput, string imageTypeOutput)
        {
            List<string> lines = new List<string>();

            lines.Add($"get_image_size ({inputVariableName}, Width, Height)");

            lines.Add(ImageTypeHalconText(inputVariableName, imageTypeOutput));

            lines.Add($"if (( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2')" +
                    $" and ({imageTypeOutput} # 'real') and ({imageTypeOutput} # 'int2') ))");

            lines.AddRange(ConvertStandardHalconText(inputVariableName, convOutput));

            lines.Add($"zoom_image_size ({convOutput}, {zoomOutput}, Width, Height, 'nearest_neighbor')");

            lines.Add($"endif");

            return lines;
        }

        public static bool IsSingleChannel(this HObject image)
        {
            HTuple channels;
            HOperatorSet.CountChannels(image, out channels);
            return channels == 1;
        }

        /// <summary>
        /// author: braml
        /// Helper Function for upper level halcon code export
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="outVariable"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static string SingleChannelHalconText(string imageName, string outputVariable)
        {
            return $"count_channels ({imageName}, {outputVariable})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image">image to print regions into</param>
        /// <param name="regions">ROIs</param>
        /// <param name="color">as RGB tuple e.g. (255, 0, 0) for red</param>
        /// <param name="type">"margin" or "fill"</param>
        /// <returns>HObject image containing the colored regions</returns>
        public static HObject Inpaint(this HObject image, HObject regions, HTuple color, string type = "margin")
        {
            HObject tmpRegions;
            HOperatorSet.CopyObj(regions, out tmpRegions, 1, -1);
            if (regions.CountObj() > 1)
            {
                HOperatorSet.Union1(tmpRegions, out tmpRegions);
            }
            HTuple numChannels;
            HObject inpaint;
            HOperatorSet.CountChannels(image, out numChannels);
            if (numChannels == 1)
            {
                inpaint = MultiChannelFromSingle(image);
            }
            else
            {
                HOperatorSet.CopyImage(image, out inpaint);
            }

            HOperatorSet.OverpaintRegion(inpaint, tmpRegions, color, type);
            return inpaint;
        }

        public static bool IsImage(this HObject hobj)
        {
            if (hobj == null) throw new NullReferenceException($"HObject is null.");
            var objClass = hobj.GetObjClass();
            if (objClass.Type == HTupleType.EMPTY) return false;
            return objClass.S.Equals("image");
        }

        public static bool IsRegion(this HObject region)
        {
            var objClass = region.GetObjClass();
            if (objClass.Type == HTupleType.EMPTY) return false;
            return region.GetObjClass().S.Equals("region");
        }

        /// <summary>
        /// Dumps this HObject and prints regions according to the following Parameters
        /// Colors should be specified as RGB Halcon tuples (or any HTuple that can be read as color), e.g.
        /// HTuple(255, 0, 0) for green
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filename"></param>
        /// <param name="reference"></param>
        /// <param name="actual"></param>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="actualColor">Default: Red</param>
        /// <param name="referenceColor">Default: Green</param>
        /// <param name="intersectionColor">Default: Yellow</param>
        public static void Dump(this HObject image, string filename, HObject reference = null, HObject actual = null, string type = "margin", string format = "jpg",
            HTuple actualColor = null, HTuple referenceColor = null, HTuple intersectionColor = null)
        {
            if (!image.IsImage()) throw new Exception("tried to dump non image hobject");
            if (format.Equals("jpg") && !image.IsImageType("byte"))
                throw new Exception(".jpg only supports byte image format");


            if (!reference.IsNullOrEmpty() && !actual.IsNullOrEmpty())
            {
                image = image.Inpaint(reference, actual, type,
                    intersectionColor: intersectionColor ?? Yellow,
                    actualColor: actualColor ?? Red,
                    referenceColor: referenceColor ?? Green);
            }
            else if (!reference.IsNullOrEmpty())
            {
                image = image.Inpaint(reference, referenceColor ?? Green, type);
            }
            else if (!actual.IsNullOrEmpty())
            {
                image = image.Inpaint(actual, actualColor ?? Red, type);
            }
            HOperatorSet.WriteImage(image, format, 0, filename);

        }


        /// <summary>
        /// Commonly used Halcon Types: "byte", "uint2", "direction", "cyclic", "real"
        /// </summary>
        /// <param name="image"></param>
        /// <param name="typeNames"></param>s
        /// <returns></returns>
        public static bool IsImageType(this HObject image, params string[] typeNames)
        {
            if (!image.IsImage()) throw new Exception($"passed something different than an image. is null or empty:{image.IsNullOrEmpty()}");
            if (typeNames.Length == 0) throw new Exception();
            HTuple type;
            HOperatorSet.GetImageType(image, out type);

            foreach (var typeName in typeNames)
            {
                if (typeName.Equals(type)) return true;
            }
            return false;
        }

        /// <summary>
        /// author: braml
        /// Helper Function for upper level halcon code export
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="outVariable"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static string ImageTypeHalconText(string imageName, string outputVariable)
        {
            return string.Format("get_image_type ({0}, {1})", imageName, outputVariable);
        }

        public static string GetImageType(this HObject image)
        {
            if (image.GetObjClass().Type == HTupleType.EMPTY) throw new Exception("image is empty");
            HTuple imageType;
            HOperatorSet.GetImageType(image, out imageType);
            return imageType;
        }

        /// <summary>
        /// Warning! This creates a black and white COPY of the original image. This method does not know whether
        /// the input image is still in use afterwards, to you need to dispose is manually
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static HObject ConvertToStandardType(this HObject image, string standardType = "byte", bool scale = true)
        {
            if (!image.IsImage()) throw new Exception("passed something different than an image.");

            HObject conv;
            if (scale)
                using (var scaledImg = image.ScaleToGray())
                {
                    HOperatorSet.ConvertImageType(scaledImg, out conv, standardType);
                }
            else
                HOperatorSet.ConvertImageType(image, out conv, standardType);
            return conv;
        }

        /// <summary>
        /// author: braml
        /// Helper Function for upper level halcon code export
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="outVariable"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static List<string> ConvertStandardHalconText(string imageName, string outVariable, string standardType = "byte", bool scale = true)
        {
            List<string> lines = new List<string>();

            //lines.Add($"if ({scale.ToString()})");
            var scaleText = ScaleToGrayHalconText(imageName, outVariable);
            lines.AddRange(scaleText);
            lines.Add($"convert_image_type ({outVariable}, {outVariable}, '{standardType}')");
            //lines.Add($"else");
            //lines.Add($"convert_image_type ({imageName}, {outVariable}, '{standardType}')");
            //lines.Add($"endif");

            return lines;
        }

        public static bool IsNullOrEmpty(this HObject obj)
        {
            if (obj == null) return true;
            try
            {
                if (obj.GetObjClass().Type == HTupleType.EMPTY) return true;
            }
            catch (NullReferenceException)
            {
                return true;
            }
            catch (HOperatorException)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Warning! This creates a black and white COPY of the original image. This method does not know whether
        /// the input image is still in use afterwards, so you need to dispose is manually
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static HObject ScaleToGray(this HObject image)
        {
            HObject scaled;
            HTuple mult, add, min, max, range;
            HOperatorSet.MinMaxGray(image, image, 0, out min, out max, out range);
            if (max <= 255 && min >= 0)
            {
                HOperatorSet.CopyImage(image, out scaled);
            }else
            {
                if (max - min > 0)
                    mult = 255.0 / (max - min);
                else
                    mult = 255.0;
                add = -mult * min;
                HOperatorSet.ScaleImage(image, out scaled, mult, add);
            }
            
            return scaled;
        }

        /// <summary>
        /// author: braml
        /// Helper Function for upper level halcon code export
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="outVariable"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static List<string> ScaleToGrayHalconText(string imageName, string outVariable)
        {
            List<string> lines = new List<string>();

            lines.Add($"min_max_gray ({imageName}, {imageName}, 0, Min, Max, Range)");
            lines.Add($"if (not( (255 >= Max) and (Min >= 0) ) )");
            lines.Add($"if ((Max - Min) > 0)");
            lines.Add($"Mult := 255.0 / (Max - Min)");
            lines.Add($"else");
            lines.Add($"Mult := 255.0");
            lines.Add($"endif");
            lines.Add($"Add := -Mult * Min");
            lines.Add($"scale_image ({imageName}, {outVariable}, Mult, Add)");
            lines.Add($"else");
            lines.Add($"scale_image ({imageName}, {outVariable}, 1, 0)");
            lines.Add($"endif");

            return lines;
        }


    }
}

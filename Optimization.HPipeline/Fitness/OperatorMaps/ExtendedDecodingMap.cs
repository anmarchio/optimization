using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Fitness.Interfaces;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{
    /// <summary>
    /// Class containing function and parameter decoding as well as execution methods for the operators.
    /// ATTENTION: currently the CGPConfiguration must have 4 parameter genes to enable the stable execution of every operator!
    /// </summary>
    public class ExtendedDecodingMap : IDecodingMap<HObject[], HTuple[], HObject[]>
    {
        protected const int MaxSelectShape = 99999;
        protected const int MaxHObjects = 2;
        /// <summary>
        /// Contains all Operators, i.e. a mapping from the operator-gene (float) to the operator itself; used for decoding
        /// </summary>
        Dictionary<float, Func<HObject[], HTuple[], HObject[]>> Map = new Dictionary<float, Func<HObject[], HTuple[], HObject[]>>()
        {
            //Input: image; Output: image
            {0, sobelAmp },
            {1, kirschAmp },
            {2, freiAmp },
            {3, meanImage },
            {4, emphasize },
            {5, illuminate },
            {6, scaleImageMax },
            {7, shockFilter },
            {8, derivateGauss },
            {9, edgesImage },
            {10, smoothImage },
            {11, medianImage },
            {12, binomialFilter },
            {13, prewittAmp },
            {14, gauss_filter },
            {15, guided_filter },
            {16, invert_image },
            {17, exp_image },
            {18, log_image },
            {19, scale_image },
            {20, sqrt_image },
            {21, pow_image },

            //Input: image; Output: region(s)
            {30, thresholdAccessChannel },
            {31, threshold },
            {32, custom_threshold },
            {33, auto_threshold },
            {34, binary_threshold },
            {35, local_threshold },
            {36, fast_threshold },

            //Input: region(s); Output: region(s)
            {40, union2 },
            {41, union1 },
            {42, closing },
            {43, selectShape },
            {44, connection },
            {45, closing_circle },
            {46, closing_rectangle1 },
            {47, dilation_circle },
            {48, dilation_rectangle1 },
            {49, erosion_circle },
            {50, erosion_rectangle1 },
            {51, opening_circle },
            {52, opening_rectangle1 },
            {53, size_selection },
            {54, difference },
            {55, intersection },
            {56, complement },
            {57, fill_up },
            {58, fill_up_shape },
        };

        public Func<HObject[], HTuple[], HObject[]> this[float i]
        {
            get
            {
                return Map[i];
            }
        }

        #region Image-Image Operators

        /// <summary>
        /// sobel_amp method. Inputtypes: (byte / int2 / uint2 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] sobelAmp(HObject[] objects, HTuple[] tuples)
        {
            HObject[] o = new HObject[MaxHObjects];
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HOperatorSet.SobelAmp(objects[0], out o[0], sobel_filterType[tuples[0]], (tuples[1]));
            return o;
        }

        /// <summary>
        /// kirsch_amp method. Inputtypes: (byte / int2 / uint2)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] kirschAmp(HObject[] objects, HTuple[] empty)
        {
            HObject[] o = new HObject[MaxHObjects];
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HOperatorSet.KirschAmp(objects[0], out o[0]);
            return o;
        }

        /// <summary>
        /// frei_amp method. Inputtypes: (byte / int2 / uint2)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] freiAmp(HObject[] objects, HTuple[] empty)
        {
            HObject[] o = new HObject[MaxHObjects];
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HOperatorSet.FreiAmp(objects[0], out o[0]);
            return o;
        }

        /// <summary>
        /// mean_image method. Inputtypes: (byte* / int2* / uint2* / int4* / int8 / real* / vector_field) *allowed for Compute Devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] meanImage(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type == "int1")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.MeanImage(objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        /// <summary>
        /// emphasize method. Inputtypes: (byte / int2 / uint2)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] emphasize(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Emphasize(objects[0], out o[0], parameters[0], parameters[1], parameters[2]);
            return o;
        }

        /// <summary>
        /// illuminate method. Inputtypes: (byte / int2 / uint2)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] illuminate(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Illuminate(objects[0], out o[0], parameters[0], parameters[1], parameters[2]);
            return o;
        }

        /// <summary>
        /// scale_image_max method. Inputtypes: (byte / int2 / uint2 / int4 / int8 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] scaleImageMax(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type == "int1")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ScaleImageMax(objects[0], out o[0]);
            return o;
        }

        /// <summary>
        /// shock_filter method. Inputtypes: (byte / uint2 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] shockFilter(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "real" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ShockFilter(objects[0], out o[0], parameters[0], 7, shockFilter_filterType[parameters[1]], parameters[2]);        //currently static number of iterations (7)
            return o;
        }

        /// <summary>
        /// derivate_gauss method. Inputtypes: (byte* / direction* / cyclic* / int1* / int2* / uint2* / int4* / real*) *erlaubt für Compute Devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] derivateGauss(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type == "int8")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.DerivateGauss(objects[0], out o[0], parameters[0], derivateGauss_filterType[parameters[1]]);
            return o;
        }

        /// <summary>
        /// edges_image method. Inputtypes: (byte / uint2 / int4 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] edgesImage(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            /*
             * Die Kantenoperatoren 'deriche1' bzw. 'deriche2' stehen auch für int4-Bilder zur Verfügung. In dem Fall liefert die Routine statt der Amplituden, 
             * also dem Betrag der Filterantwort, die eigentliche, vorzeichenbehaftete Filterantwort als int4-Bild.
             */
            if (type != "byte" && type != "real" && type != "uint2"/* && type != "int4"*/)
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            //if (edgesImage_filterType[parameters[0]].Equals("sobel_fast"))
            //{
            //    HTuple numberofchannels = new HTuple();
            //    HOperatorSet.CountChannels(objects[0], out numberofchannels);
            //}

            //try
            //{
            if (parameters[3] > parameters[4])
            {
                HOperatorSet.EdgesImage(objects[0], out o[0], out o[1], edgesImage_filterType[parameters[0]], parameters[1], edgesImage_NMS[parameters[2]], parameters[4], parameters[3]);
            }
            else
            {
                HOperatorSet.EdgesImage(objects[0], out o[0], out o[1], edgesImage_filterType[parameters[0]], parameters[1], edgesImage_NMS[parameters[2]], parameters[3], parameters[4]);
            }
            //}
            //catch (Exception e)
            //{
            //    if (edgesImage_filterType[parameters[0]].Equals("sobel_fast"))          //filterType "sobel_fast" removed because of random exceptions with no correlation to parameters
            //    {
            //        HTuple numberofchannels = new HTuple();
            //        HOperatorSet.CountChannels(objects[0], out numberofchannels);
            //    }
            //    HOperatorSet.ConvertImageType(objects[0], out objects[0], "byte");
            //    HOperatorSet.WriteImage(objects[0], "bmp", 0, "errorImage");
            //}
            o[1].Dispose();
            return o;
        }

        /// <summary>
        /// smooth_image method. Inputtypes: (byte / uint2 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] smoothImage(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "real" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.SmoothImage(objects[0], out o[0], smoothImage_filterType[parameters[0]], parameters[1]);
            return o;
        }

        /// <summary>
        /// median_image method. Inputtypes: (byte* / int2* / uint2* / int4* / real*) *erlaubt für Compute Devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] medianImage(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "real" && type != "uint2" && type != "int2" && type != "int4")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            if (parameters[2] >= 300)
            {
                HOperatorSet.MedianImage(objects[0], out o[0], medianImage_MaskType[parameters[0]], parameters[1], medianImage_Margin[parameters[2]]);
            }
            else
            {
                HOperatorSet.MedianImage(objects[0], out o[0], medianImage_MaskType[parameters[0]], parameters[1], parameters[2]);
            }
            return o;
        }

        /// <summary>
        /// binomial_filter method. Inputtypes: (byte* / uint2* / real*) *erlaubt für Compute Devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] binomialFilter(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "real" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.BinomialFilter(objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        /// <summary>
        /// prewitt_amp method. Inputtypes: (byte / int2 / uint2)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] prewittAmp(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.PrewittAmp(objects[0], out o[0]);
            return o;
        }

        /// <summary>
        /// gauss_filter method. Inputtypes: (byte* / int2* / uint2* / int4* / real*) *erlaubt für Compute Devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] gauss_filter(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int2" && type != "uint2" && type != "int4" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.GaussFilter(objects[0], out o[0], parameters[0]);
            return o;
        }

        /// <summary>
        /// guided_filter method. Inputtypes: (byte / uint2 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] guided_filter(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "uint2" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            //HOperatorSet.GuidedFilter(objects[0], objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        /// <summary>
        /// invert_image method. Inputtypes: (byte* / direction* / cyclic* / int1* / int2* / uint2* / int4* / int8 / real*) *allowed for compute devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] invert_image(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "direction" && type != "cyclic" && type != "int1" && type != "int2" && type != "uint2" && type != "int8" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.InvertImage(objects[0], out o[0]);
            return o;
        }

        /// <summary>
        /// exp_image method. Inputtypes: (byte* / int1* / uint2* / int2* / int4* / int8 / real*) *allowed for compute devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] exp_image(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int8" && type != "int4" && type != "int1" && type != "int2" && type != "uint2" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            if (parameters[0].TupleEqual(new HTuple(222)))
            {
                HOperatorSet.ExpImage(objects[0], out o[0], 'e');
            }
            else
            {
                HOperatorSet.ExpImage(objects[0], out o[0], parameters[0]);
            }
            return o;
        }

        /// <summary>
        /// log_image method. Inputtypes: (byte* / int1* / uint2* / int2* / int4* / int8 / real*) *allowed for compute devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] log_image(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int8" && type != "int4" && type != "int1" && type != "int2" && type != "uint2" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            if (parameters[0].TupleEqual(new HTuple(222)))
            {
                HOperatorSet.LogImage(objects[0], out o[0], 'e');
            }
            else
            {
                HOperatorSet.LogImage(objects[0], out o[0], parameters[0]);
            }
            return o;
        }

        /// <summary>
        /// scale_image method. Inputtypes: (byte* / int1* / int2* / uint2* / int4* / int8 / real* / direction* / cyclic* / complex*) *allowed for compute devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] scale_image(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int1" && type != "int2" && type != "uint2" && type != "int4" && type != "int8" && type != "real" && type != "direction" && type != "cyclic" && type != "complex")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ScaleImage(objects[0], out o[0], parameters[0], parameters[1]);

            return o;
        }

        /// <summary>
        /// sqrt_image method. Inputtypes: (byte* / int1* / int2* / uint2* / int4* / int8 / real*) *allowed for compute devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] sqrt_image(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int1" && type != "int2" && type != "uint2" && type != "int4" && type != "int8" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.SqrtImage(objects[0], out o[0]);
            return o;
        }

        /// <summary>
        /// pow_image method. Inputtypes: (byte* / int1* / uint2* / int2* / int4* / int8 / real*) *allowed for compute devices
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] pow_image(HObject[] objects, HTuple[] parameters)
        {
            HTuple type = new HTuple();
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "int1" && type != "int2" && type != "uint2" && type != "int4" && type != "int8" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.PowImage(objects[0], out o[0], parameters[0]);

            return o;
        }

        #endregion

        #region Image-Region Operators

        public static HObject[] thresholdAccessChannel(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[MaxHObjects];

            HTuple numberofchannels = new HTuple();
            HOperatorSet.CountChannels(arg1[0], out numberofchannels);
            if (arg2[2] > numberofchannels)     //dont access a channel that does not exist
            {
                HOperatorSet.AccessChannel(arg1[0], out o[0], numberofchannels);          //access some valid channel if specified one doesnt exist
            }
            else
            {
                HOperatorSet.AccessChannel(arg1[0], out o[0], arg2[2]);
            }

            //double sign = arg2[1] == 0.0 ? -1.0 : 1.0;
            //double sign = arg2[1];

            //if (sign == -1.0)
            //    HOperatorSet.Threshold(o[0], out o[0], -128.0, sign * arg2[0]);
            //else
            HOperatorSet.Threshold(o[0], out o[0], arg2[0], 128.0);
            return o;
        }

        public static HObject[] threshold(HObject[] arg1, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];

            if (parameters[0] > parameters[1])       //Requirement: minGray < maxGray
            {
                HOperatorSet.Threshold(arg1[0], out o[0], parameters[1], parameters[0]);
            }
            else
            {
                HOperatorSet.Threshold(arg1[0], out o[0], parameters[0], parameters[1]);
            }

            return o;
        }

        public static HObject[] custom_threshold(HObject[] objects, HTuple[] parameters)
        {

            HObject[] o = new HObject[MaxHObjects];
            HTuple min, max, range;
            var scaleImageResult = scaleImageMax(objects, parameters);      //used to normalize the min and max grayvalues to 0..255, otherwise functions like pow_image produce max values of >9999999999999
            o[0] = scaleImageResult.ElementAt(0);
            HOperatorSet.MinMaxGray(o[0], o[0], parameters[0], out min, out max, out range);

            if (custom_threshold_modes[parameters[1]].Equals("upper"))
            {
                HOperatorSet.Threshold(objects[0], out o[0], max, 255);
            }
            else if (custom_threshold_modes[parameters[1]].Equals("lower"))
            {
                HOperatorSet.Threshold(objects[0], out o[0], -255, min);
            }
            else if (custom_threshold_modes[parameters[1]].Equals("both"))
            {
                HOperatorSet.Threshold(objects[0], out o[0], max, 255);
                HOperatorSet.Threshold(objects[0], out o[1], -255, min);
                o[0].ConcatObj(o[1]);
                o[1].Dispose();
            }

            return o;
        }

        /// <summary>
        /// object (byte / uint2 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] auto_threshold(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];

            //AutoThreshold segmentiert ein einkanaliges Bild mittels mehrfacher Schwellenwertsegmentation.
            HTuple numberofchannels = new HTuple();
            HOperatorSet.CountChannels(objects[0], out numberofchannels);
            if (numberofchannels > 1)
            {
                HOperatorSet.AccessChannel(objects[0], out o[0], numberofchannels);
            }

            HTuple type;
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "uint2" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            var scaleImageResult = scaleImageMax(objects, parameters);      //used to normalize the min and max grayvalues to 0..255, otherwise functions like pow_image produce max values of >9999999999999
            o[0] = scaleImageResult.ElementAt(0);
            HOperatorSet.AutoThreshold(o[0], out o[0], parameters[0]);

            //select a minima from the automatically thresholded minimas which is the closest to the relative position provided by parameters[1]
            var number_of_regions = o[0].CountObj();
            var select = parameters[1].TupleDiv(new HTuple(50)).TupleMult(new HTuple(number_of_regions)).TupleRound();
            if (select.TupleEqual(new HTuple(0)))
            {
                select++;
            }
            var obj = o[0].SelectObj(select).Clone();

            o[0].Dispose();
            o[0] = obj;
            return o;
        }

        /// <summary>
        /// object (byte / uint2 / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] binary_threshold(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];

            HTuple numberofchannels = new HTuple();
            HOperatorSet.CountChannels(objects[0], out numberofchannels);
            if (numberofchannels > 1)
            {
                HOperatorSet.AccessChannel(objects[0], out o[0], numberofchannels);
            }

            HTuple type;
            HOperatorSet.GetImageType(objects[0], out type);

            //Das Verfahren 'max_separability' ist nur für byte- und uint2-Bilder verfügbar.
            if (binary_threshold_method[parameters[0]].Equals(new HTuple("max_separability")))
            {
                if (type != "byte" && type != "uint2")
                {
                    type = "byte";
                    HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
                }
            }
            else
            {
                if (type != "byte" && type != "uint2" && type != "real")
                {
                    type = "byte";
                    HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
                }
            }

            HTuple usedTH;
            HOperatorSet.BinaryThreshold(objects[0], out o[0], binary_threshold_method[parameters[0]], binary_threshold_lightdark[parameters[1]], out usedTH);

            return o;
        }

        /// <summary>
        /// object (byte / uint2)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] local_threshold(HObject[] objects, HTuple[] parameters)
        {
            HTuple type;
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "uint2")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];

            if ((parameters[3].TupleEqual(new HTuple(0))) == 1)
            {
                var mask_size_genparamname = "mask_size";
                HOperatorSet.LocalThreshold(objects[0], out o[0], "adapted_std_deviation", local_threshold_lightdark[parameters[0]], mask_size_genparamname, parameters[1]);
            }
            else if ((parameters[3].TupleEqual(new HTuple(1))) == 1)
            {
                var scale_genparamname = "scale";
                HOperatorSet.LocalThreshold(objects[0], out o[0], "adapted_std_deviation", local_threshold_lightdark[parameters[0]], scale_genparamname, parameters[2]);
            }
            //LocalThreshold can't work with "[]" so i don't know how to call it; doesn't work with new HTuple("[]") either
            /*  else
              {
                  HOperatorSet.LocalThreshold(objects[0], out o[0], "adapted_std_deviation", local_threshold_lightdark[parameters[0]], "[]", "[]");
              }
              */
            return o;
        }

        /// <summary>
        /// object (byte / uint2 / direction / cyclic / real)
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static HObject[] fast_threshold(HObject[] objects, HTuple[] parameters)
        {
            HTuple type;
            HOperatorSet.GetImageType(objects[0], out type);
            if (type != "byte" && type != "uint2" && type != "direction" && type != "cyclic" && type != "real")
            {
                type = "byte";
                HOperatorSet.ConvertImageType(objects[0], out objects[0], type);
            }

            HObject[] o = new HObject[MaxHObjects];

            if (parameters[0] > parameters[1])       //Requirement: minGray < maxGray
            {
                HOperatorSet.FastThreshold(objects[0], out o[0], parameters[1], parameters[0], parameters[2]);
            }
            else
            {
                HOperatorSet.FastThreshold(objects[0], out o[0], parameters[0], parameters[1], parameters[2]);
            }

            return o;
        }

        #endregion

        #region Region-Region Operators

        public static HObject[] union2(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[MaxHObjects];
            HObject tmp1, tmp2;
            HOperatorSet.Union1(arg1[0], out tmp1);
            HOperatorSet.Union1(arg1[1], out tmp2);
            tmp1 = tmp1.ConcatObj(tmp2);
            HOperatorSet.Union1(tmp1, out tmp1);
            HOperatorSet.Connection(tmp1, out tmp1);
            o[0] = tmp1;
            return o;
        }

        public static HObject[] union1(HObject[] objects, HTuple[] tuples)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Union1(objects[0], out o[0]);
            return o;
        }

        public static HObject[] closing(HObject[] objects, HTuple[] tuples)
        {
            HObject[] o = new HObject[MaxHObjects];
            HObject structEle = structElement[tuples[0]].Invoke(tuples[1], tuples[2]);
            HOperatorSet.Closing(objects[0], structEle, out o[0]);
            structEle.Dispose();
            return o;
        }

        public static HObject[] selectShape(HObject[] objects, HTuple[] tuples)
        {
            HObject[] o = new HObject[MaxHObjects];

            var feat = features[tuples[0]];

            if (feat.Equals("circularity") || feat.Equals("convexity") || feat.Equals("rectangularity"))
            {
                var lowerThresh = (double)tuples[1] / 100; // actual value / max value
                HOperatorSet.SelectShape(objects[0], out o[0], new HTuple(features[tuples[0]]), new HTuple("and"), lowerThresh, new HTuple(MaxSelectShape));
            }
            else if (feat.Equals("compactness") || feat.Equals("anisometry") || feat.Equals("bulkiness"))
            {
                var lowerThresh = (double)tuples[1] / 100 * 30; // actual value / max value
                HOperatorSet.SelectShape(objects[0], out o[0], new HTuple(features[tuples[0]]), new HTuple("and"), lowerThresh, new HTuple(MaxSelectShape));
            }
            else
            {
                HOperatorSet.SelectShape(objects[0], out o[0], new HTuple(features[tuples[0]]), new HTuple("and"), tuples[1], new HTuple(MaxSelectShape));
            }
            return o;
        }

        public static HObject[] ratio(HObject[] objects, HTuple[] tuples)
        {
            HObject[] o = new HObject[MaxHObjects];
            CGPOperatorSet.RelativeThreshold(objects[0], out o[0], (float)tuples[0].D, tuples[1].I, tuples[2].I);
            return o;
        }

        public static HObject[] connection(HObject[] objects, HTuple[] tuples)
        {
            if (tuples[0] == 0.0)
            {
                //HOperatorSet.SetSystem("neighborhood", 4); avoid racing conditions
            }
            else
            {
                //HOperatorSet.SetSystem("neighborhood", 8);
            }
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Connection(objects[0], out o[0]);
            return o;
        }

        public static HObject[] closing_circle(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ClosingCircle(objects[0], out o[0], parameters[0]);
            return o;
        }

        public static HObject[] closing_rectangle1(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ClosingRectangle1(objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        public static HObject[] dilation_circle(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.DilationCircle(objects[0], out o[0], parameters[0]);
            return o;
        }

        public static HObject[] dilation_rectangle1(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.DilationRectangle1(objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        public static HObject[] erosion_circle(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ErosionCircle(objects[0], out o[0], parameters[0]);
            return o;
        }

        public static HObject[] erosion_rectangle1(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.ErosionRectangle1(objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        public static HObject[] opening_circle(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.OpeningCircle(objects[0], out o[0], parameters[0]);
            return o;
        }

        public static HObject[] opening_rectangle1(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.OpeningRectangle1(objects[0], out o[0], parameters[0], parameters[1]);
            return o;
        }

        public static HObject[] size_selection(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];

            HOperatorSet.Connection(objects[0], out o[0]);
            if (parameters[1] == new HTuple(123456))
            {
                HOperatorSet.SelectShape(o[0], out o[0], "area", "and", parameters[0], "max");

            }
            else
            {
                HOperatorSet.SelectShape(o[0], out o[0], "area", "and", parameters[0], parameters[1]);
            }

            return o;
        }

        public static HObject[] difference(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HTuple area;

            //Region1 difference Region2 != Region2 difference Region1
            HOperatorSet.Difference(objects[0], objects[1], out o[0]);
            HOperatorSet.Union1(o[0], out o[0]);
            HOperatorSet.RegionFeatures(o[0], "area", out area);
            if (area.Length == 0)    // no difference region, e.g. region2 elem region1
            {
                HOperatorSet.Difference(objects[1], objects[0], out o[0]);
            }
            HOperatorSet.Connection(o[0], out o[0]);
            return o;
        }

        public static HObject[] intersection(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Intersection(objects[0], objects[1], out o[0]);
            return o;
        }

        public static HObject[] complement(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Complement(objects[0], out o[0]);
            return o;
        }

        public static HObject[] fill_up(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.FillUp(objects[0], out o[0]);
            return o;
        }

        public static HObject[] fill_up_shape(HObject[] objects, HTuple[] parameters)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.FillUpShape(objects[0], out o[0], fill_up_shape_features[parameters[0]], parameters[1], parameters[2]);
            return o;
        }

        #endregion

        #region operator specific translation tables
        // sobelAmp
        public static Dictionary<double, string> sobel_filterType = new Dictionary<double, string>()
        {
            {0, "y"},
            {1, "y_binomial"},
            {2, "x"},
            {3, "sum_abs_binomial"},
            {4, "sum_sqrt_binomial"},
            {5, "thin_max_abs_binomial"},
            {6, "thin_sum_abs_binomial"},      
            /* maybe add in the future
            {4, "sum_abs"}, // causes #3100
            {6, "sum_sqrt"}, #3100
            {8, "thin_max_abs"}, #3100
            {10, "thin_sum_abs"},
            */
        };

        // shockFilter
        public static Dictionary<double, string> shockFilter_filterType = new Dictionary<double, string>()
        {
            {0, "canny"},
            {1, "laplace"},
        };

        // derivateGauss
        public static Dictionary<double, string> derivateGauss_filterType = new Dictionary<double, string>()
        {
            {0, "x"},
            {1, "xx"},
            {2, "y"},
            {3, "yy"},
            {4, "xy"},
            {5, "laplace"},
            {6, "kitchen_rosenfeld"},
            {7, "zuniga_haralick"},
            //dunno if the other options are worth implementing/adding
        };

        // edgesImage
        public static Dictionary<double, string> edgesImage_filterType = new Dictionary<double, string>()
        {
            {0, "canny"},
            {1, "deriche1"},
            {2, "deriche2"},
            {3, "lanser1"},
            {4, "lanser2"},
            {5, "mshen"},
            {6, "shen"},
           // {7, "sobel_fast"},
            //dunno if the other options are worth implementing/adding
            //deriche1_int4, deriche2_int4 some errors
        };

        public static Dictionary<double, string> edgesImage_NMS = new Dictionary<double, string>()
        {
            {0, "hvnms"},
            {1, "inms"},
            {2, "nms"},
            {3, "none"},
        };

        // smoothImage
        public static Dictionary<double, string> smoothImage_filterType = new Dictionary<double, string>()
        {
            {0, "deriche1"},
            {1, "deriche2"},
            {2, "gauss"},
            {3, "shen"},
        };

        // medianImage
        public static Dictionary<double, string> medianImage_MaskType = new Dictionary<double, string>()
        {
            {0, "circle"},
            {1, "square"},
        };

        public static Dictionary<double, string> medianImage_Margin = new Dictionary<double, string>()
        {   
            //values must not overlap with the integer values of the margin parameter
            {300, "mirrored"},
            {301, "cyclic"},
            {302, "continued"},
        };

        public static Dictionary<double, string> custom_threshold_modes = new Dictionary<double, string>()
        {
            {0, "upper" },
            {1, "lower" },
            {2, "both" },
        };

        public static Dictionary<double, string> binary_threshold_method = new Dictionary<double, string>()
        {
            {0, "max_separability" },
            {1, "smooth_histo" },
        };

        public static Dictionary<double, string> binary_threshold_lightdark = new Dictionary<double, string>()
        {
            {0, "dark" },
            {1, "light" },
        };

        public static Dictionary<double, string> local_threshold_lightdark = new Dictionary<double, string>()
        {
            {0, "dark" },
            {1, "light" },
        };

        // selectShape
        public static Dictionary<double, string> features = new Dictionary<double, string>()
        {
            {0, "area"},
            {1, "width"},
            {2, "height"},
            {3, "compactness"},
            {4, "contlength"},
            {5, "convexity"},
            {6, "rectangularity"},
            {7, "ra"},
            {8, "rb"},
            {9, "anisometry"},
            {10, "bulkiness"},
            {11, "outer_radius"},
            {12, "inner_radius"},
            {13, "dist_mean"}
            //{13, "inner_width"},          //causes #3513 internal error, probably because the tuple gets too large
            //{14, "inner_height"},         //causes #3513 internal error, probably because the tuple gets too large
        };

        // fill_up_shape
        public static Dictionary<double, string> fill_up_shape_features = new Dictionary<double, string>()
        {
            {0, "anisometry"},
            {1, "area"},
            {2, "compactness"},
            {3, "convexity"},
            {4, "inner_circle"},
            {5, "outer_circle"},
            {6, "phi"},
            {7, "ra"},
            {8, "rb"}
        };


        // closing - structElement
        public static Dictionary<double, Func<double, double, HObject>> structElement = new Dictionary<double, Func<double, double, HObject>>()
        {
            {0, genCircle},
            {1, genRectangle1},
            {2, genEllipse }
        };




        #region gen structElement functions

        private static HObject genEllipse(double arg1, double arg2)
        {
            HObject o;
            HOperatorSet.GenEllipse(out o, 100, 100, 0, arg1, arg2); // how to compute phi?
            return o;
        }

        private static HObject genRectangle1(double arg1, double arg2)
        {
            HObject o;
            HOperatorSet.GenRectangle1(out o, 0, 0, arg1, arg2);
            return o;
        }

        private static HObject genCircle(double arg1, double arg2)
        {
            HObject o;
            HOperatorSet.GenCircle(out o, 100, 100, arg1);
            return o;
        }

        #endregion
        #endregion
    }
}
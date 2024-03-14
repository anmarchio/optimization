using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.Fitness.Interfaces;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{
    public class DecodingMap : IDecodingMap<HObject[], HTuple[], HObject[]>
    {
        protected const int MaxSelectShape = 99999;
        public const int MaxHObjects = 2;
        /// <summary>
        /// Contains all Operators, i.e. a mapping from the operator-gene (float) to the operator itself; used for decoding
        /// </summary>
        Dictionary<float, Func<HObject[], HTuple[], HObject[]>> Map = new Dictionary<float, Func<HObject[], HTuple[], HObject[]>>()
        {
            // filters
            {0, sobelAmp },

            // thresholds
            {1, thresholdAccessChannel },

            // morphological and set operators
            {2, union2 },
            {3, union1 },
            {4, closing },
            {5, selectShape },
            {6, connection }
        };

        public Func<HObject[], HTuple[], HObject[]> this[float i]
        {
            get
            {
                return Map[i];
            }
        }


        public static HObject[] sobelAmp(HObject[] objects, HTuple[] tuples)
        {
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.SobelAmp(objects[0], out o[0], filterType[tuples[0]], (tuples[1]));
            return o;
        }

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
            double sign = arg2[1];

            if (sign == -1.0)
                HOperatorSet.Threshold(o[0], out o[0], -128.0, sign * arg2[0]);
            else
                HOperatorSet.Threshold(o[0], out o[0], arg2[0], 128.0);
            return o;
        }

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

        internal static HObject[] thresholdratio(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[MaxHObjects];

            HOperatorSet.Threshold(o[0], out o[0], arg2[0], arg2[1]);

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
                //HOperatorSet.SetSystem("neighborhood", 4);  avoid racing conditions
            }
            else
            {
                //HOperatorSet.SetSystem("neighborhood", 8);
            }
            HObject[] o = new HObject[MaxHObjects];
            HOperatorSet.Connection(objects[0], out o[0]);
            return o;
        }

        #region operator specific translation tables
        public static Dictionary<double, string> filterType = new Dictionary<double, string>()
        {
            {0, "y"},
            {1, "y_binomial"},
            {2, "x"},
            {3, "x_binomial"},       
            /*, 
            {10, "sum_abs"}, // causes #3100
            {1, "sum_abs_binomial"}, #3100
            {2, "sum_sqrt"}, #3100
            {3, "sum_sqrt_binomial"}, #3100
            {4, "thin_max_abs"}, #3100
            {5, "thin_max_abs_binomial"},
            {6, "thin_sum_abs"},
            {7, "thin_sum_abs_binomial"},              
            */
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
            {13, "inner_width"},
            {14, "inner_height"},
            {15, "dist_mean"}
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

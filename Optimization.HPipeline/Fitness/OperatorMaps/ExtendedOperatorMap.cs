using System;
using System.Collections.Generic;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.Pipeline;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{ 
    /// <summary>
    /// Custom OperatorMap containing all the encoding information for operators, inputs and parameters.
    /// </summary>
public class ExtendedOperatorMap : IOperatorMap
    {
        public ExtendedOperatorMap()
        {
            ParameterBounds = InitializeParameterBounds();
        }

        /// <summary>
        /// Constuctor for the use of mutation step sizes for non categorical values depending on the number received form the normal distribution during mutation.
        /// Number of parameter is doubled because after the values of the parameters it is encoded if the parameter was categorical (1) or non-categorical (0).
        /// </summary>
        /// <param name="useNormalDistributedMutationStepsForNonCategoricalValues"></param>
        public ExtendedOperatorMap(bool useNormalDistributedMutationStepsForNonCategoricalValues)
        {
            if (useNormalDistributedMutationStepsForNonCategoricalValues)
            {
                useNormalDistributionMutationStepsForNonCategoricalValues = useNormalDistributedMutationStepsForNonCategoricalValues;
                ParameterBounds = InitializeParameterBoundsForNonCategoricalMutation();
            }
            else
            {
                ParameterBounds = InitializeParameterBounds();
            }
        }

        private bool useNormalDistributionMutationStepsForNonCategoricalValues = false;

        /// <summary>
        /// Specifies the valid operators (list of floats) for each column (int) as well as setting the ColumnOperatorCount property of the CGPConfiguration.
        /// </summary>
        private Dictionary<int, List<float>> operatorBounds;
        public Dictionary<int, List<float>> InitializeOperatorBounds(CGPConfiguration configuration)
        {
            if (operatorBounds == null)
            {
                var image_image_columns = configuration.ColumnDistributionOfOperatorTypes[0];
                var image_region_columns = configuration.ColumnDistributionOfOperatorTypes[1];
                var region_region_columns = configuration.ColumnDistributionOfOperatorTypes[2];

                var bounds = new Dictionary<int, List<float>>();

                int i = 0;
                int upperLoopBound = image_image_columns;

                for (; i < upperLoopBound; i++)
                {
                    bounds.Add(i, new List<float>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, /*9,*/ 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 });
                }

                upperLoopBound += image_region_columns;
                for (; i < upperLoopBound; i++)
                {
                    bounds.Add(i, new List<float>() { 30, 31, 32, 33, 34, 35, 36 });
                }

                upperLoopBound += region_region_columns;
                for (; i < upperLoopBound; i++)
                {
                    bounds.Add(i, new List<float>() { 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58 });
                }
                configuration.ColumnOperatorCount = new List<int> { 21, 7, 19 };              //specify the number of operators for each columntype, i.e. 21 image-image operators
                configuration.useNormalDistributedMutationStepSizeForNonCategoricalValues = useNormalDistributionMutationStepsForNonCategoricalValues;
                operatorBounds = bounds;
            }
            return operatorBounds;

        }

        /// <summary>
        /// Specifies the maximum number of inputs for each operator (to avoid building unecessarily wide pipelines)
        /// </summary>
        public Dictionary<float, int> OperatorInputCount = new Dictionary<float, int>()
        {
            //image-image
            {0, 1}, {1, 1}, {2, 1}, {3, 1}, {4, 1}, {5, 1}, {6, 1}, {7, 1}, {8, 1}, /*{9, 1},*/ {10, 1}, {11, 1}, {12, 1}, {13, 1}, {14, 1},
            {15, 1}, {16, 1}, {17, 1}, {18, 1}, {19, 1}, {20, 1}, {21, 1},

            //image-region
            {30, 1}, {31, 1}, {32, 1}, {33, 1}, {34, 1}, {35, 1}, {36, 1}, 
            //region-region
            {40, 2}, {41, 1}, {42, 1}, {43, 1}, {44, 1}, {45, 1}, {46, 1}, {47, 1}, {48, 1}, {49, 1}, {50, 1}, {51, 1}, {52, 1}, {53, 1},
            {54, 2}, {55, 2}, {56, 1}, {57, 1}, {58, 1},
        };

        /// <summary>
        /// Sets the set of all operators
        /// </summary>
        public HashSet<float> Operators = new HashSet<float>()
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, /*9,*/ 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
            30, 31, 32, 33, 34, 35, 36,
            40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58,
        };

        /// <summary>
        /// List of valid parameters for each operator
        /// </summary>
        public Dictionary<float, List<float>[]> ParameterBounds
        {
            get; private set;
        }


        public Dictionary<float, string> PrintingMap = new Dictionary<float, string>()
        {
            //Input: image; Output: image
            {0, "sobelAmp" },
            {1, "kirschAmp" },
            {2, "freiAmp" },
            {3, "meanImage" },
            {4, "emphasize" },
            {5, "illuminate" },
            {6, "scaleImageMax" },
            {7, "shockFilter" },
            {8, "derivateGauss" },
            //{9, "edgesImage" },
            {10, "smoothImage" },
            {11, "medianImage" },
            {12, "binomialFilter" },
            {13, "prewittAmp" },
            {14, "gauss_filter" },
            {15, "guided_filter" },
            {16, "invert_image" },
            {17, "exp_image" },
            {18, "log_image" },
            {19, "scale_image" },
            {20, "sqrt_image" },
            {21, "pow_image" },

            //Input: image; Output: region(s)
            {30, "thresholdAccessChannel" },
            {31, "threshold" },
            {32, "custom_threshold" },
            {33, "auto_threshold" },
            {34, "binary_threshold" },
            {35, "local_threshold" },
            {36, "fast_threshold" },

            //Input: region(s); Output: region(s)
            {40, "union2" },
            {41, "union1" },
            {42, "closing" },
            {43, "selectShape" },
            {44, "connection" },
            {45, "closing_circle" },
            {46, "closing_rectangle1" },
            {47, "dilation_circle" },
            {48, "dilation_rectangle1" },
            {49, "erosion_circle" },
            {50, "erosion_rectangle1" },
            {51, "opening_circle" },
            {52, "opening_rectangle1" },
            {53, "size_selection" },
            {54, "difference" },
            {55, "intersection" },
            {56, "complement" },
            {57, "fill_up" },
            {58, "fill_up_shape" },

        };

        private Dictionary<float, List<float>[]> InitializeParameterBounds()
        {
            var dictionary = new Dictionary<float, List<float>[]>();

            #region initialize image-image operator parameter bounds
            // sobelAmp
            List<float>[] parameters = new List<float>[2];
            parameters[0] = new List<float>();
            parameters[1] = new List<float>();

            for (float i = 0; i < ExtendedDecodingMap.sobel_filterType.Count; i++) parameters[0].Add(i);           //Parameter FilterType
            for (float i = 1; i < 4; i++) parameters[1].Add(i * 2 + 1);                              //Parameter size
            dictionary.Add(0, parameters);

            // kirschAmp - no parameters
            parameters = new List<float>[0];
            dictionary.Add(1, parameters);

            // freiAmp - no parameters
            parameters = new List<float>[0];
            dictionary.Add(2, parameters);

            // meanImage
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[0].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 23, 31, 43, 61, 101 });
            parameters[1].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 23, 31, 43, 61, 101 });
            dictionary.Add(3, parameters);

            // emphasize
            parameters = new List<float>[3];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[2] = new List<float>();                                                      //Factor
            parameters[0].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 21, 25, 31, 39 });
            parameters[1].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 21, 25, 31, 39 });
            parameters[2].AddRange(new List<float> { 0.3f, 0.5f, 0.7f, 1.0f, 1.4f, 1.8f, 2.0f });
            dictionary.Add(4, parameters);

            // illuminate
            parameters = new List<float>[3];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[2] = new List<float>();                                                      //Factor
            parameters[0].AddRange(new List<float> { 31, 41, 51, 71, 101, 121, 151, 201 });
            parameters[1].AddRange(new List<float> { 31, 41, 51, 71, 101, 121, 151, 201 });
            parameters[2].AddRange(new List<float> { 0.3f, 0.5f, 0.7f, 1.0f, 1.5f, 2.0f, 3.0f, 5.0f });
            dictionary.Add(5, parameters);

            // scaleImageMax - no parameters
            parameters = new List<float>[0];
            dictionary.Add(6, parameters);

            // shockFilter
            parameters = new List<float>[3];
            parameters[0] = new List<float>();                                                      //Theta
            parameters[1] = new List<float>();                                                      //Mode
            parameters[2] = new List<float>();                                                      //Sigma
            parameters[0].AddRange(new List<float> { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f });
            for (float i = 0; i < ExtendedDecodingMap.shockFilter_filterType.Count; i++) parameters[1].Add(i);  //canny/laplace
            parameters[2].AddRange(new List<float> { 0.0f, 0.5f, 1.0f, 2.0f, 5.0f });
            dictionary.Add(7, parameters);

            // derivateGauss
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Sigma
            parameters[1] = new List<float>();                                                      //Component
            parameters[0].AddRange(new List<float> { 0.7f, 1.0f, 1.5f, 2.0f, 3.0f, 4.0f, 5.0f });
            for (float i = 0; i < ExtendedDecodingMap.derivateGauss_filterType.Count; i++) parameters[1].Add(i);
            dictionary.Add(8, parameters);
            /*
                        // edgesImage
                        parameters = new List<float>[5];
                        parameters[0] = new List<float>();                                                      //Filter
                        parameters[1] = new List<float>();                                                      //Alpha
                        parameters[2] = new List<float>();                                                      //NMS (non-maximum suppression)
                        parameters[3] = new List<float>();                                                      //Low
                        parameters[4] = new List<float>();                                                      //High
                        for (float i = 0; i < ExtendedDecodingMap.edgesImage_filterType.Count; i++) parameters[0].Add(i);
                        parameters[1].AddRange(new List<float> { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.7f, 0.9f, 1.1f });
                        for (float i = 0; i < ExtendedDecodingMap.edgesImage_NMS.Count; i++) parameters[2].Add(i);
                        for (int i = 1; i < 256; i++)
                        {
                            parameters[3].Add(i);
                            parameters[4].Add(i);
                        }
                        parameters[3].Add(-1);
                        parameters[4].Add(-1);
                        dictionary.Add(9, parameters);
                        */

            // smoothImage
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Filter
            parameters[1] = new List<float>();                                                      //Alpha
            for (float i = 0; i < ExtendedDecodingMap.smoothImage_filterType.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 0.1f, 0.2f, 0.3f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f, 5.0f, 7.0f, 10.0f });
            dictionary.Add(10, parameters);

            // medianImage
            parameters = new List<float>[3];
            parameters[0] = new List<float>();                                                      //MaskType
            parameters[1] = new List<float>();                                                      //Radius
            parameters[2] = new List<float>();                                                      //Margin
            for (float i = 0; i < ExtendedDecodingMap.medianImage_MaskType.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 15, 19, 25, 31, 39, 47, 59 });
            parameters[2].AddRange(new List<float> { 0, 30, 60, 90, 120, 150, 180, 210, 240, 255 });
            //ATTENTION: Margin contains float values as well as string values, necessary to check when executing this function!
            for (float i = 0; i < ExtendedDecodingMap.medianImage_Margin.Count; i++) parameters[2].Add(i);
            dictionary.Add(11, parameters);

            // binomialImage
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[0].AddRange(new List<float> { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37 });
            parameters[1].AddRange(new List<float> { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37 });
            dictionary.Add(12, parameters);

            // prewittAmp - no parameters
            parameters = new List<float>[0];
            dictionary.Add(13, parameters);

            // gauss_filter
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                                      //Size
            parameters[0].AddRange(new List<float> { 3, 5, 7, 9, 11 });
            dictionary.Add(14, parameters);

            // guided_filter
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Radius
            parameters[1] = new List<float>();                                                      //Amplitude
            parameters[0].AddRange(new List<float> { 1, 2, 3, 5, 10 });
            parameters[1].AddRange(new List<float> { 3.0f, 10.0f, 20.0f, 50.0f, 100.0f });
            dictionary.Add(15, parameters);

            // invert_image
            parameters = new List<float>[0];
            dictionary.Add(16, parameters);

            // exp_image
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                                      //Base
            parameters[0].AddRange(new List<float> { 2, 4, 6, 10, 222 });		//222='e'
            dictionary.Add(17, parameters);

            // log_image
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                                      //Base
            parameters[0].AddRange(new List<float> { 2, 4, 6, 10, 222 });		//222='e'
            dictionary.Add(18, parameters);

            // scale_image
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Mult
            parameters[1] = new List<float>();                                                      //Add
            parameters[0].AddRange(new List<float> { 0.001f, 0.003f, 0.005f, 0.008f, 0.01f, 0.02f, 0.03f, 0.05f, 0.08f, 0.1f, 0.5f, 1.0f, 2, 3, 5, 7, 9, 11 });
            parameters[1].AddRange(new List<float> { 0, 10, 30, 50, 100, 150, 200 });
            dictionary.Add(19, parameters);

            // sqrt_image
            parameters = new List<float>[0];
            dictionary.Add(20, parameters);

            // pow_image
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                                      //Exponent
            parameters[0].AddRange(new List<float> { 0.25f, 0.5f, 0.75f, 2, 3, 4, 8 });
            dictionary.Add(21, parameters);

            #endregion

            #region initialize image-region operator parameter bounds

            // thresholdaccesschannel
            parameters = new List<float>[3];
            parameters[0] = new List<float>();                          //threshold minGray
            parameters[1] = new List<float>();                          //sign
            parameters[2] = new List<float>();                          //dunno what this is for
            for (float i = 1; i < 51; i++) parameters[0].Add(i);
            //for (float i = 0; i < 2; i++) parameters[1].Add(i);
            parameters[1].Add(-1); parameters[1].Add(-1); // sign
            for (float i = 1; i < 4; i++) parameters[2].Add(i);
            dictionary.Add(30, parameters);

            // threshold
            parameters = new List<float>[2];
            parameters[0] = new List<float>();      //minGray
            parameters[1] = new List<float>();      //maxGray, technically these are interchangeable
            for (int i = 0; i < 256; i++)
            {
                parameters[0].Add(i);
                parameters[1].Add(i);
            }
            dictionary.Add(31, parameters);

            // custom_threshold
            parameters = new List<float>[2];
            parameters[0] = new List<float>();      //percent
            parameters[1] = new List<float>();      //upper/lower/both

            for (int i = 1; i < 16; i++)        //percent 1..15
            {
                parameters[0].Add(i);
            }
            for (float i = 0; i < ExtendedDecodingMap.custom_threshold_modes.Count; i++) parameters[1].Add(i);
            dictionary.Add(32, parameters);

            // auto_threshold
            parameters = new List<float>[2];
            parameters[0] = new List<float>();      //Sigma
            parameters[1] = new List<float>();      //Selector; Values: 1..50
            parameters[0].AddRange(new List<float> { 0.0f, 0.5f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f });
            for (int i = 1; i < 51; i++)
            {
                parameters[1].Add(i);
            }
            dictionary.Add(33, parameters);

            // binary_threshold
            parameters = new List<float>[2];
            parameters[0] = new List<float>();      //Method
            parameters[1] = new List<float>();      //LightDark

            for (float i = 0; i < ExtendedDecodingMap.binary_threshold_method.Count; i++) parameters[0].Add(i);
            for (float i = 0; i < ExtendedDecodingMap.binary_threshold_lightdark.Count; i++) parameters[1].Add(i);
            dictionary.Add(34, parameters);


            // local_threshold
            parameters = new List<float>[4];
            parameters[0] = new List<float>();      //LightDark
            parameters[1] = new List<float>();      //mask_size
            parameters[2] = new List<float>();      //scale
            parameters[3] = new List<float>();      //use mask_size (bool == 0); use scale (bool == 1); use nothing (bool == 2)

            for (float i = 0; i < ExtendedDecodingMap.local_threshold_lightdark.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 15, 21, 31 });
            parameters[2].AddRange(new List<float> { 0.2f, 0.3f, 0.5f });
            parameters[3].AddRange(new List<float> { 0, 1 /*, 2*/});
            dictionary.Add(35, parameters);

            // fast_threshold
            parameters = new List<float>[3];
            parameters[0] = new List<float>();      //MinGray
            parameters[1] = new List<float>();      //MaxGray
            parameters[2] = new List<float>();      //MinSize

            parameters[0].AddRange(new List<float> { 0.0f, 10.0f, 30.0f, 64.0f, 128.0f, 200.0f, 220.0f, 255.0f });
            parameters[1].AddRange(new List<float> { 0.0f, 10.0f, 30.0f, 64.0f, 128.0f, 200.0f, 220.0f, 255.0f });
            parameters[2].AddRange(new List<float> { 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 100 });
            dictionary.Add(36, parameters);

            #endregion

            #region initialize region-region operator parameter bounds

            // union2 - no parameters
            parameters = new List<float>[0];
            dictionary.Add(40, parameters);

            // union1 - no parameters
            parameters = new List<float>[0];
            dictionary.Add(41, parameters);

            // closing
            parameters = new List<float>[3];
            parameters[0] = new List<float>(); parameters[1] = new List<float>(); parameters[2] = new List<float>();

            for (float i = 0; i < ExtendedDecodingMap.structElement.Count; i++) parameters[0].Add(i);
            for (float i = 1; i < 21; i++)
            {
                parameters[1].Add(i); parameters[2].Add(i);
            }
            dictionary.Add(42, parameters);

            // selectshape
            parameters = new List<float>[2];
            parameters[0] = new List<float>(); parameters[1] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.features.Count; i++) parameters[0].Add(i);
            for (float i = 20; i < 101; i++) parameters[1].Add(i);
            dictionary.Add(43, parameters);

            // connection
            parameters = new List<float>[1];
            parameters[0] = new List<float>();
            parameters[0].Add(4); parameters[0].Add(8);
            dictionary.Add(44, parameters);

            // closing_circle
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                  //Radius
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            dictionary.Add(45, parameters);

            // closing_rectangle1
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            dictionary.Add(46, parameters);

            // dilation_circle
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                  //Radius
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            dictionary.Add(47, parameters);

            // dilation_rectangle1
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            dictionary.Add(48, parameters);

            // erosion_circle
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                  //Radius
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            dictionary.Add(49, parameters);

            // erosion_rectangle1
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            dictionary.Add(50, parameters);

            // opening_circle
            parameters = new List<float>[1];
            parameters[0] = new List<float>();                                  //Radius
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            dictionary.Add(51, parameters);

            // opening_rectangle1
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            dictionary.Add(52, parameters);

            // size_selection
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //min area
            parameters[1] = new List<float>();                                  //max area
            parameters[0].AddRange(new List<float> { 10, 20, 30, 40, 50, 75, 100, 150, 200, 400, 600, 800, 1000, 2000, 3000, 4000, 5000 });
            parameters[1].AddRange(new List<float> { 5001, 10000, 20000, 30000, 40000, 99999, 123456 }); //123456 = "max"
            dictionary.Add(53, parameters);

            // difference - no parameters
            parameters = new List<float>[0];
            dictionary.Add(54, parameters);

            // intersection - no parameters
            parameters = new List<float>[0];
            dictionary.Add(55, parameters);

            // complement - no parameters
            parameters = new List<float>[0];
            dictionary.Add(56, parameters);

            // fill_up - no parameters
            parameters = new List<float>[0];
            dictionary.Add(57, parameters);

            // fill_up_shape
            parameters = new List<float>[3];
            parameters[0] = new List<float>();          //feature
            parameters[1] = new List<float>();          //min
            parameters[2] = new List<float>();          //max
            for (float i = 0; i < ExtendedDecodingMap.fill_up_shape_features.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 0.0f, 1.0f, 10.0f, 50.0f, 100.0f, 500.0f, 1000.0f, 10000.0f });
            parameters[2].AddRange(new List<float> { 10.0f, 50.0f, 100.0f, 500.0f, 1000.0f, 10000.0f, 100000.0f });
            dictionary.Add(58, parameters);

            #endregion

            return dictionary;
        }

        /// <summary>
        /// Categorical value = 1, e.g. "x", "canny";
        /// Non-categorical value = 0, e.g. 0, 1, 2, 3, 4,...
        /// </summary>
        /// <returns></returns>
        private Dictionary<float, List<float>[]> InitializeParameterBoundsForNonCategoricalMutation()
        {
            var dictionary = new Dictionary<float, List<float>[]>();

            #region initialize image-image operator parameter bounds
            // sobelAmp
            List<float>[] parameters = new List<float>[4];
            parameters[0] = new List<float>();
            parameters[1] = new List<float>();
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.sobel_filterType.Count; i++) parameters[0].Add(i);           //Parameter FilterType
            for (float i = 1; i < 4; i++) parameters[1].Add(i * 2 + 1);                              //Parameter size
            parameters[2].Add(1);                                                                    //categorical
            parameters[3].Add(0);                                                                    //Non-categorical
            dictionary.Add(0, parameters);

            // kirschAmp - no parameters
            parameters = new List<float>[0];
            dictionary.Add(1, parameters);

            // freiAmp - no parameters
            parameters = new List<float>[0];
            dictionary.Add(2, parameters);

            // meanImage
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 23, 31, 43, 61, 101 });
            parameters[1].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 23, 31, 43, 61, 101 });
            parameters[2].Add(0);                                                                    //Non-categorical
            parameters[3].Add(0);                                                                    //Non-categorical
            dictionary.Add(3, parameters);

            // emphasize
            parameters = new List<float>[6];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[2] = new List<float>();                                                      //Factor
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            parameters[0].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 21, 25, 31, 39 });
            parameters[1].AddRange(new List<float> { 3, 5, 7, 9, 11, 15, 21, 25, 31, 39 });
            parameters[2].AddRange(new List<float> { 0.3f, 0.5f, 0.7f, 1.0f, 1.4f, 1.8f, 2.0f });
            parameters[3].Add(0);  //Non-categorical
            parameters[4].Add(0);  //Non-categorical
            parameters[5].Add(0);  //Non-categorical
            dictionary.Add(4, parameters);

            // illuminate
            parameters = new List<float>[6];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[2] = new List<float>();                                                      //Factor
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            parameters[0].AddRange(new List<float> { 31, 41, 51, 71, 101, 121, 151, 201 });
            parameters[1].AddRange(new List<float> { 31, 41, 51, 71, 101, 121, 151, 201 });
            parameters[2].AddRange(new List<float> { 0.3f, 0.5f, 0.7f, 1.0f, 1.5f, 2.0f, 3.0f, 5.0f });
            parameters[3].Add(0);  //Non-categorical
            parameters[4].Add(0);  //Non-categorical
            parameters[5].Add(0);  //Non-categorical
            dictionary.Add(5, parameters);

            // scaleImageMax - no parameters
            parameters = new List<float>[0];
            dictionary.Add(6, parameters);

            // shockFilter
            parameters = new List<float>[6];
            parameters[0] = new List<float>();                                                      //Theta
            parameters[1] = new List<float>();                                                      //Mode
            parameters[2] = new List<float>();                                                      //Sigma
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            parameters[0].AddRange(new List<float> { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f });
            for (float i = 0; i < ExtendedDecodingMap.shockFilter_filterType.Count; i++) parameters[1].Add(i);  //canny/laplace
            parameters[2].AddRange(new List<float> { 0.0f, 0.5f, 1.0f, 2.0f, 5.0f });
            parameters[3].Add(0);  //Non-categorical
            parameters[4].Add(1);  //categorical
            parameters[5].Add(0);  //Non-categorical
            dictionary.Add(7, parameters);

            // derivateGauss
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                                      //Sigma
            parameters[1] = new List<float>();                                                      //Component
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 0.7f, 1.0f, 1.5f, 2.0f, 3.0f, 4.0f, 5.0f });
            for (float i = 0; i < ExtendedDecodingMap.derivateGauss_filterType.Count; i++) parameters[1].Add(i);
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(1);  //categorical
            dictionary.Add(8, parameters);
            /*
                        // edgesImage
                        parameters = new List<float>[5];
                        parameters[0] = new List<float>();                                                      //Filter
                        parameters[1] = new List<float>();                                                      //Alpha
                        parameters[2] = new List<float>();                                                      //NMS (non-maximum suppression)
                        parameters[3] = new List<float>();                                                      //Low
                        parameters[4] = new List<float>();                                                      //High
                        for (float i = 0; i < ExtendedDecodingMap.edgesImage_filterType.Count; i++) parameters[0].Add(i);
                        parameters[1].AddRange(new List<float> { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.7f, 0.9f, 1.1f });
                        for (float i = 0; i < ExtendedDecodingMap.edgesImage_NMS.Count; i++) parameters[2].Add(i);
                        for (int i = 1; i < 256; i++)
                        {
                            parameters[3].Add(i);
                            parameters[4].Add(i);
                        }
                        parameters[3].Add(-1);
                        parameters[4].Add(-1);
                        dictionary.Add(9, parameters);
                        */

            // smoothImage
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                                      //Filter
            parameters[1] = new List<float>();                                                      //Alpha
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.smoothImage_filterType.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 0.1f, 0.2f, 0.3f, 0.5f, 1.0f, 1.5f, 2.0f, 2.5f, 3.0f, 4.0f, 5.0f, 7.0f, 10.0f });
            parameters[2].Add(1);  //categorical
            parameters[3].Add(0);  //Non-categorical
            dictionary.Add(10, parameters);

            // medianImage
            parameters = new List<float>[6];
            parameters[0] = new List<float>();                                                      //MaskType
            parameters[1] = new List<float>();                                                      //Radius
            parameters[2] = new List<float>();                                                      //Margin
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.medianImage_MaskType.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 15, 19, 25, 31, 39, 47, 59 });
            parameters[2].AddRange(new List<float> { 0, 30, 60, 90, 120, 150, 180, 210, 240, 255 });
            //ATTENTION: Margin contains float values as well as string values, necessary to check when executing this function!
            for (float i = 0; i < ExtendedDecodingMap.medianImage_Margin.Count; i++) parameters[2].Add(i);
            parameters[3].Add(0);  //Non-categorical
            parameters[4].Add(0);  //Non-categorical
            parameters[5].Add(0);  //Non-categorical, with some categorical
            dictionary.Add(11, parameters);

            // binomialImage
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                                      //MaskWidth
            parameters[1] = new List<float>();                                                      //MaskHeight
            parameters[2] = new List<float>();                                                      
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37 });
            parameters[1].AddRange(new List<float> { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37 });
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(0);  //Non-categorical
            dictionary.Add(12, parameters);

            // prewittAmp - no parameters
            parameters = new List<float>[0];
            dictionary.Add(13, parameters);

            // gauss_filter
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Size
            parameters[1] = new List<float>();                                            
            parameters[0].AddRange(new List<float> { 3, 5, 7, 9, 11 });
            parameters[1].Add(0);   //Non-categorical
            dictionary.Add(14, parameters);

            // guided_filter
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                                      //Radius
            parameters[1] = new List<float>();                                                      //Amplitude
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 1, 2, 3, 5, 10 });
            parameters[1].AddRange(new List<float> { 3.0f, 10.0f, 20.0f, 50.0f, 100.0f });
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(0);  //Non-categorical
            dictionary.Add(15, parameters);

            // invert_image
            parameters = new List<float>[0];
            dictionary.Add(16, parameters);

            // exp_image
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Base
            parameters[1] = new List<float>();
            parameters[0].AddRange(new List<float> { 2, 4, 6, 10, 222 });       //222='e'
            parameters[1].Add(0);   //Non-categorical, with some categorical
            dictionary.Add(17, parameters);

            // log_image
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Base
            parameters[1] = new List<float>();
            parameters[0].AddRange(new List<float> { 2, 4, 6, 10, 222 });       //222='e'
            parameters[1].Add(0);   //Non-categorical, with some categorical
            dictionary.Add(18, parameters);

            // scale_image
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                                      //Mult
            parameters[1] = new List<float>();                                                      //Add
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 0.001f, 0.003f, 0.005f, 0.008f, 0.01f, 0.02f, 0.03f, 0.05f, 0.08f, 0.1f, 0.5f, 1.0f, 2, 3, 5, 7, 9, 11 });
            parameters[1].AddRange(new List<float> { 0, 10, 30, 50, 100, 150, 200 });
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(0);  //Non-categorical
            dictionary.Add(19, parameters);

            // sqrt_image
            parameters = new List<float>[0];
            dictionary.Add(20, parameters);

            // pow_image
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                                      //Exponent
            parameters[1] = new List<float>();
            parameters[0].AddRange(new List<float> { 0.25f, 0.5f, 0.75f, 2, 3, 4, 8 });
            parameters[1].Add(0);  //Non-categorical
            dictionary.Add(21, parameters);

            #endregion

            #region initialize image-region operator parameter bounds

            // thresholdaccesschannel
            parameters = new List<float>[6];
            parameters[0] = new List<float>();                          //threshold minGray
            parameters[1] = new List<float>();                          //sign
            parameters[2] = new List<float>();                          //dunno what this is for
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            for (float i = 1; i < 51; i++) parameters[0].Add(i);
            //for (float i = 0; i < 2; i++) parameters[1].Add(i);
            parameters[1].Add(-1); parameters[1].Add(-1); // sign
            for (float i = 1; i < 4; i++) parameters[2].Add(i);
            parameters[3].Add(0);   //Non-categorical
            parameters[4].Add(0);   //Non-categorical
            parameters[5].Add(0);   //Non-categorical
            dictionary.Add(30, parameters);

            // threshold
            parameters = new List<float>[4];
            parameters[0] = new List<float>();      //minGray
            parameters[1] = new List<float>();      //maxGray, technically these are interchangeable
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            for (int i = 0; i < 256; i++)
            {
                parameters[0].Add(i);
                parameters[1].Add(i);
            }
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(0);  //Non-categorical
            dictionary.Add(31, parameters);

            // custom_threshold
            parameters = new List<float>[4];
            parameters[0] = new List<float>();      //percent
            parameters[1] = new List<float>();      //upper/lower/both
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            for (int i = 1; i < 16; i++)        //percent 1..15
            {
                parameters[0].Add(i);
            }
            for (float i = 0; i < ExtendedDecodingMap.custom_threshold_modes.Count; i++) parameters[1].Add(i);
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(1);  //categorical
            dictionary.Add(32, parameters);

            // auto_threshold
            parameters = new List<float>[4];
            parameters[0] = new List<float>();      //Sigma
            parameters[1] = new List<float>();      //Selector; Values: 1..50
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 0.0f, 0.5f, 1.0f, 2.0f, 3.0f, 4.0f, 5.0f });
            for (int i = 1; i < 51; i++)
            {
                parameters[1].Add(i);
            }
            parameters[2].Add(0);  //Non-categorical
            parameters[3].Add(0);  //Non-categorical
            dictionary.Add(33, parameters);

            // binary_threshold
            parameters = new List<float>[4];
            parameters[0] = new List<float>();      //Method
            parameters[1] = new List<float>();      //LightDark
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.binary_threshold_method.Count; i++) parameters[0].Add(i);
            for (float i = 0; i < ExtendedDecodingMap.binary_threshold_lightdark.Count; i++) parameters[1].Add(i);
            parameters[2].Add(1);  //categorical
            parameters[3].Add(1);  //categorical
            dictionary.Add(34, parameters);


            // local_threshold
            parameters = new List<float>[8];
            parameters[0] = new List<float>();      //LightDark
            parameters[1] = new List<float>();      //mask_size
            parameters[2] = new List<float>();      //scale
            parameters[3] = new List<float>();      //use mask_size (bool == 0); use scale (bool == 1); use nothing (bool == 2)
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            parameters[6] = new List<float>();
            parameters[7] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.local_threshold_lightdark.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 15, 21, 31 });
            parameters[2].AddRange(new List<float> { 0.2f, 0.3f, 0.5f });
            parameters[3].AddRange(new List<float> { 0, 1 /*, 2*/});
            parameters[4].Add(1);   //Categorical
            parameters[5].Add(0);   //Non-categorical
            parameters[6].Add(0);   //Non-categorical
            parameters[7].Add(0);   //Non-categorical
            dictionary.Add(35, parameters);

            // fast_threshold
            parameters = new List<float>[6];
            parameters[0] = new List<float>();      //MinGray
            parameters[1] = new List<float>();      //MaxGray
            parameters[2] = new List<float>();      //MinSize
            parameters[3] = new List<float>();     
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            parameters[0].AddRange(new List<float> { 0.0f, 10.0f, 30.0f, 64.0f, 128.0f, 200.0f, 220.0f, 255.0f });
            parameters[1].AddRange(new List<float> { 0.0f, 10.0f, 30.0f, 64.0f, 128.0f, 200.0f, 220.0f, 255.0f });
            parameters[2].AddRange(new List<float> { 5, 10, 15, 20, 25, 30, 40, 50, 60, 70, 100 });
            parameters[3].Add(0);   //Non-categorical
            parameters[4].Add(0);   //Non-categorical
            parameters[5].Add(0);   //Non-categorical
            dictionary.Add(36, parameters);

            #endregion

            #region initialize region-region operator parameter bounds

            // union2 - no parameters
            parameters = new List<float>[0];
            dictionary.Add(40, parameters);

            // union1 - no parameters
            parameters = new List<float>[0];
            dictionary.Add(41, parameters);

            // closing
            parameters = new List<float>[6];
            parameters[0] = new List<float>();
            parameters[1] = new List<float>();
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.structElement.Count; i++) parameters[0].Add(i);
            for (float i = 1; i < 21; i++)
            {
                parameters[1].Add(i);
                parameters[2].Add(i);
            }
            parameters[3].Add(1);   //categorical
            parameters[4].Add(0);   //Non-categorical
            parameters[5].Add(0);   //Non-categorical
            dictionary.Add(42, parameters);

            // selectshape
            parameters = new List<float>[4];
            parameters[0] = new List<float>();
            parameters[1] = new List<float>();
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.features.Count; i++) parameters[0].Add(i);
            for (float i = 20; i < 101; i++) parameters[1].Add(i);
            parameters[2].Add(1);   //categorical
            parameters[3].Add(0);   //Non-categorical
            dictionary.Add(43, parameters);

            // connection
            parameters = new List<float>[2];
            parameters[0] = new List<float>();
            parameters[1] = new List<float>();
            parameters[0].Add(4); parameters[0].Add(8);
            parameters[1].Add(0);   //Non-categorical
            dictionary.Add(44, parameters);

            // closing_circle
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Radius
            parameters[1] = new List<float>();
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            parameters[1].Add(0);   //Non-categorical
            dictionary.Add(45, parameters);

            // closing_rectangle1
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            parameters[2].Add(0);   //Non-categorical
            parameters[3].Add(0);   //Non-categorical
            dictionary.Add(46, parameters);

            // dilation_circle
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Radius
            parameters[1] = new List<float>(); 
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            parameters[1].Add(0);   //Non-categorical
            dictionary.Add(47, parameters);

            // dilation_rectangle1
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            parameters[2].Add(0);   //Non-categorical
            parameters[3].Add(0);   //Non-categorical
            dictionary.Add(48, parameters);

            // erosion_circle
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Radius
            parameters[1] = new List<float>();
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            parameters[1].Add(0);   //Non-categorical
            dictionary.Add(49, parameters);

            // erosion_rectangle1
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 11, 15, 21, 31, 51, 71, 101, 151, 201 });
            parameters[2].Add(0);   //Non-categorical
            parameters[3].Add(0);   //Non-categorical
            dictionary.Add(50, parameters);

            // opening_circle
            parameters = new List<float>[2];
            parameters[0] = new List<float>();                                  //Radius
            parameters[1] = new List<float>();
            parameters[0].AddRange(new List<float> { 1.5f, 2.5f, 3.5f, 4.5f, 5.5f, 7.5f, 9.5f, 12.5f, 15.5f, 19.5f, 25.5f, 33.5f, 45.5f, 60.5f, 110.5f });
            parameters[1].Add(0);   //Non-categorical
            dictionary.Add(51, parameters);

            // opening_rectangle1
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                  //Width
            parameters[1] = new List<float>();                                  //Height
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            parameters[1].AddRange(new List<float> { 1, 2, 3, 4, 5, 7, 9, 12, 15, 19, 25, 33, 45, 60, 110, 150, 200 });
            parameters[2].Add(0);   //Non-categorical
            parameters[3].Add(0);   //Non-categorical
            dictionary.Add(52, parameters);

            // size_selection
            parameters = new List<float>[4];
            parameters[0] = new List<float>();                                  //min area
            parameters[1] = new List<float>();                                  //max area
            parameters[2] = new List<float>();
            parameters[3] = new List<float>();
            parameters[0].AddRange(new List<float> { 10, 20, 30, 40, 50, 75, 100, 150, 200, 400, 600, 800, 1000, 2000, 3000, 4000, 5000 });
            parameters[1].AddRange(new List<float> { 5001, 10000, 20000, 30000, 40000, 99999, 123456 }); //123456 = "max"
            parameters[2].Add(0);   //Non-categorical
            parameters[3].Add(0);   //Non-categorical
            dictionary.Add(53, parameters);

            // difference - no parameters
            parameters = new List<float>[0];
            dictionary.Add(54, parameters);

            // intersection - no parameters
            parameters = new List<float>[0];
            dictionary.Add(55, parameters);

            // complement - no parameters
            parameters = new List<float>[0];
            dictionary.Add(56, parameters);

            // fill_up - no parameters
            parameters = new List<float>[0];
            dictionary.Add(57, parameters);

            // fill_up_shape
            parameters = new List<float>[6];
            parameters[0] = new List<float>();          //feature
            parameters[1] = new List<float>();          //min
            parameters[2] = new List<float>();          //max
            parameters[3] = new List<float>();
            parameters[4] = new List<float>();
            parameters[5] = new List<float>();
            for (float i = 0; i < ExtendedDecodingMap.fill_up_shape_features.Count; i++) parameters[0].Add(i);
            parameters[1].AddRange(new List<float> { 0.0f, 1.0f, 10.0f, 50.0f, 100.0f, 500.0f, 1000.0f, 10000.0f });
            parameters[2].AddRange(new List<float> { 10.0f, 50.0f, 100.0f, 500.0f, 1000.0f, 10000.0f, 100000.0f });
            parameters[3].Add(1);   //categorical
            parameters[4].Add(0);   //Non-categorical
            parameters[5].Add(0);   //Non-categorical
            dictionary.Add(58, parameters);

            #endregion

            return dictionary;
        }


        /// <summary>
        /// Lists for each column (int key) the set of node names that may be used as input.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        
        public Dictionary<int, Dictionary<float, List<float>[]>> ComputeInputBounds(CGPConfiguration param)
        {/*
            if (operatorBounds == null) InitializeOperatorBounds(param);

            var image_image_columns = param.ColumnDistributionOfOperatorTypes[0];
            var image_region_columns = param.ColumnDistributionOfOperatorTypes[1];
            var region_region_columns = param.ColumnDistributionOfOperatorTypes[2];

            int image_image_rows = 0;

            if (image_image_columns == 0)
            {
            }
            else
            {
                image_image_rows = param.ColumnOperatorCount[image_image_columns - 1];
            }
            var image_region_rows = param.ColumnOperatorCount[image_image_columns];
            var region_region_rows = param.ColumnOperatorCount[image_image_columns + image_region_columns];


            // default stuff
            //    int gridColumnsCount = param.ColumnCount, programInputsCount = param.ProgramInputCount, gridRowsCount = param.RowCount, levelsBack = param.LevelsBack;    
            var mat = new Dictionary<int, Dictionary<float, List<float>>>();
            for (int col = 0; col < param.ColumnCount + 1; col++) mat[col] = new Dictionary<float, List<float>>();

            foreach(var op in operatorBounds)
                mat[0].Add(param.ProgramInputIdentifiers);             // first column may only take programinputs as inputs

            int i = 1;
            int upperLoopBound = image_image_columns;

            //image_image columns can get their input from all previous columns (including ProgramInputParameters)
            for (; i < upperLoopBound; i++)
            {
                var inputList = new List<float>();
                inputList.AddRange(param.ProgramInputIdentifiers);

                var inputnodes = Enumerable.Range(0, i * image_image_rows);
                foreach (var node in inputnodes)
                {
                    inputList.Add(node);
                }
                mat.Add(i, inputList);
            }

            //image_region columns can get their input from image_image columns and ProgramInputParameters
            upperLoopBound += image_region_columns;
            for (; i < upperLoopBound; i++)
            {
                var inputList = new List<float>();
                inputList.AddRange(param.ProgramInputIdentifiers);

                var inputnodes = Enumerable.Range(0, image_image_columns * image_image_rows);
                foreach (var node in inputnodes)
                {
                    inputList.Add(node);
                }
                mat.Add(i, inputList);
            }

            //region_region columns can get their input from image_region columns and region_region columns
            upperLoopBound += region_region_columns;
            for (; i < upperLoopBound; i++)
            {
                var inputList = new List<float>();

                var inputnodes = Enumerable.Range(image_image_columns * image_image_rows, image_region_columns * image_region_rows);
                foreach (var node in inputnodes)
                {
                    inputList.Add(node);
                }
                inputnodes = Enumerable.Range(image_image_columns * image_image_rows + image_region_columns * image_region_rows, (i - image_region_columns - image_image_columns) * region_region_rows);
                foreach (var node in inputnodes)
                {
                    inputList.Add(node);
                }
                mat.Add(i, inputList);
            }

            //define inputs of output node(s), e.g. region_region columns
            var outputList = new List<float>();
            var outputnodeinputs = Enumerable.Range(image_image_columns * image_image_rows + image_region_columns * image_region_rows, region_region_columns * region_region_rows);
            foreach (var node in outputnodeinputs)
            {
                outputList.Add(node);
            }
            mat.Add(i, outputList);
           
            return mat; */
            return null;
        }

        public List<float> ProgramOutputBounds { get; set; } = null;

        public void Initialize(CGPConfiguration configuration)
        {
        }

        public void SerializeXml(string filename)
        {
            throw new NotImplementedException();
        }

        public void SerializeBinary(string filename)
        {
            throw new NotImplementedException();
        }

        public float Encode(Node op)
        {
            throw new NotImplementedException();
        }

        public Type Decode(float op)
        {
            throw new NotImplementedException();
        }

        HashSet<float> IOperatorMap.OperatorIdentifiers
        {
            get
            {
                return Operators;
            }
        }

        Dictionary<float, int> IOperatorMap.OperatorInputCount
        {
            get
            {
                return OperatorInputCount;
            }
        }

        Dictionary<float, string> IOperatorMap.PrintingMap
        {
            get
            {
                return PrintingMap;
            }
        }

        public bool SerializeBinarySupported
        {
            get
            {
                return false;
            }
        }

        public bool SerializeXmlSupported
        {
            get
            {
                return false;
            }
        }

        public IOperatorMap OperatorMap
        {
            get
            {
                return this;
            }
        }

        public bool IsInitialized
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DependencyTree Dependencies
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}


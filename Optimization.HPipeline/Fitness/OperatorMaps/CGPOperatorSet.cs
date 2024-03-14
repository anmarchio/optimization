using System;
using HalconDotNet;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{
    public class CGPOperatorSet
    {

        public static void AreaSizeThreshold(HObject image, out HObject FaultyRegion, HTuple minGray, HTuple maxGray,
            HTuple minSize, HTuple maxSize, HTuple W_Size, HTuple H_Size)
        {
            minGray = new HTuple(20);
            maxGray = new HTuple(255);
            HOperatorSet.GenEmptyRegion(out FaultyRegion);
            HObject TempRegion;
            HOperatorSet.GenEmptyRegion(out TempRegion);

            HTuple width, height;
            HTuple row1, column1, row2, column2;
            HOperatorSet.GetImageSize(image, out width, out height);

            HTuple I_W = width / W_Size;
            HTuple I_H = height / H_Size;
            for (int i = 0; i < I_W; i++)
            {
                for (int j = 0; j < I_H; j++)
                {
                    row1 = j * H_Size;
                    column1 = i * W_Size;
                    row2 = j * H_Size + H_Size;
                    column2 = i * W_Size + W_Size;

                    if (row2 > height)
                    {
                        row2 = height;
                    }
                    if (column2 > width)
                    {
                        column2 = width;
                    }
                    if (row1 > height)
                    {
                        row1 = height - 1;
                    }
                    if (column1 > width)
                    {
                        column1 = width - 1;
                    }
                    HObject imagePart, threads;
                    HTuple row, col, areaSize;
                    HOperatorSet.CropRectangle1(image, out imagePart, row1, column1, row2, column2);
                    HOperatorSet.Threshold(imagePart, out threads, new HTuple(40), new HTuple(255));
                    //HOperatorSet.MinMaxGray(imagePart, imagePart, new HTuple(0), out min, out max, out range);
                    HOperatorSet.AreaCenter(threads, out areaSize, out row, out col);

                    if (areaSize < maxSize && areaSize > minSize)
                    {
                        TempRegion.Dispose();
                        HOperatorSet.GenRectangle1(out TempRegion, row1, column1, row2, column2);
                        HOperatorSet.Union2(TempRegion, FaultyRegion, out FaultyRegion);
                    }
                }
                HTuple val;
                HOperatorSet.SmallestRectangle1(FaultyRegion, out row1, out column1, out row2, out column2);
                HOperatorSet.RegionFeatures(FaultyRegion, new HTuple("area"), out val);
            }
        }

        public static void RelativeThreshold(HObject image, out HObject relativeRegion, HTuple minRatio, HTuple maskWidth, HTuple maskHeight)
        {
            HObject Region;
            HOperatorSet.TupleReal(minRatio, out minRatio);
            //minRatio = minRatio / 1000.0;
            //minRatio = new HTuple(0.075); maskWidth = new HTuple(15); maskHeight = new HTuple(15);
            if (image != null)
            {
                // Local iconic variables 
                HObject Rectangle, NewImgReduced, ImgPart, FaultyRegion;
                // Isolate roving
                HOperatorSet.GenEmptyObj(out Rectangle);
                //image.Dump("relativeTHresholdError");
                var tpm = image.GetObjClass();

                if (!image.IsSingleChannel())
                {
                    image = image.SingleChannelFromMulti();
                }
                if(!image.IsImageType("byte", "uint2", "direction", "cyclic", "real"))
                {
                    image = image.ConvertToStandardType("byte");
                }
                HTuple imgType;
                HOperatorSet.GetImageType(image, out imgType);
                HOperatorSet.FastThreshold(image, out Region, new HTuple(45), new HTuple(255), new HTuple(80));
                HOperatorSet.FillUp(Region, out Rectangle);
                // Initialize variables                    
                HTuple row1 = new HTuple(), row2 = new HTuple(), col1 = new HTuple(), col2 = new HTuple();
                HTuple Width = new HTuple(), Height = new HTuple(), HStep = new HTuple(), WStep = new HTuple();
                HTuple imgWidth = new HTuple(), imgHeight = new HTuple(), histo = new HTuple(), binSize = new HTuple();
                HTuple pixelcount = new HTuple(), ratio = new HTuple();
                // Initialize local and output iconic variables 
                HOperatorSet.GenEmptyObj(out FaultyRegion);
                HOperatorSet.GenEmptyObj(out NewImgReduced);
                HOperatorSet.GenEmptyObj(out ImgPart);
                HOperatorSet.GenEmptyObj(out relativeRegion);

                HOperatorSet.SmallestRectangle1(Rectangle, out row1, out col1, out row2, out col2);
                HOperatorSet.ReduceDomain(image, Rectangle, out NewImgReduced);
                //HOperatorSet.GetImageSize(ho_SelectedRegions, out hv_Width, out hv_Height);
                HOperatorSet.RegionFeatures(Rectangle, "width", out Width);
                HOperatorSet.RegionFeatures(Rectangle, "height", out Height);
                /*
                if (Width.Type == HTupleType.EMPTY)
                {
                    Width = new HTuple(0);
                }
                if (Height.Type == HTupleType.EMPTY)
                {
                    Height = new HTuple(0);
                }

                if (row1.Type == HTupleType.EMPTY)
                {
                    row1 = new HTuple(0);
                }
                if (col1.Type == HTupleType.EMPTY)
                {
                    col1 = new HTuple(0);
                }
                if (row2.Type == HTupleType.EMPTY)
                {
                    row2 = new HTuple(0);
                }
                if (col2.Type == HTupleType.EMPTY)
                {
                    col2 = new HTuple(0);
                }*/
                //Define the Height and Width of the Rectangle that gets checked iteratively throughout the image
                WStep = Width / maskWidth;
                HStep = Height / maskHeight;

                HTuple endW = (col2 - (WStep / 1.5)) - 20;
                HTuple stepW = WStep / 2;

                for (imgWidth = col1 + 20; imgWidth.Continue(endW, stepW); imgWidth = imgWidth.TupleAdd(stepW))
                {
                    HTuple endH = row2 - (HStep / 1.5);
                    HTuple stepH = HStep / 2;
                    for (imgHeight = row1 + 3; imgHeight.Continue(endH, stepH); imgHeight = imgHeight.TupleAdd(stepH))
                    {
                        ImgPart.Dispose();
                        HOperatorSet.CropRectangle1(NewImgReduced, out ImgPart, imgHeight, imgWidth, imgHeight + HStep, imgWidth + WStep);

                        //possibility to play around with the number of bins
                        if (!ImgPart.IsSingleChannel())
                            ImgPart = ImgPart.SingleChannelFromMulti();
                        HOperatorSet.GrayHistoRange(ImgPart, ImgPart, 0, 255, 2, out histo, out binSize);

                        pixelcount = histo.TupleSelect(0) + histo.TupleSelect(1);

                        HOperatorSet.TupleReal(pixelcount, out pixelcount);

                        if (pixelcount <= 0.6 * WStep * HStep)
                        {
                            continue;
                        }
                        ratio = histo.TupleSelect(1) / pixelcount;

                        //if ((new HTuple(hv_ratio.TupleLess(0.057))) != 0)
                        if (ratio < minRatio)
                        {
                            FaultyRegion.Dispose();
                            HOperatorSet.GenRectangle1(out FaultyRegion, imgHeight, imgWidth, imgHeight + HStep, imgWidth + WStep);
                            HOperatorSet.Union1(FaultyRegion.ConcatObj(relativeRegion), out relativeRegion);
                        }
                    }
                }

            }
            else
            {
                //HOperatorSet.GenEmptyRegion(out relativeRegion);
                throw new Exception("This really shouldn't happen. Don't just generate empty regions to obfuscate this.");
            }
            //HTuple area;
            //HOperatorSet.RegionFeatures(relativeRegion, new HTuple("area"), out area);
        }

        public static void GaussFFT(HObject image, out HObject filteredImage, float sigma1, float sigma2)
        {
            HOperatorSet.GenEmptyObj(out filteredImage);
            HObject gaussFilter1, gaussFilter2, imageFFT, filter;
            HTuple width = new HTuple(), height = new HTuple(), phi = new HTuple(0), norm = new HTuple("none"), mode = new HTuple("rft");

            HOperatorSet.GetImageSize(image, out width, out height);

            if (width.Type != HTupleType.EMPTY && height.Type != HTupleType.EMPTY)
            {
                HOperatorSet.GenGaussFilter(out gaussFilter1, sigma1, sigma1, phi, norm, mode, width, height);
                HOperatorSet.GenGaussFilter(out gaussFilter2, sigma2, sigma2, phi, norm, mode, width, height);
                HOperatorSet.SubImage(gaussFilter1, gaussFilter2, out filter, new HTuple(1), new HTuple(0));
                HTuple direction = new HTuple("to_freq"), resulttype = new HTuple("complex");
                HOperatorSet.RftGeneric(image, out imageFFT, direction, norm, resulttype, width);
                HOperatorSet.ConvolFft(imageFFT, filter, out imageFFT);
                direction = new HTuple("from_freq");
                norm = new HTuple("n");
                resulttype = new HTuple("real");
                HOperatorSet.RftGeneric(imageFFT, out filteredImage, direction, norm, resulttype, width);
                // For testing
                HObject Region;
                HOperatorSet.Threshold(filteredImage, out Region, new HTuple(0), new HTuple(255));
                HTuple row1, column1, row2, column2;
                HOperatorSet.SmallestRectangle1(Region, out row1, out column1, out row2, out column2);
            }
        }

        public static void BandpassThreshold(HObject image, out HObject bandRegion, double minRatio, double blackRatio, int maskWidth, int maskHeight)
        {
            HObject ho_ErrorNitting;
            HObject ho_ErrorGap;

            // Local iconic variables 
            HObject ho_GrayImage = null, ho_FaultyObjects = null;
            HObject ho_SelectedRegions = null, ho_Image = null;
            HObject ho_NewImgReduced = null,
                /*ho_ImgPart_Roving = null,*/ ho_ImgPart_rectChecked = null,
                ho_FaultyRegion = null;
            // Local control variables 
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HTuple hv_widthstep = new HTuple(),
                hv_heightstep = new HTuple(),
                hv_img_width = new HTuple(),
                hv_img_height = new HTuple(),
                hv_Histo_ImagePart = new HTuple();
            HTuple hv_binsize1 = new HTuple(),
                hv_pixelcount = new HTuple(),
                hv_ratio = new HTuple();
            HTuple row1 = new HTuple(), row2 = new HTuple(), col1 = new HTuple(), col2 = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_NewImgReduced);
            HOperatorSet.GenEmptyObj(out ho_ErrorGap);
            HOperatorSet.GenEmptyObj(out ho_ImgPart_rectChecked);
            HOperatorSet.GenEmptyObj(out ho_FaultyRegion);
            HOperatorSet.GenEmptyObj(out ho_FaultyObjects);
            HOperatorSet.GenEmptyObj(out ho_ErrorNitting);

            ho_Image = image.Clone();

            HOperatorSet.SmallestRectangle1(ho_SelectedRegions, out row1, out col1, out row2, out col2);
            //HOperatorSet.GetImageSize(ho_SelectedRegions, out hv_Width, out hv_Height);
            HOperatorSet.RegionFeatures(ho_SelectedRegions, "width", out hv_Width);
            HOperatorSet.RegionFeatures(ho_SelectedRegions, "height", out hv_Height);
            if (hv_Width.Type == HTupleType.EMPTY)
            {
                hv_Width = new HTuple(0);
            }
            if (hv_Height.Type == HTupleType.EMPTY)
            {
                hv_Height = new HTuple(0);
            }
            if (row1.Type == HTupleType.EMPTY)
            {
                row1 = new HTuple(0);
            }
            if (col1.Type == HTupleType.EMPTY)
            {
                col1 = new HTuple(0);
            }
            if (row2.Type == HTupleType.EMPTY)
            {
                row2 = new HTuple(0);
            }
            if (col2.Type == HTupleType.EMPTY)
            {
                col2 = new HTuple(0);
            }
            //Define the Height and Width of the Rectangle that gets checked iteratively throughout the image
            hv_widthstep = hv_Width / maskWidth;
            hv_heightstep = hv_Height / maskHeight;

            HTuple end_value_widthloop = (col2 - (hv_widthstep / 1.5)) - 20;
            HTuple step_value_widthloop = hv_widthstep / 2;

            for (hv_img_width = col1 + 20;
                hv_img_width.Continue(end_value_widthloop, step_value_widthloop);
                hv_img_width = hv_img_width.TupleAdd(step_value_widthloop))
            {
                HTuple end_value_heightloop = row2 - (hv_heightstep / 1.5);
                HTuple step_value_heightloop = hv_heightstep / 2;
                for (hv_img_height = row1 + 3;
                    hv_img_height.Continue(end_value_heightloop, step_value_heightloop);
                    hv_img_height = hv_img_height.TupleAdd(step_value_heightloop))
                {
                    ho_ImgPart_rectChecked.Dispose();
                    HOperatorSet.CropRectangle1(ho_NewImgReduced, out ho_ImgPart_rectChecked, hv_img_height,
                        hv_img_width, hv_img_height + hv_heightstep, hv_img_width + hv_widthstep);

                    //possibility to play around with the number of bins
                    HOperatorSet.GrayHistoRange(ho_ImgPart_rectChecked, ho_ImgPart_rectChecked, 0, 255, 6,
                        out hv_Histo_ImagePart, out hv_binsize1);

                    hv_pixelcount = (hv_Histo_ImagePart.TupleSelect(0)) + (hv_Histo_ImagePart.TupleSelect(1)) +
                                    (hv_Histo_ImagePart.TupleSelect(2)) + (hv_Histo_ImagePart.TupleSelect(3)) +
                                    (hv_Histo_ImagePart.TupleSelect(4)) + (hv_Histo_ImagePart.TupleSelect(5));

                    if (hv_pixelcount <= 0.6 * hv_widthstep * hv_heightstep)
                    {
                        continue;
                    }
                    HOperatorSet.TupleReal(hv_pixelcount, out hv_pixelcount);
                    hv_ratio = (hv_Histo_ImagePart.TupleSelect(3) + hv_Histo_ImagePart.TupleSelect(4) +
                                hv_Histo_ImagePart.TupleSelect(5)) / hv_pixelcount;
                    HTuple hv_ratio_black = hv_Histo_ImagePart.TupleSelect(0) / hv_pixelcount;


                    HTuple ratioThresh = new HTuple(minRatio);

                    if (hv_ratio.TupleLess(ratioThresh) && hv_ratio_black > 0.65) //Gap!
                    {
                        ho_FaultyRegion.Dispose();
                        HOperatorSet.GenRectangle1(out ho_FaultyRegion, hv_img_height, hv_img_width, hv_img_height + hv_heightstep, hv_img_width + hv_widthstep);
                        {
                            HObject ExpTmpOutVar_0;
                            HOperatorSet.Union2(ho_FaultyRegion, ho_ErrorGap, out ExpTmpOutVar_0);
                            ho_ErrorGap.Dispose();
                            ho_ErrorGap = ExpTmpOutVar_0;
                        }
                    }
                }
            }
            bandRegion = ho_ErrorGap;
        }

        public static void BandpassFFT(HObject image, out HObject filteredImage, float minFrequency, float maxFrequency, float size)
        {
            HOperatorSet.GenEmptyObj(out filteredImage);
            HObject imageGauss, imageFFT, filter;
            HTuple width = new HTuple(),
                height = new HTuple(),
                phi = new HTuple(0),
                norm = new HTuple("none"),
                mode = new HTuple("rft"),
                direction = new HTuple("to_freq"),
                resulttype = new HTuple("complex");

            if (!image.IsImage()) throw new Exception("object in variable 'image' is not an hobject containing an image.");
            if (!image.IsImageType("byte", "int2", "uint2", "int4", "real")) image = image.ConvertToStandardType();

            HOperatorSet.GetImageSize(image, out width, out height);

            if (width.Type != HTupleType.EMPTY && height.Type != HTupleType.EMPTY)
            {
                HOperatorSet.GaussImage(image, out imageGauss, new HTuple(size));
                HOperatorSet.GenBandfilter(out filter, new HTuple(minFrequency), new HTuple(maxFrequency), norm, mode, width, height);
                HOperatorSet.RftGeneric(image, out imageFFT, direction, norm, resulttype, width);
                HOperatorSet.ConvolFft(imageFFT, filter, out imageFFT);
                direction = new HTuple("from_freq");
                norm = new HTuple("n");
                resulttype = new HTuple("real");
                HOperatorSet.RftGeneric(imageFFT, out filteredImage, direction, norm, resulttype, width);
            }
            HOperatorSet.GetImageSize(filteredImage, out width, out height);
        }

        public static void AreaToRectangle(HObject image, out HObject rectangles)
        {
            /*
            area_center(Shapes, Area, Row, Column)
            Num:= | Area |
            gen_empty_region(NittingErrors)
            for Index1 := 1 to Num by 1
                select_obj(Shapes, obj, Index1)
                smallest_rectangle1(obj, Row11, Column11, Row21, Column21)
                gen_rectangle1(Rectangle, Row11, Column11, Row21, Column21)
                union2(NittingErrors, Rectangle, NittingErrors)
            endfor
            */
            HTuple area, row, col;
            HOperatorSet.GenEmptyRegion(out rectangles);
            if (image != null)
            {
                HOperatorSet.AreaCenter(image, out area, out row, out col);
                for (int i = 1; i <= area.Length; i++)
                {
                    HObject obj, rect;
                    HTuple row1, column1, row2, column2;
                    HOperatorSet.SelectObj(image, out obj, new HTuple(i));
                    HOperatorSet.SmallestRectangle1(obj, out row1, out column1, out row2, out column2);
                    HOperatorSet.GenRectangle1(out rect, row1, column1, row2, column2);
                    HOperatorSet.Union2(rectangles, rect, out rectangles);
                    obj.Dispose();
                }
            }
        }
    }
}
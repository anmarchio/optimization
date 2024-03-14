using System;
using HalconDotNet;

namespace Optimization.HPipeline.Fitness
{
    public static class Collection
    {
        #region halcon fitness functions -- formerlly located in FitnessEvaluator

        public static double MCC(HObject reference, HObject actual, int height, int width)
        {
            int TP, FP, FN, TN;
            ConfusionMatrix(actual, reference, height, width, out TP, out TN, out FP, out FN);

            var TPFP = TP + FP;
            var TPFN = TP + FN;
            var TNFP = TN + FP;
            var TNFN = TN + FN;

            // compute MMC if any are zero, then MCC is defined as 0
            // if any are below zero, then there was an issue computing the confusion matrix
            // this can happen if regions are empty
            if (TPFP <= 0 || TPFN <= 0 || TNFP <= 0 || TNFN <= 0) return 0;

            return Optimization.Fitness.Collection.MCC(TP, TN, FP, FN);
        }

        public static double Recall(HObject reference, HObject actual)
        {
            HTuple rows, columns, area;
            long intersectionPixelCount = 0, differencePixelCount = 0;
            HObject difference; // == false positives (for precision), false negatives (for recall)
            HObject intersection; // == true positives
                                  // compute intersection

            // intersection == TP
            HOperatorSet.Intersection(actual, reference, out intersection);
            HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
            if (area.Length != 0)
                intersectionPixelCount = area;

            // difference == FN
            HOperatorSet.Difference(reference, intersection, out difference);
            HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
            if (area.Length != 0)
                differencePixelCount = area;

            return Optimization.Fitness.Collection.Recall(intersectionPixelCount, differencePixelCount);
        }

        /// <summary>
        /// Computes precision
        /// </summary>
        public static double Precision(HObject reference, HObject actual)
        {
            HTuple rows, columns, area;
            double intersectionPixelCount = 0, differencePixelCount = 0;
            HObject difference; // == false positives (for precision), false negatives (for recall)
            HObject intersection; // == true positives
                                  // compute intersection


            HObject referenceUnion, actualUnion;
            HOperatorSet.Union1(reference, out referenceUnion);
            HOperatorSet.Union1(actual, out actualUnion);
            // intersection == TP
            HOperatorSet.Intersection(actualUnion, referenceUnion, out intersection);
            HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
            if (area.Length != 0)
                intersectionPixelCount = area;


            // difference == FP
            HOperatorSet.Difference(actualUnion, intersection, out difference);
            HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
            if (area.Length != 0)
                differencePixelCount = area;


            return Optimization.Fitness.Collection.Precision(intersectionPixelCount, differencePixelCount);
        }

        public static double Accuracy(HObject actual, HObject reference, int height, int width)
        {
            int tp, fp, tn, fn;
            ConfusionMatrix(actual, reference, height, width, out tp, out tn, out fp, out fn);
            return Optimization.Fitness.Collection.Accuracy(tp, tn, fp, fn);
        }

        #endregion

        public static double IntersectionOverUnion(HObject actual, HObject unionReferenceRegions)
        {
            HObject actualUnion = null, intersection = null, union = null;
            try
            {
                HOperatorSet.Union1(actual, out actualUnion);
                HTuple intersectionSize, unionSize, row, column;

                HOperatorSet.Union1(actualUnion.ConcatObj(unionReferenceRegions), out union);
                HOperatorSet.Intersection(actualUnion, unionReferenceRegions, out intersection);
                HOperatorSet.AreaCenter(intersection, out intersectionSize, out row, out column);
                if (intersectionSize.Length == 0)
                    intersectionSize = 0;
                HOperatorSet.AreaCenter(union, out unionSize, out row, out column);
                if (unionSize.Length == 0)
                    unionSize = 0;
                return Optimization.Fitness.Collection.IntersectionOverUnion(intersectionSize, unionSize);
            }
            finally
            {
                if (actualUnion != null) actualUnion.Dispose();
                if (intersection != null) intersection.Dispose();
                if (union != null) union.Dispose();
            }
        }

        public static void ConfusionMatrix(HObject actual, HObject reference,
            int referenceImageHeight, int referenceImageWidth,
            out int truePositives, out int trueNegatives,
            out int falsePositives, out int falseNegatives)
        {

            HObject difference = null; // == false positives (for precision), false negatives (for recall)
            HObject intersection = null; // == true positives
            truePositives = 0;
            trueNegatives = 0;
            falsePositives = 0;
            falseNegatives = 0;
            try
            {
                HOperatorSet.Union1(reference, out reference); // should be safe to dispose of reference and actual in finally
                HOperatorSet.Union1(actual, out actual);

                HTuple rows, columns, area;
                int totalNumPixels = referenceImageHeight * referenceImageWidth;
                
                // actual and reference pixel counts
                HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                int actualPixelCount = 0;
                if (area.Length > 0)
                    actualPixelCount = area;
                HOperatorSet.AreaCenter(reference, out area, out rows, out columns);
                int referencePixelCount = 0;
                if (area.Length > 0)
                    referencePixelCount = area;


                // compute true positives (TP)
                HOperatorSet.Intersection(actual, reference, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                    truePositives = area;


                // compute false negatives (FN)

                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                    falseNegatives = area;


                // compute true negatives

                trueNegatives = totalNumPixels - (actualPixelCount + referencePixelCount - truePositives); // TP == intersection

                // compute false positives
                //HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                //if (area.Length > 0)
                //   FP = area - TP;
                falsePositives = actualPixelCount - truePositives;
                if (truePositives < 0 || falsePositives < 0 || trueNegatives < 0 || falseNegatives < 0)
                    throw new ArithmeticException($"Invalid confusion matrix entry tp: {truePositives}," +
                                                  $" fp: {falsePositives}, tn: {trueNegatives}, fn: {falseNegatives}");

            }
            finally
            {
                if (difference != null) difference.Dispose();
                if (intersection != null) intersection.Dispose();
                if (reference != null) reference.Dispose(); // since we're overwriting reference with a union of itself
                if (actual != null) actual.Dispose(); // since we're overwriting actual with a union of itself
            }
        }

    }
}

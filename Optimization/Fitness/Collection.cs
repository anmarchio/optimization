using System;
using Extensions;

namespace Optimization.Fitness
{
    public static class Collection
    {
        #region common fitness functions based on double input - used by other methods

        public static double MCC(double tp, double tn, double fp, double fn)
        {
            var n = tp + tn + fp + fn;
            var s = (tp + fn) / n;
            var p = (tp + fp) / n;
            if (s == 1 || s == 0 || p == 1 || p == 0) return 0;
            var num = (tp / n - s * p);
            var den = (Math.Sqrt(p * s * (1 - p) * (1 - s)));
            return num.DivisionNotNaN(den);
        }
        
        public static double FBetaScore(double precision, double recall, double betaSquared)
        {
            if (recall > 0 || precision > 0)
                return (1 + betaSquared) * recall * precision / (recall + betaSquared * precision);
            return 0;
        }

        public static double Recall(double tp, double fn)
        {
            return tp.DivisionNotNaN(tp + fn);
        }

        public static double Precision(double tp, double fp)
        {
            return tp.DivisionNotNaN(tp + fp);
        }

        public static double IntersectionOverUnion(double intersectionSize, double unionSize)
        {
            return intersectionSize.DivisionNotNaN(unionSize);
        }

        public static double IntersectionOverUnion(int intersectionSize, int unionSize)
        {
            return IntersectionOverUnion((double) intersectionSize, unionSize);
        }

        #endregion

        public static double Accuracy(double tp, double tn, double fp, double fn)
        {
            return (tp + tn).DivisionNotNaN(tp + tn + fp + fn);
        }
    }
}

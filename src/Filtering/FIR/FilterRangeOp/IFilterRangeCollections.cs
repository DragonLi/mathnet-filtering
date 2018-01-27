using System.Collections.Generic;
using System.Text;

namespace MathNet.Filtering.FIR.FilterRangeOp
{
    public interface IFirFilterRangeCollections
    {
        IEnumerable<PrimitiveFilterRange> PrimitiveRanges { get; }
        double[] GetFirCoefficients(double sampleRate, int halfOrder);
        IFirFilterRangeCollections Add(PrimitiveFilterRange range);
    }

    public static class FilterRangeHelper
    {
        public static string Show(this IFirFilterRangeCollections col)
        {
            var builder = new StringBuilder();
            foreach (var range in col.PrimitiveRanges)
                builder.Append(range.Show()).Append(" ");

            return builder.ToString();
        }

        public static double[] Acc(this double[] acc, double[] other)
        {
            if (acc == null) return null;
            if (other == null) return null;
            for (var i = 0; i < acc.Length; i++)
                acc[i] += other[i];
            return acc;
        }

        public static string Show(this double[] coeff)
        {
            if (coeff==null) return "full range";
            var builder = new StringBuilder();
            foreach (var t in coeff)
                builder.Append(t).Append(" ");

            return builder.ToString();
        }
    }
}

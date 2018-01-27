using System.Collections.Generic;
using System.Text;

namespace MathNet.Filtering.FIR.FilterRangeOp
{
    public interface IFirFilterRangeCollections
    {
        IEnumerable<PrimitiveFilterRange> PrimitiveRanges { get; }
        double[] FirCoefficients { get; }
        IFirFilterRangeCollections Add(PrimitiveFilterRange range);
    }

    public static class FilterRangeHelper
    {
        public static string Show(this IFirFilterRangeCollections col)
        {
            var builder = new StringBuilder();
            foreach (var range in col.PrimitiveRanges)
            {
                builder.Append(range.Show());
                builder.Append(" ");
            }

            return builder.ToString();
        }
    }
}

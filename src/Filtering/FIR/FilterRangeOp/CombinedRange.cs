﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace MathNet.Filtering.FIR.FilterRangeOp
{
    public class CombinedRange:IFirFilterRangeCollections
    {
        private IFirFilterRangeCollections _passRanges;
        private IFirFilterRangeCollections _stopRanges;

        public CombinedRange(PassRangeBase passRange, BandStopRange range)
        {
            passRange.CheckRange(range);
            _passRanges = passRange;
            _stopRanges = range;
        }

        public CombinedRange(FilterPassRange passRange, BandStopRange range)
        {
            passRange.CheckRange(range);
            _passRanges = passRange;
            _stopRanges = range;
        }

        public CombinedRange(PassRangeBase passRange, FilterStopRange range)
        {
            range.CheckRange(passRange);
            _passRanges = passRange;
            _stopRanges = range;
        }

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges =>
            _passRanges.PrimitiveRanges.Concat(_stopRanges.PrimitiveRanges);

        public double[] GetFirCoefficients(double sampleRate, int halfOrder)
        {
            var first = _passRanges.GetFirCoefficients(sampleRate, halfOrder);
            return first?.Acc(_stopRanges.GetFirCoefficients(sampleRate, halfOrder));
        }

        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            if (range.IsPassType)
            {
                foreach (var pRange in _stopRanges.PrimitiveRanges)
                    pRange.CheckRange(range);
                _passRanges = _passRanges.Add(range);
            }
            else
            {
                foreach (var pRange in _passRanges.PrimitiveRanges)
                    pRange.CheckRange(range);
                _stopRanges = _stopRanges.Add(range);
            }
            return this;
        }
    }

    public class FilterPassRange : IFirFilterRangeCollections
    {
        private readonly List<PassRangeBase> _passRangeList;

        public FilterPassRange(PassRangeBase lowPassRange, PassRangeBase range)
        {
            _passRangeList = new List<PassRangeBase> {lowPassRange, range};
            _passRangeList.Sort(PassRangeBase.PassRangeComparator);
        }

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges => _passRangeList;

        public double[] GetFirCoefficients(double sampleRate, int halfOrder)
        {
            var acc = new double[2*halfOrder+1];
            foreach (var t in _passRangeList)
            {
                var coeff=t.GetFirCoefficients(sampleRate, halfOrder);
                if (coeff == null) return null;
                acc.Acc(coeff);
            }

            return acc;
        }

        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            return range.Add(this);
        }

        public IFirFilterRangeCollections Merge(PassRangeBase other)
        {
            var tmpPassRangeList = new List<PassRangeBase>();
            var lastInd = _passRangeList.Count;
            for (var i = 0; i < _passRangeList.Count; i++)
            {
                var range = _passRangeList[i];
                var compare = range.Compare(other);
                if (compare < 0)
                    tmpPassRangeList.Add(range);
                if (compare == 0)
                {
                    other = MergeOverlap(range,other);
                    if (other == AllRange.Instance)
                        return other;
                }

                if (compare > 0)
                {
                    lastInd = i;
                    break;
                }
            }
            tmpPassRangeList.Add(other);
            for (var j = lastInd; j < _passRangeList.Count; j++)
                tmpPassRangeList.Add(_passRangeList[j]);
            _passRangeList.Clear();
            _passRangeList.AddRange(tmpPassRangeList);
            return this;
        }

        private static PassRangeBase MergeOverlap(PassRangeBase range, PassRangeBase other)
        {
            var min = (int)Math.Min(range.Min, other.Min);
            var max = Math.Max(range.Max, other.Max);
            if (max < double.PositiveInfinity)
            {
                var intmax = (int)max;
                if (min > 0)
                    return new BandPassRange(min,intmax);
                return new LowPassRange(intmax);
            }
            if (min == 0)
                return AllRange.Instance;
            return new HighPassRange(min);
        }

        public void CheckRange(BandStopRange range)
        {
            foreach (var pass in _passRangeList)
                pass.CheckRange(range);
        }
    }

    public class FilterStopRange : IFirFilterRangeCollections
    {
        private readonly List<BandStopRange> _stopRangeList;

        public FilterStopRange(BandStopRange bandStopRange, BandStopRange range)
        {
            _stopRangeList=new List<BandStopRange>{bandStopRange,range};
            _stopRangeList.Sort(BandStopRange.Compare);
        }

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges => _stopRangeList;

        public double[] GetFirCoefficients(double sampleRate, int halfOrder)
        {
            var acc = new double[2*halfOrder+1];
            foreach (var t in _stopRangeList)
            {
                var coeff=t.GetFirCoefficients(sampleRate, halfOrder);
                if (coeff == null) return null;
                acc.Acc(coeff);
            }

            return acc;
        }

        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            return range.Add(this);
        }

        public void Merge(BandStopRange other)
        {
            var tmpStopList=new List<BandStopRange>();
            var lastInd = _stopRangeList.Count;
            for (var i = 0; i < _stopRangeList.Count; i++)
            {
                var stop = _stopRangeList[i];
                var compare = stop.Compare(other);
                if (compare < 0)
                    tmpStopList.Add(stop);
                if (compare == 0)
                    other = MergeOverlap(stop, other);

                if (compare > 0)
                {
                    lastInd = i;
                    break;
                }
            }
            tmpStopList.Add(other);
            for (var j = lastInd; j < _stopRangeList.Count; j++)
                tmpStopList.Add(_stopRangeList[j]);
            _stopRangeList.Clear();
            _stopRangeList.AddRange(tmpStopList);
        }

        private static BandStopRange MergeOverlap(BandStopRange stop, BandStopRange other)
        {
            var min = Math.Min(stop.LowPassRate, other.LowPassRate);
            var max = Math.Max(stop.HighPassRate, other.HighPassRate);
            return new BandStopRange(min,max);
        }

        public void CheckRange(PassRangeBase passRange)
        {
            foreach (var t in _stopRangeList)
                passRange.CheckRange(t);
        }
    }
}

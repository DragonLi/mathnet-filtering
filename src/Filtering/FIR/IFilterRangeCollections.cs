using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Filtering.FIR
{
    public interface IFirFilterRangeCollections
    {
        IEnumerable<PrimitiveFilterRange> PrimitiveRanges { get; }
        double[] FirCoefficients { get; }
        IFirFilterRangeCollections Add(PrimitiveFilterRange range);
    }

    public abstract class PrimitiveFilterRange : IFirFilterRangeCollections
    {
        private readonly PrimitiveFilterRange[] _primitiveRanges;

        protected PrimitiveFilterRange()
        {
            _primitiveRanges = new[] {this};
        }

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges => _primitiveRanges;

        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            if (range == null) throw new ArgumentNullException(nameof(range));
            switch (range)
            {
                case LowPassRange t1:
                    return Add(t1);
                case HighPassRange t2:
                    return Add(t2);
                case BandWithRange t3:
                    return Add(t3);
                case BandStopRange t4:
                    return Add(t4);
            }
            throw new NotImplementedException($"can not add this range type:{range.GetType()}");
        }

        public void CheckRange(PrimitiveFilterRange range)
        {
            if (!(range.IsPassType ^ IsPassType)) return;
            if (this is PassRangeBase thizpass)
                thizpass.CheckRange(range);
            if (range is PassRangeBase pass)
            {
                pass.CheckRange(this);
            }
        }

        public abstract bool IsPassType { get; }

        protected abstract IFirFilterRangeCollections Add(LowPassRange range);
        protected abstract IFirFilterRangeCollections Add(HighPassRange range);
        protected abstract IFirFilterRangeCollections Add(BandWithRange range);
        protected abstract IFirFilterRangeCollections Add(BandStopRange range);
        public abstract double[] FirCoefficients { get; }
    }

    public abstract class PassRangeBase : PrimitiveFilterRange
    {
        public new void CheckRange(PrimitiveFilterRange range)
        {
         if (range is BandStopRange stop)
             CheckRange(stop);
        }
        public override bool IsPassType => true;

        public abstract double Max { get; }
        public abstract double Min { get; }
        public abstract void CheckRange(BandStopRange range);
    }

    /// range:[0,lowPassRate], half->normalize(lowPassRate)
    public class LowPassRange:PassRangeBase
    {
        private readonly int _lowPassRate;
        public int LowPassRate => _lowPassRate;

        /// range:[0,lowPassRate], half->normalize(lowPassRate)
        public LowPassRange(int lowPassRate)
        {
            if(lowPassRate<0)throw new ArgumentException(nameof(lowPassRate));
            _lowPassRate = lowPassRate;
        }

        public override double[] FirCoefficients { get; }
        protected override IFirFilterRangeCollections Add(LowPassRange range)
        {
            return range._lowPassRate > _lowPassRate ? range : this;
        }

        protected override IFirFilterRangeCollections Add(HighPassRange range)
        {
            if (range.HighPassRate <= _lowPassRate) return AllRange.Instance;
            return new FilterPassRange(this, range);
        }

        protected override IFirFilterRangeCollections Add(BandWithRange range)
        {
            if (_lowPassRate < range.LowCutoffRate) return new FilterPassRange(this, range);
            if (_lowPassRate < range.HighCutoffRate) return new LowPassRange(range.HighCutoffRate);
            return this;
        }

        protected override IFirFilterRangeCollections Add(BandStopRange range)
        {
            return new CombinedRange(this,range);
        }

        public override double Max => _lowPassRate;

        public override double Min => 0;

        public override void CheckRange(BandStopRange range)
        {
            if (range.LowPassRate <= _lowPassRate)
                throw new ArgumentException($"Pass/Stop range overlap: {range.LowPassRate}~{_lowPassRate}");
        }
    }

    /// range:[highPassRate,Infinity), half->1-normalize(highPassRate)
    public class HighPassRange:PassRangeBase
    {
        private readonly int _highPassRate;

        /// range:[highPassRate,Infinity), half->1-normalize(highPassRate)
        public HighPassRange(int highPassRate)
        {
            if(highPassRate<0)throw new ArgumentException(nameof(highPassRate));
            _highPassRate = highPassRate;
        }

        public override double[] FirCoefficients { get; }
        protected override IFirFilterRangeCollections Add(LowPassRange range)
        {
            return range.Add(this);
        }

        protected override IFirFilterRangeCollections Add(HighPassRange range)
        {
            return _highPassRate > range._highPassRate ? range : this;
        }

        protected override IFirFilterRangeCollections Add(BandWithRange range)
        {
            if (_highPassRate > range.HighCutoffRate) return new FilterPassRange(range,this);
            if (_highPassRate > range.LowCutoffRate) return new HighPassRange(range.LowCutoffRate);
            return this;
        }

        protected override IFirFilterRangeCollections Add(BandStopRange range)
        {
            return new CombinedRange(this,range);
        }

        public override double Max => double.PositiveInfinity;

        public override double Min => _highPassRate;

        public override void CheckRange(BandStopRange range)
        {
            if (range.HighPassRate >= _highPassRate)
                throw new ArgumentException($"Pass/Stop range overlap: {_highPassRate}~{range.HighPassRate}");
        }

        public int HighPassRate => _highPassRate;
    }

    /// range:[lowCutoffRate,highCutoffRate], half-> normalize(highCutoffRate)-normalize(lowCutoffRate)
    public class BandWithRange:PassRangeBase
    {
        private readonly int _lowCutoffRate;
        private readonly int _highCutoffRate;
        public int LowCutoffRate => _lowCutoffRate;
        public int HighCutoffRate => _highCutoffRate;

        /// range:[lowCutoffRate,highCutoffRate], half-> normalize(highCutoffRate)-normalize(lowCutoffRate)
        public BandWithRange(int lowCutoffRate, int highCutoffRate)
        {
            if (lowCutoffRate<0)throw new ArgumentException(nameof(lowCutoffRate));
            if (highCutoffRate<0)throw new ArgumentException(nameof(highCutoffRate));
            if (lowCutoffRate>highCutoffRate)
                throw new ArgumentException($"range error:lowCutoffRate({lowCutoffRate}) > highCutoffRate({highCutoffRate})");
            _lowCutoffRate = lowCutoffRate;
            _highCutoffRate = highCutoffRate;
        }

        public override double[] FirCoefficients { get; }
        protected override IFirFilterRangeCollections Add(LowPassRange range)
        {
            return range.Add(this);
        }

        protected override IFirFilterRangeCollections Add(HighPassRange range)
        {
            return range.Add(this);
        }

        protected override IFirFilterRangeCollections Add(BandWithRange range)
        {
            if (_lowCutoffRate <= range._lowCutoffRate && range._highCutoffRate <= _highCutoffRate) return this;
            if (range._lowCutoffRate <= _lowCutoffRate && _highCutoffRate <= range._highCutoffRate) return range;
            return new FilterPassRange(this,range);
        }

        protected override IFirFilterRangeCollections Add(BandStopRange range)
        {
            return new CombinedRange(this,range);
        }

        public override double Max => _highCutoffRate;

        public override double Min => _lowCutoffRate;

        public override void CheckRange(BandStopRange range)
        {
            if (_lowCutoffRate<=range.LowPassRate&& range.HighPassRate <=_highCutoffRate)
                throw new ArgumentException($"Pass/Stop range overlap: {range.LowPassRate}~{range.HighPassRate}");
            if (_lowCutoffRate<=range.LowPassRate&& _highCutoffRate <=range.HighPassRate)
                throw new ArgumentException($"Pass/Stop range overlap: {range.LowPassRate}~{_highCutoffRate}");
            if (range.LowPassRate<=_lowCutoffRate && _highCutoffRate<=range.HighPassRate)
                throw new ArgumentException($"Pass/Stop range overlap: {_lowCutoffRate}~{_highCutoffRate}");
            if (range.LowPassRate<=_lowCutoffRate && range.HighPassRate<=_highCutoffRate)
                throw new ArgumentException($"Pass/Stop range overlap: {_lowCutoffRate}~{range.HighPassRate}");
        }
    }

    /// range:[0,lowPassRate]+[highPassRate,infinity), half->1-(normalize(highPassRate)-normalize(lowPassRate))
    public class BandStopRange:PrimitiveFilterRange
    {
        private int _lowPassRate;
        private int _highPassRate;
        public int HighPassRate => _highPassRate;
        public int LowPassRate => _lowPassRate;

        /// range:[0,lowPassRate]+[highPassRate,infinity), half->1-(normalize(highPassRate)-normalize(lowPassRate))
        public BandStopRange(int lowPassRate, int highPassRate)
        {
            _lowPassRate = lowPassRate;
            _highPassRate = highPassRate;
        }

        public override double[] FirCoefficients { get; }
        public override bool IsPassType => false;

        protected override IFirFilterRangeCollections Add(LowPassRange range) => new CombinedRange(range,this);

        protected override IFirFilterRangeCollections Add(HighPassRange range) => new CombinedRange(range,this);

        protected override IFirFilterRangeCollections Add(BandWithRange range) => new CombinedRange(range,this);

        protected override IFirFilterRangeCollections Add(BandStopRange range)
        {
            if (_lowPassRate <= range._lowPassRate && range._highPassRate <= _highPassRate) return this;
            if (range._lowPassRate <= _lowPassRate && _highPassRate <= range._highPassRate) return range;
            return new FilterStopRange(this,range);
        }
    }

    public class FilterPassRange : IFirFilterRangeCollections
    {
        private readonly List<PassRangeBase> _passRangeList;
        public FilterPassRange(PassRangeBase lowPassRange, PassRangeBase range)
        {
            _passRangeList = new List<PassRangeBase> {lowPassRange, range};
            _passRangeList.Sort(PassRangeComparator);
        }

        //     比较两个对象并返回指示一个是否小于、 等于还是大于另一个值。
        //   x:
        //     要比较的第一个对象。
        //   y:
        //     要比较的第二个对象。
        // 返回结果:
        //     一个有符号整数，指示 x 和 y 的相对值，如下表所示。 值 含义 小于零 x 小于 y。 零 x 等于 y。 大于零 x 大于 y。
        private int PassRangeComparator(PassRangeBase x, PassRangeBase y)
        {
            if (x.Max < y.Min) return -1;
            if (x.Min > y.Max) return 1;
            return 0;//overlap
        }

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges => _passRangeList;
        public double[] FirCoefficients { get; }
        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            switch (range)
            {
                case BandStopRange stop:
                    return new CombinedRange(this,stop);
                case PassRangeBase pass:
                    throw new NotImplementedException();
                    _passRangeList.Add(pass);
                    _passRangeList.Sort(PassRangeComparator);
                    break;
            }
            throw new ArgumentOutOfRangeException($"{range.GetType()} is not supported");
        }

        public void CheckRange(BandStopRange range)
        {
            foreach (var pass in _passRangeList)
            {
                pass.CheckRange(range);
            }
        }
    }

    public class FilterStopRange : IFirFilterRangeCollections
    {
        private readonly List<BandStopRange> _stopRangeList;
        public FilterStopRange(BandStopRange bandStopRange, BandStopRange range)
        {
            _stopRangeList=new List<BandStopRange>{bandStopRange,range};
            _stopRangeList.Sort(StopRangeComparator);
        }

        private int StopRangeComparator(BandStopRange x, BandStopRange y)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges => _stopRangeList;
        public double[] FirCoefficients { get; }
        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            switch (range)
            {
                case BandStopRange stop:
                case PassRangeBase pass:
                    throw new NotImplementedException();
                    break;
            }
            throw new ArgumentOutOfRangeException($"{range.GetType()} is not supported");
        }
    }

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

        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges =>
            _passRanges.PrimitiveRanges.Concat(_stopRanges.PrimitiveRanges);

        public double[] FirCoefficients { get; }
        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            if (range.IsPassType)
            {
                foreach (var pRange in _stopRanges.PrimitiveRanges)
                {
                    pRange.CheckRange(range);
                }
                _passRanges = _passRanges.Add(range);
            }
            else
            {
                foreach (var pRange in _passRanges.PrimitiveRanges)
                {
                    pRange.CheckRange(range);
                }
                _stopRanges = _stopRanges.Add(range);
            }
            return this;
        }
    }

    public class AllRange : IFirFilterRangeCollections
    {
        public IEnumerable<PrimitiveFilterRange> PrimitiveRanges => null;
        public double[] FirCoefficients => null;
        private static readonly AllRange _instance = new AllRange();
        public static IFirFilterRangeCollections Instance => _instance;
        private AllRange(){}

        public IFirFilterRangeCollections Add(PrimitiveFilterRange range)
        {
            return range;
        }
    }
}

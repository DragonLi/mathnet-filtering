using System.Collections.Generic;

namespace MathNet.Filtering.Median
{
    public class OnlineFastMedianFilter:OnlineFilter
    {
        private readonly int _halfWinSize;
        private bool _bufferFull;
        private readonly List<double> _orderCache;
        private readonly int _size;

        public OnlineFastMedianFilter(int halfWinSize)
        {
            _halfWinSize = halfWinSize;
            _size = 2*halfWinSize+1;
            _orderCache = new List<double>(_size);
        }

        public override double ProcessSample(double sample)
        {
            if (_bufferFull)
            {
                //replace last one in cache by current sample
                _orderCache[_halfWinSize] = sample;
            }
            else
            {
                _orderCache.Add(sample);
                if (_orderCache.Count == _size)
                    _bufferFull = true;
            }
            _orderCache.Sort();
            var mid = _orderCache.Count / 2;
            if (mid * 2 == _orderCache.Count)
            {
                return (_orderCache[mid - 1] + _orderCache[mid]) / 2;
            }
            return _orderCache[mid];
        }

        public override void Reset()
        {
            _bufferFull = false;
            _orderCache.Clear();
        }
    }
}

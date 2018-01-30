using System.Collections.Generic;

namespace MathNet.Filtering.Median
{
    public class OnlineFastMedianFilter:OnlineFilter
    {
        private bool _bufferFull;
        private readonly List<double> _orderCache;
        private readonly int _size;
        private readonly double[] _buffer;
        private int _offset;

        public OnlineFastMedianFilter(int halfWinSize)
        {
            _size = 2*halfWinSize+1;
            _orderCache = new List<double>(_size);
            _buffer = new double[_size];
        }

        public override double ProcessSample(double sample)
        {
            _offset = (_offset == 0) ? _buffer.Length - 1 : _offset - 1;
            if (_bufferFull)
            {
                //replace last one in cache by current sample
                var lastIndex = _orderCache.BinarySearch(_buffer[_offset]);
                _orderCache[lastIndex] = sample;
            }
            else
            {
                _orderCache.Add(sample);
                if (_orderCache.Count == _size)
                    _bufferFull = true;
            }
            _orderCache.Sort();
            _buffer[_offset] = sample;
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

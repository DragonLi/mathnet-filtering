namespace MathNet.Filtering
{
    public class SeqCombinedOnlineFilter:IOnlineFilter
    {
        private IOnlineFilter[] _lst;

        public SeqCombinedOnlineFilter(IOnlineFilter[] lst)
        {
            _lst = lst;
        }

        public double ProcessSample(double sample)
        {
            for (var i = 0; i < _lst.Length; i++)
            {
                var filter = _lst[i];
                sample=filter.ProcessSample(sample);
            }

            return sample;
        }

        public double[] ProcessSamples(double[] samples)
        {
            var result = new double[samples.Length];
            for (var i = 0; i < samples.Length; i++)
            {
                result[i] = ProcessSample(samples[i]);
            }

            return result;
        }

        public void Reset()
        {
            for (var i = 0; i < _lst.Length; i++)
            {
                _lst[i].Reset();
            }
        }
    }
}

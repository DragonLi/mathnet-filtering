using System;
using MathNet.Filtering.FIR.FilterRangeOp;

namespace TestFilterConsole
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var low = PrimitiveFilterRange.CreateLowPassRange(10);
            var with = PrimitiveFilterRange.CreateBandWithRange(20, 30);
            var high = PrimitiveFilterRange.CreateHighPassRange(10000);
            var stop = PrimitiveFilterRange.CreatBandStopRange(35,38);
            var mix = low.Add(with).Add(high).Add(stop);
            PrintRange(mix);

            mix = low.Add(with).Add(stop).Add(high);
            PrintRange(mix);

            mix = with.Add(low).Add(high).Add(stop);
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(20, 30));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(30, 31));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(50, 100));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(1, 30));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(310, 400));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(310, 400));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(390, 500));
            PrintRange(mix);

            mix = low.Add(with).Add(high);
            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(10, 30));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(30,10000));
            PrintRange(mix);

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(30,10000));
            PrintRange(mix);

            try
            {
                mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(60, 80));
                PrintRange(mix);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }

        private static void PrintRange(IFirFilterRangeCollections mix)
        {
            Console.WriteLine(mix.Show());
            Console.WriteLine(mix.GetFirCoefficients(500, 2).Show());
        }
    }
}

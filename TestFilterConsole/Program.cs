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
            Console.WriteLine(mix.Show());

            mix = low.Add(with).Add(stop).Add(high);
            Console.WriteLine(mix.Show());

            mix = with.Add(low).Add(high).Add(stop);
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(20, 30));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(30, 31));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(50, 100));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(1, 30));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(310, 400));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(310, 400));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(390, 500));
            Console.WriteLine(mix.Show());

            mix = low.Add(with).Add(high);
            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(10, 30));
            Console.WriteLine(mix.Show());

            mix = mix.Add(PrimitiveFilterRange.CreateBandWithRange(30,10000));
            Console.WriteLine(mix.Show());

            try
            {
                mix = mix.Add(PrimitiveFilterRange.CreatBandStopRange(60, 80));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.ReadLine();
        }
    }
}

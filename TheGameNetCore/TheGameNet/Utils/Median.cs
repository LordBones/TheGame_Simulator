using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Utils
{
    public class Median<T>
    {
        List<T> _data = new List<T>();

        public void Add(T data)
        {
            _data.Add(data);
        }

        public MedianResult<T> Get_Median()
        {
            this._data.Sort();

            MedianResult<T> result = new MedianResult<T>();

            result.Median = this._data[(this._data.Count - 1) / 2];

            var kk = (from x in this._data
                      group x by x into xGroup

                      select new { xGroup.Key, Count = xGroup.Count() }).ToArray();

            result.MostCount = kk.OrderByDescending(x => x.Count).First().Key;

            return result;
        }

        public void PrintData(TextWriter console)
        {
            this._data.Sort();
            console.WriteLine("Median");
            for (int i = 0; i < this._data.Count; i++)
            {
                console.WriteLine($"{this._data[i]},");
            }
        }

        public void PrintDataGroup(TextWriter console, int baseForPercent = 0)
        {
            var kk = (from x in this._data
                      group x by x into xGroup

                      select new { xGroup.Key, Count = xGroup.Count() }).ToArray();

            var kkResult = kk.OrderBy(x => x.Key).ToArray();

            int totalSum =(kkResult.Length > 0)? kkResult.Max(x => x.Count) : 0; 


            if (baseForPercent <= 0)
            {
                baseForPercent = totalSum;
            }

            StringBuilder sb = new StringBuilder();

            console.WriteLine("GroupCounts");
            foreach (var item in kkResult)
            {
                sb.Clear();
                int countBar = Helper_ComputeBarGraph(60, totalSum, item.Count);
                while(countBar > 0)
                {
                    sb.Append("#");
                    countBar--;
                }

                double percent = item.Count*100 / (double)baseForPercent;
                string percentStr = string.Format("{0:###.00} %", percent).PadLeft(8);

                console.Write($"{item.Key,3} : {percentStr} - {item.Count,6}   | ");

                foreach (var item2 in sb.GetChunks())
                {
                    console.Write(item2.Span);
                }

                console.WriteLine();
            }
        }

        private int Helper_ComputeBarGraph(int maxChars, int totalCount, int currentCount)
        {
            if (currentCount == 0) return 0;
            return (maxChars * currentCount) / totalCount;
        }


        public void Clear() => _data.Clear();
    }

    public struct MedianResult<T>
    {
        public T Median;
        public T MostCount;
        public double Std;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.FNN
{
    public class EvolveProgress
    {
        readonly struct ProgressItem
        {
            public int Min { get; }
            public int Avg { get; }
            public int Max { get; }

            public ProgressItem(int min, int max, int avg)
            {
                Min = min;
                Max = max;
                Avg = avg;
            }

        }

        private int _step;
        private int _stepSum;



        List<GroupProgressItem> _progress = new List<GroupProgressItem>();

        List<ProgressItem> _gameResultsPartial = new List<ProgressItem>();

        struct GroupProgressItem
        {
            public int Step;
            public float Avg;
            public int Min;
            public int Max;
            public float SpreadUp;
            public float SpreadDown;
        }

        public EvolveProgress(int step)
        {
            _step = step;
        }

        public void Clear()
        {
            _gameResultsPartial.Clear();
            _progress.Clear();
        }

        public void Update(int min, int max, int avg)
        {
            _gameResultsPartial.Add(new ProgressItem(min,max, avg));

            if (_gameResultsPartial.Count == _step)
            {
                ComputeProgressItem();
                _gameResultsPartial.Clear();
            }
        }

        private void ComputeProgressItem()
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            int sum = 0;


            for (int i = 0; i < _gameResultsPartial.Count; i++)
            {
                var tmp = _gameResultsPartial[i];

                if (min > tmp.Min) min = tmp.Min;
                if (max < tmp.Max) max = tmp.Max;

                sum += tmp.Avg;
            }


            float avg = (sum / (float)_gameResultsPartial.Count);

            float sumSpreadUp = 0.0f;
            float sumSpreadDown = 0.0f;
            int countSpreadUp = 0;
            int countSpreadDown = 0;

            for (int i = 0; i < _gameResultsPartial.Count; i++)
            {
                int tmp = _gameResultsPartial[i].Avg;

                if (tmp < avg)
                {
                    sumSpreadDown += tmp;
                    countSpreadDown++;
                }
                if (tmp > avg)
                {
                    countSpreadUp++;
                    sumSpreadUp += tmp;
                }
            }


            _progress.Add(new GroupProgressItem()
            {
                Step = _stepSum,
                Max = max,
                Min = min,
                Avg = avg,
                SpreadDown = (countSpreadDown > 0) ? sumSpreadDown / countSpreadDown : avg,
                SpreadUp = (countSpreadUp > 0) ? sumSpreadUp / countSpreadUp : avg

            });

            _stepSum += _step;
        }


        public void PrintProgress(TextWriter tw)
        {
            char[] graph = new char[101];

            foreach (var item in _progress)
            {

                tw.Write($"step: {item.Step,8} min: {item.Min,3} avg: {item.Avg.ToString("0.###"),7} max: {item.Max,3}  | ");


                for (int i = 0; i < graph.Length; i++)
                {
                    graph[i] = ' ';
                }

                graph[(byte)(100 - item.SpreadDown)] = ']';
                graph[(byte)(100 - item.SpreadUp)] = '[';

                graph[100 - item.Min] = '<';
                graph[100 - item.Max] = '>';
                graph[(byte)(100 - item.Avg)] = '|';



                tw.Write(graph);

                tw.Write(" #");
                tw.WriteLine();
            }
        }

        private void PrintLastXGroup(TextWriter tw)
        {
            char[] graph = new char[101];

            foreach (var item in _progress)
            {

                tw.Write($"step: {item.Step,8} min: {item.Min,3} avg: {item.Avg.ToString("0.###"),7} max: {item.Max,3}  | ");


                for (int i = 0; i < graph.Length; i++)
                {
                    graph[i] = ' ';
                }

                graph[(byte)(100 - item.SpreadDown)] = ']';
                graph[(byte)(100 - item.SpreadUp)] = '[';

                graph[100 - item.Min] = '<';
                graph[100 - item.Max] = '>';
                graph[(byte)(100 - item.Avg)] = '|';



                tw.Write(graph);

                tw.Write(" #");
                tw.WriteLine();
            }
        }
    }
}

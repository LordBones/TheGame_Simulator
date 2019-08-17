using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core
{
    internal class GameProgress
    {
        private int _step;
        private int _stepSum;

        List<GameProgressItem> _progress = new List<GameProgressItem>();

        List<byte> _gameResultsPartial = new List<byte>();

        struct GameProgressItem
        {
            public int Step;
            public float Avg;
            public byte Min;
            public byte Max;
        }

        public GameProgress(int step)
        {
            _step = step;
        }

        public void Update(byte gameScore)
        {
            _gameResultsPartial.Add(gameScore);

            if(_gameResultsPartial.Count == _step)
            {
                ComputeProgressItem();
                _gameResultsPartial.Clear();
            }
        }

        private void ComputeProgressItem()
        {
            byte min = byte.MaxValue;
            byte max = byte.MinValue;
            int sum = 0;


            for (int i = 0;i < _gameResultsPartial.Count; i++)
            {
                byte tmp = _gameResultsPartial[i];

                if (min > tmp) min = tmp;
                if (max < tmp) max = tmp;

                sum += tmp;
            }

            _progress.Add(new GameProgressItem()
            {
                Step = _stepSum,
                Max = max,
                Min = min,
                Avg = (sum / (float)_gameResultsPartial.Count)
            });

            _stepSum += _step;
        }


        public void PrintProgress(TextWriter tw)
        {
            char[] graph = new char[101];

            foreach(var item in _progress)
            {

                tw.Write($"step: {item.Step,8} min: {item.Min,3} avg: {item.Avg.ToString("0.###"),7} max: {item.Max,3}  | ");


                for(int i = 0;i < graph.Length; i++)
                {
                    graph[i] = ' ';
                }

                graph[100-item.Min] = '<';
                graph[100-item.Max] = '>';
                graph[(byte)(100-item.Avg)] = '|';

                tw.Write(graph);

                tw.Write(" #");
                tw.WriteLine();
            }
        }
    }
}

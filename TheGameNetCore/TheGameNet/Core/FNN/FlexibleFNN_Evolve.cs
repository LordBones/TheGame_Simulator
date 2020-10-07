using BonesLib.FlexibleForwardNN;
using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.Players;

namespace TheGameNet.Core.FNN
{
    public class FlexibleFNN_Evolve
    {
        private float _mutationChance;
        private int _populationSize;

        private FixList<FlexibleForwardNN> _fnn_pool;

        private FlexibleFNN_LayerManipulator _fnn_manipulator;

        private float _best_fitness;
        private int _best_countCards;
        private FlexibleForwardNN _best_fnn;

        private float[] _pop_Fitness;
        private int[] _pop_CountCards;
        
        private FlexibleForwardNN[] _pop_generation;
        private FlexibleForwardNN[] _pop_generation_tmp;

        private RandomGen _rnd;

        private EvolveProgress _evolveProgress = new EvolveProgress(10);
        // 0 - 1.0
        private readonly double coefSelection = 1.0;
        private float _learningRateInterval = 200;
        private float _learningRateCounter = 0;

        private System.IO.StreamWriter _progressLog = System.IO.StreamWriter.Null;

        public FlexibleFNN_Evolve(float mutation, int populationSize)
        {
            _mutationChance = mutation;
            _populationSize = populationSize;
            _pop_Fitness = new float[populationSize];
            _pop_generation = new FlexibleForwardNN[populationSize];
            _pop_generation_tmp = new FlexibleForwardNN[populationSize];
            _pop_CountCards = new int[populationSize];
            _fnn_manipulator = new FlexibleFNN_LayerManipulator(0);
            _rnd = new RandomGen(0);

            _tgs_players = RoundSimulations.RoundSimulator.CreatePlayers<Player_FlexibleFNN>(namePlayers);

            var _deckGen = new DeckGenerator(100, 0);
            _deckTestBatch = new DecksTestBatch(5, _deckGen);

        }

        public void Enable_Log(System.IO.StreamWriter gameRunLog)
        {
            this._progressLog = gameRunLog ?? System.IO.StreamWriter.Null;
        }

        public void RunEvolution(int generations, FlexibleForwardNN fnn, System.IO.StreamWriter evolveProgress)
        {
            evolveProgress = evolveProgress ?? System.IO.StreamWriter.Null;
            _evolveProgress.Clear();
            _best_fitness = int.MaxValue;
            _deckBatchDurabilityCounter = _deckBatchDurability;

            CreatePoolFnn(fnn);

            InitStartPop(fnn);
            ComputeFitnesses();
            UpdateStats();

            for (int i = 0; i < generations; i++)
            {
                GenerateNewPop();
                ComputeFitnesses();
                UpdateStats();
                if (_learningRateInterval <= _learningRateCounter)
                    _learningRateCounter = 0;
                else
                    _learningRateCounter++;
            }

            PrintProgress(evolveProgress);

        }

        private void CreatePoolFnn(FlexibleForwardNN fnn)
        {


            _fnn_pool = new FixList<FlexibleForwardNN>(_populationSize * 2 + 1);

            for (int i = 0; i < _fnn_pool.MaxSize; i++)
            {
                _fnn_pool.Add(fnn.Clone());
            }

            if (_best_fnn == null)
            {
                _best_fnn = _fnn_pool.Pop();
                fnn.CopyTo(_best_fnn);
            }
        }

        private void InitStartPop(FlexibleForwardNN fnn)
        {
            var newModelNNx = _fnn_pool.Pop();
            fnn.CopyTo(newModelNNx);
            _pop_generation[0] = newModelNNx;


            for (int i = 1; i < _populationSize; i++)
            {
                var newModelNN = _fnn_pool.Pop();
                _fnn_manipulator.InitRandomTopology(newModelNN.Layers);
                //fnn.CopyTo(newModelNN);

                //ApplyMutation(newModelNN);

                _pop_generation[i] = newModelNN;
            }
        }



        private void ApplyMutation(FlexibleForwardNN fnn)
        {
            float learningRate

                = 1.0f;
                //= 0.001f+ (1.0f-_learningRateCounter / (float)_learningRateInterval);

            int countMutations =
                 1;
             //(fnn.Layers.NeuronLinks.Length / 50) + 1;

            for (int i = 0; i < countMutations; i++)
            {
                _fnn_manipulator.MutateWeight(fnn.Layers,  learningRate);
            }
        }

        TheGameSimulator tgs = new TheGameSimulator(null);

        private static string[] namePlayers = new string[] { "ferda" };
        private List<Player> _tgs_players;

        public FlexibleForwardNN Best_fnn { get => _best_fnn; set => _best_fnn = value; }
        public float Best_fitness { get => _best_fitness; set => _best_fitness = value; }

        
        private int _deckBatchDurability = 100;
        private int _deckBatchDurabilityCounter = 0;
        private int _elitismCountPercent = 5;
        private int _discardWorsePercent = 0;

        DecksTestBatch _deckTestBatch = null;

        private int Helper_GetElitsmCount()
        {
            return (_populationSize * _elitismCountPercent) / 100;
        }

        private int Helper_GetDiscardCount()
        {
            return (_populationSize * _discardWorsePercent) / 100;
        }
        private void ComputeFitnesses()
        {
            CheckAndGensDeck();

          //  var players = RoundSimulations.RoundSimulator.CreatePlayers<Player_FlexibleFNN>(namePlayers);
            tgs.SetPlayers(_tgs_players);

            var decks = _deckTestBatch.Decks;

            //var deck = deckGen.Get_CreatedSuffledDeck();
            for (int i = 0; i < _populationSize; i++)
            {
                foreach (var player in _tgs_players)
                {
                    ((Player_FlexibleFNN)player).Fnn = _pop_generation[i];
                }

                int fittness = 0;

                for (int deckIndex = 0; deckIndex < decks.Length; deckIndex++)
                {
                    var deck = decks[deckIndex];
                    var resultGame = tgs.Simulate(deck);
                    int rest = 0;
                    rest += tgs.GameBoard.CardPlaceholdersULight[0].Get_CardDiff_FromBase(tgs.GameBoard.CardPlaceholdersULight[0].GetPeakCard());
                    rest += tgs.GameBoard.CardPlaceholdersULight[1].Get_CardDiff_FromBase(tgs.GameBoard.CardPlaceholdersULight[1].GetPeakCard());
                    rest += tgs.GameBoard.CardPlaceholdersULight[2].Get_CardDiff_FromBase(tgs.GameBoard.CardPlaceholdersULight[2].GetPeakCard());
                    rest += tgs.GameBoard.CardPlaceholdersULight[3].Get_CardDiff_FromBase(tgs.GameBoard.CardPlaceholdersULight[3].GetPeakCard());

                    fittness += resultGame.Rest_Cards* resultGame.Rest_Cards ;
                }

                float f = fittness / (float)decks.Length;
                _pop_Fitness[i] = f;
            }
        }

        private void CheckAndGensDeck()
        {
            if (_deckBatchDurabilityCounter == _deckBatchDurability)
            {
                _deckTestBatch.Refresh_Decks();

                //if (_deckTestBatch.Is_Full)
                //    _deckTestBatch.Refresh_Decks();
                //else
                //    _deckTestBatch.Add_Deck();


                _deckBatchDurabilityCounter = 0;
                _best_fitness = int.MaxValue;
            }

            _deckBatchDurabilityCounter++;
        }

        private void UpdateStats()
        {
            float minFitt = float.MaxValue;
            float maxFitt = float.MinValue;
            float sumFitt = 0;

            for (int p = 0; p < _populationSize; p++)
            {
                float popFitt = _pop_Fitness[p];
                float popFittOne = popFitt ;

                if (Helper_NormalizeFittness(popFitt) < _best_fitness)
                {
                    _best_fitness = Helper_NormalizeFittness(popFitt);
                    _pop_generation[p].CopyTo(_best_fnn);
                    //_best_fnn = _pop_generation[p];
                }


                if (minFitt > popFittOne)
                    minFitt = popFittOne;
                if (maxFitt < popFittOne)
                    maxFitt = popFittOne;

                sumFitt += popFittOne;

            }

            int progressFitt = (int)Math.Round(Helper_NormalizeFittness(sumFitt / (_populationSize)));
            int progressMinFitt = (int)Math.Floor(Helper_NormalizeFittness(minFitt));
            int progressMaxFitt = (int)Math.Ceiling(Helper_NormalizeFittness(maxFitt));

            _evolveProgress.Update(progressMinFitt, progressMaxFitt, progressFitt);
        }
        private float Helper_NormalizeFittness(float fittness)
        {
             return (float)Math.Sqrt(fittness);
        }

        private void PrintProgress(System.IO.StreamWriter progressOutput)
        {
            if (progressOutput != System.IO.StreamWriter.Null)
            {
                _evolveProgress.PrintProgress(progressOutput);
            }
        }

        private void GenerateNewPop()
        {
            Span<int> popIndexs = stackalloc int[_pop_generation.Length];
            for (int i = 0; i < popIndexs.Length; i++)
                popIndexs[i] = i;

            Sort_Fitnesses(popIndexs);

            if (_progressLog != System.IO.StreamWriter.Null)
            {
                _progressLog.WriteLine("gen");
                for (int i = 0; i < popIndexs.Length; i++)
                {
                    _progressLog.Write($"{_pop_Fitness[popIndexs[i]],6:F3}  ");
                }
                _progressLog.WriteLine();

                _progressLog.Flush();
            }


            GenerateNewPop_ApplyElitism(popIndexs);

            int elitismCount = Helper_GetElitsmCount();

            for (int p = elitismCount; p < _populationSize; p++)
            {
                int foundIndex =
                    Helper_GetProportionalSelectionIndex(_populationSize- Helper_GetDiscardCount());
                    //Helper_GetIndexBySumnorm_selectFunc(_rnd.GetRandomNumber(0, sumFitness));
                var tmpFnn = _fnn_pool.Pop();
                _pop_generation[popIndexs[foundIndex]].CopyTo(tmpFnn);

                ApplyMutation(tmpFnn);

                _pop_generation_tmp[p] = tmpFnn;
            }

            for (int i = 0; i < _pop_generation.Length; i++)
            {
                _fnn_pool.Add(_pop_generation[i]);

                _pop_generation[i] = _pop_generation_tmp[i];
                _pop_generation_tmp[i] = null;
            }

        }

        private void Sort_Fitnesses(Span<int> indexs)
        {
            for(int i =0; i < indexs.Length;i++)
            {
                int tmp = indexs[i];
                float tmpFitt = _pop_Fitness[tmp];
                int tmpIndex = i;
                while(tmpIndex>0 && _pop_Fitness[indexs[tmpIndex-1]]> tmpFitt)
                {
                    indexs[tmpIndex] = indexs[tmpIndex - 1];
                    tmpIndex--;
                }

                indexs[tmpIndex] = tmp;
            }
        }

        private void GenerateNewPop_ApplyElitism(Span<int> popIndexs)
        {
            int elitismCount = Helper_GetElitsmCount();
            for (int i = 0; i < elitismCount; i++)
            {
                var tmpFnn = _fnn_pool.Pop();
                _pop_generation[popIndexs[i]].CopyTo(tmpFnn);
                _pop_generation_tmp[i] = tmpFnn;
            }

        }

        private int Helper_GetProportionalSelectionIndex(int popSize)
        {
            double val = _rnd.GetRandomNumberDouble();
            double yVal = Math.Pow(val, 2 + coefSelection);

            return (int)Math.Floor((yVal / (1.0 / popSize)));
        }


        //private int Helper_GetBestFitnessIndex(FixListSpan<int> excludeElites)
        //{
        //    float bestFittness = float.MaxValue;
        //    int resultIndex = -1;

        //    for (int p = 0; p < _populationSize; p++)
        //    {
        //        if (excludeElites.IndexOf(_pop_Fitness[p]) >= 0)
        //            continue;

        //        if (_pop_Fitness[p] < bestFittness)
        //        {
        //            bestFittness = _pop_Fitness[p];
        //            resultIndex = p;
        //        }
        //    }

        //    return resultIndex;
        //}
        //CreateInstanceBinder 


    }
}


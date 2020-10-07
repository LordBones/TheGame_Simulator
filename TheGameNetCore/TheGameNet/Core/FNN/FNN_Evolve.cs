using BonesLib.ForwardNN;
using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.Players;

namespace TheGameNet.Core.FNN
{
    class FNN_Evolve
    {
        private float _mutationChance;
        private int _populationSize;

        private FixList<ForwardNN> _fnn_pool;

        private FNN_LayerManipulator _fnn_manipulator;

        private int _best_fitness;
        private ForwardNN _best_fnn;

        private int[] _pop_Fitness;
        private ForwardNN[] _pop_generation;
        private ForwardNN[] _pop_generation_tmp;

        private RandomGen _rnd;

        private EvolveProgress _evolveProgress = new EvolveProgress(10);
       

        public FNN_Evolve(float mutation,int populationSize)
        {
            _mutationChance = mutation;
            _populationSize = populationSize;
            _pop_Fitness = new int[populationSize];
            _pop_generation = new ForwardNN[populationSize];
            _pop_generation_tmp = new ForwardNN[populationSize];
            _fnn_manipulator = new FNN_LayerManipulator(0);
            _rnd = new RandomGen(0);

            _decksForTest = new byte[_deckBatchSize][];

            

        }


        public void RunEvolution(int generations, ForwardNN fnn, System.IO.StreamWriter evolveProgress)
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
            }

            PrintProgress(evolveProgress);

        }

        private void CreatePoolFnn(ForwardNN fnn)
        {
            

            _fnn_pool = new FixList<ForwardNN>(_populationSize * 2+1);

            for(int i = 0;i < _fnn_pool.MaxSize; i++)
            {
                _fnn_pool.Add(fnn.Clone());
            }

            if (_best_fnn == null)
            {
                _best_fnn = _fnn_pool.Pop();
                fnn.CopyTo(_best_fnn);
            }
        }

        private void InitStartPop(ForwardNN fnn)
        {
            for(int i  = 0; i < _populationSize; i++)
            {
                var newModelNN = _fnn_pool.Pop();
                fnn.CopyTo(newModelNN);

                ApplyMutation(newModelNN);

                _pop_generation[i] = newModelNN;
            }
        }



        private void ApplyMutation(ForwardNN fnn)
        {

            int countMutations = 
            //    1;
             (fnn.GetCountWeights() / 50) + 1;

            for (int i = 0; i < countMutations; i++)
            {
                _fnn_manipulator.MutateWeight(fnn.Layers);
            }
        }

        TheGameSimulator tgs = new TheGameSimulator(null);

        private static string[] namePlayers = new string[] { "ferda" };

        public ForwardNN Best_fnn { get => _best_fnn; set => _best_fnn = value; }
        public int Best_fitness { get => _best_fitness; set => _best_fitness = value; }


        private int _deckBatchSize = 1;
        private byte[][] _decksForTest;
        private int _deckBatchDurability = 100;
        private int _deckBatchDurabilityCounter = 0;
        private int _elitismCount = 4;

        DeckGenerator deckGen = new DeckGenerator(100, 0);
        private void ComputeFitnesses()
        {
            CheckAndGensDeck();

            var players = RoundSimulations.RoundSimulator.CreatePlayers<Player_FNN>(namePlayers);    
            tgs.SetPlayers(players);
            
            //var deck = deckGen.Get_CreatedSuffledDeck();
            for (int i =0; i < _populationSize; i++)
            {
                foreach(var player in players)
                {
                    ((Player_FNN)player).Fnn = _pop_generation[i];
                }

                int fittness = 0;
                for (int deckIndex = 0; deckIndex < _decksForTest.Length; deckIndex++)
                {
                    var deck = _decksForTest[deckIndex];
                    var resultGame = tgs.Simulate(deck);
                    fittness += resultGame.Rest_Cards;
                }
                _pop_Fitness[i] = fittness;
            }
        }

        private void CheckAndGensDeck()
        {
            if(_deckBatchDurabilityCounter == _deckBatchDurability)
            {
                for(int i =0;i < _decksForTest.Length; i++)
                {
                    _decksForTest[i] = deckGen.Get_CreatedSuffledDeck();
                }

                _deckBatchDurabilityCounter = 0;
            }

            _deckBatchDurabilityCounter++;
        }

        private void UpdateStats()
        {
            int minFitt = int.MaxValue;
            int maxFitt = int.MinValue;
            int sumFitt = 0;

            for (int p = 0; p < _populationSize; p++)
            {
                int popFitt = _pop_Fitness[p];
                int popFittOne = popFitt / _deckBatchSize;

                if (popFitt < _best_fitness)
                {
                    _best_fitness = popFitt;
                    _pop_generation[p].CopyTo(_best_fnn);
                    //_best_fnn = _pop_generation[p];
                }


                if (minFitt > popFittOne)
                    minFitt = popFittOne;
                if (maxFitt < popFittOne)
                    maxFitt = popFittOne;

                sumFitt += popFittOne;

            }

            _evolveProgress.Update(minFitt, maxFitt, sumFitt / _populationSize);
        }

        private void PrintProgress(System.IO.StreamWriter progressOutput)
        {
            if(progressOutput != System.IO.StreamWriter.Null)
            {
                _evolveProgress.PrintProgress(progressOutput);
            }
        }

        private void GenerateNewPop()
        {
            int sumFitness = Helper_GetSumFitness();

            GenerateNewPop_ApplyElitism();
            

            for (int p = _elitismCount; p < _populationSize; p++)
            {
                int foundIndex = Helper_GetIndexByFitnessNumber(_rnd.GetRandomNumber(0, sumFitness));
                var tmpFnn = _fnn_pool.Pop();
                _pop_generation[foundIndex].CopyTo(tmpFnn);

                ApplyMutation(tmpFnn);

                _pop_generation_tmp[p] = tmpFnn;
            }

            for(int i = 0;i < _pop_generation.Length; i++)
            {
                _fnn_pool.Add(_pop_generation[i]);

                _pop_generation[i] = _pop_generation_tmp[i];
                _pop_generation_tmp[i] = null;
            }
            
        }

        private void GenerateNewPop_ApplyElitism()
        {
            Span<int> elites = stackalloc int[_elitismCount];
            FixListSpan<int> elitesList = new FixListSpan<int>(elites);

            for(int i =0;i < _elitismCount; i++)
            {
                int bestIndex = Helper_GetBestFitnessIndex(elitesList);
                if (bestIndex < 0)
                    throw new Exception("not allowed");

                elitesList.Add(bestIndex);
                var tmpFnn = _fnn_pool.Pop();
                _pop_generation[bestIndex].CopyTo(tmpFnn);
                _pop_generation_tmp[i] = tmpFnn;
            }
           
        }

        private int Helper_GetIndexByFitnessNumber(int fitness)
        {
            int sum = 0;
            for (int i = 0; i < _pop_Fitness.Length; i++)
            {
                sum += _pop_Fitness[i];

                if (fitness < sum)
                {
                    return i;
                }
                
            }

            return -1;
        }

        private int Helper_GetSumFitness()
        {
            int sum = 0;
            for(int i =0;i < _pop_Fitness.Length; i++)
            {
                sum += _pop_Fitness[i];
            }
            return sum;
        }

        
        private int Helper_GetBestFitnessIndex(FixListSpan<int> excludeElites)
        {
            float bestFittness = float.MaxValue;
            int resultIndex = -1;

            for (int p = 0; p < _populationSize; p++)
            {
                if (excludeElites.IndexOf(p) >= 0)
                    continue;

                if (_pop_Fitness[p] < bestFittness)
                {
                    bestFittness = _pop_Fitness[p];
                    resultIndex = p;
                }
            }

            return resultIndex;
        }
        //CreateInstanceBinder 

        
    }
}

using BonesLib.ForwardNN;
using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public FNN_Evolve(float mutation,int populationSize)
        {
            _mutationChance = mutation;
            _populationSize = populationSize;
            _pop_Fitness = new int[populationSize];
            _pop_generation = new ForwardNN[populationSize];
            _fnn_manipulator = new FNN_LayerManipulator(0);
        }


        public void RunEvolution(int generations, ForwardNN fnn)
        {
            CreatePoolFnn(fnn);


        }

        private void CreatePoolFnn(ForwardNN fnn)
        {
            _fnn_pool = new FixList<ForwardNN>(_populationSize * 2);

            for(int i = 0;i < _fnn_pool.Length; i++)
            {
                _fnn_pool.Add(fnn.Clone());
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
            _fnn_manipulator.MutateWeight(fnn.Layers);
        }
        //CreateInstanceBinder 
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.QLearning
{
    internal class QLearningCompute
    {
        private float _learningRate;
        private float _sigmaFutureDecision;

        public QLearningCompute(float learningRate, float sigmafutureDecision)
        {
            _learningRate = learningRate;
            _sigmaFutureDecision = sigmafutureDecision;
        }


        public float Q_Compute(float qCurrentReward, float currentReward, float qFutureReward)
        {
            float newQ = (1.0f - _learningRate) * qCurrentReward + _learningRate * (currentReward + _sigmaFutureDecision * qFutureReward);

            return newQ;
        }
    }
}

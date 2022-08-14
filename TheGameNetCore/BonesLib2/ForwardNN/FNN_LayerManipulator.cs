using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.ForwardNN
{
    public class FNN_LayerManipulator
    {
        private RandomGen _rnd;
        public FNN_LayerManipulator(int randomSeed)
        {
            _rnd = new RandomGen(randomSeed);
        }


        public void InitRandomWeights(NNLayer [] nnls)
        {
            for( int level = 0; level < nnls.Length; level++)
            {
                var links = nnls[level].NeuronLinks;
                for (int linkIndex = 0; linkIndex < links.Length; linkIndex++)
                {
                    int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
                    links[linkIndex].Weight = ((float)_rnd.GetRandomNumberDoubleGausisan())*sign;
                        //((float)_rnd.GetRandomNumberDouble())/1*sign;
                }
            }
        }

        public void MutateWeight(NNLayer [] nnls)
        {
            int level = _rnd.GetRandomNumber(0, nnls.Length);
            int linkIndex = _rnd.GetRandomNumber(0, nnls[level].NeuronLinks.Length);



            //disable link
                if (_rnd.GetRandomNumber(0, 41) == 0)
                {
                                       

                    nnls[level].NeuronLinks[linkIndex].Weight = 0.0f;
                    return;
                }
            
            //change link
            if (level > 0)
            {
                if (_rnd.GetRandomNumber(0, 81) == 0)
                {
                    int maxNeurons = nnls[level - 1].Neurons.Length;

                    int newLinkIndex = _rnd.GetRandomNumber(0, maxNeurons);



                    nnls[level].NeuronLinks[linkIndex].BackNeuronIndex = (ushort)newLinkIndex;

                    //int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
                    nnls[level].NeuronLinks[linkIndex].Weight = ((float)_rnd.GetRandomNumberDoubleGausisan()) - 0.5f;// * sign;

                    return;
                }
            }

            // swap sign
            if(_rnd.GetRandomNumber(0, 11) == 0)
            {
                nnls[level].NeuronLinks[linkIndex].Weight *= -1.0f;
                return;
            }

            float newWeight = (float)(((_rnd.GetRandomNumberDoubleGausisan()*2)-1.0));
            const float learnRating = 0.1f;
            //nnls[level].NeuronLinks[linkIndex].Weight         -=
                //(nnls[level].NeuronLinks[linkIndex].Weight - newWeight) * learnRating;

            nnls[level].NeuronLinks[linkIndex].Weight += newWeight;
        }

    }
}

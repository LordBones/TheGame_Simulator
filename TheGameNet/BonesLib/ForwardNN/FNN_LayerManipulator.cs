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
                    links[linkIndex].Weight = (float)_rnd.GetRandomNumberDouble();
                }
            }
        }

    }
}

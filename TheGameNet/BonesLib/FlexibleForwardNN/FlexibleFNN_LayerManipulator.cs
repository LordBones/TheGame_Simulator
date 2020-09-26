using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.FlexibleForwardNN
{
    public class FlexibleFNN_LayerManipulator
    {
        private RandomGen _rnd;
        public FlexibleFNN_LayerManipulator(int randomSeed)
        {
            _rnd = new RandomGen(randomSeed);
        }

        public void InitRandomWeights(FlexibleForwardNN.NNLayer nnls)
        {
            var links = nnls.NeuronLinks;
            for (int linkIndex = 0; linkIndex < links.Length; linkIndex++)
            {
                int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
                links[linkIndex].Weight = ((float)_rnd.GetRandomNumberDoubleGausisan()) * sign;
                //((float)_rnd.GetRandomNumberDouble())/1*sign;
            }
        }

        public void MutateWeight(FlexibleForwardNN.NNLayer nnl, int koefOfRunning)
        {
            int chance = 4;

            //change link

            if (_rnd.GetRandomNumber(0, chance * koefOfRunning+1) == 0)
            {
                if (nnl.NeuronLinks.Length > 0)
                {
                    int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length - 1);
                    ref var nl = ref nnl.NeuronLinks[linkIndex];


                    int countNWO = nnl.Get_CountNeuronsWithoutOutput(nl.NeuronIndex);

                    int neuronOrder = _rnd.GetRandomNumber(0, countNWO)+1;

                   int newIndex = nnl.Get_IndexOfNeuronWithouOutput(neuronOrder, nl.NeuronIndex);

                    if (newIndex > 0)
                    {
                        nnl.ChangeBackNeuronLink((ushort)newIndex, linkIndex);

                        //nl.BackNeuronIndex = (ushort)newLinkIndex;

                        //int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
                        nl.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) - 0.5f) * 0.1f;// * sign;
                    }

                    
                    
                }
                //return;
            }



            //add link
            if (_rnd.GetRandomNumber(0, chance * koefOfRunning+1) == 0)
            {
                //NN_AddLink(nnl);
                NN_AddLink_Inteligent(nnl);

            }

            //remove link
            if (_rnd.GetRandomNumber(0, chance * koefOfRunning+1) == 0)
            {
                if (nnl.CanRemoveLink())
                {
                    int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length - 1);
                    nnl.RemoveLink(linkIndex);
                }
                //nnl.NeuronLinks[linkIndex].Weight = 0.0f;
                // return;
            }

            // swap sign
            if (_rnd.GetRandomNumber(0, chance+1) == 0)
            {
                if (nnl.NeuronLinks.Length > 0)
                {
                    int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length - 1);
                    nnl.NeuronLinks[linkIndex].Weight *= -1.0f;
                }
                // return;
            }


            if (nnl.NeuronLinks.Length > 0)
            {
                int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length - 1);
                float newWeight = (float)(((_rnd.GetRandomNumberDoubleGausisan() * 2) - 1.0));
                const float learnRating = 0.1f;
                newWeight *= learnRating;
                //nnls[level].NeuronLinks[linkIndex].Weight         -=
                //(nnls[level].NeuronLinks[linkIndex].Weight - newWeight) * learnRating;

                nnl.NeuronLinks[linkIndex].Weight += newWeight;
            }
        }

        private void NN_AddLink(FlexibleForwardNN.NNLayer nnl)
        {
            if (nnl.CanAddLink())
            {
                var nnlink = new FlexibleForwardNN.NNLink();
                nnlink.NeuronIndex = (ushort)(_rnd.GetRandomNumber(nnl.InputCounts, nnl.InputsPerNeuronList.Length) );
                nnlink.BackNeuronIndex = (ushort)_rnd.GetRandomNumber(0, nnlink.NeuronIndex - 1);
                nnlink.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) - 0.5f) * 0.1f;// * sign;

                nnl.AddLink(nnlink);
                //return;
            }
        }

        private void NN_AddLink_Inteligent(FlexibleForwardNN.NNLayer nnl)
        {
            if (nnl.CanAddLink())
            {
                var nnlink = new FlexibleForwardNN.NNLink();
                nnlink.NeuronIndex = (ushort)(_rnd.GetRandomNumber(nnl.InputCounts, nnl.InputsPerNeuronList.Length) );
                nnlink.BackNeuronIndex = (ushort)_rnd.GetRandomNumber(0, nnlink.NeuronIndex - 1);
                nnlink.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) - 0.5f) * 0.01f;// * sign;

                nnl.AddLink(nnlink);

                ushort ni = nnlink.NeuronIndex;

                while(!nnl.IsNeuronOutput(ni) && nnl.CanAddLink() && !nnl.IsNeuronConnectAsBackward(ni)){
                    var nnlinkTmp = new FlexibleForwardNN.NNLink();
                    nnlinkTmp.NeuronIndex = (ushort)(_rnd.GetRandomNumber(ni, nnl.InputsPerNeuronList.Length));
                    nnlinkTmp.BackNeuronIndex = ni;
                    nnlinkTmp.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) - 0.5f) * 0.01f;// * sign;

                    nnl.AddLink(nnlinkTmp);

                    ni = nnlinkTmp.NeuronIndex;
                }
           
            }
        }
    }
}

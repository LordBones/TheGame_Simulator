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
                //int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
                links[linkIndex].Weight = ((float)_rnd.GetRandomNumberDoubleGausisan()-0.5f)*0.1f;
                //((float)_rnd.GetRandomNumberDouble())/1*sign;
            }
        }

        public void InitRandomTopology(FlexibleForwardNN.NNLayer nnls)
        {
            nnls.NeuronLinks.Clear();
            nnls.InputsPerNeuronList.AsSpan().Fill(0);
            nnls.OutputsPerNeuronList.AsSpan().Fill(0);
            

            for(int i = 0; i < nnls.InputCounts; i++)
            {
                if (nnls.CanAddLink())
                {
                    var nnlink = new FlexibleForwardNN.NNLink();
                    nnlink.NeuronIndex = (ushort)(_rnd.GetRandomNumber(nnls.InputCounts, nnls.NeuronInternalState.Length));
                    nnlink.BackNeuronIndex = (ushort)i;
                    nnlink.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) ) ;// * sign;

                    nnls.AddLink(nnlink);

                    ushort ni = nnlink.NeuronIndex;

                    Helper_AddLinks_Up_IfEmpty_Recursive(nnls, ni, 1.0f);
                }
                
            }

            for (int i = 0; i < nnls.OutputCounts; i++)
            {
                if (nnls.CanAddLink())
                {
                    var nnlink = new FlexibleForwardNN.NNLink();
                    nnlink.NeuronIndex = (ushort)(nnls.NOutputIndexStart+i);
                    nnlink.BackNeuronIndex = (ushort)(_rnd.GetRandomNumber(0, nnls.NOutputIndexStart)); 
                    nnlink.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) );// * sign;

                    nnls.AddLink(nnlink);

                    ushort ni = nnlink.BackNeuronIndex;

                    Helper_AddLinks_Down_IfEmpty_Recursive(nnls, ni, 1.0f);
                    
                }

            }

            Helper_ExtendN_AtLeastTwoNeurons(nnls, 1.0f);
        }


        Func<FlexibleForwardNN.NNLayer, float, bool>[] _mutators = new Func<FlexibleForwardNN.NNLayer, float, bool>[3];
        public void MutateWeight(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            _mutators[0] = Mutate_ChangeWeight;
            _mutators[1] = Mutate_RemoveLink;
            _mutators[2] = Mutate_AddLink;

            bool mutationIsDone = false;
            int safeCounter = 255;

            while (!mutationIsDone && safeCounter > 0)
            {
                int index = _rnd.GetRandomNumber(0, _mutators.Length);
                mutationIsDone = _mutators[index].Invoke(nnl, learningRate);

                safeCounter--;
            }


            return;

            int chance = 15;

            //change link

            if (false && _rnd.GetRandomNumber(0, 11) == 0)
            {
                if (nnl.NeuronLinks.Length > 0)
                {
                    int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length );
                    ref var nl = ref nnl.NeuronLinks[linkIndex];


                    int countNWO = nnl.Get_CountNeuronsWithoutOutput(nl.NeuronIndex);

                    int neuronOrder = _rnd.GetRandomNumber(0, countNWO)+1;

                   int newIndex = nnl.Get_IndexOfNeuronWithouOutput(neuronOrder, nl.NeuronIndex);

                    if (newIndex > 0)
                    {
                        nnl.ChangeBackNeuronLink((ushort)newIndex, linkIndex);

                        //nl.BackNeuronIndex = (ushort)newLinkIndex;

                        //int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
                        nl.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan())) * learningRate;// * sign;
                    }

                    Helper_Remove_AllOneInputsNeurons(nnl);
                    Helper_Remove_NotConnectedNeurons(nnl);
                    Helper_Remove_AllOneInputsNeurons(nnl);
                    Helper_Remove_NotConnectedNeurons(nnl);
                }
                //return;
            }



            //add link
            if (_rnd.GetRandomNumber(0, 2) == 0)
            {
                //NN_AddLink(nnl);
                NN_AddLink_Inteligent(nnl, learningRate);
                Helper_ExtendN_AtLeastTwoNeurons(nnl, learningRate);

            }

            //remove link
            if (_rnd.GetRandomNumber(0, 10) == 0)
            {
                if (nnl.CanRemoveLink())
                {
                    int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
                    nnl.RemoveLink(linkIndex);

                    Helper_Remove_AllOneInputsNeurons(nnl);
                    Helper_Remove_NotConnectedNeurons(nnl);
                    Helper_Remove_AllOneInputsNeurons(nnl);
                    Helper_Remove_NotConnectedNeurons(nnl);

                }
                //nnl.NeuronLinks[linkIndex].Weight = 0.0f;
                // return;
            }

            // swap sign
            //if (_rnd.GetRandomNumber(0, 2) == 0)
            //{
            //    if (nnl.NeuronLinks.Length > 0)
            //    {
            //        int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
            //        nnl.NeuronLinks[linkIndex].Weight = -nnl.NeuronLinks[linkIndex].Weight ;
            //    }
            //    // return;
            //}


            if (nnl.NeuronLinks.Length > 0)
            {
                int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length );
                float newWeight = (float)(((_rnd.GetRandomNumberDoubleGausisan() ) ));
                 float learnRating = learningRate;
                newWeight *= learnRating;
                //nnls[level].NeuronLinks[linkIndex].Weight         -=
                //(nnls[level].NeuronLinks[linkIndex].Weight - newWeight) * learnRating;

                nnl.NeuronLinks[linkIndex].Weight = newWeight;
            }
        }

        private bool Mutate_ChangeWeight(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            if (nnl.NeuronLinks.Length > 0)
            {
                int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
                float newWeight = (float)(((_rnd.GetRandomNumberDouble())));
                float learnRating = learningRate;
                newWeight *= 1.5f;
                //newWeight *= 1.0f;// learnRating;
                //nnls[level].NeuronLinks[linkIndex].Weight         -=
                //(nnls[level].NeuronLinks[linkIndex].Weight - newWeight) * learnRating;

                nnl.NeuronLinks[linkIndex].Weight *= newWeight+0.5f;
                return true;
            }

            return false;
        }

        private bool Mutate_RemoveLink(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            if (nnl.CanRemoveLink())
            {
                int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
                if (nnl.InputsPerNeuronList[nnl.NeuronLinks[linkIndex].NeuronIndex] < 3)
                    return false;

                nnl.RemoveLink(linkIndex);

                Helper_Remove_AllOneInputsNeurons(nnl);
                Helper_Remove_NotConnectedNeurons(nnl);
                Helper_Remove_AllOneInputsNeurons(nnl);
                Helper_Remove_NotConnectedNeurons(nnl);
                return true;
            }

            return false;
        }

        private bool Mutate_AddLink(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            if (nnl.CanAddLink())
            {
                NN_AddLink_Inteligent(nnl, learningRate);
                Helper_ExtendN_AtLeastTwoNeurons(nnl, learningRate);
                return true;
            }

            return false;
        }

        //public void MutateWeight(FlexibleForwardNN.NNLayer nnl, float learningRate)
        //{
        //    int chance = 15;

        //    //change link

        //    if (false && _rnd.GetRandomNumber(0, 11) == 0)
        //    {
        //        if (nnl.NeuronLinks.Length > 0)
        //        {
        //            int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
        //            ref var nl = ref nnl.NeuronLinks[linkIndex];


        //            int countNWO = nnl.Get_CountNeuronsWithoutOutput(nl.NeuronIndex);

        //            int neuronOrder = _rnd.GetRandomNumber(0, countNWO) + 1;

        //            int newIndex = nnl.Get_IndexOfNeuronWithouOutput(neuronOrder, nl.NeuronIndex);

        //            if (newIndex > 0)
        //            {
        //                nnl.ChangeBackNeuronLink((ushort)newIndex, linkIndex);

        //                //nl.BackNeuronIndex = (ushort)newLinkIndex;

        //                //int sign = _rnd.GetRandomNumber(0, 2) == 0 ? 1 : -1;
        //                nl.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan())) * learningRate;// * sign;
        //            }

        //            Helper_Remove_AllOneInputsNeurons(nnl);
        //            Helper_Remove_NotConnectedNeurons(nnl);
        //            Helper_Remove_AllOneInputsNeurons(nnl);
        //            Helper_Remove_NotConnectedNeurons(nnl);
        //        }
        //        //return;
        //    }



        //    //add link
        //    if (_rnd.GetRandomNumber(0, 2) == 0)
        //    {
        //        //NN_AddLink(nnl);
        //        NN_AddLink_Inteligent(nnl, learningRate);
        //        Helper_ExtendN_AtLeastTwoNeurons(nnl, learningRate);

        //    }

        //    //remove link
        //    if (_rnd.GetRandomNumber(0, 10) == 0)
        //    {
        //        if (nnl.CanRemoveLink())
        //        {
        //            int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
        //            nnl.RemoveLink(linkIndex);

        //            Helper_Remove_AllOneInputsNeurons(nnl);
        //            Helper_Remove_NotConnectedNeurons(nnl);
        //            Helper_Remove_AllOneInputsNeurons(nnl);
        //            Helper_Remove_NotConnectedNeurons(nnl);

        //        }
        //        //nnl.NeuronLinks[linkIndex].Weight = 0.0f;
        //        // return;
        //    }

        //    // swap sign
        //    //if (_rnd.GetRandomNumber(0, 2) == 0)
        //    //{
        //    //    if (nnl.NeuronLinks.Length > 0)
        //    //    {
        //    //        int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
        //    //        nnl.NeuronLinks[linkIndex].Weight = -nnl.NeuronLinks[linkIndex].Weight ;
        //    //    }
        //    //    // return;
        //    //}


        //    if (nnl.NeuronLinks.Length > 0)
        //    {
        //        int linkIndex = _rnd.GetRandomNumber(0, nnl.NeuronLinks.Length);
        //        float newWeight = (float)(((_rnd.GetRandomNumberDoubleGausisan())));
        //        float learnRating = learningRate;
        //        newWeight *= learnRating;
        //        //nnls[level].NeuronLinks[linkIndex].Weight         -=
        //        //(nnls[level].NeuronLinks[linkIndex].Weight - newWeight) * learnRating;

        //        nnl.NeuronLinks[linkIndex].Weight = newWeight;
        //    }
        //}

        private void NN_AddLink(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            if (nnl.CanAddLink())
            {
                var nnlink = new FlexibleForwardNN.NNLink();
                nnlink.NeuronIndex = (ushort)(_rnd.GetRandomNumber(nnl.InputCounts, nnl.InputsPerNeuronList.Length) );
                nnlink.BackNeuronIndex = (ushort)_rnd.GetRandomNumber(0, nnlink.NeuronIndex - 1);
                nnlink.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) - 0.5f) * learningRate;// * sign;

                nnl.AddLink(nnlink);
                //return;
            }
        }

        private void NN_AddLink_Inteligent(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            if (nnl.CanAddLink())
            {
                var nnlink = new FlexibleForwardNN.NNLink();
                nnlink.NeuronIndex = (ushort)(_rnd.GetRandomNumber(nnl.InputCounts, nnl.NeuronInternalState.Length) );
                int bni = (nnl.IsNeuronOutput( nnlink.NeuronIndex)) ? nnl.NOutputIndexStart : nnlink.NeuronIndex;
                nnlink.BackNeuronIndex = (ushort)_rnd.GetRandomNumber(0, bni);
                nnlink.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan())) * 0.1f;// * learningRate;// * sign;

                nnl.AddLink(nnlink);

                ushort ni = nnlink.NeuronIndex;

                Helper_AddLinks_Up_IfEmpty_Recursive(nnl, ni, learningRate);
                Helper_AddLinks_Down_IfEmpty_Recursive(nnl, nnlink.BackNeuronIndex, learningRate);
            }
        }

        private void Helper_AddLinks_Down_IfEmpty_Recursive(FlexibleForwardNN.NNLayer nnl, int ni, float learningRate)
        {
            while (!nnl.IsNeuronInput(ni) && nnl.CanAddLink() && !nnl.HasNeuronInputs(ni))
            {
                var nnlinkTmp = new FlexibleForwardNN.NNLink();
                nnlinkTmp.NeuronIndex = (ushort)ni;
                int bni = (nnl.IsNeuronOutput(nnlinkTmp.NeuronIndex)) ? nnl.NOutputIndexStart  : ni;

                nnlinkTmp.BackNeuronIndex = (ushort)(_rnd.GetRandomNumber(0, bni));
                nnlinkTmp.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan()) )*0.1f;// * learningRate;// * sign;

                nnl.AddLink(nnlinkTmp);

                

                ni = nnlinkTmp.BackNeuronIndex;
            }
        }

            private void Helper_AddLinks_Up_IfEmpty_Recursive(FlexibleForwardNN.NNLayer nnl, int ni, float learningRate)
        {
            while (!nnl.IsNeuronOutput(ni) && nnl.CanAddLink() && !nnl.HasNeuronOutputs(ni))
            {
                var nnlinkTmp = new FlexibleForwardNN.NNLink();
                nnlinkTmp.NeuronIndex = (ushort)(_rnd.GetRandomNumber(ni+1, nnl.InputsPerNeuronList.Length));
                nnlinkTmp.BackNeuronIndex = (ushort)ni;
                nnlinkTmp.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan())) * 0.1f;// * learningRate;// * sign;

                nnl.AddLink(nnlinkTmp);

              

                ni = nnlinkTmp.NeuronIndex;
            }
        }

        private void Helper_Remove_NotConnectedNeurons(FlexibleForwardNN.NNLayer nnl)
        {
            int ni = nnl.Get_IndexOfNeuronWithoutInput_First();
            while(ni >= 0)
            {
                int indexLink = nnl.Get_IndexLink_byBackN_First(ni);
                while(indexLink >= 0)
                {
                    nnl.RemoveLink(indexLink);
                    indexLink = nnl.Get_IndexLink_byBackN_First(ni);
                }

                ni = nnl.Get_IndexOfNeuronWithoutInput_First();
            }

            ni = nnl.Get_IndexOfNeuronWithoutOutput_First();
            while (ni >= 0)
            {
                int indexLink = nnl.Get_IndexLink_byN_First(ni);
                while (indexLink >= 0)
                {
                    nnl.RemoveLink(indexLink);
                    indexLink = nnl.Get_IndexLink_byN_First(ni);
                }

                ni = nnl.Get_IndexOfNeuronWithoutOutput_First();
            }
        }

        private void Helper_Remove_AllOneInputsNeurons(FlexibleForwardNN.NNLayer nnl)
        {
            var ni = nnl.Get_IndexOfNeuron_byInputs_First(1);
            while (ni >= 0)
            {
                int indexLink = nnl.Get_IndexLink_byN_First(ni);
                while (indexLink >= 0)
                {
                    nnl.RemoveLink(indexLink);
                    indexLink = nnl.Get_IndexLink_byN_First(ni);
                }

                ni = nnl.Get_IndexOfNeuron_byInputs_First(1);
            }
        }

        private void Helper_ExtendN_AtLeastTwoNeurons(FlexibleForwardNN.NNLayer nnl, float learningRate)
        {
            var ni = nnl.Get_IndexOfNeuron_byInputs_First(1);
            if (ni < 0) 
                ni = nnl.Get_IndexOfNeuron_byInputs_hasOutput_First(0);

            while (ni >= 0 && nnl.CanAddLink())
            {
                var nnlinkTmp = new FlexibleForwardNN.NNLink();
                nnlinkTmp.NeuronIndex = (ushort)ni;
                int bni =  ni;

                nnlinkTmp.BackNeuronIndex = (ushort)(_rnd.GetRandomNumber(0, bni));
                nnlinkTmp.Weight = (((float)_rnd.GetRandomNumberDoubleGausisan())) * 0.1f;// * learningRate;// * sign;

                nnl.AddLink(nnlinkTmp);

                

                ni = nnl.Get_IndexOfNeuron_byInputs_First(1);
                if (ni < 0)
                    ni = nnl.Get_IndexOfNeuron_byInputs_hasOutput_First(0);
            }
        }
    }
}

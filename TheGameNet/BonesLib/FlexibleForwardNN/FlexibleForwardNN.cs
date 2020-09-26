using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.FlexibleForwardNN
{
    public class FlexibleForwardNN
    {
        private const int CONST_MaxLinkPerNeuron = 10;
        private const float CONST_Bias = 1.0f;

        public int InputsCount { get { return _nnLayer.InputCounts; } }
        public int OutputsCount { get { return _nnLayer.OutputCounts; } }
        public Span<float> Outputs { get { return _nnLayer.Outputs; } }
        public Span<float> Inputs { get { return _nnLayer.Inputs; } }

        private NNLayer _nnLayer;

        public ref NNLayer Layers => ref _nnLayer;

        public FlexibleForwardNN(short countInputs, short countOutputs)
        {
            //Outputs = new float[countOutputs];
            //Inputs = new float[countInputs];
            _nnLayer = new NNLayer();
            
        }

        public void SetTopology(short[] topology)
        {
            
            
            int linksPerNeuron = InputsCount;
            int countLastNeurons = InputsCount;

            if (linksPerNeuron > CONST_MaxLinkPerNeuron)
                linksPerNeuron = CONST_MaxLinkPerNeuron;
            
            int backNeuronStartIndex = 0;
            int neuronStartIndex = InputsCount;

            for (int i = 0; i < topology.Length; i++)
            {
                
                InitLinks(countLastNeurons, topology[i], linksPerNeuron,neuronStartIndex,backNeuronStartIndex);

                neuronStartIndex += topology[i];
                backNeuronStartIndex += countLastNeurons;

                countLastNeurons = topology[i];
                linksPerNeuron = topology[i];

                if (linksPerNeuron > CONST_MaxLinkPerNeuron)
                    linksPerNeuron = CONST_MaxLinkPerNeuron;

                
            }

            InitLinks(countLastNeurons, OutputsCount, linksPerNeuron, neuronStartIndex, backNeuronStartIndex);
        }

        private void InitLinks(int totalBackNeurons, int countNeurons, int linkPerNeuron, int neuronStartId, int backNeuronStartId)
        {
            int nLinks = countNeurons * linkPerNeuron;

            ushort idCounter = 0;
            ushort idStartPos = 0;
            int neuronCounter = 0;
            for (int i = 0; i < nLinks; i++)
            {
                if (idCounter == linkPerNeuron + idStartPos)
                {
                    if (idStartPos + linkPerNeuron >= totalBackNeurons)
                    {
                        idStartPos = 0;
                    }
                    else
                    {
                        idStartPos++;
                    }

                    idCounter = idStartPos;
                    neuronCounter++;
                }

                NNLink nl = new NNLink();
                nl.BackNeuronIndex = (ushort)(idCounter + backNeuronStartId);
                nl.NeuronIndex = (ushort)(neuronCounter + neuronStartId);

                _nnLayer.AddLink(nl);
                

                idCounter++;
            }
        }

        private bool IsNeuronOutput(int nIndex)
        {
            return _nnLayer.NOutputIndexStart <= nIndex;
        }

        public bool CanAddLink()
        {
            return _nnLayer.CanAddLink();
        }

        public FlexibleForwardNN Clone()
        {
            FlexibleForwardNN newObj = new FlexibleForwardNN((short)InputsCount, (short)OutputsCount);
            newObj.Layers.Init(this.Layers.InputCounts, this.Layers.OutputCounts, this.Layers.PosibleNeuronCounts, this.Layers.NeuronLinks.MaxSize);
            
            CopyTo(newObj);
            return newObj;
        }

        public void CopyTo(FlexibleForwardNN dest)
        {
            if (!this.IsEqualTopology(dest)) { throw new Exception("Bad topology"); };


            Layers.CopyTo(dest.Layers);
            //var sourceNNLayer = Layers;
            //dest.Layers.NeuronLinks.Clear();

            //var sNLinks = Layers.NeuronLinks;
            //for (int nl = 0; nl < sNLinks.Length;nl ++)
            //{
            //    dest.Layers.NeuronLinks.Add(sNLinks.Get(nl));
            //}

            //sourceNNLayer.InputsPerNeuronList.CopyTo(dest.Layers.InputsPerNeuronList.AsSpan());
            //sourceNNLayer.OutputsPerNeuronList.CopyTo(dest.Layers.OutputsPerNeuronList.AsSpan());
        }

        public bool IsEqualTopology(FlexibleForwardNN fnn)
        {
            return fnn.Layers.IsEqualTopology(this.Layers);
        }

       public  void PrintNetwork(System.IO.StreamWriter log)
        {
            log.WriteLine($"Inputs: 0-{InputsCount} Outputs: {_nnLayer.NeuronInternalState.Length-OutputsCount}-{ _nnLayer.NeuronInternalState.Length -1} Links: {_nnLayer.NeuronLinks.Length}" );



            int nlIndex = 0;
            StringBuilder sb = new StringBuilder();
            for (int n =0;n< _nnLayer.InputsPerNeuronList.Length; n++)
            {
                byte countNLinks = _nnLayer.InputsPerNeuronList[n];
                if (countNLinks == 0) continue;


                sb.Clear();
                sb.AppendFormat("n: {0,6:D}  # ", n + _nnLayer.InputCounts);

                for (int i = 0; i < countNLinks; i++)
                {
                    ref var link = ref _nnLayer.NeuronLinks[nlIndex+i];
                    sb.AppendFormat("{0,6:D} -w-> {1,10:F5}", link.BackNeuronIndex, link.Weight);
                }

                sb.AppendLine();
                nlIndex += countNLinks;
                log.WriteLine(sb.ToString());
            }

            log.Flush();
        }


        public void Evaluate()
        {
            //var inputs = _nnLayer.Inputs;
            //Inputs.CopyTo(inputs);

            _nnLayer.Evaluate();

            //var outputs = _nnLayer.Outputs;
            //outputs.CopyTo(Outputs);

        }

        public struct NNLayer
        {
            public int InputCounts;
            public int OutputCounts;
            public int PosibleNeuronCounts;
            public float[] NeuronInternalState;
            public FixList<NNLink> NeuronLinks;
            public ushort[] OutputsPerNeuronList;
            public byte[] InputsPerNeuronList;


            public Span<float> Inputs => NeuronInternalState.AsSpan(0, InputCounts);
            public Span<float> Outputs => NeuronInternalState.AsSpan(NeuronInternalState.Length-OutputCounts, OutputCounts);

            private Span<float> Internals => NeuronInternalState.AsSpan(InputCounts, PosibleNeuronCounts);
            public int NOutputIndexStart => NeuronInternalState.Length - OutputCounts;
            public void Init(int countInputs, int countOutputs, int possibleNeuronCounts, int maxLinks)
            {
                this.InputCounts = countInputs;
                this.OutputCounts = countOutputs;
                this.PosibleNeuronCounts = possibleNeuronCounts;
                this.NeuronInternalState = new float[InputCounts + OutputCounts + PosibleNeuronCounts];
                this.NeuronLinks = new FixList<NNLink>(maxLinks);
                this.InputsPerNeuronList = new byte[InputCounts + OutputCounts + PosibleNeuronCounts];
                this.OutputsPerNeuronList = new ushort[InputCounts + OutputCounts + PosibleNeuronCounts];
            }

            public bool IsNeuronOutput(int nIndex)
            {
                return NOutputIndexStart <= nIndex;
            }

            public bool IsNeuronConnectAsBackward(int nIndex)
            {
                for(int i = 0; i < this.NeuronLinks.Length; i++)
                {
                    if (this.NeuronLinks.Get(i).BackNeuronIndex == nIndex)
                        return true;
                }
                return false;
            }

            public bool CanAddLink()
            {
                return NeuronLinks.Length < NeuronLinks.MaxSize;
            }

            public void AddLink(NNLink link)
            {
                Increment_InputPerNeuron(link.NeuronIndex);
                
                Increment_OutputPerNeuron(link.BackNeuronIndex);
                
                // pridani vazby neuronu
                NeuronLinks.Add(link);

                // addSorted
                int index = NeuronLinks.Length - 1;
                for (; index > 0; index--)
                {
                    if (link.NeuronIndex < NeuronLinks[index - 1].NeuronIndex)
                    {
                        NeuronLinks[index] = NeuronLinks[index - 1];
                    }
                    else
                    {
                        break;
                    }

                }

                NeuronLinks[index] = link;
            }

            public bool CanRemoveLink()
            {
                return NeuronLinks.Length > 0;
            }

            public  void ChangeBackNeuronLink(ushort newIndex, int linkIndex)
            {
                ref var link = ref NeuronLinks[linkIndex];

                Decrement_OutputPerNeuron(link.BackNeuronIndex);
                link.BackNeuronIndex = newIndex;
                Increment_OutputPerNeuron(link.BackNeuronIndex);


            }

            public void RemoveLink(int index)
            {
                // decrement vazby poctu vazeb u daneho neuronu
                int neuronIndex = NeuronLinks[index].NeuronIndex ;

                Decrement_InputPerNeuron(neuronIndex);
                
                // decrement vazby poctu vazeb u daneho neuronu
                neuronIndex = NeuronLinks[index].BackNeuronIndex;

                Decrement_OutputPerNeuron(neuronIndex);

                // odebrani vazby

                for (int i = index + 1; i < NeuronLinks.Length; i++)
                {
                    NeuronLinks[i - 1] = NeuronLinks[i];
                }

                NeuronLinks.RemoveLast();
            }


            private void Decrement_InputPerNeuron(int index)
            {
                byte countLinks = this.InputsPerNeuronList[index];
                if (countLinks == 0)
                    throw new Exception("not allowed");

                this.InputsPerNeuronList[index]--;
            }

            private void Decrement_OutputPerNeuron(int index)
            {
                ushort countLinks2 = this.OutputsPerNeuronList[index];
                if (countLinks2 == 0)
                    throw new Exception("not allowed");

                this.OutputsPerNeuronList[index]--;
            }

            private void Increment_InputPerNeuron(int index)
            {
                byte countLinks = this.InputsPerNeuronList[index];
                if (countLinks == 255)
                    throw new Exception("not allowed");

                this.InputsPerNeuronList[index]++;
            }

            private void Increment_OutputPerNeuron(int index)
            {
                ushort countLinks2 = this.OutputsPerNeuronList[index];
                if (countLinks2 == ushort.MaxValue)
                    throw new Exception("not allowed");

                this.OutputsPerNeuronList[index]++;
            }


            public int Get_CountNeuronsWithoutOutput(int neuronIndexMax)
            {
                int result = 0;

                for(int i = 0; i < neuronIndexMax; i++)
                {
                    if (this.OutputsPerNeuronList[i] == 0 && 
                        (this.InputsPerNeuronList[i] > 0
                        || i < InputCounts))
                        result++;
                }

                return result;
            }

            public int Get_IndexOfNeuronWithouOutput(int orderNeuron,int neuronIndexMax)
            {
                int increment = 0;

                for (int i = 0; i < neuronIndexMax; i++)
                {
                    if (this.OutputsPerNeuronList[i] == 0 &&
                        (this.InputsPerNeuronList[i] > 0
                        || i < InputCounts))
                    {
                        increment++;
                        if(increment == orderNeuron)
                        {
                            return i;
                        }
                    }
                }

                return -1;
            }

            public void Evaluate()
            {
                Internals.Fill(0.0f);

                //var internals = NeuronInternalState;
                
                int nlIndex = 0;
                for(int n = InputCounts; n < InputsPerNeuronList.Length; n++)
                {
                    byte countNLinks = InputsPerNeuronList[n];
                    if (countNLinks == 0) continue;

                    float sumOutputs = CONST_Bias;
                    for (int i = 0;i < countNLinks; i++)
                    {
                        ref var link = ref NeuronLinks.Get(nlIndex);// NeuronLinks[nlIndex];
                        sumOutputs += NeuronInternalState[link.BackNeuronIndex] * link.Weight;

                        nlIndex++;
                    }

                    NeuronInternalState[n] = NeuronActivationFunction.ActivationFunction_ReLu(sumOutputs);
                }
            }

            public void CopyTo(NNLayer dest)
            {
                //dest.NeuronLinks.Clear();

                //var sNLinks = NeuronLinks;
                //for (int nl = 0; nl < sNLinks.Length; nl++)
                //{
                //    dest.NeuronLinks.Add(sNLinks.Get(nl));
                //}

                NeuronLinks.CopyTo(dest.NeuronLinks);

                InputsPerNeuronList.CopyTo(dest.InputsPerNeuronList.AsSpan());
                OutputsPerNeuronList.CopyTo(dest.OutputsPerNeuronList.AsSpan());
            }

            public bool IsEqualTopology(NNLayer fnn)
            {
                if (fnn.InputCounts != InputCounts ||
                   fnn.OutputCounts != OutputCounts ||
                   fnn.PosibleNeuronCounts != this.PosibleNeuronCounts ||
                   fnn.NeuronInternalState.Length != this.NeuronInternalState.Length ||
                   fnn.NeuronLinks.MaxSize != this.NeuronLinks.MaxSize

                   )
                {
                    return false;
                }

                return true;
            }

        }

        public struct NNLink
        {
            public ushort BackNeuronIndex;
            public ushort NeuronIndex;
            public float Weight;

            public override string ToString()
            {
                return $"BNIndex:{BackNeuronIndex} , NIndex:{NeuronIndex}, weight:{Weight}";
            }
        }


    }
}

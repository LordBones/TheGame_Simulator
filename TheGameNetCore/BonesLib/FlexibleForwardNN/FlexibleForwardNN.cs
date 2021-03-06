﻿using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BonesLib.FlexibleForwardNN
{
    public class FlexibleForwardNN
    {
        private const int CONST_MaxLinkPerNeuron = 10;
        private const float CONST_Bias = 0.0f;

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

            InitLinks(countLastNeurons, OutputsCount, linksPerNeuron, this._nnLayer.NOutputIndexStart, backNeuronStartIndex);
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

        private bool IsNeuronInput(int nIndex)
        {
            return _nnLayer.InputCounts > nIndex;
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
            log.WriteLine($"Inputs: 0-{InputsCount-1} Outputs: {_nnLayer.NeuronInternalState.Length-OutputsCount}-{ _nnLayer.NeuronInternalState.Length -1} Links: {_nnLayer.NeuronLinks.Length}" );



            int nlIndex = 0;
            StringBuilder sb = new StringBuilder();

            float ni = _nnLayer.InputsPerNeuronList.Sum(x => x) / (float)_nnLayer.InputsPerNeuronList.Length;
            float no = _nnLayer.OutputsPerNeuronList.Sum(x => x) / (float)_nnLayer.OutputsPerNeuronList.Length;

            sb.Append($"avg N Inputs: {ni:F3}  Outputs: {no:F3}");
           
            log.WriteLine(sb.ToString());
            sb.Clear();

            for (int n =0;n< _nnLayer.InputsPerNeuronList.Length; n++)
            {
                byte countNLinks = _nnLayer.InputsPerNeuronList[n];
                if (countNLinks == 0) continue;


                sb.Clear();
                sb.AppendFormat("n: {0,6:D}  # ", n );

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

        public void PrintNetworkForGraphWiz(System.IO.StreamWriter log)
        {
            log.WriteLine(" digraph G {");
            //log.WriteLine("size =\"4, 4\";");
            log.WriteLine("rotate = 90;");
            


            int nlIndex = 0;
            StringBuilder sb = new StringBuilder();

         
            for (int n = 0; n < _nnLayer.NeuronLinks.Length; n++)
            {

                var link = _nnLayer.NeuronLinks[n];

                log.WriteLine( $"{GetNodeName(link.BackNeuronIndex)} -> {GetNodeName(link.NeuronIndex)} ;");
            }

            log.WriteLine("}");
            log.Flush();

            string GetNodeName(int index)
            {
                if (_nnLayer.IsNeuronInput(index)) return $"I{index}";
                if (_nnLayer.IsNeuronOutput(index)) return $"O{index}";
                return $"N{index}";
            }
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

            public bool IsNeuronInput(int nIndex)
            {
                return InputCounts > nIndex;
            }

            public bool HasNeuronInputs(int nIndex)
            {
                return this.InputsPerNeuronList[nIndex] > 0;
            }

            public bool HasNeuronOutputs(int nIndex)
            {
                return this.OutputsPerNeuronList[nIndex] > 0;
            }


            public bool CanAddLink()
            {
                return CanAddLink(1);
            }

            public bool CanAddLink(int count)
            {
                return NeuronLinks.Length+count-1 < NeuronLinks.MaxSize;
            }

            public bool AddLink(NNLink link)
            {
                //if(IsNeuronOutput(link.BackNeuronIndex) && IsNeuronInput(link.NeuronIndex))
                //{
                //    throw new Exception();
                //}

                if (ExistLink(link.NeuronIndex, link.BackNeuronIndex))
                    return false;

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

                return true;
            }

            private bool ExistLink(int ni, int backNi)
            {
                for(int i =0; i < this.NeuronLinks.Length; i++)
                {
                    var link = NeuronLinks[i];
                    if (link.NeuronIndex == ni && link.BackNeuronIndex == backNi)
                        return true;
                }

                return false;
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
                    if (this.OutputsPerNeuronList[i] == 0 && !IsNeuronOutput(i) &&
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
                    if (this.OutputsPerNeuronList[i] == 0 && !IsNeuronOutput(i) &&
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

            public int Get_IndexOfNeuronWithoutOutput_First( )
            {
                for (int i = 0; i < this.OutputsPerNeuronList.Length; i++)
                {
                    if (this.InputsPerNeuronList[i] > 0
                        && this.OutputsPerNeuronList[i] == 0
                        && !IsNeuronOutput(i) && !IsNeuronInput(i))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public int Get_IndexOfNeuron_byInputs_First(int countInputs)
            {
                for (int i = 0; i < this.InputsPerNeuronList.Length; i++)
                {
                    if (this.InputsPerNeuronList[i] == countInputs
                        
                        && !IsNeuronOutput(i) && !IsNeuronInput(i))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public int Get_IndexOfNeuron_New_First()
            {
                for (int i = InputCounts; i < this.InputsPerNeuronList.Length; i++)
                {
                    if (this.InputsPerNeuronList[i] == 0 
                        && this.OutputsPerNeuronList[i] == 0

                        && !IsNeuronOutput(i) )
                    {
                        return i;
                    }
                }

                return -1;
            }

            public int Get_IndexOfNeuron_byInputs_hasOutput_First(int countInputs)
            {
                for (int i = 0; i < this.InputsPerNeuronList.Length; i++)
                {
                    if (this.InputsPerNeuronList[i] == countInputs

                        && !IsNeuronOutput(i) && !IsNeuronInput(i)
                        && this.OutputsPerNeuronList[i] > 0)
                    {
                        return i;
                    }
                }

                return -1;
            }

            public int Get_IndexOfNeuronWithoutInput_First()
            {
                for (int i = 0; i < this.OutputsPerNeuronList.Length; i++)
                {
                    if (this.InputsPerNeuronList[i] == 0
                        && this.OutputsPerNeuronList[i] > 0
                        && !IsNeuronOutput(i) && !IsNeuronInput(i))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public int Get_IndexLink_byBackN_First(int neuron)
            {
                for(int i = 0;i < this.NeuronLinks.Length; i++)
                {
                    if (this.NeuronLinks[i].BackNeuronIndex == neuron)
                        return i;
                }

                return -1;
            }

            public int Get_IndexLink_byN_First(int neuron)
            {
                for (int i = 0; i < this.NeuronLinks.Length; i++)
                {
                    if (this.NeuronLinks[i].NeuronIndex == neuron)
                        return i;
                }

                return -1;
            }

            public void Evaluate()
            {
                Internals.Fill(0.0f);
                Outputs.Fill(0.0f);

                var nlSpan = NeuronLinks.GetSpan();
               
                if (NeuronLinks.Length == 0) return;
                int nlIndex = 0;


                while (nlIndex < NeuronLinks.Length)
                {
                    int ni = nlSpan[nlIndex].NeuronIndex;
                    int countNLinks = InputsPerNeuronList[ni];
                    float sumOutputs = CONST_Bias;

                    for (int i = 0; i < countNLinks; i++)
                    {
                        ref var link = ref nlSpan[nlIndex + i];
                        sumOutputs += NeuronInternalState[link.BackNeuronIndex] * link.Weight;

                        //nlIndex++;
                    }

                    //NeuronInternalState[n] = NeuronActivationFunction.ActivationFunction_tanh(sumOutputs);
                    NeuronInternalState[ni] = NeuronActivationFunction.ActivationFunction_ReLu(sumOutputs);

                    nlIndex += countNLinks;
                }
            }

            public void EvaluateFast()
            {
                Internals.Fill(0.0f);
                Outputs.Fill(0.0f);

                var nlSpan = NeuronLinks.GetSpan();
                var internals = NeuronInternalState.AsSpan();

                if (NeuronLinks.Length == 0) return;
                int nlIndex = 0;

                //var weightW = new System.Numerics.Vector2();

                while (nlIndex < NeuronLinks.Length)
                {
                    ref var link = ref nlSpan[nlIndex];
                    int ni = link.NeuronIndex;
                    float sumOutputs = CONST_Bias;

                    sumOutputs += NeuronInternalState[link.BackNeuronIndex] * link.Weight;
                    nlIndex++;

                    int countNLinks = InputsPerNeuronList[ni];
                    if (countNLinks == 2)
                    {
                        link = ref nlSpan[nlIndex];
                        sumOutputs += link.Compute(internals);// internals[link.BackNeuronIndex] * link.Weight;
                        
                        nlIndex ++;
                    }
                    else if (countNLinks == 3)
                    {
                        link = ref nlSpan[nlIndex];
                        sumOutputs += link.Compute(internals);

                        ref var link2 = ref nlSpan[nlIndex + 1];
                        sumOutputs += internals[link2.BackNeuronIndex] * link2.Weight;

                        nlIndex += 2;
                    }
                    else if(countNLinks > 3)
                    {
                        for (int i = 1; i < countNLinks; i++)
                        {
                            link = ref nlSpan[nlIndex];
                            sumOutputs += internals[link.BackNeuronIndex] * link.Weight;
                            nlIndex++;
                        }
                    }

                    //NeuronInternalState[n] = NeuronActivationFunction.ActivationFunction_tanh(sumOutputs);
                    NeuronInternalState[ni] = NeuronActivationFunction.ActivationFunction_ReLu(sumOutputs);
                }


            }

            public void EvaluateFastOld6()
            {
                Internals.Fill(0.0f);
                Outputs.Fill(0.0f);

                var nlSpan = NeuronLinks.GetSpan();
                var internals = NeuronInternalState.AsSpan();

                if (NeuronLinks.Length == 0) return;
                int nlIndex = 0;

                var weightW = new System.Numerics.Vector2();

                while (nlIndex < NeuronLinks.Length)
                {
                    int ni = nlSpan[nlIndex].NeuronIndex;
                    int countNLinks = InputsPerNeuronList[ni];
                    float sumOutputs = CONST_Bias;

                    
                    for (int i = 0; i < countNLinks-1; i+=2)
                    {
                        ref var link = ref nlSpan[nlIndex ];
                        ref var link2 = ref nlSpan[nlIndex+1];
                        // weightW.X = link.Weight;
                        // weightW.Y = link2.Weight;

                        //var weight = new System.Numerics.Vector4(link.Weight, link2.Weight,0.0f,0.0f);
                        //var bniV = new System.Numerics.Vector4(NeuronInternalState[link.BackNeuronIndex], NeuronInternalState[link2.BackNeuronIndex], 0.0f, 0.0f);

                        //sumOutputs += System.Numerics.Vector4.Dot(weight, bniV);

                        //sumOutputs +=
                        // nlSpan[nlIndex].Compute(internals)
                        //    + nlSpan[nlIndex + 1].Compute(internals);

                        // ref var link = ref nlSpan[nlIndex];
                        sumOutputs +=
                            internals[link2.BackNeuronIndex] * link2.Weight;

                        sumOutputs += internals[link.BackNeuronIndex] * link.Weight;

                        //link = ref nlSpan[nlIndex + 1];
                      
                        nlIndex +=2;
                    }

                    if((countNLinks&1) == 1)
                    {
                        ref var link = ref nlSpan[nlIndex ];
                        sumOutputs += NeuronInternalState[link.BackNeuronIndex] * link.Weight;
                        nlIndex ++;
                    }

                    //NeuronInternalState[n] = NeuronActivationFunction.ActivationFunction_tanh(sumOutputs);
                    NeuronInternalState[ni] = NeuronActivationFunction.ActivationFunction_ReLu(sumOutputs);

                    
                }


            }

            public void EvaluateFastOlder()
            {
                Internals.Fill(0.0f);
                Outputs.Fill(0.0f);

                var nlSpan = NeuronLinks.GetSpan();

                if (NeuronLinks.Length == 0) return;
                int nlIndex = 0;


                while (nlIndex < NeuronLinks.Length)
                {
                    ref var link = ref nlSpan[nlIndex];

                    int ni = link.NeuronIndex;
                    
                    float sumOutputs = CONST_Bias;

                    sumOutputs += NeuronInternalState[link.BackNeuronIndex] * link.Weight;
                    nlIndex++;

                    int countNLinks = InputsPerNeuronList[ni];
                    for (int i = 1; i < countNLinks; i++)
                    {
                        link = ref nlSpan[nlIndex ];
                        sumOutputs += NeuronInternalState[link.BackNeuronIndex] * link.Weight;

                        nlIndex++;
                    }

                    //NeuronInternalState[n] = NeuronActivationFunction.ActivationFunction_tanh(sumOutputs);
                    NeuronInternalState[ni] = NeuronActivationFunction.ActivationFunction_ReLu(sumOutputs);
                }


            }

            

            

            
            
            /// <summary>
            /// najde nejblizsi predchazejici neuron jeho posledni link
            /// </summary>
            /// <param name="linkIndex"></param>
            /// <returns></returns>
            private int FindEndLinkOfNeuronBefore(int linkIndex)
            {
                int ni = this.NeuronLinks[linkIndex].NeuronIndex;

                while(linkIndex > 0 && 
                    this.NeuronLinks[linkIndex-1].NeuronIndex == ni)
                {
                    linkIndex--;
                }


                if( this.NeuronLinks[linkIndex].NeuronIndex == ni)
                {
                    linkIndex--;
                }

                return linkIndex;
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
            public float BackNeuronValue;

            public override string ToString()
            {
                return $"BNIndex:{BackNeuronIndex} , NIndex:{NeuronIndex}, weight:{Weight}";
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float Compute(Span<float> internalStateNeuron)
            {

                return internalStateNeuron[BackNeuronIndex] * Weight;
            }
        }


    }
}

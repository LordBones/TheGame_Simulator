using BonesLib.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BonesLib.FixedForwardNN
{
    public class FixedForwardNN
    {
        private const float CONST_Bias = 1.0f;


        public short[] Topology { get; private set; }
        float[] _outputs;
        int _outputsPadding;
        public Span<float> Outputs { get => _outputs.AsSpan().Slice(0, _outputs.Length - _outputsPadding); }
        float[] _inputs;
        int _inputsPadding;
        public Span<float> Inputs { get => _inputs.AsSpan().Slice(0, _inputs.Length - _inputsPadding); }

        private sLayersNN _layersNN;
        //public sLayersNN LayersNN { get =>  _layersNN; }
        public ref sLayersNN LayersNN { get => ref _layersNN; }
        public int GetTotalWeighs()
        {
            int sum = 0;
            for(int i = 0; i < _layersNN.LayersDef.Length; i++)
            {
                sum += _layersNN.GetWeights(i).Length;
            }

            return sum;
        }


        public FixedForwardNN(short countInputs, short countOutputs)
        {
            _inputsPadding = ComputePaddingLenth(countInputs);
            _outputsPadding = ComputePaddingLenth(countOutputs);
            _outputs = new float[countOutputs + _outputsPadding];
            _inputs = new float[countInputs+_inputsPadding];

        }

        public FixedForwardNN Clone()
        {
            var result = new FixedForwardNN( (short)Inputs.Length, (short)Outputs.Length);
            result.Topology = new short[Topology.Length];
            Topology.CopyTo(result.Topology,0);

            result._layersNN = LayersNN.Clone();
            return result;
        }

        public void SetTopology(short[] topology)
        {
            Topology = topology.ToArray();

            for (int i = 0; i < Topology.Length; i++)
                Topology[i] = (short)(Topology[i]+ComputePaddingLenth(Topology[i]));

            LayersNN.LayersDef = new LayerDef[topology.Length + 1];
      
            int totalCountN = TotalCountN_FromTopology(Topology);
            int totalCountW = TotalCountWeights_FromTopology(Topology);

            LayersNN.Biases = new float[totalCountN];
            LayersNN.NOuts = new float[totalCountN];
            LayersNN.NSumss = new float[totalCountN];

            LayersNN.Weights = new float[totalCountW];
            LayersNN.WeightsLastDelta = new float[totalCountW];


            int lastBiasesIndex = 0;
            int lastWeightsIndex = 0;


            short linksPerNeuron = (short)_inputs.Length;
            short countLastNeurons = (short)_inputs.Length;


            for (int i = 0; i < Topology.Length; i++)
            {
                var topologyPadding = ComputePaddingLenth(topology[i]);
                LayersNN.LayersDef[i] = new LayerDef((Topology[i]), linksPerNeuron,
                    LayerDef.eActivationFunction.Elu
                    , lastWeightsIndex, countLastNeurons * Topology[i]
                    , lastBiasesIndex, Topology[i]
                    , (short)topologyPadding
                    );

            
                lastBiasesIndex +=Topology[i];

                lastWeightsIndex += countLastNeurons* Topology[i];


                linksPerNeuron = Topology[i];
                countLastNeurons = Topology[i];
            }

            int outputLayer = topology.Length;
            LayersNN.LayersDef[outputLayer] = new LayerDef((short)_outputs.Length, linksPerNeuron
                , LayerDef.eActivationFunction.Logmoid
                , lastWeightsIndex, countLastNeurons * _outputs.Length
                , lastBiasesIndex, _outputs.Length
                , (short)_outputsPadding
                );

        }

        private int TotalCountN_FromTopology(short[] topology)
        {
            int sum = 0;
            for(int i = 0; i < topology.Length; i++)
            {
                sum+=topology[i];
            }
            sum += _outputs.Length;
            return sum;
        }

        private int TotalCountWeights_FromTopology(short[] topology)
        {
            int lastLayerSize = _inputs.Length;
            int sum = 0;
            for (int i = 0; i < topology.Length; i++)
            {
                sum += lastLayerSize* topology[i];
                lastLayerSize = topology[i];
            }
            sum += topology[topology.Length-1] * _outputs.Length;
            return sum;
        }


        public void InitBaseWeights()
        {
            for (int l = 0; l < LayersNN.LayersDef.Length; l++)
            {
                Span<float> tmp = LayersNN.GetWeights(l);
                //float scale =  GetWeightScaleBy_ActivationFunction(LayersNN.LayersDef[l].typeAF, LayersNN.LayersDef[l].CountWPerN+1, LayersNN.LayersDef[l].CountN);

                //for (int i = 0; i < tmp.Length; i++)
                //    tmp[i] =
                //// (float)(RandomGen.Default.GetRandomNumberDouble()*scale
                //(float)((RandomGen.Default.GetRandomNumberDouble()) * scale 
                //* GetRandSign()
                //)+float.Epsilon;
                //        //(float)(RandomGen.Default.GetRandomNumberDoubleGausisan() * scale
                        ;

                var tmpBias = LayersNN.GetBias(l);
                for (int i = 0; i < tmpBias.Length; i++)
                    tmpBias[i] = //0.0f
                //0.1f;
                 (float)(RandomGen.Default.GetRandomNumberDouble())* //*0.1)
                  (1.0f / (LayersNN.LayersDef[l].CountWPerN))//scale//* GetRandSign()
                  //+ float.Epsilon
                  ;

                float maxSumW = GetMaxWeightSumBy_ActivationFunction(LayersNN.LayersDef[l].typeAF);

                Span<float> Ns = LayersNN.GetNOuts(l);
                Span<double> tmpBuff = new double[LayersNN.LayersDef[l].CountWPerN];
                int wIndex = 0;
                for (int i = 0; i < Ns.Length; i++)
                {
                    Helper_RandomSequences.SequenceEqualsOne(RandomGen.Default, tmpBuff,maxSumW 
                        //- tmpBias[i]
                        );

                    for(int j = 0; j < tmpBuff.Length; j++)
                    {
                        tmp[wIndex++] = (float)tmpBuff[j] ;// * GetRandSign(0.4);
                    }
                }

                
            }
        }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetRandSign(double coefMinus)
    {
        return (RandomGen.Default.GetRandomNumberDouble() < coefMinus) ? -1.0f : 1.0f;
    }
        private float GetWeightScaleBy_ActivationFunction(LayerDef.eActivationFunction eAF, int countLInput, int countLOutput)
        {
            if (eAF == LayerDef.eActivationFunction.Sigmoid)//|| eAF == LayerDef.eActivationFunction.Tanh)
                return MathF.Sqrt(0.65f / (countLInput));

            if (eAF == LayerDef.eActivationFunction.Tanh)
                return MathF.Sqrt(1.0f / (countLInput * 2));

            if (eAF == LayerDef.eActivationFunction.Logmoid)
                return MathF.Sqrt(1.0f / (countLInput));

            return (1.0f / (countLInput)) / 2.0f;
        }

        private float GetMaxWeightSumBy_ActivationFunction(LayerDef.eActivationFunction eAF)
        {
            if(eAF == LayerDef.eActivationFunction.Sigmoid )//|| eAF == LayerDef.eActivationFunction.Tanh)
                return 4.0f;

            if ( eAF == LayerDef.eActivationFunction.Tanh)
                return 2.0f;

            if (eAF == LayerDef.eActivationFunction.Logmoid)
                return 2.0f;

            return  1.0f;
        }

        public void Evaluate()
        {
            EvaluateFirstLevel();
            EvaluateOtherLevels();

            var neurons = this.LayersNN.GetNOuts(this.LayersNN.LayersDef.Length-1);

            neurons.CopyTo(this._outputs);
            //Array.Copy(neurons, this.Outputs, this.Outputs.Length);
            //for (int i = 0; i < this.Outputs.Length; i++)
            //{
            //    this.Outputs[i] = neurons[i].Output;
            //}

        }

        /// <summary>
        /// output coef is 0 .. 1 . It reduce diff error
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="expectOutput"></param>
        /// <param name="outputCoef"></param>
        public void BackPropagate(float alpha, Span<float> expectOutput,float moment, float outputCoef)
        {
            float[] expOutputExtend = ArrayPool<float>.Shared.Rent(_outputs.Length);
            _outputs.CopyTo(expOutputExtend.AsSpan());
            expectOutput.CopyTo(expOutputExtend);
            

            float [] oGradsSource = ArrayPool<float>.Shared.Rent(_outputs.Length);
            var oGrads = oGradsSource.AsSpan(0,_outputs.Length);
            int lastLIndex = this.LayersNN.LayersDef.Length - 1;
            
            BP_ComputeGradFromOutput(oGrads, expOutputExtend.AsSpan(0,_outputs.Length), outputCoef);
            ArrayPool<float>.Shared.Return(expOutputExtend);


            for (int i = lastLIndex - 1; i >= 0; i--)
            {
                int lAsInputIndex = i;
                var lastLNM1 = this.LayersNN.GetNOuts(lAsInputIndex);

                float[] hGradsSource = ArrayPool<float>.Shared.Rent(lastLNM1.Length);
                var hGrads = hGradsSource.AsSpan(0, lastLNM1.Length);

                

                BP_UpdateWeights(oGrads, lAsInputIndex, alpha, moment);

                BP_ComputeNextGrad(oGrads, lAsInputIndex, hGrads);

                ArrayPool<float>.Shared.Return(oGradsSource);
                oGrads = hGrads;
                oGradsSource = hGradsSource;
            }

            BP_UpdateWeights_AtStartInputs(oGrads, alpha, moment);

            ArrayPool<float>.Shared.Return(oGradsSource);
        }

        public void PrintNet(TextWriter tw)
        {
            const int CONST_WeightsPerRow = 10;
            tw.WriteLine("Inputs: {0}  Outputs: {1} \n", Inputs.Length, Outputs.Length);

            int countInputs = _inputs.Length;

            for(int i = 0;i < this.LayersNN.LayersDef.Length; i++)
            {
                var lNS = this.LayersNN.GetNOuts(i);
                var weights = this.LayersNN.GetWeights(i);
                var bias = this.LayersNN.GetBias(i);

                tw.WriteLine("Layer: {0}  Inputs:{1} Outputs: {2} \n",i, countInputs, lNS.Length);
                tw.Write("# ");
                int biasIndex = 0;
                tw.Write("b: {0,7:0.0000}, ", bias[biasIndex++]);

                int counter = countInputs;
                
                for(int j = 0;j < weights.Length; j++)
                {
                    if(counter == 0)
                    {
                        tw.WriteLine();
                        tw.Write("# ");
                        tw.Write("b: {0,7:0.0000}, ", bias[biasIndex++]);
                        counter = countInputs;
                    }

                    tw.Write("{0,7:0.0000}, ", weights[j]);

                    counter--;
                }

                tw.WriteLine();
                countInputs = lNS.Length;
            }

         
        }

        private void BP_ComputeGradFromOutput(Span<float> grads, Span<float> expectOutput, float outputCoef)
        {
            int lastLIndex = this.LayersNN.LayersDef.Length - 1;

            var derivativeF = this.LayersNN.LayersDef[lastLIndex].FuncActivateReverseDeleg;
            var lastLayerSums = this.LayersNN.GetNSumss(lastLIndex);
            // 1. compute output gradients. assumes log-sigmoid!
            for (int i = 0; i < grads.Length; ++i)
            {
                float derivative = derivativeF(lastLayerSums[i]);
                grads[i] = derivative * ((expectOutput[i] - _outputs[i])*outputCoef);
            }
        }

        private void BP_ComputeNextGrad(Span<float> oGrads, int indexLayerAsInput, Span<float> nextGrads)
        {
            var asInuptN = this.LayersNN.GetNOuts(indexLayerAsInput);
            var asInuptSumsN = this.LayersNN.GetNSumss(indexLayerAsInput);
            var lastLNMWeights = this.LayersNN.GetWeights(indexLayerAsInput+1);

            var derivateF = this.LayersNN.LayersDef[indexLayerAsInput].FuncActivateReverseDeleg;
            // 2. compute hidden gradients. assumes tanh!
            for (int i = 0; i < nextGrads.Length; ++i)
            {
                float derivative = derivateF(asInuptSumsN[i]);
                float sum = 0.0f;
                int indexWeight = i;
                int j = 0;

                float sum1 = 0.0f;
                float sum2 = 0.0f;
                float sum3 = 0.0f;
                float sum4 = 0.0f;
                for (; j < oGrads.Length-3; j+=4)
                {  // each hidden delta is the sum of numOutput terms
                    sum4 += oGrads[j + 3] * lastLNMWeights[indexWeight + asInuptN.Length * 3];
                    sum1 += oGrads[j] * lastLNMWeights[indexWeight] ;
                    sum2 +=  oGrads[j + 1] * lastLNMWeights[indexWeight + asInuptN.Length] ;
                    sum3 +=  oGrads[j + 2] * lastLNMWeights[indexWeight + asInuptN.Length * 2] ;
                   

                    indexWeight += asInuptN.Length*4;
                }// each downstream gradient * outgoing weight

                sum += sum1 + sum2 + sum3 + sum4;

                while ( j < oGrads.Length)
                {  // each hidden delta is the sum of numOutput terms
                    sum += oGrads[j] * lastLNMWeights[indexWeight];
                    indexWeight += asInuptN.Length;
                    j++;
                }// each downstream gradient * outgoing weight

                nextGrads[i] = derivative * sum;
            }
        }


        private void BP_UpdateWeights_AtStartInputs(Span<float> oGrads, float alpha, float moment)
        {
            
            var LNMWeights = this.LayersNN.GetWeights(0);
            var LNBias = this.LayersNN.GetBias(0);
            var LNMWeightsDelta = this.LayersNN.GetWeightsDelta(0);

            int weightIndex = 0;
            // 5. update hidden to output weights
            for (int i = 0; i < oGrads.Length; ++i)  // 0..3 (4)
            {
                float ograd = oGrads[i];
                float alphaTimesOGrad = alpha * ograd;
                for (int j = 0; j < _inputs.Length; ++j) // 0..1 (2)
                {
                    float delta = alphaTimesOGrad * _inputs[j];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex] += delta + moment * LNMWeightsDelta[weightIndex]; ;
                    
                    LNMWeightsDelta[weightIndex] = delta;
                    weightIndex++;
                }

                LNBias[i] += alphaTimesOGrad * 1.0f;
            }
        }

        private void BP_UpdateWeights(Span<float> oGrads, int indexLayerAsInput, float alpha, float moment)
        {
            var  LNAsInput = this.LayersNN.GetNOuts(indexLayerAsInput);
            var LNMWeights = this.LayersNN.GetWeights(indexLayerAsInput+1);
            var LNMWeightsDelta = this.LayersNN.GetWeightsDelta(indexLayerAsInput + 1);
            var LNBias = this.LayersNN.GetBias(indexLayerAsInput + 1);

            int weightIndex = 0;
            // 5. update hidden to output weights
            for (int i = 0; i < oGrads.Length; ++i)  // 0..3 (4)
            {
                //for (int j = 0; j < LNAsInput.Length; ++j) // 0..1 (2)
                //{
                //    float delta = alpha * oGrads[i] * LNAsInput[j];  // hOutputs are inputs to next layer
                //    LNMWeights[weightIndex] += delta + moment * LNMWeightsDelta[weightIndex];
                //    LNMWeightsDelta[weightIndex] = delta;
                //    weightIndex++;
                //}

                var oGrad = oGrads[i];

                int j = 0;
                float alphaTimesOGrad = alpha * oGrad;
                for (; j < LNAsInput.Length-3; j+=4) // 0..1 (2)
                {
                    float delta4 = alphaTimesOGrad * LNAsInput[j + 3];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex + 3] += delta4 + moment * LNMWeightsDelta[weightIndex + 3];
                    LNMWeightsDelta[weightIndex + 3] = delta4;
                    float delta = alphaTimesOGrad * LNAsInput[j];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex] += delta + moment * LNMWeightsDelta[weightIndex];
                    LNMWeightsDelta[weightIndex] = delta;
                    float delta2 = alphaTimesOGrad * LNAsInput[j+1];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex+1] += delta2 + moment * LNMWeightsDelta[weightIndex + 1];
                    LNMWeightsDelta[weightIndex + 1] = delta2;
                    float delta3 = alphaTimesOGrad * LNAsInput[j+2];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex + 2] += delta3 + moment * LNMWeightsDelta[weightIndex+2];
                    LNMWeightsDelta[weightIndex+2] = delta3;
                 
                    weightIndex +=4;
                }

                while ( j < LNAsInput.Length) // 0..1 (2)
                {
                    float delta = alphaTimesOGrad * LNAsInput[j];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex] += delta + moment * LNMWeightsDelta[weightIndex];
                    LNMWeightsDelta[weightIndex] = delta;
                    weightIndex ++;
                    j++;
                }

                LNBias[i] += alphaTimesOGrad * 1.0f;
            }
        }

        private void EvaluateFirstLevel()
        {
            var lnn = this.LayersNN;
            var layerDef =  lnn.LayersDef[0];
          //  var countWPerN = layerDef.CountWPerN;
            var layerW = lnn.GetWeights(0);
            var layerN = lnn.GetNOuts(0);
            var layerNSums = lnn.GetNSumss(0);
            var layerBias = lnn.GetBias(0);
            var inputs = this._inputs;

            int wIndex = 0;
            for (int n = 0; n < layerN.Length; n++)
            {
                float sum = 0.0f;
                int inLenght = inputs.Length;

                //float sum1 = 0.0f;
                //float sum2 = 0.0f;
                //float sum3 = 0.0f;
                //float sum4 = 0.0f;
                //for (; w < countWPerN-3; w += 4)
                //{

                //    sum1 += inputs[w] * layerW[wIndex] ;
                //    sum2 +=  inputs[w + 1] * layerW[wIndex + 1] ;
                //    sum3 += inputs[w + 2] * layerW[wIndex + 2] ;
                //    sum4 += inputs[w + 3] * layerW[wIndex + 3];
                //    wIndex += 4;
                //}

                //sum += sum1+sum2+sum3 + sum4;
                ////for (; w < countWPerN - 3; w += 4)
                ////{
                ////    sum += inputs[w] * layerW[wIndex] + inputs[w + 1] * layerW[wIndex + 1] +
                ////        inputs[w + 2] * layerW[wIndex + 2] + inputs[w + 3] * layerW[wIndex + 3];
                ////    wIndex += 4;
                ////}

                //while (w < countWPerN )
                //{
                //    sum += inputs[w] * layerW[wIndex];
                //    w++; wIndex++;
                //}

                sum += SumNeuronInputs(inputs, layerW.Slice(wIndex, inLenght));
                wIndex += inLenght;
                //sum += layerBias[n];
                layerNSums[n] = sum + layerBias[n] ;
                layerN[n] = layerDef.FuncActivateDeleg(layerNSums[n]);
            }
        }

        private void EvaluateOtherLevels()
        {
            var lnn = this.LayersNN;
            var layerNInputs = lnn.GetNOuts(0);
            var ld = lnn.LayersDef;
            for (int L = 1; L < ld.Length; L++)
            {
                 var layerDef =   ld[L];
                var countPerW = layerDef.CountWPerN;
                var lDeleg = layerDef.FuncActivateDeleg;
                var layerW = lnn.GetWeights(L);
                var layerN = lnn.GetNOuts(L);
                var layerNSums = lnn.GetNSumss(L);
                var layerBias = lnn.GetBias(L);
                


                int wIndex = 0;
                for (int n = 0; n < layerN.Length; n++)
                {
                    float sum = SumNeuronInputs(layerNInputs, layerW.Slice(wIndex, countPerW));
                    wIndex += countPerW;

                    sum += layerBias[n];
                    layerNSums[n] = sum;
                    layerN[n] = lDeleg(sum);

                }

                layerNInputs = layerN;
            }
        }

        private void EvaluateOtherLevelsFast()
        {
            var lnn = this.LayersNN;
            var layerNInputs = lnn.GetNOuts(0);
            ReadOnlySpan<System.Numerics.Vector4> layerNInputsVec = MemoryMarshal.Cast<float, System.Numerics.Vector4>(layerNInputs);

            var ld = lnn.LayersDef;
            for (int L = 1; L < ld.Length; L++)
            {
                var layerDef = ld[L];
                var countPerW = layerDef.CountWPerN;
                var lDeleg = layerDef.FuncActivateDeleg;
                var layerW = lnn.GetWeights(L);
                var layerN = lnn.GetNOuts(L);
                var layerNSums = lnn.GetNSumss(L);
                var layerBias = lnn.GetBias(L);

                var layerWVec = MemoryMarshal.Cast<float, System.Numerics.Vector4>(layerW);


                int wIndex = 0;
                for (int n = 0; n < layerN.Length; n++)
                {
                    float sum = SumNeuronInputsFast(layerNInputsVec, layerWVec.Slice(wIndex, countPerW>>2));
                    wIndex += countPerW>>2;

                    sum += layerBias[n] ;
                    layerNSums[n] = sum;
                    layerN[n] = lDeleg(sum);

                }

                layerNInputsVec =  MemoryMarshal.Cast<float, System.Numerics.Vector4>(layerN);
                layerNInputs = layerN;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float SumNeuronInputs(Span<float> layerNInputs, Span<float> layerW)
        {
            float sum = 0.0f;

            int w = 0;

            float sum1 = 0.0f;
            float sum2 = 0.0f;
            float sum3 = 0.0f;
            float sum4 = 0.0f;

            for (; w < layerW.Length - 3; w += 4)
            {
                sum4 += layerNInputs[w + 3] * layerW[w + 3];
                sum1 += layerNInputs[w] * layerW[w];
                sum2 += layerNInputs[w + 1] * layerW[w + 1];
                sum3 += layerNInputs[w + 2] * layerW[w + 2];
            }

        


            sum += sum1 + sum2 + sum3 + sum4;

            while (w < layerW.Length)
            {
                sum += layerNInputs[w] * layerW[w];
                w++;
            }

            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float SumNeuronInputsFast(Span<float> layerNInputs, Span<float> layerW)
        {
            float sum = 0.0f;

            int w = 0;

            ReadOnlySpan<System.Numerics.Vector4> v = MemoryMarshal.Cast<float, System.Numerics.Vector4> (layerNInputs);
            ReadOnlySpan<System.Numerics.Vector4> v2 = MemoryMarshal.Cast<float, System.Numerics.Vector4>(layerW);
            for (int k =0; k < v.Length ; k += 1)
            {
                sum += System.Numerics.Vector4.Dot(v[k], v2[k]);
            }

            //while (w < layerW.Length)
            //{
            //    sum += layerNInputs[w] * layerW[w];
            //    w++;
            //}

            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float SumNeuronInputsFast(ReadOnlySpan<System.Numerics.Vector4> layerNInputs, ReadOnlySpan<System.Numerics.Vector4> layerW)
        {
            float sum = 0.0f;

            int w = 0;

            // ReadOnlySpan<System.Numerics.Vector4> v = MemoryMarshal.Cast<float, System.Numerics.Vector4>(layerNInputs);
            //ReadOnlySpan<System.Numerics.Vector4> v2 = MemoryMarshal.Cast<float, System.Numerics.Vector4>(layerW);
            var len = layerW.Length;
            for (int k = 0; k < len; k += 1)
            {
                sum += System.Numerics.Vector4.Dot(layerNInputs[k], layerW[k]);
            }

            //while (w < layerW.Length)
            //{
            //    sum += layerNInputs[w] * layerW[w];
            //    w++;
            //}

            return sum;
        }

       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float SumNeuronInputsFaster(Span<float> layerNInputs, Span<float> layerW)
        {
            float sum = 0.0f;
            float sum2 = 0.0f;
            int w = 0;

            int len = layerNInputs.Length - (layerNInputs.Length & 1);
            ReadOnlySpan<System.Numerics.Vector2> v = MemoryMarshal.Cast<float, System.Numerics.Vector2>(layerNInputs.Slice(0, len));
            ReadOnlySpan<System.Numerics.Vector2> v2 = MemoryMarshal.Cast<float, System.Numerics.Vector2>(layerW.Slice(0, len));
            for (int k = 0; k+1 < v.Length; k += 2)
            {
                sum += System.Numerics.Vector2.Dot(v[k], v2[k]);
                sum2 += System.Numerics.Vector2.Dot(v[k+1], v2[k+1]);
            }

            //while (w < layerW.Length)
            //{
            //    sum += layerNInputs[w] * layerW[w];
            //    w++;
            //}

            return sum + sum2;
        }

        private static int ComputePaddingLenth(int arrayLenght)
        {
            int incr = ((arrayLenght& 3) > 0) ? 4 : 0;
            return (arrayLenght & (~3)) + incr - arrayLenght;
        }

        public struct sLayersNN
        {
            public LayerDef[] LayersDef;

            
            public float [] Biases;
            public float[] NOuts;
            public float[] NSumss;

            public float[] Weights;
            public float[] WeightsLastDelta;


            public sLayersNN Clone()
            {
                var result = new sLayersNN();
                result.LayersDef = new LayerDef[LayersDef.Length];
                LayersDef.CopyTo(result.LayersDef, 0);

                result.Biases = new float[Biases.Length];
                this.Biases.CopyTo(result.Biases, 0);
          
                result.NOuts = new float[NOuts.Length];
                this.NOuts.CopyTo(result.NOuts, 0);
                result.NSumss = new float[NSumss.Length];
                this.NSumss.CopyTo(result.NSumss, 0);


                result.Weights = new float[Weights.Length];
                this.Weights.CopyTo(result.Weights, 0);
                result.WeightsLastDelta = new float[WeightsLastDelta.Length];
                this.WeightsLastDelta.CopyTo(result.WeightsLastDelta, 0);



                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetWeights(int layerIndex)
            {
                ref  var  ld = ref LayersDef[layerIndex];
                return Weights.AsSpan(ld.WeightMomentIndexStart, ld.WeightMomentCount);

                //return Layers[IndexWeight(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public  Span<float> GetWeightsDelta(int layerIndex)
            {
                ref readonly var ld = ref LayersDef[layerIndex];
                return WeightsLastDelta.AsSpan(ld.WeightMomentIndexStart, ld.WeightMomentCount);

                //return Layers[IndexWeightDelta(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetNOuts(int layerIndex)
            {
                ref  var ld = ref LayersDef[layerIndex];
                return NOuts.AsSpan(ld.BiasNIndexStart, ld.BiasNCount);

                //return Layers[IndexN(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetNSumss(int layerIndex)
            {
                ref var ld = ref LayersDef[layerIndex];
                return NSumss.AsSpan(ld.BiasNIndexStart, ld.BiasNCount);

                //return Layers[IndexN(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetBias(int layerIndex)
            {
                ref  var ld = ref LayersDef[layerIndex];
                return Biases.AsSpan(ld.BiasNIndexStart, ld.BiasNCount);

                //return Layers[IndexBias(layerIndex)];
            }
        }

        public  struct LayerDef
        {
            public enum eActivationFunction:byte {None, Relu,Elu,Sigmoid,Swish, Tanh, Logmoid, Mish}
            public  short CountN;
            public  short CountWPerN;
            public short Padding;

            public  int WeightMomentIndexStart;
            public  int WeightMomentCount;

            public  int BiasNIndexStart;
            public  int BiasNCount;

            public eActivationFunction typeAF;
            public delegate float FuncActivate(float x);
            public delegate float FuncActivateReverse(float x);

            public  FuncActivate FuncActivateDeleg;
            public  FuncActivateReverse FuncActivateReverseDeleg;
            public LayerDef(short countN, short countWPerN, eActivationFunction eAF,
                int weightMomentIndexStart, int weightMomentCount, int biasNIndexStart, int biasNCount, short padding)
            {
                CountN = countN;
                CountWPerN = countWPerN;
                FuncActivateDeleg = null;
                FuncActivateReverseDeleg = null;
                typeAF = eAF;
                this.WeightMomentCount = weightMomentCount;
                this.WeightMomentIndexStart = weightMomentIndexStart;
                this.BiasNIndexStart = biasNIndexStart;
                this.BiasNCount = biasNCount;
                this.Padding = padding;
                SetAFunc(eAF);
               
            }

            private void SetAFunc(eActivationFunction eAF)
            {
                if(eAF== eActivationFunction.Sigmoid)
                {
                    this.FuncActivateDeleg = FA_Sigmoid;
                    this.FuncActivateReverseDeleg = FAR_Sigmoid;
                }
                if (eAF == eActivationFunction.Relu)
                {
                    this.FuncActivateDeleg = FA_RELU;
                    this.FuncActivateReverseDeleg = FAR_RELU;
                }
                if (eAF == eActivationFunction.Elu)
                {
                    this.FuncActivateDeleg = FA_ELU;
                    this.FuncActivateReverseDeleg = FAR_ELU;
                }
                if (eAF == eActivationFunction.Swish)
                {
                    this.FuncActivateDeleg = FA_Swish;
                    this.FuncActivateReverseDeleg = FAR_Swish;
                }
                if (eAF == eActivationFunction.Tanh)
                {
                    this.FuncActivateDeleg = FA_Tanh;
                    this.FuncActivateReverseDeleg = FAR_Tanh;
                }
                if (eAF == eActivationFunction.Logmoid)
                {
                    this.FuncActivateDeleg = FA_Logmoid;
                    this.FuncActivateReverseDeleg = FAR_Logmoid;
                }
                if (eAF == eActivationFunction.Mish)
                {
                    this.FuncActivateDeleg = FA_Mish;
                    this.FuncActivateReverseDeleg = FAR_Mish;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_RELU(float x)
            {
                
                return (x < 0.0f) ? 0.01f * x : x;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_RELU(float x)
            {
                return (x < 0.0f)? 0.01f : 1.0f;
            }

            private const float eluAlpha = 1.0f;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_ELU(float x)
            {

                return (x < 0.0f) ? eluAlpha*(
                    //MathF.Exp(x)
                    RandomGen.FastExp(x)
                    - 1.0f) : x;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_ELU(float x)
            {
              
                return (x < 0.0f) ? FA_ELU(x) + eluAlpha : 1.0f;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Sigmoid(float x)
            {

                if (x < -45.0f) return 0.0f;
                else if (x > 45.0f) return 1.0f;


                return 1.0f / (1.0f + MathF.Exp( -x));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Sigmoid(float x)
            {
               float sigm = FA_Sigmoid(x);
                return sigm * (1- sigm);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Swish(float x)
            {
                const float alpha = 1.0f;
                return x / (alpha + MathF.Exp( -x));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Swish(float x)
            {
                const float alpha = 1.0f;
                var tmp = FA_Swish(x);
                var sig = FA_Sigmoid(x);

                return tmp +sig*(alpha - tmp);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Tanh(float x)
            {
                //float ex = MathF.Pow(MathF.E, x);
                //float e_x = MathF.Pow(MathF.E, -x);

                //return (ex-e_x)/(ex + e_x);
                return 2 * (1 / (1 + MathF.Exp( -2*x)))-1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Tanh(float x)
            {
                float tanh = FA_Tanh(x);

                return 1-
                    tanh * tanh;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Logmoid(float x)
            {
                if (x >= 0.0f)
                    return MathF.Log(x + 1);

                return -MathF.Log(-x + 1);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Logmoid(float x)
            {
                // x by melo byt puvodni pred aktivacni funkci
                if (x >= 0.0f)
                    return 1 / (x + 1);

                return 1 / (1 - x);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Mish(float x)
            {
                return x*
                    //MathF.Tanh
                    RandomGen.FastTanh
                    (MathF.Log(1+
                    //MathF.Exp(x)
                    RandomGen.FastExp(x)
                    )) ;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Mish(float x)
            {
                float expX = MathF.Exp(x);
                float gama =(4*(x+1)+4*MathF.Exp(2*x)+  MathF.Exp(3 * x)+ expX * (4*x+6));
                float fi = expX + MathF.Exp(2 * x) + 2;

                return expX * gama / (fi * fi);
            }

            
        }
    }
}

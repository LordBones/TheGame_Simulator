using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BonesLib.FixedForwardNN
{
    public class FixedForwardNN
    {
        private const float CONST_Bias = 1.0f;


        public short[] Topology { get; private set; }
        public float[] Outputs { get; private set; }
        public float[] Inputs { get; private set; }

        private sLayersNN _layersNN;
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
            Outputs = new float[countOutputs];
            Inputs = new float[countInputs];

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

            LayersNN.LayersDef = new LayerDef[topology.Length + 1];
            LayersNN.Layers = new float[(topology.Length + 1) * 4][];

            int totalCountN = TotalCountN_FromTopology(topology);
            int totalCountW = TotalCountWeights_FromTopology(topology);

            LayersNN.BiasesIndex = new int[topology.Length+1];
            LayersNN.Biases = new float[totalCountN];
            LayersNN.Ns = new float[totalCountN];

            LayersNN.WeightIndex = new int[topology.Length + 1];
            LayersNN.Weights = new float[totalCountW];
            LayersNN.WeightsLastDelta = new float[totalCountW];


            int lastBiasesIndex = 0;
            int lastWeightsIndex = 0;


            short linksPerNeuron = (short)Inputs.Length;
            short countLastNeurons = (short)Inputs.Length;


            for (int i = 0; i < topology.Length; i++)
            {
                LayersNN.LayersDef[i] = new LayerDef(topology[i], linksPerNeuron,
                    //LayerDef.FA_Swift, LayerDef.FAR_Swift
                    //LayerDef.FA_Sigmoid, LayerDef.FAR_Sigmoid

                    LayerDef.FA_RELU, LayerDef.FAR_RELU

                    , lastWeightsIndex, countLastNeurons * topology[i]
                    , lastBiasesIndex, topology[i]

                    );

                LayersNN.Layers[sLayersNN.IndexWeight(i)] = new float[countLastNeurons * topology[i]];
                LayersNN.Layers[sLayersNN.IndexWeightDelta(i)] = new float[countLastNeurons * topology[i]];
                LayersNN.Layers[sLayersNN.IndexN(i)] = new float[topology[i]];
                LayersNN.Layers[sLayersNN.IndexBias(i)] = new float[topology[i]];

                LayersNN.BiasesIndex[i] = lastBiasesIndex;
                lastBiasesIndex +=topology[i];

                LayersNN.WeightIndex[i] = lastWeightsIndex;
                lastWeightsIndex += countLastNeurons* topology[i];


                linksPerNeuron = topology[i];
                countLastNeurons = topology[i];
            }

            int outputLayer = topology.Length;
            LayersNN.LayersDef[outputLayer] = new LayerDef((short)Outputs.Length, linksPerNeuron
                ,LayerDef.FA_Sigmoid,LayerDef.FAR_Sigmoid
                , lastWeightsIndex, countLastNeurons * Outputs.Length
                , lastBiasesIndex, Outputs.Length

                );

            LayersNN.Layers[sLayersNN.IndexWeight(outputLayer)] = new float[countLastNeurons * Outputs.Length];
            LayersNN.Layers[sLayersNN.IndexN(outputLayer)] = new float[Outputs.Length];
            LayersNN.Layers[sLayersNN.IndexBias(outputLayer)] = new float[Outputs.Length];
            LayersNN.Layers[sLayersNN.IndexWeightDelta(outputLayer)] = new float[countLastNeurons * Outputs.Length];

            LayersNN.BiasesIndex[outputLayer] = lastBiasesIndex;
            LayersNN.WeightIndex[outputLayer] = lastWeightsIndex;


        }

        private int TotalCountN_FromTopology(short[] topology)
        {
            int sum = 0;
            for(int i = 0; i < topology.Length; i++)
            {
                sum+=topology[i];
            }
            sum += Outputs.Length;
            return sum;
        }

        private int TotalCountWeights_FromTopology(short[] topology)
        {
            int lastLayerSize = Inputs.Length;
            int sum = 0;
            for (int i = 0; i < topology.Length; i++)
            {
                sum += lastLayerSize* topology[i];
                lastLayerSize = topology[i];
            }
            sum += topology[topology.Length-1] * Outputs.Length;
            return sum;
        }


        public void InitBaseWeights()
        {
            for (int l = 0; l < LayersNN.LayersDef.Length; l++)
            {
                Span<float> tmp = LayersNN.GetWeights(l);
                float scale = 1.000f/ (LayersNN.LayersDef[l].CountWPerN*2);
                //MathF.Sqrt(2.0f / LayersNN.LayersDef[l].CountWPerN);
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i] =
                // (float)(RandomGen.Default.GetRandomNumberDouble()*scale
                (float)(RandomGen.Default.GetRandomNumberDoubleGausisan() * scale);
                        //(float)(RandomGen.Default.GetRandomNumberDoubleGausisan() * scale
                        ;

                tmp = LayersNN.GetBias(l);
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i] = //1.0f;
                 (float)(RandomGen.Default.GetRandomNumberDouble() - 0.5f);
            }
        }

        public void Evaluate()
        {
            EvaluateFirstLevel();
            EvaluateOtherLevels();

            var neurons = this.LayersNN.GetNs(this.LayersNN.LayersDef.Length-1);

            neurons.CopyTo(this.Outputs);
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
            float [] oGrads = new float[Outputs.Length];
            int lastLIndex = this.LayersNN.LayersDef.Length - 1;
            
            BP_ComputeGradFromOutput(oGrads.AsSpan(),expectOutput,outputCoef);


            for (int i = lastLIndex - 1; i >= 0; i--)
            {
                int lAsInputIndex = i;
                var lastLNM1 = this.LayersNN.GetNs(lAsInputIndex);

                float[] hGrads = new float[lastLNM1.Length];

                BP_UpdateWeights(oGrads.AsSpan(), lAsInputIndex, alpha, moment);

                BP_ComputeNextGrad(oGrads.AsSpan(), lAsInputIndex, hGrads.AsSpan());
                oGrads = hGrads;
            }

            BP_UpdateWeights_AtStartInputs(oGrads, alpha);
        }

        private void BP_ComputeGradFromOutput(Span<float> grads, Span<float> expectOutput, float outputCoef)
        {
            int lastLIndex = this.LayersNN.LayersDef.Length - 1;

            var derivativeF = this.LayersNN.LayersDef[lastLIndex].FuncActivateReverseDeleg;
            // 1. compute output gradients. assumes log-sigmoid!
            for (int i = 0; i < grads.Length; ++i)
            {
                float derivative = derivativeF(Outputs[i]);
                grads[i] = derivative * ((expectOutput[i] - Outputs[i])*outputCoef);
            }
        }

        private void BP_ComputeNextGrad(Span<float> oGrads, int indexLayerAsInput, Span<float> nextGrads)
        {
            var asInuptN = this.LayersNN.GetNs(indexLayerAsInput);
            var lastLNMWeights = this.LayersNN.GetWeights(indexLayerAsInput+1);

            var derivateF = this.LayersNN.LayersDef[indexLayerAsInput].FuncActivateReverseDeleg;
            // 2. compute hidden gradients. assumes tanh!
            for (int i = 0; i < nextGrads.Length; ++i)
            {
                float derivative = derivateF(asInuptN[i]);
                float sum = 0.0f;
                int indexWeight = i;
                for (int j = 0; j < oGrads.Length; ++j)
                {  // each hidden delta is the sum of numOutput terms
                    sum += oGrads[j] * lastLNMWeights[indexWeight];
                    indexWeight += asInuptN.Length;
                }// each downstream gradient * outgoing weight
                nextGrads[i] = derivative * sum;
            }
        }


        private void BP_UpdateWeights_AtStartInputs(Span<float> oGrads, float alpha)
        {
            
            var LNMWeights = this.LayersNN.GetWeights(0);
            var LNBias = this.LayersNN.GetBias(0);

            int weightIndex = 0;
            // 5. update hidden to output weights
            for (int i = 0; i < oGrads.Length; ++i)  // 0..3 (4)
            {
                float ograd = oGrads[i];
                for (int j = 0; j < Inputs.Length; ++j) // 0..1 (2)
                {
                    float delta = alpha * ograd * Inputs[j];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex++] += delta;

                }

                LNBias[i] += alpha * ograd * 1.0f;
            }
        }

        private void BP_UpdateWeights(Span<float> oGrads, int indexLayerAsInput, float alpha, float moment)
        {
            var  LNAsInput = this.LayersNN.GetNs(indexLayerAsInput);
            var LNMWeights = this.LayersNN.GetWeights(indexLayerAsInput+1);
            var LNMWeightsDelta = this.LayersNN.GetWeightsDelta(indexLayerAsInput + 1);
            var LNBias = this.LayersNN.GetBias(indexLayerAsInput + 1);

            int weightIndex = 0;
            // 5. update hidden to output weights
            for (int i = 0; i < oGrads.Length; ++i)  // 0..3 (4)
            {
                for (int j = 0; j < LNAsInput.Length; ++j) // 0..1 (2)
                {
                    float delta = alpha * oGrads[i] * LNAsInput[j];  // hOutputs are inputs to next layer
                    LNMWeights[weightIndex] += delta;
                    LNMWeights[weightIndex] += moment * LNMWeightsDelta[weightIndex];
                    LNMWeightsDelta[weightIndex] = delta;
                    weightIndex++;
                }

                LNBias[i] += alpha * oGrads[i] * 1.0f;
            }
        }

        private void EvaluateFirstLevel()
        {
            var layerDef =  this.LayersNN.LayersDef[0];
            var layerW = this.LayersNN.GetWeights(0);
            var layerN = this.LayersNN.GetNs(0);
            var layerBias = this.LayersNN.GetBias(0);
            var inputs = this.Inputs;

            int wIndex = 0;
            for (int n = 0; n < layerN.Length; n++)
            {
                {
                    float sum = 0.0f;
                    for (int w = 0; w < layerDef.CountWPerN; w++)
                    {
                        sum += inputs[w] * layerW[wIndex];
                        wIndex++;
                    }

                    sum += layerBias[n];
                    layerN[n] = layerDef.FuncActivateDeleg(sum);
                }
            }
        }

        private void EvaluateOtherLevels()
        {
            var layerNInputs = this.LayersNN.GetNs(0);
            var ld =  this.LayersNN.LayersDef;
            for (int L = 1; L < ld.Length; L++)
            {
                var layerDef =  ld[L];
                var layerW = this.LayersNN.GetWeights(L);
                var layerN = this.LayersNN.GetNs(L);
                var layerBias = this.LayersNN.GetBias(L);
                


                int wIndex = 0;
                for (int n = 0; n < layerN.Length; n++)
                {
                    {
                        float sum = 0.0f;
                        for (int w = 0; w < layerDef.CountWPerN; w++)
                        {
                            sum += layerNInputs[w] * layerW[wIndex];
                            wIndex++;
                        }
                        sum += layerBias[n];
                        layerN[n] = layerDef.FuncActivateDeleg(sum);
                    }
                }

                layerNInputs = layerN;
            }
        }

        public struct sLayersNN
        {
            public LayerDef[] LayersDef;

            public float[][] Layers;

            public int [] BiasesIndex;
            public float [] Biases;
            public float[] Ns;

            public int[] WeightIndex;
            public float[] Weights;
            public float[] WeightsLastDelta;


            public static int IndexWeight(int index) => index *4;
            public static int IndexWeightDelta(int index) => index * 4+1;
            public static int IndexN(int index) => index * 4 + 2;

            public static int IndexBias(int index) => (index * 4)+3;


            public sLayersNN Clone()
            {
                var result = new sLayersNN();
                result.LayersDef = new LayerDef[LayersDef.Length];
                LayersDef.CopyTo(result.LayersDef, 0);

                result.Biases = new float[Biases.Length];
                this.Biases.CopyTo(result.Biases, 0);
                result.BiasesIndex = new int[BiasesIndex.Length];
                this.BiasesIndex.CopyTo(result.BiasesIndex, 0);

                result.Ns = new float[Ns.Length];
                this.Ns.CopyTo(result.Ns, 0);

                result.WeightIndex = new int[WeightIndex.Length];
                this.WeightIndex.CopyTo(result.WeightIndex, 0);
                result.Weights = new float[Weights.Length];
                this.Weights.CopyTo(result.Weights, 0);
                result.WeightsLastDelta = new float[WeightsLastDelta.Length];
                this.WeightsLastDelta.CopyTo(result.WeightsLastDelta, 0);




                result.Layers = new float[Layers.Length][];
                for (int i = 0; i < Layers.Length; i++)
                {
                    result.Layers[i] = new float[Layers[i].Length];
                    Layers[i].CopyTo(result.Layers[i], 0);
                }

                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetWeights(int layerIndex)
            {
                ref var  ld = ref LayersDef[layerIndex];
                return Weights.AsSpan(ld.WeightMomentIndexStart, ld.WeightMomentCount);

                int start = WeightIndex[layerIndex];
                int end = (layerIndex + 1 < WeightIndex.Length) ? WeightIndex[layerIndex + 1] : Weights.Length;

                return Weights.AsSpan(start, end - start);
                //return Layers[IndexWeight(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public  Span<float> GetWeightsDelta(int layerIndex)
            {
                ref var ld = ref LayersDef[layerIndex];
                return WeightsLastDelta.AsSpan(ld.WeightMomentIndexStart, ld.WeightMomentCount);

                int start = WeightIndex[layerIndex];
                int end = (layerIndex + 1 < WeightIndex.Length) ? WeightIndex[layerIndex + 1] : WeightsLastDelta.Length;

                return  WeightsLastDelta.AsSpan(start, end - start);

                //return Layers[IndexWeightDelta(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetNs(int layerIndex)
            {
                ref var ld = ref LayersDef[layerIndex];
                return Ns.AsSpan(ld.BiasNIndexStart, ld.BiasNCount);

                int start = BiasesIndex[layerIndex];
                int end = (layerIndex + 1 < BiasesIndex.Length) ? BiasesIndex[layerIndex + 1] : Ns.Length;

                return Ns.AsSpan(start, end - start);
                //return Layers[IndexN(layerIndex)];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Span<float> GetBias(int layerIndex)
            {
                ref var ld = ref LayersDef[layerIndex];
                return Biases.AsSpan(ld.BiasNIndexStart, ld.BiasNCount);

                int start = BiasesIndex[layerIndex];
                int end = (layerIndex+1 < BiasesIndex.Length)? BiasesIndex[layerIndex+1]  : Biases.Length;

                return Biases.AsSpan(start, end - start);
                //return Layers[IndexBias(layerIndex)];
            }
        }

        public readonly struct LayerDef
        {
            public readonly short CountN;
            public readonly short CountWPerN;

            public readonly int WeightMomentIndexStart;
            public readonly int WeightMomentCount;

            public readonly int BiasNIndexStart;
            public readonly int BiasNCount;


            public delegate float FuncActivate(float x);
            public delegate float FuncActivateReverse(float x);

            public readonly FuncActivate FuncActivateDeleg;
            public readonly FuncActivateReverse FuncActivateReverseDeleg;
            public LayerDef(short countN, short countWPerN, FuncActivate fa, FuncActivateReverse far,
                int weightMomentIndexStart, int weightMomentCount, int biasNIndexStart, int biasNCount)
            {
                CountN = countN;
                CountWPerN = countWPerN;
                FuncActivateDeleg = fa;
                FuncActivateReverseDeleg = far;
                this.WeightMomentCount = weightMomentCount;
                this.WeightMomentIndexStart = weightMomentIndexStart;
                this.BiasNIndexStart = biasNIndexStart;
                this.BiasNCount = biasNCount;
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Sigmoid(float x)
            {
              
                if (x < -45.0f) return 0.0f;
                else if (x > 45.0f) return 1.0f;


                return 1.0f / (1.0f + (float)Math.Pow(Math.E, -x));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Sigmoid(float x)
            {
               
                return x*(1-x);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FA_Swift(float x)
            {
                return x / (1 + (float)Math.Pow(Math.E, -x));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static float FAR_Swift(float x)
            {
                var ex = (float)Math.Pow(Math.E, -x);
                var tmp = (ex*(1-x)+1) / ((1 + ex)*(1+ex));
                return tmp;
            }
        }
    }
}

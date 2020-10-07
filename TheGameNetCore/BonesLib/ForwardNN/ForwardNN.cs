using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BonesLib.ForwardNN
{
    public class ForwardNN
    {
        private const int CONST_MaxLinkPerNeuron = 10;
        private const float CONST_Bias = 1.0f;

        public short [] Topology { get; private set; }
        public float[] Outputs { get; private set; }
        public float [] Inputs { get; private set; }

        private NNLayer[] _layers;

        public NNLayer[] Layers => _layers;

        public ForwardNN(short countInputs, short countOutputs)
        {
            Outputs = new float[countOutputs];
            Inputs = new float[countInputs];

        }

        public int GetCountWeights()
        {
            int result = 0;
            foreach(var item in Layers)
            {
                result += item.NeuronLinks.Length;
            }

            return result;
        }

        public void SetTopology(short  [] topology)
        {
            Topology = topology.ToArray();
            _layers = new NNLayer[topology.Length +1];
            short linksPerNeuron = (short)Inputs.Length;
            short countLastNeurons = (short)Inputs.Length;

            if (linksPerNeuron > CONST_MaxLinkPerNeuron) 
                linksPerNeuron = CONST_MaxLinkPerNeuron;

            for (int i = 0;i < topology.Length; i++)
            {
                ref var layer = ref _layers[i];

                layer = new NNLayer(topology[i], linksPerNeuron);
                layer.InitLinks(countLastNeurons);

                countLastNeurons = topology[i];
                linksPerNeuron = topology[i];

                if (linksPerNeuron > CONST_MaxLinkPerNeuron)
                    linksPerNeuron = CONST_MaxLinkPerNeuron;
            }


            ref var layerEnd = ref _layers[_layers.Length - 1];
            layerEnd = new NNLayer(Outputs.Length, linksPerNeuron);
            layerEnd.InitLinks(countLastNeurons);
        }

        public ForwardNN Clone()
        {
            ForwardNN newObj = new ForwardNN((short)Inputs.Length, (short)Outputs.Length);
            newObj.SetTopology(Topology);

            CopyTo(newObj);
            return newObj;
        }

        public  void CopyTo( ForwardNN dest)
        {
            if (!this.IsEqualTopology(dest)) { throw new Exception("Bad topology"); };

            for(int level = 0; level < Layers.Length; level++)
            {
                ref var levelDest = ref dest.Layers[level];
                var levelSource = Layers[level];

                for(int i =0; i < levelDest.Neurons.Length; i++)
                {
                    levelDest.Neurons[i] = levelSource.Neurons[i];
                }

                for (int i = 0; i < levelDest.NeuronLinks.Length; i++)
                {
                    levelDest.NeuronLinks[i] = levelSource.NeuronLinks[i];
                }

                levelDest.CountLinksPerNeuron = levelSource.CountLinksPerNeuron;
            }

        }

#region evaluate

        public void Evaluate()
        {
            EvaluateFirstLevel();
            EvaluateOtherLevels();

            var neurons = this._layers[this._layers.Length-1].Neurons;
            for(int i = 0; i < this.Outputs.Length; i++)
            {
                this.Outputs[i] = neurons[i].Output;
            }

        }

        private void EvaluateFirstLevel()
        {
            var layer = this._layers[0];
            var links = layer.NeuronLinks;

            for (int i = 0, n=0; i < links.Length; i += layer.CountLinksPerNeuron,n++)
            {
                float sum = CONST_Bias;
                for(int k = 0; k < layer.CountLinksPerNeuron; k++)
                {
                    ref var link = ref links[i + k];
                    sum+=  this.Inputs[link.BackNeuronIndex] * link.Weight;
                }

                ref var neuron = ref layer.Neurons[n];
                neuron.Output = neuron.ActivationFunction_ReLu(sum);
            }
        }

        private void EvaluateOtherLevels()
        {
            for (int level = 1; level < this._layers.Length; level++)
            {
                ref var layer = ref this._layers[level];

                var links = layer.NeuronLinks;
                var lastLevelNeurons = this._layers[level-1].Neurons;

                for (int i = 0, n = 0; i < links.Length; i += layer.CountLinksPerNeuron, n++)
                {
                    float sum = CONST_Bias;
                    for (int k = 0; k < layer.CountLinksPerNeuron; k++)
                    {
                        ref var link = ref links[i + k];
                        sum += lastLevelNeurons[link.BackNeuronIndex].Output * link.Weight;
                    }

                    ref var neuron = ref layer.Neurons[n];
                    neuron.Output = neuron.ActivationFunction_ReLu(sum);
                }
            }
        }

        #endregion

        public bool IsEqualTopology(ForwardNN fnn)
        {
            if(fnn.Inputs.Length != Inputs.Length ||
               fnn.Outputs.Length != Outputs.Length ||
               fnn._layers.Length != _layers.Length)
            {
                return false;
            }


            for(int i =0; i < _layers.Length; i++)
            {
                if(fnn._layers[i].Neurons.Length != _layers[i].Neurons.Length ||
                   fnn._layers[i].NeuronLinks.Length != _layers[i].NeuronLinks.Length)
                {
                    return false;
                }
            }

            return true;
        }
    }


    public struct NNLayer
    {
        public Neuron[] Neurons ;
        public NeuronLink[] NeuronLinks;
        public short CountLinksPerNeuron;

        public NNLayer(int countNeurons, short countLinksPerNeuron)
        {
            Neurons = new Neuron[countNeurons];
            NeuronLinks = new NeuronLink[countLinksPerNeuron * countNeurons];
            
            CountLinksPerNeuron = countLinksPerNeuron;
        }

        public void InitLinks(int totalBackNeurons)
        {
            ushort idCounter = 0;
            ushort idStartPos = 0;
            for (int i = 0; i < NeuronLinks.Length; i++)
            {
                if (idCounter == CountLinksPerNeuron+idStartPos)
                {
                    if(idStartPos + CountLinksPerNeuron >= totalBackNeurons)
                    {
                        idStartPos = 0;
                    }
                    else
                    {
                        idStartPos++;
                    }

                    idCounter = idStartPos;
                }

                NeuronLinks[i].BackNeuronIndex = idCounter;

                idCounter++;
            }
        }

        public void InitRandWeights()
        {
            for(int i = 0;i < NeuronLinks.Length; i++)
            {
                NeuronLinks[i].Weight = (float)RandomGen.Default.GetRandomNumberDouble();
            }
        }
    }

    public struct Neuron
    {
        public float Output;

        public float ActivationFunction_tanh2(float sum)
        {
            double eSum = Math.Exp(sum);
            double eSumNeg = Math.Exp(-sum);

            return (float)((eSum - eSumNeg) / (eSum + eSumNeg));
        }

        
        public float ActivationFunction_tanh(float sum)
        {
            float eSum = FastExp(sum);
            float eSumNeg = FastExp(-sum);

            return ((eSum - eSumNeg) / (eSum + eSumNeg));
        }

        public float ActivationFunction_ReLu(float sum)
        {
            return (sum < 0) ? 0.01f*sum : sum;
            
        }

        public float ActivationFunction_Swish(float sum)
        {
            return sum/(1+ FastExp(-0.5f*sum));
            //return (float)(sum/(1+ Math.Exp(-sum)));
        }

        public float FastExp(float val)
        {
            float x = 1.0f + val / 1024;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x;
            return x;
        }

        public override string ToString()
        {
            return $"output:{Output}";
        }

    }

    public struct NeuronLink
    {
        public float Weight;
        public ushort BackNeuronIndex;
        

        public override string ToString()
        {
            return $"BNIndex:{BackNeuronIndex} , weight:{Weight}";
        }
    }
}

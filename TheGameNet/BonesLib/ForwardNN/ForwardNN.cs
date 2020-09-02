using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BonesLib.ForwardNN
{
    public class ForwardNN
    {
        priv
        public float[] Outputs { get; private set; }
        public float [] Inputs { get; private set; }

        private NNLayer[] _layers;

        public NNLayer[] Layers => _layers;

        public ForwardNN(short countInputs, short countOutputs)
        {
            Outputs = new float[countOutputs];
            Inputs = new float[countInputs];

        }

        public void SetTopology(short  [] topology)
        {
            _layers = new NNLayer[topology.Length +1];
            short linksPerNeuron = (short)Inputs.Length;

            for(int i = 0;i < topology.Length; i++)
            {
                ref var layer = ref _layers[i];

                layer = new NNLayer(topology[i], linksPerNeuron);
                layer.InitLinks();

                linksPerNeuron = topology[i];
            }


            ref var layerEnd = ref _layers[_layers.Length - 1];
            layerEnd = new NNLayer(Outputs.Length, linksPerNeuron);
            layerEnd.InitLinks();
        }

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
                float sum = 0.0f;
                for(int k = 0; k < layer.CountLinksPerNeuron; k++)
                {
                    var link = links[i + k];
                    sum+=  this.Inputs[link.BackNeuronIndex] * link.Weight;
                }

                layer.Neurons[n].Output = layer.Neurons[n].ActivationFunction_tanh(sum);
            }
        }

        private void EvaluateOtherLevels()
        {
            for (int level = 1; level < this._layers.Length; level++)
            {
                var layer = this._layers[level];

                var links = layer.NeuronLinks;
                var lastLevelNeurons = this._layers[level-1].Neurons;

                for (int i = 0, n = 0; i < links.Length; i += layer.CountLinksPerNeuron, n++)
                {
                    float sum = 0.0f;
                    for (int k = 0; k < layer.CountLinksPerNeuron; k++)
                    {
                        var link = links[i + k];
                        sum += lastLevelNeurons[link.BackNeuronIndex].Output * link.Weight;
                    }

                    layer.Neurons[n].Output = layer.Neurons[n].ActivationFunction_tanh(sum);
                }
            }
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

        public void InitLinks()
        {
            ushort idCounter = 0;
            for (int i = 0; i < NeuronLinks.Length; i++)
            {
                if (idCounter == CountLinksPerNeuron)
                    idCounter = 0;

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

        public float ActivationFunction_tanh(float sum)
        {
            return (float)((Math.Exp(sum) - Math.Exp(-sum)) / (Math.Exp(sum) + Math.Exp(-sum)));
        }

        public override string ToString()
        {
            return $"output:{Output}";
        }

    }

    public struct NeuronLink
    {
        public ushort BackNeuronIndex;
        public float Weight;

        public override string ToString()
        {
            return $"BNIndex:{BackNeuronIndex} , weight:{Weight}";
        }
    }
}

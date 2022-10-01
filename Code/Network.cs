using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;
public class Network
{
    public Neuron[][] layers;

    public Network(params Neuron[][] layers)
    {
        for(int i = 1; i < layers.Length; i++)
            foreach (Neuron neuron in layers[i])
                neuron.incomingWeights = layers[i - 1].Select(linqNeuron => new Weight(linqNeuron, Random.Range(-1f, 1f))).ToArray();

        this.layers = layers;
    }

    public void Cross(Network A, Network B, float mutationRate)
    {
        if (A.layers.Length != B.layers.Length || A.layers.Select(layer => layer.Length).ToArray().Length != B.layers.Select(layer => layer.Length).ToArray().Length || mutationRate > 1 || mutationRate < 0) throw new ArgumentException();
        for(int i = 1; i < A.layers.Length; i++)
            for(int k = 0; k < A.layers[i].Length; k++)
                for (int j = 0; j < A.layers[i][k].incomingWeights.Length; j++)
                {
                    float weightA = A.layers[i][k].incomingWeights[j].weight;
                    float weightB = B.layers[i][k].incomingWeights[j].weight;
                    if(Random.Range(0f, 1f) < mutationRate) layers[i][k].incomingWeights[j].weight = (weightA + weightB) / 2;
                }
    }

    public void SetInputs(float[] values)
    {
        for (int i = 0; i < values.Length; i++)
            layers[0][i].value = values[i];

        for(int i = 1; i < layers.Length; i++) 
            foreach (Neuron neuron in layers[i])
                neuron.value = neuron.ActivationFunction(neuron.incomingWeights.Select(weight => weight.connection.value * weight.weight).ToArray().Sum());
    }
}

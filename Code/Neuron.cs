using System;
public class Neuron
{
    public float value = 0f;
    public Weight[] incomingWeights;
    public NeuronType type;

    public Neuron(NeuronType type)
    {
        this.type = type;
    }

    public float ActivationFunction(float value)
    {
        switch (type)
        {
            case NeuronType.SIGMOID:
                return (float) (1 / (1 + Math.Exp(-value)));
            case NeuronType.BIPOLAR:
                return (float) ((1 - Math.Exp(-value)) / (1 + Math.Exp(-value)));
            case NeuronType.IDENTITY:
                return value;
        }

        return 0f;
    }
}

public enum NeuronType
{
    SIGMOID,
    BIPOLAR,
    IDENTITY
}

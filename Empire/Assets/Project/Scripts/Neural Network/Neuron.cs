#region

using System;
using static NeuralLayer;
using Random = UnityEngine.Random;

#endregion

[Serializable]
public class Neuron
{
    private const float MIN_WEIGHT_VALUE = 0.01f;
    private const float MAX_WEIGHT_VALUE = 2f;

    /// <summary>
    ///     ���� ����� ��������� ����������� ���� � ���� ��������
    /// </summary>
    public double[] weight;

    public double value;
    private NeuralLayer layer;

    /// <summary>
    /// </summary>
    /// <param name="weightMatrix">���� ����� ���������� ����� � ���� ����� [���������� ����, ���� ����]</param>
    /// <param name="myID"></param>
    public Neuron(double[,] weightMatrix, int myID, NeuralLayer layer)
    {
        this.layer = layer;
        weight = new double[weightMatrix.GetLength(0)];
        for (int id = 0; id < weightMatrix.GetLength(0); id++)
            weight[id] = weightMatrix[id, myID];
    }

    public Neuron(int countWeight, NeuralLayer layer)
    {
        this.layer = layer;
        SetRandomWeight(countWeight);
    }

    public Neuron(double[] weight, NeuralLayer layer)
    {
        this.layer = layer;
        this.weight = weight;
    }

    public Neuron(double value, NeuralLayer layer)
    {
        this.layer = layer;
        this.value = value;
    }

    /// <summary>
    ///     ������� ���������
    /// </summary>
    /// <param name="x">��������</param>
    /// <returns>���������</returns>
    public void ActivationFunction(double x)
    {
        value = layer.activationFunction switch
        {
            ActivationFunctionType.Sigmoid => 1 / (1 + Math.Exp(-x)),
            ActivationFunctionType.ReLu => Math.Max(0, x),
            ActivationFunctionType.Th => (Math.Exp(x) - Math.Exp(-x)) / (Math.Exp(x) + Math.Exp(-x)),
            _ => 0
        };
    }

    /// <summary>
    ///     ����������� �������
    /// </summary>
    /// <param name="x">��������</param>
    /// <returns>���������</returns>
    public double DerivativeFunction()
    {
        return layer.activationFunction switch
        {
            ActivationFunctionType.Sigmoid => value * (1 - value),
            ActivationFunctionType.ReLu => value > 0 ? 1 : 0,
            ActivationFunctionType.Th => 1 - value * value,
            _ => 0
        };
    }

    public void SetRandomWeight(int countWeight)
    {
        weight = new double[countWeight];
        for (int id = 0; id < weight.Length; id++)
            weight[id] = Random.Range(MIN_WEIGHT_VALUE, MAX_WEIGHT_VALUE) * (Random.Range(0, 1) == 1 ? 1 : -1);
    }
}
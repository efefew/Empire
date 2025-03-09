#region

using UnityEngine;

#endregion

public class NeuralLayer : MonoBehaviour
{
    /// <summary>
    ///     ��� ������� ���������
    /// </summary>
    public enum ActivationFunctionType
    {
        /// <summary>
        ///     ������������� (�������� ��� ������� ���������)
        /// </summary>
        Sigmoid,

        /// <summary>
        ///     ReLu
        /// </summary>
        ReLu,

        /// <summary>
        ///     ��������������� �������
        /// </summary>
        Th
    }

    #region Fields

    [Tooltip("������� ���������")] public ActivationFunctionType activationFunction;

    private NeuralLayer previosLayer;

    /// <summary>
    ///     ��������
    /// </summary>
    public double bias;

    public Neuron[] neurons;

    #endregion Fields

    #region Methods

    /// <summary>
    ///     ������� ����
    /// </summary>
    /// <param name="previosLayer">���������� ����</param>
    public void CreateNeuralLayer(int countNeurons, NeuralLayer previosLayer = null)
    {
        this.previosLayer = previosLayer;
        neurons = new Neuron[countNeurons];
        if (previosLayer == null)
            return;

        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(previosLayer.neurons.Length, this);
    }

    /// <summary>
    ///     ������� ����
    /// </summary>
    /// <param name="previosLayer">���������� ����</param>
    public void CreateNeuralLayer(int countNeurons, double[][] wight, NeuralLayer previosLayer = null)
    {
        this.previosLayer = previosLayer;
        neurons = new Neuron[countNeurons];
        if (previosLayer == null)
            return;

        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(wight[id], this);
    }

    public void LoadNeuralLayer(double[,] weight, NeuralLayer previosLayer)
    {
        neurons = new Neuron[weight.GetLength(1)];
        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(weight, id, this);

        this.previosLayer = previosLayer;
    }

    public void LoadNeurons(double[] neuronsValue)
    {
        neurons = new Neuron[neuronsValue.Length];
        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(neuronsValue[id], this);
    }

    public void ActivationNeurons()
    {
        double summ;
        for (int id = 0; id < neurons.Length; id++)
        {
            summ = 0;
            for (int idPrevios = 0; idPrevios < previosLayer.neurons.Length; idPrevios++)
                summ += previosLayer.neurons[idPrevios].value * neurons[id].weight[idPrevios];
            neurons[id].ActivationFunction(summ + bias);
            if (double.IsNaN(neurons[id].value) || double.IsInfinity(neurons[id].value))
                Debug.Log(
                    "<color=#F13939> \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd = NaN \ufffd\ufffd\ufffd Infinity </color>");
        }
    }

    public double[] GetNeuronsValue()
    {
        double[] array = new double[neurons.Length];
        for (int id = 0; id < neurons.Length; id++) array[id] = neurons[id].value;

        return array;
    }

    public double[,] GetWeight2D()
    {
        double[,] weight2D = new double[neurons.Length, neurons[0].weight.Length];
        for (int x = 0; x < weight2D.GetLength(0); x++)
        for (int y = 0; y < weight2D.GetLength(1); y++)
            weight2D[x, y] = neurons[x].weight[y];

        return weight2D;
    }

    #endregion Methods
}
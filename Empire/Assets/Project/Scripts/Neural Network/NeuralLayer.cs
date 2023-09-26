using UnityEngine;

public class NeuralLayer : MonoBehaviour
{

    #region Delegates

    public delegate double ActivationFunctionHandler(double x);

    #endregion Delegates

    #region Fields

    private NeuralLayer previosLayer;
    /// <summary>
    /// смещение
    /// </summary>
    public double bias;

    public Neuron[] neurons;
    #endregion Fields

    #region Methods

    /// <summary>
    /// Создать весы
    /// </summary>
    /// <param name="previosLayer">предыдущий слой</param>
    public void CreateNeuralLayer(int countNeurons, NeuralLayer previosLayer = null)
    {
        this.previosLayer = previosLayer;
        neurons = new Neuron[countNeurons];
        if (previosLayer == null)
            return;

        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(previosLayer.neurons.Length);
    }
    /// <summary>
    /// Создать весы
    /// </summary>
    /// <param name="previosLayer">предыдущий слой</param>
    public void CreateNeuralLayer(int countNeurons, double[][] wight, NeuralLayer previosLayer = null)
    {
        this.previosLayer = previosLayer;
        neurons = new Neuron[countNeurons];
        if (previosLayer == null)
            return;

        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(wight[id]);
    }
    public void LoadNeuralLayer(double[,] weight, NeuralLayer previosLayer)
    {
        neurons = new Neuron[weight.GetLength(1)];
        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(weight, id);

        this.previosLayer = previosLayer;
    }

    public void LoadNeurons(double[] neuronsValue)
    {
        neurons = new Neuron[neuronsValue.Length];
        for (int id = 0; id < neurons.Length; id++)
            neurons[id] = new Neuron(neuronsValue[id]);
    }

    public void ActivationNeurons(ActivationFunctionHandler function)
    {
        double summ;
        for (int id = 0; id < neurons.Length; id++)
        {
            summ = 0;
            for (int idPrevios = 0; idPrevios < neurons.Length; idPrevios++)
                summ += previosLayer.neurons[idPrevios].value * neurons[id].weight[idPrevios];
            neurons[id].value = function(summ + bias);
            if (double.IsNaN(neurons[id].value) || double.IsInfinity(neurons[id].value))
            {
                Debug.Log($"<color=#F13939> нейрон = NaN или Infinity </color>");
            }
        }
    }
    public double[] GetNeuronsValue()
    {
        double[] array = new double[neurons.Length];
        for (int id = 0; id < neurons.Length; id++)
        {
            array[id] = neurons[id].value;
        }

        return array;
    }
    public double[,] GetWeight2D()
    {
        double[,] weight2D = new double[neurons.Length, neurons[0].weight.Length];
        for (int x = 0; x < weight2D.GetLength(0); x++)
        {
            for (int y = 0; y < weight2D.GetLength(1); y++)
                weight2D[x, y] = neurons[x].weight[y];
        }

        return weight2D;
    }
    #endregion Methods

}
using System;
[Serializable]
public class Neuron
{
    private const float MIN_WEIGHT_VALUE = 0.01f;
    private const float MAX_WEIGHT_VALUE = 2f;

    /// <summary>
    /// Весы между нейронами предыдущего слоя и этим нейроном
    /// </summary>
    public double[] weight;
    public double value;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="weightMatrix">Весы между предыдущим слоем и этим слоем [предыдущий слой, этот слой]</param>
    /// <param name="myID"></param>
    public Neuron(double[,] weightMatrix, int myID)
    {
        weight = new double[weightMatrix.GetLength(0)];
        for (int id = 0; id < weightMatrix.GetLength(0); id++)
            weight[id] = weightMatrix[id, myID];
    }

    public Neuron(int countWeight) => SetRandomWeight(countWeight);
    public Neuron(double[] weight) => this.weight = weight;
    public Neuron(double value) => this.value = value;
    public void SetRandomWeight(int countWeight)
    {
        weight = new double[countWeight];
        for (int id = 0; id < weight.Length; id++)
            weight[id] = UnityEngine.Random.Range(MIN_WEIGHT_VALUE, MAX_WEIGHT_VALUE);
    }
}

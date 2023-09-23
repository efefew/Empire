using UnityEngine;

public class NeuralLayer
{
    public double[] neurons;
    /// <summary>
    /// ���� ����� ���������� ����� � ���� ����� [���������� ����, ���� ����]
    /// </summary>
    public double[,] weight;
    private NeuralLayer previosLayer;
    /// <summary>
    /// ������� ����
    /// </summary>
    /// <param name="previosLayer">���������� ����</param>
    public void CreateNeuralLayer(int countNeurons, NeuralLayer previosLayer = null)
    {
        this.previosLayer = previosLayer;
        neurons = new double[countNeurons];
        if (previosLayer == null)
            return;
        weight = new double[previosLayer.neurons.Length, countNeurons];
        SetRandomWeight();
    }
    public void LoadNeurons(double[] neurons) => this.neurons = (double[])neurons.Clone();
    public void LoadWeight(double[,] weight) => this.weight = (double[,])weight.Clone();
    public void ActivationNeurons()
    {
        for (int id = 0; id < neurons.Length; id++)
        {
            for (int idPrevios = 0; idPrevios < neurons.Length; idPrevios++)
            {
                neurons[id] = ActivationFunction(previosLayer.neurons[idPrevios] * weight[idPrevios, id]);
            }
        }
    }
    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="x">��������</param>
    /// <returns>���������</returns>
    private double ActivationFunction(double x) => 1 / (1 + System.Math.Pow(System.Math.E, -x));
    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="x">��������</param>
    /// <returns>���������</returns>
    private double DerivativeFunction(double x) => ActivationFunction(x) * (1 - ActivationFunction(x));
    public void SetRandomWeight()
    {
        for (int x = 0; x < weight.GetLength(0); x++)
        {
            for (int y = 0; y < weight.GetLength(1); y++)
                weight[x, y] = Random.value + 0.001;
        }
    }
}

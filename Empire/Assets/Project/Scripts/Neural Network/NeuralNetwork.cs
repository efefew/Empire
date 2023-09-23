using System.Linq;

using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    public NeuralLayer inputLayer = new(), outputLayer = new();
    private NeuralLayer[] hiddenLayer;
    private void Awake() => CreateNeuralNetwork(30 * 30, new int[1] { 500 }, 10);
    public void CreateNeuralNetwork(int countInputNeurons, int[] countHiddenNeurons, int countOutputNeurons)
    {
        if (countHiddenNeurons.Length == 0)
            return;
        hiddenLayer = new NeuralLayer[countHiddenNeurons.Length];
        inputLayer.CreateNeuralLayer(countInputNeurons);

        hiddenLayer[0] = new();
        hiddenLayer[0].CreateNeuralLayer(countHiddenNeurons[0], inputLayer);
        if (hiddenLayer.Length > 1)
        {
            for (int id = 1; id < hiddenLayer.Length; id++)
            {
                hiddenLayer[id] = new();
                hiddenLayer[id].CreateNeuralLayer(countHiddenNeurons[id], hiddenLayer[id - 1]);
            }
        }

        outputLayer.CreateNeuralLayer(countOutputNeurons, hiddenLayer.Last());
    }
    public void Run(double[] inputNeurons)
    {
        inputLayer.LoadNeurons(inputNeurons);
        for (int id = 0; id < hiddenLayer.Length; id++)
            hiddenLayer[id].ActivationNeurons();

        outputLayer.ActivationNeurons();
    }
}

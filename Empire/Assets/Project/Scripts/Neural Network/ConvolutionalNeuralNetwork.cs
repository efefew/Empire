#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

[RequireComponent(typeof(NeuralNetwork))]
[DisallowMultipleComponent]
public class ConvolutionalNeuralNetwork : MonoBehaviour
{
    [SerializeField] private ConvolutionalLayer convolutionalLayerPrefab;

    [SerializeField] private PoolingLayer poolingLayerPrefab;

    public List<ConvolutionalNetworkLayer> layers;
    private NeuralNetwork neuralNetwork;

    private void Awake()
    {
        neuralNetwork = GetComponent<NeuralNetwork>();
    }

    public void CreateNeuralNetwork(int widthInput, int heightInput, int[] countHiddenNeurons, int countOutputNeurons)
    {
        for (int id = 0; id < 2; id++)
        {
            layers.Add(Instantiate(convolutionalLayerPrefab, transform));
            layers.Last().Create(id == 0 ? widthInput : layers[^2].outWidth,
                id == 0 ? heightInput : layers[^2].outHeight, 3, 3);

            layers.Add(Instantiate(poolingLayerPrefab, transform));
            layers.Last().Create(layers[^2].outWidth, layers[^2].outHeight, 2, 2);
        }

        neuralNetwork.CreateNeuralNetwork(layers.Last().outWidth * layers.Last().outHeight, countHiddenNeurons,
            countOutputNeurons);
    }

    public void Learn(List<double[,]> inputMatrix, List<double[]> outputNeeded)
    {
        List<double[]> input = new();
        for (int idMatrix = 0; idMatrix < inputMatrix.Count; idMatrix++)
        {
            for (int idLayer = 0; idLayer < layers.Count; idLayer++)
                _ = layers[idLayer].Run(idLayer == 0 ? inputMatrix[idMatrix] : layers[idLayer - 1].outMatrix);

            input.Add(layers.Last().outMatrix.ToArray());
        }

        Debug.Log(input[0].Length);
        neuralNetwork.Learn(input, outputNeeded);
    }
}
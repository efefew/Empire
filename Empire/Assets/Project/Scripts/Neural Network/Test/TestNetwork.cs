using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(NeuralNetwork))]
public class TestNetwork : MonoBehaviour
{
    private NeuralNetwork network;
    private List<double[]> inputs = new();
    private List<double[]> outputsNeeded = new();
    private void Start()
    {
        network = GetComponent<NeuralNetwork>();
        Learn();
    }
    private void AliLearn()
    {
        network.CreateNeuralNetwork(2, new int[1] { 2 }, 2);
        network.hiddenLayers[0].bias = .35;

        network.hiddenLayers[0].neurons[0].weight[0] = .15;
        network.hiddenLayers[0].neurons[0].weight[1] = .20;

        network.hiddenLayers[0].neurons[1].weight[0] = .25;
        network.hiddenLayers[0].neurons[1].weight[1] = .30;

        network.outputLayer.bias = .60;

        network.outputLayer.neurons[0].weight[0] = .40;
        network.outputLayer.neurons[0].weight[1] = .45;

        network.outputLayer.neurons[1].weight[0] = .50;
        network.outputLayer.neurons[1].weight[1] = .55;

        inputs.Clear();
        inputs.Add(new double[2] { 0.05, 0.1 });
        outputsNeeded.Add(new double[2] { 0.01, 0.99 });
        network.Learn(inputs, outputsNeeded);
    }
    private void Learn()
    {
        network.CreateNeuralNetwork(3 * 3, new int[2] { 4, 3 }, 2);
        inputs.Clear();
        CreateInputs();
        outputsNeeded.Add(new double[2] { 1, 0 });
        outputsNeeded.Add(new double[2] { 0, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });

        outputsNeeded.Add(new double[2] { 1, 0 });
        outputsNeeded.Add(new double[2] { 0, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });

        outputsNeeded.Add(new double[2] { 1, 0 });
        outputsNeeded.Add(new double[2] { 1, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });

        outputsNeeded.Add(new double[2] { 0, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });
        outputsNeeded.Add(new double[2] { 1, 1 });
        outputsNeeded.Add(new double[2] { 0, 0 });

        network.Learn(inputs, outputsNeeded);
    }

    private void CreateInputs()
    {
        inputs.Add(new double[9]{
         1,1,1,
         0,0,0,
         0,0,0
        });
        inputs.Add(new double[9]{
         0,1,0,
         0,1,0,
         0,1,0
        });
        inputs.Add(new double[9]{
         1,1,1,
         0,0,1,
         0,0,1
        });
        inputs.Add(new double[9]{
         1,0,0,
         1,0,0,
         1,1,1
        });
        inputs.Add(new double[9]{
         0,0,0,
         1,1,1,
         0,0,0
        });
        inputs.Add(new double[9]{
         0,0,1,
         0,0,1,
         0,0,1
        });
        inputs.Add(new double[9]{
         1,0,0,
         1,1,1,
         1,0,0
        });
        inputs.Add(new double[9]{
         0,1,0,
         0,1,0,
         1,1,1
        });
        inputs.Add(new double[9]{
         0,0,0,
         0,0,0,
         1,1,1
        });
        inputs.Add(new double[9]{
         1,1,1,
         1,0,0,
         1,0,0
        });
        inputs.Add(new double[9]{
         0,1,0,
         1,1,1,
         0,1,0
        });
        inputs.Add(new double[9]{
         0,0,1,
         0,0,1,
         1,1,1
        });
        inputs.Add(new double[9]{
         1,0,0,
         1,0,0,
         1,0,0
        });
        inputs.Add(new double[9]{
         1,1,1,
         0,1,0,
         0,1,0
        });
        inputs.Add(new double[9]{
         0,0,1,
         1,1,1,
         0,0,1
        });
        inputs.Add(new double[9]{
         0,0,0,
         0,0,0,
         0,0,0
        });
    }
    private void OnDisable() => network.SaveNeuralNetwork();
}

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;

#endregion

[DisallowMultipleComponent]
/// <summary>
/// �������� ����
/// https://habr.com/ru/articles/556076/
/// https://programforyou.ru/poleznoe/convolutional-network-from-scratch-part-zero-introduction
/// https://ru.stackoverflow.com/questions/834750/
/// https://habr.com/ru/articles/456738/
/// </summary>
public class NeuralNetwork : MonoBehaviour
{
    public Action<double> OnEndOfTheLearningEra;

    #region Enums

    /// <summary>
    ///     ��� ������� ������
    /// </summary>
    public enum LossFunctionType
    {
        /// <summary>
        ///     ������� ���������� ������
        /// </summary>
        MAE,

        /// <summary>
        ///     ������������������ ������� ������
        /// </summary>
        MSE,

        /// <summary>
        ///     ������������������ ����������
        /// </summary>
        RMSE
    }

    /// <summary>
    ///     ��� ������ ����������� (�������) ������� ��������/������ ������������ ����� �������� ��� ��������� �����
    ///     ������������������.
    ///     https://habr.com/ru/articles/722628/
    ///     https://skine.ru/articles/559485/
    /// </summary>
    public enum EvaluationOfResultsType
    {
        /// <summary>
        ///     ������� "������������"
        /// </summary>
        Accuracy,

        /// <summary>
        ///     ������� "�������"
        /// </summary>
        Recall,

        /// <summary>
        ///     ������� " ���� �����-������������� (False Positive Rate)"
        /// </summary>
        FPR,

        /// <summary>
        ///     ������� "��������"
        /// </summary>
        Precision,

        /// <summary>
        ///     ������� "F-����"
        /// </summary>
        F_score
    }

    #endregion Enums

    #region Fields

    /// <summary>
    ///     ��������, ����������� ����������������, �� �������� ������� �������� �������� ���������
    /// </summary>
    [SerializeField] [Range(0f, 1f)] private double learningRate = 0.3;

    [SerializeField] private NeuralLayer prefabNeuralLayer;

    public NeuralLayer inputLayer;
    public NeuralLayer[] hiddenLayers;
    public NeuralLayer outputLayer;

    [Tooltip("������ ����������� (�� ������������)")]
    public EvaluationOfResultsType evaluationOfResults;

    [Tooltip("������� ������")] public LossFunctionType lossFunction;

    public int countEra, exampleCount;
    private bool stop, step;
    public double error, E = 10E-3;

    #endregion Fields

    #region Methods

    /// <summary>
    ///     ������������� ����� ����
    /// </summary>
    /// <param name="neuronsNeeded">��������� �������� �������� ����</param>
    /// <param name="previousLayer">���������� ����</param>
    /// <param name="layer">����</param>
    /// <param name="errorNeurons"></param>
    /// <returns></returns>
    private double[] AdjustingLayerWeights(double[] neuronsNeeded, NeuralLayer previousLayer, NeuralLayer layer,
        double[] errorNeurons = null)
    {
        double[] errorPrevousNeurons = new double[previousLayer.neurons.Length];
        for (int id = 0; id < errorPrevousNeurons.Length; id++)
            errorPrevousNeurons[id] = 0;

        double delta, error;

        for (int neuronID = 0; neuronID < layer.neurons.Length; neuronID++)
        {
            error = errorNeurons == null
                ? layer.neurons[neuronID].value - neuronsNeeded[neuronID]
                : errorNeurons[neuronID];
            delta = errorNeurons == null ? error : error * layer.neurons[neuronID].DerivativeFunction();
            for (int prevousNeuronID = 0; prevousNeuronID < previousLayer.neurons.Length; prevousNeuronID++)
            {
                errorPrevousNeurons[prevousNeuronID] += delta * layer.neurons[neuronID].weight[prevousNeuronID];
                layer.neurons[neuronID].weight[prevousNeuronID] -=
                    previousLayer.neurons[prevousNeuronID].value * delta * learningRate;
            }
        }

        return errorPrevousNeurons;
    }

    private void WriteDat(ref BinaryWriter writer)
    {
        writer.Write(inputLayer.neurons.Length);
        writer.Write(hiddenLayers.Length);
        foreach (NeuralLayer layer in hiddenLayers)
        {
            writer.WriteArray2D(layer.GetWeight2D());
            writer.Write((int)layer.activationFunction);
        }

        writer.WriteArray2D(outputLayer.GetWeight2D());
        writer.Write((int)outputLayer.activationFunction);
    }

    private void ReadDat(ref BinaryReader reader)
    {
        inputLayer = Instantiate(prefabNeuralLayer, transform);
        inputLayer.CreateNeuralLayer(reader.ReadInt32());
        hiddenLayers = new NeuralLayer[reader.ReadInt32()];

        for (int id = 0; id < hiddenLayers.Length; id++)
        {
            hiddenLayers[id] = Instantiate(prefabNeuralLayer, transform);
            hiddenLayers[id].LoadNeuralLayer(reader.ReadArray2D(), id == 0 ? inputLayer : hiddenLayers[id - 1]);
            hiddenLayers[id].activationFunction = (NeuralLayer.ActivationFunctionType)reader.ReadInt32();
        }

        outputLayer = Instantiate(prefabNeuralLayer, transform);
        outputLayer.LoadNeuralLayer(reader.ReadArray2D(), hiddenLayers.Last());
        outputLayer.activationFunction = (NeuralLayer.ActivationFunctionType)reader.ReadInt32();
    }

    /// <summary>
    ///     ������ �����������
    /// </summary>
    /// <param name="TP">
    ///     ���������� ������� ������������� �������, �� ���� ����� ������ ��� ����� ��������������� � ��
    ///     ����������� ���������� �������� ���������
    /// </param>
    /// <param name="TN">
    ///     ���������� ������� �������������  �������, �� ���� ����� ������ ��� ����� ��������������� � ��
    ///     ����������� ���������� ���������
    /// </param>
    /// <param name="FP">�����-������������� � ����� ������� ������������� �������� ���������� ��������� ��� ��������</param>
    /// <param name="FN">�����-������������� � ����� ������� ������������� �������� �������� ��������� ��� ����������</param>
    /// <returns>�������� ������ �����������</returns>
    private double EvaluationOfResults(double TP, double TN, double FP, double FN)
    {
        return evaluationOfResults switch
        {
            EvaluationOfResultsType.Accuracy => (TP + TN) / (TP + TN + FP + FN),
            EvaluationOfResultsType.Recall => TP / (TP + FN),
            EvaluationOfResultsType.FPR => FP / (FP + TN),
            EvaluationOfResultsType.Precision => TP / (TP + FP),
            EvaluationOfResultsType.F_score => TP * 2 / (2 * TP + FN + FP),
            _ => 0
        };
    }

    /// <summary>
    ///     ����� ��������� ��������������� ������
    /// </summary>
    /// <param name="input">����</param>
    /// <param name="outputNeuronsNeeded">����������� �����</param>
    private double Backpropagation(double[] input, double[] outputNeuronsNeeded)
    {
        double[] outputNeurons = Run(input);

        double[] errorNeurons = AdjustingLayerWeights(outputNeuronsNeeded, hiddenLayers.Last(), outputLayer);
        for (int id = hiddenLayers.Length - 1; id >= 0; id--)
            errorNeurons = AdjustingLayerWeights(null, id == 0 ? inputLayer : hiddenLayers[id - 1], hiddenLayers[id],
                errorNeurons);
        double summ = 0;
        for (int id = 0; id < outputNeurons.Length; id++)
            summ += SummLossFunction(outputNeurons[id], outputNeuronsNeeded[id]);
        return summ;
    }

    public void Learn(List<double[]> input, List<double[]> outputNeeded)
    {
        StartCoroutine(ILearn(input, outputNeeded));
    }

    private IEnumerator ILearn(List<double[]> inputs, List<double[]> outputsNeeded)
    {
        while (true)
        {
            MyExtentions.MixingTwoLists(inputs, outputsNeeded);
            double summ = 0;
            int count = 0;
            for (int idExample = 0; idExample < inputs.Count; idExample++)
            {
                yield return new WaitWhile(() => stop && !step);
                step = false;
                yield return new WaitForEndOfFrame();
                summ += Backpropagation(inputs[idExample], outputsNeeded[idExample]);
                count += outputsNeeded[idExample].Length;
                exampleCount++;
            }

            countEra++;
            error = LossFunction(summ, count);
            if (error < E)
            {
                SaveNeuralNetwork();
                yield break;
            }
        }
    }

    /// <summary>
    ///     ������� ������ https://habr.com/ru/articles/722628/
    /// </summary>
    /// <param name="result">����� �������</param>
    /// <param name="needResult">����������� ����� �������</param>
    /// <returns>����������</returns>
    public double LossFunction(double summ, int count)
    {
        return lossFunction switch
        {
            LossFunctionType.MAE => summ / count,
            LossFunctionType.MSE => summ / count,
            LossFunctionType.RMSE => Math.Sqrt(summ / count),
            _ => 0
        };
    }

    public double SummLossFunction(double result, double needResult)
    {
        return lossFunction switch
        {
            LossFunctionType.MAE => result - needResult,
            LossFunctionType.MSE => Math.Pow(result - needResult, 2),
            LossFunctionType.RMSE => Math.Abs(result - needResult),
            _ => 0
        };
    }

    [Button("Next", 15)]
    public void Next()
    {
        step = true;
    }

    [Button("Stop or Resume", 15)]
    public void StopOrResume()
    {
        stop = !stop;
    }

    public void CreateNeuralNetwork(int countInputNeurons, int[] countHiddenNeurons, int countOutputNeurons)
    {
        if (countHiddenNeurons.Length == 0)
            return;
        hiddenLayers = new NeuralLayer[countHiddenNeurons.Length];

        CreateNeuralLayer(out inputLayer, null, countInputNeurons, "Input layer");
        for (int id = 0; id < hiddenLayers.Length; id++)
            CreateNeuralLayer(out hiddenLayers[id], id == 0 ? inputLayer : hiddenLayers[id - 1], countHiddenNeurons[id],
                $"Hidden layer {id + 1}");
        CreateNeuralLayer(out outputLayer, hiddenLayers.Last(), countOutputNeurons, "Output layer");
    }

    private void CreateNeuralLayer(out NeuralLayer layer, NeuralLayer previousLayer, int countNeurons, string nameLayer)
    {
        layer = Instantiate(prefabNeuralLayer, transform);
        layer.name = nameLayer;
        layer.CreateNeuralLayer(countNeurons, previousLayer);
        layer.activationFunction = NeuralLayer.ActivationFunctionType.Sigmoid;
    }

    public double[] Run(double[] input)
    {
        //input.Normalize();
        inputLayer.LoadNeurons(input);
        for (int id = 0; id < hiddenLayers.Length; id++)
            hiddenLayers[id].ActivationNeurons();

        outputLayer.ActivationNeurons();
        return outputLayer.GetNeuronsValue();
    }

    public void LoadNeuralNetwork()
    {
        MyExtentions.ReadDat("Neural Network", ReadDat);
    }

    public void SaveNeuralNetwork()
    {
        MyExtentions.WriteDat("Neural Network", WriteDat);
    }

    #endregion Methods
}
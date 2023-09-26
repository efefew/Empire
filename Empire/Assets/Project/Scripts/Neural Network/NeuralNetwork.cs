using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AdvancedEditorTools.Attributes;

using UnityEngine;

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
    ///  ��� ������� ������
    /// </summary>
    public enum LossFunctionType
    {
        /// <summary>
        /// ������� ���������� ������
        /// </summary>
        MAE,
        /// <summary>
        /// ������������������ ������� ������
        /// </summary>
        MSE,
        /// <summary>
        /// ������������������ ����������
        /// </summary>
        RMSE
    }
    /// <summary>
    /// ��� ������� ���������
    /// </summary>
    public enum ActivationFunctionType
    {
        /// <summary>
        /// ������������� (�������� ��� ������� ���������)
        /// </summary>
        Sigmoid,
        /// <summary>
        /// ReLu
        /// </summary>
        ReLu
    }
    /// <summary>
    /// ��� ������ ����������� (�������) ������� ��������/������ ������������ ����� �������� ��� ��������� ����� ������������������.
    /// https://habr.com/ru/articles/722628/
    /// https://skine.ru/articles/559485/
    /// </summary>
    public enum EvaluationOfResultsType
    {
        /// <summary>
        /// ������� "������������"
        /// </summary>
        Accuracy,

        /// <summary>
        /// ������� "�������"
        /// </summary>
        Recall,

        /// <summary>
        /// ������� " ���� �����-������������� (False Positive Rate)"
        /// </summary>
        FPR,

        /// <summary>
        /// ������� "��������"
        /// </summary>
        Precision,

        /// <summary>
        /// ������� "F-����"
        /// </summary>
        F_score
    }

    #endregion Enums

    #region Fields

    /// <summary>
    /// ��������, ����������� ����������������, �� �������� ������� �������� �������� ���������
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private double learningRate = 0.3;
    [SerializeField]
    private NeuralLayer prefabNeuralLayer;

    [SerializeField]
    private NeuralLayer inputLayer;
    [SerializeField]
    private NeuralLayer[] hiddenLayers;
    [SerializeField]
    private NeuralLayer outputLayer;

    [Tooltip("������ ����������� (�� ������������)")]
    public EvaluationOfResultsType evaluationOfResults;
    [Tooltip("������� ������")]
    public LossFunctionType lossFunction;
    [Tooltip("������� ���������")]
    public ActivationFunctionType activationFunction;
    public int countEra, countPhoto;
    private bool stop, step;
    public double error, E = 10E-3;
    #endregion Fields

    #region Methods

    /// <summary>
    /// ������������� ����� ����
    /// </summary>
    /// <param name="neuronsNeeded">��������� �������� �������� ����</param>
    /// <param name="previousLayer">���������� ����</param>
    /// <param name="layer">����</param>
    /// <param name="errorNeurons"></param>
    /// <returns></returns>
    private double[] AdjustingLayerWeights(double[] neuronsNeeded, NeuralLayer previousLayer, NeuralLayer layer, double[] errorNeurons = null)
    {
        double[] errorPrevousNeurons = new double[previousLayer.neurons.Length];
        for (int id = 0; id < errorPrevousNeurons.Length; id++)
            errorPrevousNeurons[id] = 0;

        double delta, error;

        for (int neuronID = 0; neuronID < layer.neurons.Length; neuronID++)
        {
            error = errorNeurons == null ?
                  layer.neurons[neuronID].value - neuronsNeeded[neuronID]
                : errorNeurons[neuronID];
            delta = error * DerivativeFunction(layer.neurons[neuronID].value);
            for (int prevousNeuronID = 0; prevousNeuronID < previousLayer.neurons.Length; prevousNeuronID++)
            {
                layer.neurons[neuronID].weight[prevousNeuronID] -= previousLayer.neurons[prevousNeuronID].value * delta * learningRate;
                errorPrevousNeurons[prevousNeuronID] += delta * layer.neurons[neuronID].weight[prevousNeuronID];
            }
        }

        return errorPrevousNeurons;
    }

    private void WriteDat(ref BinaryWriter writer)
    {
        writer.Write(inputLayer.neurons.Length);
        writer.Write(hiddenLayers.Length);
        foreach (NeuralLayer layer in hiddenLayers)
            writer.WriteArray2D(layer.GetWeight2D());
        writer.WriteArray2D(outputLayer.GetWeight2D());
    }

    private void ReadDat(ref BinaryReader reader)
    {
        inputLayer.CreateNeuralLayer(reader.ReadInt32());
        hiddenLayers = new NeuralLayer[reader.ReadInt32()];
        hiddenLayers[0] = new();
        hiddenLayers[0].LoadNeuralLayer(reader.ReadArray2D(), inputLayer);
        if (hiddenLayers.Length > 1)
        {
            for (int id = 1; id < hiddenLayers.Length; id++)
            {
                hiddenLayers[id] = new();
                hiddenLayers[id].LoadNeuralLayer(reader.ReadArray2D(), hiddenLayers[id - 1]);
            }
        }

        outputLayer.LoadNeuralLayer(reader.ReadArray2D(), hiddenLayers.Last());
    }

    /// <summary>
    ///  ������ �����������
    /// </summary>
    /// <param name="TP">���������� ������� ������������� �������, �� ���� ����� ������ ��� ����� ��������������� � �� ����������� ���������� �������� ���������</param>
    /// <param name="TN">���������� ������� �������������  �������, �� ���� ����� ������ ��� ����� ��������������� � �� ����������� ���������� ���������</param>
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
            EvaluationOfResultsType.F_score => TP * 2 / ((2 * TP) + FN + FP),
            _ => 0
        };
    }
    /// <summary>
    /// ����� ��������� ��������������� ������
    /// </summary>
    /// <param name="input">����</param>
    /// <param name="outputNeuronsNeeded">����������� �����</param>
    private double[] Backpropagation(double[] input, double[] outputNeuronsNeeded)
    {

        double[] outputNeurons = Run(input);

        double[] errorNeurons = AdjustingLayerWeights(outputNeuronsNeeded, hiddenLayers.Last(), outputLayer);
        for (int id = hiddenLayers.Length - 1; id >= 1; id--)
            errorNeurons = AdjustingLayerWeights(null, hiddenLayers[id - 1], hiddenLayers[id], errorNeurons);

        _ = AdjustingLayerWeights(null, inputLayer, hiddenLayers[0], errorNeurons);
        return outputNeurons;
    }
    /// <summary>
    /// ����� ��������
    /// </summary>
    /// <param name="input">����</param>
    /// <param name="outputNeeded">����������� �����</param>
    public void EraOfLearning(List<double[]> input, List<double[]> outputNeeded) => StartCoroutine(IEraOfLearning(input, outputNeeded));
    private IEnumerator IEraOfLearning(List<double[]> inputs, List<double[]> outputsNeeded)
    {
        while (true)
        {
            //MyExtentions.MixingTwoLists(inputs, outputsNeeded);
            List<double> result = new();
            List<double> needResult = new();
            for (int idPhoto = 0; idPhoto < inputs.Count; idPhoto++)
            {
                yield return new WaitWhile(() => stop && !step);
                step = false;
                yield return new WaitForEndOfFrame();
                //yield return new WaitForSeconds(0.001f);
                needResult.AddRange(outputsNeeded[idPhoto]);
                result.AddRange(Backpropagation(inputs[idPhoto], outputsNeeded[idPhoto]));
                countPhoto++;
            }

            countEra++;
            error = LossFunction(result.ToArray(), needResult.ToArray());
            if (error < E)
            {
                SaveNeuralNetwork();
                yield break;
            }
        }
    }
    /// <summary>
    ///  ������� ������ https://habr.com/ru/articles/722628/
    /// </summary>
    /// <param name="result">����� �������</param>
    /// <param name="needResult">����������� ����� �������</param>
    /// <returns>����������</returns>
    public double LossFunction(double[] result, double[] needResult)
    {
        double lossValue = 0;
        int n = result.Length;
        switch (lossFunction)
        {
            case LossFunctionType.MAE:
                for (int i = 0; i < n; i++)
                    lossValue += Math.Abs(result[i] - needResult[i]);
                lossValue /= n;
                return lossValue;

            case LossFunctionType.MSE:
                for (int i = 0; i < n; i++)
                    lossValue += (result[i] - needResult[i]) * (result[i] - needResult[i]);
                lossValue /= n;
                return lossValue;

            case LossFunctionType.RMSE:
                for (int i = 0; i < n; i++)
                    lossValue += Math.Abs(result[i] - needResult[i]);
                lossValue = Math.Sqrt(lossValue / n);
                return lossValue;

            default:
                return lossValue;
        }
    }
    [Button("Next", 15)]
    public void Next() => step = true;
    [Button("Stop or Resume", 15)]
    public void StopOrResume() => stop = !stop;
    public void CreateNeuralNetwork(int countInputNeurons, int[] countHiddenNeurons, int countOutputNeurons)
    {
        if (countHiddenNeurons.Length == 0)
            return;
        hiddenLayers = new NeuralLayer[countHiddenNeurons.Length];

        CreateNeuralLayer(out inputLayer, null, countInputNeurons, "Input layer");
        for (int id = 0; id < hiddenLayers.Length; id++)
            CreateNeuralLayer(out hiddenLayers[id], id == 0 ? inputLayer : hiddenLayers[id - 1], countHiddenNeurons[id], $"Hidden layer {id + 1}");
        CreateNeuralLayer(out outputLayer, hiddenLayers.Last(), countOutputNeurons, "Output layer");
    }

    private void CreateNeuralLayer(out NeuralLayer layer, NeuralLayer previousLayer, int countNeurons, string nameLayer)
    {
        layer = Instantiate(prefabNeuralLayer, transform);
        layer.name = nameLayer;
        layer.CreateNeuralLayer(countNeurons, previousLayer);
    }

    public double[] Run(double[] input)
    {
        input.Normalize();
        inputLayer.LoadNeurons(input);
        for (int id = 0; id < hiddenLayers.Length; id++)
            hiddenLayers[id].ActivationNeurons(ActivationFunction);

        outputLayer.ActivationNeurons(ActivationFunction);
        return outputLayer.GetNeuronsValue();
    }

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="x">��������</param>
    /// <returns>���������</returns>
    public double ActivationFunction(double x)
    {
        return activationFunction switch
        {
            ActivationFunctionType.Sigmoid => 1 / (1 + Math.Pow(Math.E, -x)),
            ActivationFunctionType.ReLu => Math.Max(0, x),
            _ => 0,
        };
    }

    /// <summary>
    /// ����������� �������
    /// </summary>
    /// <param name="x">��������</param>
    /// <returns>���������</returns>
    public double DerivativeFunction(double x)
    {
        return activationFunction switch
        {
            ActivationFunctionType.Sigmoid => x * (1 - x),
            //ActivationFunctionType.Sigmoid => ActivationFunction(x) * (1 - ActivationFunction(x)),
            ActivationFunctionType.ReLu => x > 0 ? 1 : 0,
            _ => 0,
        };
    }

    public void LoadNeuralNetwork() => MyExtentions.ReadDat("Neural Network", ReadDat);

    public void SaveNeuralNetwork() => MyExtentions.WriteDat("Neural Network", WriteDat);

    #endregion Methods

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AdvancedEditorTools.Attributes;

using UnityEngine;
[DisallowMultipleComponent()]
/// <summary>
/// Нейроная сеть
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
    ///  Тип функции потерь
    /// </summary>
    public enum LossFunctionType
    {
        /// <summary>
        /// средняя абсолютная ошибка
        /// </summary>
        MAE,
        /// <summary>
        /// среднеквадратичная функция потерь
        /// </summary>
        MSE,
        /// <summary>
        /// среднеквадратичное отклонение
        /// </summary>
        RMSE
    }

    /// <summary>
    /// Тип оценки результатов (метрика) Метрика Критерий/Оценка используется после обучения для измерения общей производительности.
    /// https://habr.com/ru/articles/722628/
    /// https://skine.ru/articles/559485/
    /// </summary>
    public enum EvaluationOfResultsType
    {
        /// <summary>
        /// Метрика "Правильность"
        /// </summary>
        Accuracy,

        /// <summary>
        /// Метрика "Полнота"
        /// </summary>
        Recall,

        /// <summary>
        /// Метрика " Доля ложно-положительных (False Positive Rate)"
        /// </summary>
        FPR,

        /// <summary>
        /// Метрика "Точность"
        /// </summary>
        Precision,

        /// <summary>
        /// Метрика "F-мера"
        /// </summary>
        F_score
    }

    #endregion Enums

    #region Fields

    /// <summary>
    /// значение, подбираемое экспериментально, от которого зависит скорость обучения нейросети
    /// </summary>
    [SerializeField]
    [Range(0f, 1f)]
    private double learningRate = 0.3;
    [SerializeField]
    private NeuralLayer prefabNeuralLayer;

    public NeuralLayer inputLayer;
    public NeuralLayer[] hiddenLayers;
    public NeuralLayer outputLayer;

    [Tooltip("Оценка результатов (не используется)")]
    public EvaluationOfResultsType evaluationOfResults;
    [Tooltip("Функция потерь")]
    public LossFunctionType lossFunction;

    public int countEra, exampleCount;
    private bool stop, step;
    public double error, E = 10E-3;
    #endregion Fields

    #region Methods

    /// <summary>
    /// Корректировка весов слоя
    /// </summary>
    /// <param name="neuronsNeeded">требуемое значение нейронов слоя</param>
    /// <param name="previousLayer">предыдущий слой</param>
    /// <param name="layer">слой</param>
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
            delta = error * layer.neurons[neuronID].DerivativeFunction();
            for (int prevousNeuronID = 0; prevousNeuronID < previousLayer.neurons.Length; prevousNeuronID++)
            {
                errorPrevousNeurons[prevousNeuronID] += delta * layer.neurons[neuronID].weight[prevousNeuronID];
                layer.neurons[neuronID].weight[prevousNeuronID] -= previousLayer.neurons[prevousNeuronID].value * delta * learningRate;
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
    ///  Оценка результатов
    /// </summary>
    /// <param name="TP">количество истинно положительных случаев, то есть когда случай был верно классифицирован и он принадлежит выбранному целевому множеству</param>
    /// <param name="TN">количество истинно отрицательных  случаев, то есть когда случай был верно классифицирован и он принадлежит нецелевому множеству</param>
    /// <param name="FP">ложно-положительный – число случаев классификации элемента нецелевого множества как целевого</param>
    /// <param name="FN">ложно-отрицательный – число случаев классификации элемента целевого множества как нецелевого</param>
    /// <returns>значение оценки результатов</returns>
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
    /// Метод обратного распространения ошибки
    /// </summary>
    /// <param name="input">вход</param>
    /// <param name="outputNeuronsNeeded">необходимый выход</param>
    private double Backpropagation(double[] input, double[] outputNeuronsNeeded)
    {

        double[] outputNeurons = Run(input);

        double[] errorNeurons = AdjustingLayerWeights(outputNeuronsNeeded, hiddenLayers.Last(), outputLayer);
        for (int id = hiddenLayers.Length - 1; id >= 0; id--)
            errorNeurons = AdjustingLayerWeights(null, id == 0 ? inputLayer : hiddenLayers[id - 1], hiddenLayers[id], errorNeurons);
        double summ = 0;
        for (int id = 0; id < outputNeurons.Length; id++)
            summ += SummLossFunction(outputNeurons[id], outputNeuronsNeeded[id]);
        return summ;
    }
    public void Learn(List<double[]> input, List<double[]> outputNeeded) => StartCoroutine(ILearn(input, outputNeeded));
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
    ///  Функция потерь https://habr.com/ru/articles/722628/
    /// </summary>
    /// <param name="result">выход нейрона</param>
    /// <param name="needResult">необходимый выход нейрона</param>
    /// <returns>отклонение</returns>
    public double LossFunction(double summ, int count)
    {
        return lossFunction switch
        {
            LossFunctionType.MAE => summ / count,
            LossFunctionType.MSE => summ / count,
            LossFunctionType.RMSE => Math.Sqrt(summ / count),
            _ => 0,
        };
    }
    public double SummLossFunction(double result, double needResult)
    {
        return lossFunction switch
        {
            LossFunctionType.MAE => result - needResult,
            LossFunctionType.MSE => Math.Pow(result - needResult, 2),
            LossFunctionType.RMSE => Math.Abs(result - needResult),
            _ => 0,
        };
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

    public void LoadNeuralNetwork() => MyExtentions.ReadDat("Neural Network", ReadDat);

    public void SaveNeuralNetwork() => MyExtentions.WriteDat("Neural Network", WriteDat);

    #endregion Methods

}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AdvancedEditorTools.Attributes;

using UnityEngine;

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
    /// Тип функции активации
    /// </summary>
    public enum ActivationFunctionType
    {
        /// <summary>
        /// Логистическая (сигмоида или Гладкая ступенька)
        /// </summary>
        Sigmoid,
        /// <summary>
        /// ReLu
        /// </summary>
        ReLu
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

    [SerializeField]
    private NeuralLayer inputLayer;
    [SerializeField]
    private NeuralLayer[] hiddenLayers;
    [SerializeField]
    private NeuralLayer outputLayer;

    [Tooltip("Оценка результатов (не используется)")]
    public EvaluationOfResultsType evaluationOfResults;
    [Tooltip("Функция потерь")]
    public LossFunctionType lossFunction;
    [Tooltip("Функция активации")]
    public ActivationFunctionType activationFunction;
    public int countEra, countPhoto;
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
    /// Эпоха обучения
    /// </summary>
    /// <param name="input">вход</param>
    /// <param name="outputNeeded">необходимый выход</param>
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
    ///  Функция потерь https://habr.com/ru/articles/722628/
    /// </summary>
    /// <param name="result">выход нейрона</param>
    /// <param name="needResult">необходимый выход нейрона</param>
    /// <returns>отклонение</returns>
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
    /// Функция активации
    /// </summary>
    /// <param name="x">параметр</param>
    /// <returns>результат</returns>
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
    /// Производная функции
    /// </summary>
    /// <param name="x">параметр</param>
    /// <returns>результат</returns>
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
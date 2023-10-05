using UnityEngine;
/// <summary>
/// Слой подвыборки
/// </summary>
public class PoolingLayer : ConvolutionalNetworkLayer
{
    public override double[,] Run(double[,] matrix)
    {
        if (inWidth != matrix.GetLength(0) || inHeight != matrix.GetLength(1))
        {
            Debug.LogError("условия изменились!");
            throw new System.Exception();
        }

        double max;
        int idStepX = 0, idStepY = 0;
        for (int xOut = 0; xOut < outWidth; xOut++)
        {
            for (int yOut = 0; yOut < outHeight; yOut++)
            {
                max = double.MinValue;
                for (int xIn = idStepX; xIn < size + idStepX; xIn++)
                {
                    for (int yIn = idStepY; yIn < size + idStepY; yIn++)
                    {
                        if (matrix[xIn, yIn] > max)
                            max = matrix[xIn, yIn];
                    }
                }

                outMatrix[xOut, yOut] = max;
                idStepY += step;
            }

            idStepY = 0;
            idStepX += step;
        }

        return outMatrix;
    }
}

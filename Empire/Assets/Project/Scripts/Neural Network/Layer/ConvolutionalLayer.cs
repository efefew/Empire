using UnityEngine;
/// <summary>
/// Свёрточный слой
/// </summary>
public class ConvolutionalLayer : ConvolutionalNetworkLayer
{
    private const float MIN_KERNEL_VALUE = -5f, MAX_KERNEL_VALUE = 5f;
    private double[,] kernel;
    public override void Create(int width, int height, int step, int size)
    {
        base.Create(width, height, step, size);
        SetRandomKernel(size);
    }
    public void SetRandomKernel(int size)
    {
        kernel = new double[size, size];
        for (int x = 0; x < kernel.GetLength(0); x++)
        {
            for (int y = 0; y < kernel.GetLength(1); y++)
                kernel[x, y] = Random.Range(MIN_KERNEL_VALUE, MAX_KERNEL_VALUE);
        }
    }
    public override double[,] Run(double[,] matrix)
    {
        if (inWidth != matrix.GetLength(0) || inHeight != matrix.GetLength(1))
        {
            Debug.LogError("условия изменились!");
            throw new System.Exception();
        }

        double summ;
        int idStepX = 0, idStepY = 0;
        for (int xOut = 0; xOut < outWidth; xOut++)
        {
            for (int yOut = 0; yOut < outHeight; yOut++)
            {
                summ = 0;
                for (int xIn = idStepX; xIn < size + idStepX; xIn++)
                {
                    for (int yIn = idStepY; yIn < size + idStepY; yIn++)
                        summ += matrix[xIn, yIn] * kernel[yIn - idStepY, xIn - idStepX];
                }

                outMatrix[xOut, yOut] = summ;
                idStepY += step;
            }

            idStepY = 0;
            idStepX += step;
        }

        return outMatrix;
    }
}

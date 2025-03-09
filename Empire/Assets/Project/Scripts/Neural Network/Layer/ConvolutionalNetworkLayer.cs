#region

using UnityEngine;

#endregion

public abstract class ConvolutionalNetworkLayer : MonoBehaviour
{
    public double[,] outMatrix;
    protected int size, step;
    public int inWidth { get; protected set; }
    public int inHeight { get; protected set; }
    public int outWidth { get; protected set; }
    public int outHeight { get; protected set; }

    public virtual void Create(int width, int height, int step, int size)
    {
        this.step = step;
        this.size = size;
        inWidth = width;
        inHeight = height;
        if (step > size)
            Debug.Log("������ ���� ������ ��� ������ ��� ������");
        outWidth = (width - size) / step + 1;
        outHeight = (height - size) / step + 1;
        outMatrix = new double[outWidth, outHeight];
    }

    public abstract double[,] Run(double[,] matrix);
}
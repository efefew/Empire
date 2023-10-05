using UnityEngine;

public abstract class ConvolutionalNetworkLayer : MonoBehaviour
{
    public int inWidth { get; protected set; }
    public int inHeight { get; protected set; }
    public int outWidth { get; protected set; }
    public int outHeight { get; protected set; }
    protected int size, step;
    public double[,] outMatrix;
    public virtual void Create(int width, int height, int step, int size)
    {
        this.step = step;
        this.size = size;
        inWidth = width;
        inHeight = height;
        if (step > size)
            Debug.Log("плохая идея делать шаг больше чем размер");
        outWidth = ((width - size) / step) + 1;
        outHeight = ((height - size) / step) + 1;
        outMatrix = new double[outWidth, outHeight];
    }
    public abstract double[,] Run(double[,] matrix);
}

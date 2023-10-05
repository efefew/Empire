using System.Collections.Generic;
using System.IO;

using UnityEngine;

using ReadOnlyAttribute = AdvancedEditorTools.Attributes.ReadOnlyAttribute;

[RequireComponent(typeof(ConvolutionalNeuralNetwork))]
public class PhotoNetwork : MonoBehaviour
{
    private ConvolutionalNeuralNetwork network;
    private List<double[,]> inputs = new();
    private List<double[]> outputsNeeded = new();
    public const int COUNT_ELLIPSE = 197, COUNT_RECTANGLE = 199, COUNT_TRIANGLE = 188;
    [ReadOnly]
    public int countEllipse, countRectangle, countTriangle;
    private void Start()
    {
        network = GetComponent<ConvolutionalNeuralNetwork>();
        //SaveBmpToDat();
        Learn();
    }

    private void Learn()
    {
        LoadBmpToDat();
        network.CreateNeuralNetwork(58, 58, new int[] { 128 }, 3);
        network.Learn(inputs, outputsNeeded);
    }
    //private void OnDisable() => network.SaveNeuralNetwork();
    private void WriteEllipseDat(ref BinaryWriter writer)
    {
        for (int i = 0; i < 200; i++)
        {
            Texture2D tex = MyExtentions.LoadBmpTexture($@"W:\Users\prodi\Downloads\drawings\ellipse{i}.Bmp");
            if (tex == null)
                continue;
            countEllipse++;
            writer.WriteArray2D(ColorToDoubleArray(tex));
        }
    }
    private void WriteRectangleDat(ref BinaryWriter writer)
    {
        for (int i = 0; i < 200; i++)
        {
            Texture2D tex = MyExtentions.LoadBmpTexture($@"W:\Users\prodi\Downloads\drawings\rectangle{i}.Bmp");
            if (tex == null)
                continue;
            countRectangle++;
            writer.WriteArray2D(ColorToDoubleArray(tex));
        }
    }
    private void WriteTriangleDat(ref BinaryWriter writer)
    {
        for (int i = 0; i < 200; i++)
        {
            Texture2D tex = MyExtentions.LoadBmpTexture($@"W:\Users\prodi\Downloads\drawings\triangle{i}.Bmp");
            if (tex == null)
                continue;
            countTriangle++;
            writer.WriteArray2D(ColorToDoubleArray(tex));
        }
    }
    private double[,] ColorToDoubleArray(Texture2D tex)
    {
        double[,] matrix = new double[tex.width, tex.height];
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                Color color = tex.GetPixel(x, y);
                matrix[x, y] = ((color.r + color.g + color.b) / 3.0) - 1.0;
            }
        }

        return matrix;
    }
    private void ReadEllipseDat(ref BinaryReader reader)
    {
        for (int i = 0; i < COUNT_ELLIPSE; i++)
        {
            outputsNeeded.Add(new double[3] { 0, 0, 1 });
            inputs.Add(reader.ReadArray2D());
        }
    }

    private void ReadRectangleDat(ref BinaryReader reader)
    {
        for (int i = 0; i < COUNT_RECTANGLE; i++)
        {
            outputsNeeded.Add(new double[3] { 0, 1, 0 });
            inputs.Add(reader.ReadArray2D());
        }
    }
    private void ReadTriangleDat(ref BinaryReader reader)
    {
        for (int i = 0; i < COUNT_TRIANGLE; i++)
        {
            outputsNeeded.Add(new double[3] { 0, 0, 1 });
            inputs.Add(reader.ReadArray2D());
        };
    }
    private void LoadBmpToDat()
    {
        MyExtentions.ReadDat("BMP Ellipse", ReadEllipseDat);
        MyExtentions.ReadDat("BMP Rectangle", ReadRectangleDat);
        MyExtentions.ReadDat("BMP Triangle", ReadTriangleDat);
    }
    private void SaveBmpToDat()
    {
        MyExtentions.WriteDat("BMP Ellipse", WriteEllipseDat);
        MyExtentions.WriteDat("BMP Rectangle", WriteRectangleDat);
        MyExtentions.WriteDat("BMP Triangle", WriteTriangleDat);
    }
}

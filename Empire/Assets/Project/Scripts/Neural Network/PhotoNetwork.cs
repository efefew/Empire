using System.Collections.Generic;
using System.IO;

using UnityEngine;

using ReadOnlyAttribute = AdvancedEditorTools.Attributes.ReadOnlyAttribute;

[RequireComponent(typeof(NeuralNetwork))]
public class PhotoNetwork : MonoBehaviour
{
    private NeuralNetwork network;
    private List<double[]> inputs = new();
    private List<double[]> outputsNeeded = new();
    public const int COUNT_ELLIPSE = 49, COUNT_RECTANGLE = 46, COUNT_TRIANGLE = 49;
    [ReadOnly]
    public int countEllipse, countRectangle, countTriangle;
    private void Start()
    {
        network = GetComponent<NeuralNetwork>();
        //SaveBmpToDat();
        Learn();
    }

    private void Learn()
    {
        LoadBmpToDat();
        network.CreateNeuralNetwork(58 * 58, new int[2] { 500, 300 }, 3);
        network.EraOfLearning(inputs, outputsNeeded);
    }
    private void OnDisable() => network.SaveNeuralNetwork();
    private void WriteEllipseDat(ref BinaryWriter writer)
    {
        for (int i = 0; i < 200; i++)
        {
            Texture2D tex = MyExtentions.LoadBmpTexture($@"W:\Users\prodi\Downloads\drawings\ellipse{i}.Bmp");
            if (tex == null)
                continue;
            countEllipse++;
            writer.WriteArray(ColorToDoubleArray(tex.GetPixels()));
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
            writer.WriteArray(ColorToDoubleArray(tex.GetPixels()));
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
            writer.WriteArray(ColorToDoubleArray(tex.GetPixels()));
        }
    }
    private double[] ColorToDoubleArray(Color[] colors)
    {
        double[] array = new double[colors.Length];
        for (int id = 0; id < array.Length; id++)
            array[id] = (colors[id].r + colors[id].g + colors[id].b) / 3.0;
        return array;
    }
    private void ReadEllipseDat(ref BinaryReader reader)
    {
        for (int i = 0; i < COUNT_ELLIPSE; i++)
        {
            outputsNeeded.Add(new double[3] { 0, 0, 1 });
            inputs.Add(reader.ReadArray());
        }
    }

    private void ReadRectangleDat(ref BinaryReader reader)
    {
        for (int i = 0; i < COUNT_RECTANGLE; i++)
        {
            outputsNeeded.Add(new double[3] { 0, 1, 0 });
            inputs.Add(reader.ReadArray());
        }
    }
    private void ReadTriangleDat(ref BinaryReader reader)
    {
        for (int i = 0; i < COUNT_TRIANGLE; i++)
        {
            outputsNeeded.Add(new double[3] { 0, 0, 1 });
            inputs.Add(reader.ReadArray());
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

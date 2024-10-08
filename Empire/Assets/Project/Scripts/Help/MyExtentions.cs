using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using B83.Image.BMP;

using UnityEngine;
using UnityEngine.EventSystems;

public static class MyExtentions
{
    #region Delegates

    public delegate void BinaryWriteHandler(ref BinaryWriter writer);

    public delegate void BinaryReadHandler(ref BinaryReader reader);

    #endregion Delegates

    #region Methods

    private static System.Random random = new((int)DateTime.Now.Ticks & 0x0000FFFF);

    /// <summary>
    /// Нормализовать массив
    /// </summary>
    /// <param name="array">массив</param>
    public static void Normalize(this double[] array, double min = 0, double max = 1)
    {
        double minInArray = array.Min();
        double maxInArray = array.Max();
        if (minInArray == maxInArray)
        {
            double value = maxInArray > 0 ? 1 : 0;
            for (int id = 0; id < array.Length; id++)
                array[id] = value;
            return;
        }

        double relativeValue;
        for (int id = 0; id < array.Length; id++)
        {
            relativeValue = (array[id] - minInArray) / (maxInArray - minInArray);//от 0 до 1
            array[id] = (relativeValue * (max - min)) + min;
        }
    }

    /// <summary>
    /// Перемешивание списка
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list">список</param>
    public static void Mixing<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    /// <summary>
    /// Перемешивание массива
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array">массив</param>
    public static void Mixing<T>(this T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }

    /// <summary>
    /// Перемешивание двух списков однаково
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list1">список 1</param>
    /// <param name="list2">список 2</param>
    public static void MixingTwoLists<T>(IList<T> list1, IList<T> list2)
    {
        if (list1.Count != list2.Count)
            throw new Exception("списки должны иметь одинаковый размер");
        int n = list1.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list1[n], list1[k]) = (list1[k], list1[n]);
            (list2[n], list2[k]) = (list2[k], list2[n]);
        }
    }

    public static T[] ToArray<T>(this T[,] matrix)
    {
        T[] array = new T[matrix.GetLength(0) * matrix.GetLength(1)];
        int id = 0;
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                array[id] = matrix[x, y];
                id++;
            }
        }

        return array;
    }

    public static void WriteArray(this BinaryWriter writer, double[] arr)
    {
        writer.Write(arr.Length);

        for (int id = 0; id < arr.Length; id++)
            writer.Write(arr[id]);
    }

    public static void WriteArray2D(this BinaryWriter writer, double[,] arr)
    {
        writer.Write(arr.GetLength(1));
        writer.Write(arr.GetLength(0));

        for (int y = 0; y < arr.GetLength(1); y++)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
                writer.Write(arr[x, y]);
        }
    }

    public static double[] ReadArray(this BinaryReader reader)
    {
        int length = reader.ReadInt32();
        double[] array = new double[length];

        for (int id = 0; id < length; id++)
            array[id] = reader.ReadDouble();

        return array;
    }

    public static double[,] ReadArray2D(this BinaryReader reader)
    {
        int yCount = reader.ReadInt32();
        int xCount = reader.ReadInt32();
        double[,] array = new double[xCount, yCount];
        for (int y = 0; y < yCount; y++)
        {
            for (int x = 0; x < xCount; x++)
                array[x, y] = reader.ReadDouble();
        }

        return array;
    }

    public static void WriteDat(string fileName, BinaryWriteHandler write)
    {
        using FileStream fs = new($"{fileName}.dat", FileMode.Create);
        BinaryWriter binaryWriter = new(fs);
        write?.Invoke(ref binaryWriter);
        binaryWriter.Flush();
        binaryWriter.Close();
    }

    public static void ReadDat(string path, BinaryReadHandler read)
    {
        path = Path.GetExtension(path) == ".dat" ? path : $"{path}.dat";
        if (!File.Exists(path))
        {
            Debug.LogWarning($"не существует файла {path}");
            return;
        }

        using FileStream fs = new(path, FileMode.Open);
        BinaryReader binaryReader = new(fs);
        read?.Invoke(ref binaryReader);
        binaryReader.Close();
    }

    public static Sprite ToSprite(this Texture2D tex) => Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

    public static Texture2D LoadBmpTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);

            BMPLoader bmpLoader = new();
            //bmpLoader.ForceAlphaReadWhenPossible = true; //Uncomment to read alpha too

            //Load the BMP data
            BMPImage bmpImg = bmpLoader.LoadBMP(fileData);

            //Convert the Color32 array into a Texture2D
            tex = bmpImg.ToTexture2D();
        }

        return tex;
    }

    public static Texture2D ToTexture2D(this byte[] bytes)
    {
        Texture2D texture = new(2, 2);
        _ = texture.LoadImage(bytes);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Очистить объект от вложенных объектов
    /// </summary>
    /// <param name="transform">объект</param>
    public static void Clear(this Transform transform)
    {
        if (transform.childCount == 0)
            return;
        for (int idChild = 0; idChild < transform.childCount; idChild++)
            UnityEngine.Object.Destroy(transform.GetChild(idChild).gameObject);
    }

    public static Vector3 X(this Vector3 vector, float value) => new(value, vector.y, vector.z);

    public static Vector3 Y(this Vector3 vector, float value) => new(vector.x, value, vector.z);

    public static Vector3 Z(this Vector3 vector, float value) => new(vector.x, vector.y, value);

    /// <summary>
    /// Следить за целью (2D версия)
    /// </summary>
    /// <param name="transform">следящий</param>
    /// <param name="target">цель</param>
    public static void LookAt2D(this Transform transform, Vector3 target)
    {
        Vector2 direction = target - transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        transform.eulerAngles = transform.eulerAngles.Z(angle);
    }

    /// <summary>
    /// Проверяет, находится ли указатель мыши над объектом UI.
    /// </summary>
    /// <returns>находится ли указатель мыши над объектом UI</returns>
    public static bool IsPointerOverUI()
    {
        // Создаем экземпляр PointerEventData с текущим положением указателя мыши.
        PointerEventData eventDataCurrentPosition = new(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };

        List<RaycastResult> results = new();

        // Выполняем лучевой кастинг для всех объектов в текущей позиции указателя мыши.
        // Результаты сохраняются в списке results.
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // Если количество результатов больше нуля, значит указатель мыши находится над объектом UI.
        return results.Count > 0;
    }

    /// <summary>
    /// Проверяет, находится ли указатель мыши над конкретным объектом UI.
    /// </summary>
    /// <param name="targetUI">цель</param>
    /// <returns>находится ли указатель мыши над объектом UI</returns>
    public static bool IsPointerOverUI(GameObject targetUI)
    {
        // Создаем экземпляр PointerEventData с текущим положением указателя мыши.
        PointerEventData eventDataCurrentPosition = new(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };

        List<RaycastResult> results = new();

        // Выполняем лучевой кастинг для всех объектов в текущей позиции указателя мыши.
        // Результаты сохраняются в списке results.
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        foreach (RaycastResult result in results)
        {
            GameObject objectUI = result.module.gameObject;
            if (objectUI == targetUI)
                return true;
        }
        // Если количество результатов больше нуля, значит указатель мыши находится над объектом UI.
        return false;
    }

    /// <summary>
    /// Нарастить границы массиву заполнив значением value
    /// </summary>
    /// <param name="arr">массив</param>
    /// <param name="value">значение</param>
    /// <returns>массив с наращенными границами</returns>
    public static bool[,] AddBorders(this bool[,] arr, bool value = true)
    {
        bool[,] newArr = new bool[arr.GetLength(0) + 2, arr.GetLength(1) + 2];
        for (int x = 0; x < newArr.GetLength(0); x++)
        {
            newArr[x, 0] = value;
            newArr[x, newArr.GetLength(0) - 1] = value;
        }

        for (int y = 0; y < newArr.GetLength(1); y++)
        {
            newArr[0, y] = value;
            newArr[newArr.GetLength(1) - 1, y] = value;
        }

        for (int x = 0; x < arr.GetLength(0); x++)
        {
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                newArr[x + 1, y + 1] = arr[x, y];
            }
        }

        return newArr;
    }

    /// <summary>
    /// Таймер
    /// </summary>
    /// <param name="timer">значение таймера</param>
    /// <returns>значение таймера равно нулю и не изменилось?</returns>
    public static bool Timer(this ref float timer)
    {
        if (timer == 0)
            return true;

        timer -= Time.fixedDeltaTime;
        if (timer < 0)
            timer = 0;
        return false;
    }

    /// <summary>
    /// Попробовать получить значение другого типа
    /// </summary>
    /// <typeparam name="T">другой тип</typeparam>
    /// <param name="obj">исходное значение</param>
    /// <param name="valueOtherType">значение другого типа</param>
    /// <returns>Получилось ли получить значение другого типа</returns>
    public static bool TryGetValueOtherType<T>(this object obj, out T valueOtherType)
    {
        if (obj.GetType() == typeof(T))
        {
            valueOtherType = (T)obj;
            return true;
        }

        valueOtherType = default;
        return false;
    }

    public static DateTimeOffset ToDateTimeOffset(this string text)
    {
        string[] date = text.Split(' ')[0].Split('.');
        string[] clock = text.Split(' ')[1].Split(':');
        string[] offset = text.Split(' ')[2].Split(':');
        return new DateTimeOffset(
            Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]),
            Convert.ToInt32(clock[0]), Convert.ToInt32(clock[1]), Convert.ToInt32(clock[2]),
            new TimeSpan(Convert.ToInt32(offset[0]), Convert.ToInt32(offset[1]), 0));
    }

    /// <summary>
    /// Ищет min x, min y, max x, max y
    /// </summary>
    /// <param name="points">точки</param>
    /// <returns>min x, min y, max x, max y</returns>
    public static (float, float, float, float) MinMax(this Vector2[] points)
    {
        Vector2 minPoint = new()
        { x = points[0].x, y = points[0].y };
        Vector2 maxPoint = new()
        { x = points[0].x, y = points[0].y };
        for (int id = 0; id < points.Length; id++)
        {
            if (points[id].x < minPoint.x)
                minPoint.x = points[id].x;
            if (points[id].y < minPoint.y)
                minPoint.y = points[id].y;

            if (points[id].x > maxPoint.x)
                maxPoint.x = points[id].x;
            if (points[id].y > maxPoint.y)
                maxPoint.y = points[id].y;
        }

        return (minPoint.x, minPoint.y, maxPoint.x, maxPoint.y);
    }

    #endregion Methods
}
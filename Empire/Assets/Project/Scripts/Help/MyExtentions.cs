using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

public static class MyExtentions
{
    #region Methods

    public static Sprite ToSprite(this Texture2D tex) => Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);

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
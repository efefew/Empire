using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class PointsAB : MonoBehaviour
{
    #region Events

    public event Action<Transform, Transform> SetPositionsHandler;

    #endregion Events

    #region Properties

    public bool groupAB { get; private set; }
    public ToggleGroup conteinerToggle { private get; set; }
    public PointsAB parentAB { get; set; }
    public List<PointsAB> childrensAB { get; set; } = new List<PointsAB>();

    #endregion Properties

    #region Fields

    private Vector3 screenPosition, worldPosition;
    public Transform a, b;

    #endregion Fields

    #region Methods

    private void OnGUI()
    {
        // Получаем основную камеру
        Camera c = Camera.main;

        // Получаем текущее событие мыши
        Event e = Event.current;

        // Получаем позицию мыши на экране
        Vector2 mousePos = new()
        {
            x = e.mousePosition.x,
            y = c.pixelHeight - e.mousePosition.y
        };

        // Создаем вектор 3D-позиции на экране
        // с учетом ближней плоскости отсечения
        screenPosition = new Vector3(mousePos.x, mousePos.y, c.nearClipPlane);

        // Преобразуем 2D-координаты на экране в 3D-координаты в мировом пространстве
        worldPosition = c.ScreenToWorldPoint(screenPosition);
    }

    private void Update()
    {
        if (parentAB && parentAB.groupAB)
            return;
        if (Input.GetKeyDown(KeyCode.Mouse0) && !MyExtentions.IsPointerOverUI())
        {
            SetPositionA(worldPosition);
        }

        if (Input.GetKey/*Up*/(KeyCode.Mouse0) && !MyExtentions.IsPointerOverUI())
        {
            SetPositionB(worldPosition);
        }
    }

    private void SetGroupPoints()
    {
        if (!groupAB)
            return;
        int countActiveAB = childrensAB.Where((PointsAB points) => { return points.enabled; }).Count();
        float distance = Mathf.Max(0, (Vector2.Distance(a.position, b.position) - (Army.OFFSET_BETWEEN_ARMIES * (countActiveAB - 1))) / countActiveAB);
        Vector3 position = a.position;
        for (int id = 0; id < childrensAB.Count; id++)
        {
            if (!childrensAB[id].enabled)
                continue;
            childrensAB[id].SetPositionA(position);
            position += a.right * distance;
            childrensAB[id].SetPositionB(position);
            position += a.right * Army.OFFSET_BETWEEN_ARMIES;
        }
    }

    public void SetActive(bool on)
    {
        a.gameObject.SetActive(on);
        b.gameObject.SetActive(on);
        enabled = on;
    }

    public void SetPositionA(Vector2 vector) => a.position = vector;

    public void SetPositionB(Vector2 vector)
    {
        b.position = vector;
        a.LookAt2D(b.position);
        SetPositionsHandler?.Invoke(a, b);

        SetGroupPoints();
    }

    public void SwitchGroup(bool on)
    {
        groupAB = on;
        conteinerToggle.enabled = !on;
        SetActive(on);
    }

    #endregion Methods
}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class PointsAB : MonoBehaviour
{
    #region Fields

    private const float MIN_DISTANCE_AB = 1f;

    #endregion Fields

    #region Events

    public event Action<Transform, Transform> OnChangePositions, OnChangedPositions;

    #endregion Events

    #region Properties

    public bool groupAB { get; private set; }
    public ToggleGroup conteinerToggle { private get; set; }
    public PointsAB parentAB { get; set; }
    public List<PointsAB> childrensAB { get; set; } = new();

    #endregion Properties

    #region Fields

    public Battlefield battlefield;
    public Transform a, b;
    private bool wasOverUI;

    #endregion Fields

    #region Methods

    private void Start()
    {
        if (Battlefield.Singleton != null) battlefield = Battlefield.Singleton;
    }

    private void Update()
    {
        if (parentAB && parentAB.groupAB) return;

        wasOverUI = MyExtentions.IsPointerOverUI();
        ChangePointsAB(KeyCode.Mouse0);
    }

    private void ChangePointsAB(KeyCode key)
    {
        if (wasOverUI) return;

        if (Input.GetKeyDown(key)) ChangePositionA(battlefield.WorldPosition);

        if (Input.GetKey(key)) ChangePositionB(battlefield.WorldPosition);

        if (Input.GetKeyUp(key))
        {
            battlefield.DeactiveAllArmies();
            ChangedPositions();
        }
    }

    private void SetGroupPoints()
    {
        if (!groupAB) return;

        int countActiveAB = childrensAB.Where(points => { return points.enabled; }).Count();
        float distance = Mathf.Max(0,
            (Vector2.Distance(a.position, b.position) - Army.OFFSET_BETWEEN_ARMIES * (countActiveAB - 1)) /
            countActiveAB);
        Vector3 position = a.position;
        for (int id = 0; id < childrensAB.Count; id++)
        {
            if (!childrensAB[id].enabled) continue;

            childrensAB[id].ChangePositionA(position);
            position += a.right * distance;
            childrensAB[id].ChangePositionB(position);
            position += a.right * Army.OFFSET_BETWEEN_ARMIES;
        }
    }

    public void SetActive(bool on)
    {
        a.gameObject.SetActive(on);
        b.gameObject.SetActive(on);
        enabled = on;
    }

    public void ChangePositionA(Vector2 vector)
    {
        a.position = vector;
    }

    public void ChangePositionB(Vector2 vector, bool firstCall = false)
    {
        b.position = vector;
        a.LookAt2D(b.position);
        if (firstCall) OnChangedPositions?.Invoke(a, b);

        if (Vector2.Distance(a.position, b.position) >= MIN_DISTANCE_AB) OnChangePositions?.Invoke(a, b);

        SetGroupPoints();
    }

    public void ChangedPositions()
    {
        if (Vector2.Distance(a.position, b.position) >= MIN_DISTANCE_AB) OnChangedPositions?.Invoke(a, b);

        if (!groupAB) return;

        for (int id = 0; id < childrensAB.Count; id++) childrensAB[id].ChangedPositions();
    }

    public void Group(bool on)
    {
        groupAB = on;
        conteinerToggle.enabled = !on;
        SetActive(on);
    }

    #endregion Methods
}
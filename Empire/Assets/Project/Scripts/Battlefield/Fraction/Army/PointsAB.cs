#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

public class PointsAb : MonoBehaviour
{
    private const float MIN_DISTANCE_AB = 1f;
    public event Action<Transform, Transform> OnChangePositions, OnChangedPositions;
    private bool GroupAb { get; set; }
    public ToggleGroup ContainerToggle { private get; set; }
    public PointsAb ParentAb { get; set; }
    public List<PointsAb> ChildrenAb { get; set; } = new();

    [FormerlySerializedAs("battlefield")] public Battlefield Battlefield;
    [FormerlySerializedAs("a")] public Transform A;
    [FormerlySerializedAs("b")] public Transform B;
    private bool _wasOverUI;


    private void Start()
    {
        if (Battlefield.Instance != null) Battlefield = Battlefield.Instance;
    }

    private void Update()
    {
        if (ParentAb && ParentAb.GroupAb) return;

        _wasOverUI = MyExtentions.IsPointerOverUI();
        ChangePointsAb(KeyCode.Mouse0);
    }

    private void ChangePointsAb(KeyCode key)
    {
        if (_wasOverUI) return;

        if (Input.GetKeyDown(key)) ChangePositionA(Battlefield.WorldPosition);

        if (Input.GetKey(key)) ChangePositionB(Battlefield.WorldPosition);

        if (Input.GetKeyUp(key))
        {
            Battlefield.DeactiveAllArmies();
            ChangedPositions();
        }
    }

    private void SetGroupPoints()
    {
        if (!GroupAb) return;

        int countActiveAb = ChildrenAb.Count(points => points.enabled);
        float distance = Mathf.Max(0,
            (Vector2.Distance(A.position, B.position) - Army.OFFSET_BETWEEN_ARMIES * (countActiveAb - 1)) /
            countActiveAb);
        Vector3 position = A.position;
        for (int id = 0; id < ChildrenAb.Count; id++)
        {
            if (!ChildrenAb[id].enabled) continue;

            ChildrenAb[id].ChangePositionA(position);
            position += A.right * distance;
            ChildrenAb[id].ChangePositionB(position);
            position += A.right * Army.OFFSET_BETWEEN_ARMIES;
        }
    }

    public void SetActive(bool on)
    {
        A.gameObject.SetActive(on);
        B.gameObject.SetActive(on);
        enabled = on;
    }

    public void ChangePositionA(Vector2 vector)
    {
        A.position = vector;
    }

    public void ChangePositionB(Vector2 vector, bool firstCall = false)
    {
        B.position = vector;
        A.LookAt2D(B.position);
        if (firstCall) OnChangedPositions?.Invoke(A, B);

        if (Vector2.Distance(A.position, B.position) >= MIN_DISTANCE_AB) OnChangePositions?.Invoke(A, B);

        SetGroupPoints();
    }

    public void ChangedPositions()
    {
        if (Vector2.Distance(A.position, B.position) >= MIN_DISTANCE_AB) OnChangedPositions?.Invoke(A, B);

        if (!GroupAb) return;

        for (int id = 0; id < ChildrenAb.Count; id++) ChildrenAb[id].ChangedPositions();
    }

    public void Group(bool on)
    {
        GroupAb = on;
        ContainerToggle.enabled = !on;
        SetActive(on);
    }

}
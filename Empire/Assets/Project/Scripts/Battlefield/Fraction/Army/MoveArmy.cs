#region

using UnityEngine;

#endregion

[RequireComponent(typeof(PointsAb))]
public partial class Army : MonoBehaviour // ����������� �����
{
    #region Fields

    public int TargetButtonPersonId
    {
        get
        {
            if (targetButtonPersonId >= Persons.Count)
            {
                MovePoints(anchors.A, anchors.B);
                TargetButtonPersonId = newTargetButtonPersonId;
            }

            return targetButtonPersonId;
        }
        private set => targetButtonPersonId = value;
    }

    private const float MIN_DISTANCE = 1f, FIRST_MIN_DISTANCE = 5f;
    private bool firstMinDistance;
    private int targetButtonPersonId;
    private int newTargetButtonPersonId;
    public PointsAb anchors;
    public float offsetX, offsetY;

    #endregion Fields

    #region Methods

    private void Update()
    {
        if (Persons.Count == 0)
            return;
        if (TargetButtonPersonId >= Persons.Count)
        {
            MovePoints(anchors.A, anchors.B);
            TargetButtonPersonId = newTargetButtonPersonId;
        }

        if (ButtonArmy.gameObject.activeInHierarchy)
            ButtonArmy.transform.position = Persons[TargetButtonPersonId].transform.position;
        if (!status.Fraction.Bot)
            ArmyGlobalUI.transform.position = Persons[TargetButtonPersonId].transform.position;
    }

    private void MovePoints(Transform a, Transform b)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || status.Fraction.Bot)
            firstMinDistance = true;
        float distance = Vector2.Distance(a.position, b.position);
        if (distance < (firstMinDistance ? FIRST_MIN_DISTANCE : MIN_DISTANCE))
            return;

        int countX = (int)(distance / offsetX + 1), x = 0, y = 0;
        for (int id = 0; id < Persons.Count; id++)
        {
            Persons[id].Target.position = a.position - a.up * offsetY * y + a.right * offsetX * x;
            x++;
            if (x == countX)
            {
                y++;
                x = 0;
            }
        }

        int widhArmy = Persons.Count < countX ? Persons.Count : countX;
        int heightArmy = x == 0 ? y : y - 1;
        newTargetButtonPersonId = widhArmy * (heightArmy / 2) + widhArmy / 2;
        firstMinDistance = false;
    }

    private void MoveArmy(Transform a, Transform b)
    {
        for (int id = 0; id < Persons.Count; id++)
            Persons[id].MoveUpdate();
    }

    #endregion Methods
}
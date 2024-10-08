using UnityEngine;

[RequireComponent(typeof(PointsAB))]
public partial class Army : MonoBehaviour// Мобильность армии
{
    #region Fields

    private const float MIN_DISTANCE = 1f, FIRST_MIN_DISTANCE = 5f;
    private bool firstMinDistance;
    public PointsAB anchors;
    public float offsetX, offsetY;
    private int targetButtonPersonId;
    public int TargetButtonPersonId
    {
        get
        {
            if (targetButtonPersonId >= persons.Count)
            {
                MovePoints(anchors.a, anchors.b);
                TargetButtonPersonId = newTargetButtonPersonId;
            }

            return targetButtonPersonId;
        }
        private set => targetButtonPersonId = value;
    }
    private int newTargetButtonPersonId;
    #endregion Fields

    #region Methods

    private void Update()
    {

        if (persons.Count == 0)
            return;
        if (TargetButtonPersonId >= persons.Count)
        {
            MovePoints(anchors.a, anchors.b);
            TargetButtonPersonId = newTargetButtonPersonId;
        }

        if (buttonArmy.gameObject.activeInHierarchy)
            buttonArmy.transform.position = persons[TargetButtonPersonId].transform.position;
        if (!status.fraction.bot)
            armyGlobalUI.transform.position = persons[TargetButtonPersonId].transform.position;
    }
    private void MovePoints(Transform a, Transform b)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || status.fraction.bot)
            firstMinDistance = true;
        float distance = Vector2.Distance(a.position, b.position);
        if (distance < (firstMinDistance ? FIRST_MIN_DISTANCE : MIN_DISTANCE))
            return;

        int countX = (int)((distance / offsetX) + 1), x = 0, y = 0;
        for (int id = 0; id < persons.Count; id++)
        {
            persons[id].target.position = a.position - (a.up * offsetY * y) + (a.right * offsetX * x);
            x++;
            if (x == countX)
            {
                y++;
                x = 0;
            }
        }

        int widhArmy = persons.Count < countX ? persons.Count : countX;
        int heightArmy = x == 0 ? y : y - 1;
        newTargetButtonPersonId = (widhArmy * (heightArmy / 2)) + (widhArmy / 2);
        firstMinDistance = false;
    }
    private void MoveArmy(Transform a, Transform b)
    {
        for (int id = 0; id < persons.Count; id++)
            persons[id].MoveUpdate();
    }
    #endregion Methods
}
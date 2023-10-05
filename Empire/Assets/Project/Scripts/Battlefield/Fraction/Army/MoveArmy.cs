using UnityEngine;

[RequireComponent(typeof(PointsAB))]
public partial class Army : MonoBehaviour// Мобильность армии
{
    #region Fields

    private const float MIN_DISTANCE = 1f, FIRST_MIN_DISTANCE = 5f;
    private bool firstMinDistance;
    public PointsAB anchors;
    public float offsetX, offsetY;
    #endregion Fields

    #region Methods

    private void Update()
    {

        if (persons.Count == 0)
            return;
        if (buttonArmy.gameObject.activeInHierarchy)
            buttonArmy.transform.position = persons[persons.Count / 2].transform.position;
        if (!status.fraction.bot)
            armyGlobalUI.transform.position = persons[persons.Count / 2].transform.position;
    }
    private void MovePoints(Transform a, Transform b)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || status.fraction.bot)
            firstMinDistance = true;
        float distance = Vector2.Distance(a.position, b.position);
        if (distance < (firstMinDistance ? FIRST_MIN_DISTANCE : MIN_DISTANCE))
            return;

        int countX = (int)((distance / offsetX) + 1);
        int x = 0, y = 0;
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

        firstMinDistance = false;
    }
    private void MoveArmy(Transform a, Transform b)
    {
        for (int id = 0; id < persons.Count; id++)
            persons[id].MoveUpdate();
    }
    #endregion Methods
}
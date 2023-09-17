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
    private void MoveArmy(Transform a, Transform b)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
            firstMinDistance = true;
        float distance = Vector2.Distance(a.position, b.position);
        if (distance < (firstMinDistance ? FIRST_MIN_DISTANCE : MIN_DISTANCE))
        {
            for (int id = 0; id < persons.Count; id++)
                persons[id].MoveUpdate();

            return;
        }

        Transform target;
        int countX = (int)((distance / offsetX) + 1);
        int x = 0, y = 0;
        for (int id = 0; id < persons.Count; id++)
        {
            Person person = persons[id];
            target = person.target;
            target.position = a.position - (a.up * offsetY * y) + (a.right * offsetX * x);
            x++;
            if (x == countX)
            {
                y++;
                x = 0;
            }

            person.MoveUpdate();
        }

        firstMinDistance = false;
    }

    #endregion Methods
}
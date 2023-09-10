using UnityEngine;

[RequireComponent(typeof(PointsAB))]
public partial class Army : MonoBehaviour// Мобильность армии
{
    #region Fields

    private const float minDistance = 1f;
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

    private void SetTargetArmy(Transform a, Transform b)
    {
        float distance = Vector2.Distance(a.position, b.position);
        if (distance < minDistance)
            return;
        Transform target;
        int countX = (int)((distance / offsetX) + 1);
        int x = 0, y = 0;
        foreach (Person warrior in persons)
        {
            target = warrior.target;
            target.position = a.position - (a.up * offsetY * y) + (a.right * offsetX * x);
            x++;
            if (x == countX)
            {
                y++;
                x = 0;
            }
        }
    }

    #endregion Methods
}
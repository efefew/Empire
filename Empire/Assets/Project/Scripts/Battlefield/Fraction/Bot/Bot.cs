#region

using System.Collections;
using UnityEngine;

#endregion

[RequireComponent(typeof(FractionBattlefield))]
public class Bot : MonoBehaviour
{
    private FractionBattlefield myFraction;
    private PointsAB pointsAB;

    private void Awake()
    {
        myFraction = GetComponent<FractionBattlefield>();
        pointsAB = GetComponent<PointsAB>();
    }

    private void Start()
    {
        MoveArmy(myFraction.armies[2], new Vector2(3, 3), new Vector2(5, 8));
    }

    private IEnumerator IMoveArmy(Army army, Vector2 a, Vector2 b)
    {
        yield return new WaitUntil
        (() =>
        {
            foreach (Person person in army.persons)
                if (!person.agentMove.agent.isOnNavMesh)
                    return false;

            return true;
        });

        if (!myFraction.armies.Contains(army))
            yield break;
        army.anchors.ChangePositionA(a);
        army.anchors.ChangePositionB(b);
        army.anchors.ChangedPositions();
    }

    private void MoveArmy(Army army, Vector2 a, Vector2 b)
    {
        StartCoroutine(IMoveArmy(army, a, b));
    }
}
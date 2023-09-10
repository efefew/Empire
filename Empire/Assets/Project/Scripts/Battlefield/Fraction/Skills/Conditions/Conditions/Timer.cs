using System.Collections;

using UnityEngine;

public class Timer : Condition
{
    [Min(0)]
    public float time;
    public override IEnumerator GetConditionEnumerator() => IGetCondition();
    private IEnumerator IGetCondition()
    {
        yield return new WaitForSeconds(time);
    }
}

using System.Collections;

using UnityEngine;

/// <summary>
/// Замедление
/// </summary>
public class Slowdown : Buff
{
    [Min(0)]
    public float scaleSlowdown;
    public override void AddBuff(Person caster, Person target)
    {
        base.AddBuff(caster, target);
        _ = StartCoroutine(IBuff(caster, target));
    }
    private IEnumerator IBuff(Person caster, Person target)
    {

        if (conditionOfAction.conditionFunction)
        {
            if (target)
                target.speedScale -= scaleSlowdown;
            yield return new WaitUntil(conditionOfAction.GetConditionFunction());
            if (target)
                target.speedScale += scaleSlowdown;
        }
        else
        {
            if (target)
                target.speedScale -= scaleSlowdown;
            yield return StartCoroutine(conditionOfAction.GetConditionEnumerator());
            if (target)
                target.speedScale += scaleSlowdown;
        }

        yield return null;
        RemoveBuff();
    }
}

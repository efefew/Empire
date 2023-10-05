using UnityEngine;
[AddComponentMenu("Buff/Speed")]
[RequireComponent(typeof(Condition))]
/// <summary>
/// Изменение скорости
/// </summary>
public class Speed : Buff
{
    #region Fields

    [Min(0.01f)]
    public float scaleSlowdown;
    #endregion Fields

    #region Methods

    protected override void EndBuff(object[] parameters)
    {
        base.EndBuff(parameters);
        Person caster = parameters[0] as Person;
        Person target = parameters[1] as Person;
        if (!target || scaleSlowdown == 0)
            return;
        target.speedScale /= scaleSlowdown;
        target.MoveUpdate();
    }

    protected override void StartBuff(object[] parameters)
    {
        base.StartBuff(parameters);
        Person caster = parameters[0] as Person;
        Person target = parameters[1] as Person;
        if (!target || scaleSlowdown == 0)
            return;
        target.speedScale *= scaleSlowdown;
        target.MoveUpdate();
    }

    #endregion Methods
}
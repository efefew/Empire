using UnityEngine;

/// <summary>
/// Наложенный временный эффект
/// </summary>
public abstract class Buff : MonoBehaviour
{
    #region Enums

    /// <summary>
    /// Ограничение суммирования эффектов
    /// </summary>
    public enum BuffStackType
    {
        OneFraction,
        OneArmy,
        OneCaster,
        None
    }

    #endregion Enums

    #region Fields

    private Person caster, target;

    /// <summary>
    /// условие действия эффекта
    /// </summary>
    public Condition conditionOfAction;
    [SerializeField]
    public BuffStackType buffStack = BuffStackType.None;

    #endregion Fields

    #region Methods

    public virtual void AddBuff(Person caster, Person target)
    {
        this.caster = caster;
        this.target = target;
        target.buffs.Add(this);
    }

    public virtual void RemoveBuff()
    {
        _ = target.buffs.Remove(this);
        Destroy(this);
    }

    #endregion Methods
}
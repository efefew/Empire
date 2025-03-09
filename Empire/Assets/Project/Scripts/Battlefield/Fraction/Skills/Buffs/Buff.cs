#region

using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;

#endregion

/// <summary>
///     �������� ��� ����������
/// </summary>
public abstract class Buff : MonoBehaviour
{
    #region Enums

    /// <summary>
    ///     ����������� ������������ �������� �� ���������
    /// </summary>
    public enum SpecificityStackType
    {
        /// <summary>
        ///     ���� ����� �������� ��� ����������
        /// </summary>
        OneBuff,

        /// <summary>
        ///     ���������� ��� �������� ��� ����������
        /// </summary>
        OneThisBuff,
        None
    }

    /// <summary>
    ///     ����������� ������������ �������� �� �����������
    /// </summary>
    public enum InitiatorStackType
    {
        OneSide,
        OneFraction,
        OneArmy,
        OneCaster,
        None
    }

    #endregion Enums

    #region Fields

    [Header("�����������")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelOrange)]
    [SerializeField]
    public SpecificityStackType specificityStack = SpecificityStackType.None;

    [SerializeField] public InitiatorStackType initiatorStack = InitiatorStackType.None;

    [EndColumnArea]
    /// <summary>
    /// ������� �������� �������
    /// </summary>
    public Condition condition;

    #endregion Fields

    #region Methods

    protected virtual void StartBuff(object[] parameters)
    {
        Person target = parameters[1] as Person;
        target.buffs.Add(this);
    }

    protected virtual void EndBuff(object[] parameters)
    {
        Person target = parameters[1] as Person;
        _ = target.buffs.Remove(this);
    }

    public virtual void Run(Person caster, Person target)
    {
        if (!LimitRun(caster, target))
            return;
        caster.temporaryBuff.Do(condition, StartBuff, EndBuff, new object[2] { caster, target });
    }

    protected virtual bool LimitRun(Person initiator, Person target)
    {
        switch (specificityStack)
        {
            case SpecificityStackType.OneBuff:
                if (target.buffs.Any(buff => buff.GetType() == GetType()))
                    return false;
                break;
            case SpecificityStackType.OneThisBuff:
                if (target.buffs.Contains(this))
                    return false;
                break;
            case SpecificityStackType.None:
                break;
        }

        switch (initiatorStack)
        {
            case InitiatorStackType.OneSide:
                break;
            case InitiatorStackType.OneFraction:
                break;
            case InitiatorStackType.OneArmy:
                break;
            case InitiatorStackType.OneCaster:
                break;
            case InitiatorStackType.None:
                break;
        }

        return true;
    }

    #endregion Methods
}
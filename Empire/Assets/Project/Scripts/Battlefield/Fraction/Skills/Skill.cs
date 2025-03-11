#region

using System;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

/// <summary>
///     �����
/// </summary>
public abstract class Skill : MonoBehaviour
{
    public enum TriggerType
    {
        Enemy,
        Self,
        Friend,
        SelfAndFriend,
        EnemyAndFriend,
        SelfAndEnemy,
        All
    }

    public enum SkillType
    {
        Attack,
        Defend,
        Move,
        Heal,
        Buff,
        Debuff,
    }
    [FormerlySerializedAs("buttonSkillPrefab")] public ButtonSkill ButtonSkillPrefab;

    [FormerlySerializedAs("timeCooldown")]
    [Header("�����")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.Bevel)]
    [Min(0)]
    [Tooltip("����� �����������")]
    public float TimeCooldown;

    /// <summary>
    ///     ����� �������������
    /// </summary>
    [FormerlySerializedAs("timeTargetMove")] [Min(0)] [Tooltip("����� �������������")]
    public float TimeTargetMove;

    /// <summary>
    ///     ����� ������ ������
    /// </summary>
    [FormerlySerializedAs("timeCast")] [Min(0)] [Tooltip("����� ������ ������")]
    public float TimeCast;

    [FormerlySerializedAs("mana")]
    [EndColumnArea]
    [Header("������� ��� ������������� ������")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelGreen)]
    [Min(0)]
    public float Mana;

    [FormerlySerializedAs("stamina")] [Min(0)] public float Stamina;

    [FormerlySerializedAs("range")] [Min(0)] public float Range;

    [FormerlySerializedAs("maxAmountSkill")] [Min(0)] public float MaxAmountSkill;

    [FormerlySerializedAs("pointCanBeTarget")] public bool PointCanBeTarget;
    [FormerlySerializedAs("�anBePatrol")] public bool �anBePatrol;
    [FormerlySerializedAs("triggerTarget")] public TriggerType TriggerTarget;

    [FormerlySerializedAs("consumable")]
    [EndColumnArea]
    [Header("�������� ��������� ������")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelBlue)]
    public bool Consumable;

    /// <summary>
    ///     ������������ ��� ���������
    /// </summary>
    [FormerlySerializedAs("targetMove")] public bool TargetMove;

    /// <summary>
    ///     ������������ �����
    /// </summary>
    [FormerlySerializedAs("collective")] public bool Collective;

    [FormerlySerializedAs("maxCountCatch")] [Min(1)] public int MaxCountCatch;

    [FormerlySerializedAs("triggerDanger")] [SerializeField] public TriggerType TriggerDanger;
    [SerializeField] public SkillType Type;

    [FormerlySerializedAs("buffs")] public Buff[] Buffs;
    [FormerlySerializedAs("effects")] public Effect[] Effects;

    [FormerlySerializedAs("nameAnimation")] [EndColumnArea] public string NameAnimation;

    private const float LIMIT_CLOSE_RANGE = 0.7f;

    #region Methods

    public static bool OnTrigger(TriggerType trigger, Person initiator, Person target)
    {
        return trigger switch
        {
            TriggerType.Enemy => IsEnemy(initiator, target),
            TriggerType.Self => IsMe(initiator, target),
            TriggerType.Friend => IsFriend(initiator, target),
            TriggerType.SelfAndFriend => !IsEnemy(initiator, target),
            TriggerType.EnemyAndFriend => !IsMe(initiator, target),
            TriggerType.SelfAndEnemy => !IsFriend(initiator, target),
            TriggerType.All => true,
            _ => false
        };
    }

    public static bool OnTrigger(TriggerType trigger, Army initiator, Army target)
    {
        return trigger switch
        {
            TriggerType.Enemy => IsEnemy(initiator, target),
            TriggerType.Self => IsMe(initiator, target),
            TriggerType.Friend => IsFriend(initiator, target),
            TriggerType.SelfAndFriend => !IsEnemy(initiator, target),
            TriggerType.EnemyAndFriend => !IsMe(initiator, target),
            TriggerType.SelfAndEnemy => !IsFriend(initiator, target),
            TriggerType.All => true,
            _ => false
        };
    }

    private static bool IsMe(Person initiator, Person target)
    {
        return initiator == target;
    }

    private static bool IsEnemy(Person initiator, Person target)
    {
        return target.Status.SideID != initiator.Status.SideID;
    }

    private static bool IsFriend(Person initiator, Person target)
    {
        return target.Status.SideID == initiator.Status.SideID && target != initiator;
    }

    private static bool IsMe(Army initiator, Army target)
    {
        return initiator == target;
    }

    private static bool IsEnemy(Army initiator, Army target)
    {
        return target.status.SideID != initiator.status.SideID;
    }

    private static bool IsFriend(Army initiator, Army target)
    {
        return target.status.SideID == initiator.status.SideID && target != initiator;
    }

    public static (bool, bool, bool) GetTrigger(TriggerType trigger)
    {
        bool enemy = false;
        bool self = false;
        bool friend = false;

        switch (trigger)
        {
            case TriggerType.Enemy:
                enemy = true;
                break;

            case TriggerType.Self:
                self = true;
                break;

            case TriggerType.Friend:
                friend = true;
                break;

            case TriggerType.SelfAndFriend:
                self = true;
                friend = true;
                break;

            case TriggerType.EnemyAndFriend:
                enemy = true;
                friend = true;
                break;

            case TriggerType.SelfAndEnemy:
                self = true;
                enemy = true;
                break;

            case TriggerType.All:
                enemy = true;
                self = true;
                friend = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null);
        }

        return (enemy, self, friend);
    }

    public void SetEffectsAndBuffs(Person initiator, Person target)
    {
        foreach (Effect effect in Effects) effect.Run(initiator, target, this);

        foreach (Buff buff in Buffs) buff.Run(initiator, target);
    }

    /// <summary>
    ///     ���������� ������
    /// </summary>
    /// <param name="initiator">����������� �����</param>
    /// <param name="target">���� ������</param>
    public abstract void Run(Person initiator, Person target = null);

    /// <summary>
    ///     ���������� ������
    /// </summary>
    /// <param name="initiator">����������� �����</param>
    /// <param name="target">���� ������</param>
    public abstract void Run(Person initiator, Vector3 target);

    /// <summary>
    ///     ��������� ����������� ���������� ������
    /// </summary>
    /// <param name="initiator">����������� �����</param>
    /// <param name="target">���� ������</param>
    public virtual bool LimitRun(Person initiator, Vector3 target)
    {
        if (!initiator)
            return false;
        // ���������, ����� �� �������� ������������ ��� ������
        if ((!Consumable || !(initiator.amountSkill[this] - 1 < 0)) && initiator.CanUseSkill(this))
            return LimitRangeRun(initiator, target);
        initiator.RemoveStateAnimation(NameAnimation);
        return false;

    }

    /// <summary>
    ///     ���������, ����� �� �������� ���������� �� ����� ���� �������
    /// </summary>
    /// <param name="initiator">��������</param>
    /// <param name="target">�����</param>
    /// <param name="close"></param>
    /// <returns></returns>
    public virtual bool LimitRangeRun(Person initiator, Vector3 target, bool close = false)
    {
        float distance = Vector2.Distance(initiator.transform.position, target);
        if ((!(distance > Range * (close ? LIMIT_CLOSE_RANGE : 1)) || Range == 0)) return true;
        initiator.RemoveStateAnimation(NameAnimation);
        return false;

    }

    #endregion Methods
}
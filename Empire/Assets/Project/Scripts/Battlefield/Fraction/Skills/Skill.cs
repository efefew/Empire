using AdvancedEditorTools.Attributes;

using UnityEngine;

/// <summary>
/// �����
/// </summary>
public abstract class Skill : MonoBehaviour
{
    #region Enums

    public enum TriggerType
    {
        enemy,
        self,
        friend,
        selfAndFriend,
        enemyAndFriend,
        selfAndEnemy,
        all
    }

    #endregion Enums

    ///// <summary>
    ///// ����������� �� ���������������� ������������ � ��������� ��������
    ///// </summary>
    //public enum ZoneType
    //{
    //    Fraction,
    //    Army,
    //    Person
    //}

    #region Fields

    public ButtonSkill buttonSkillPrefab;

    [Header("�����")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.Bevel)]
    /// <summary>
    /// ����� �����������
    /// </summary>
    [Min(0)]
    [Tooltip("����� �����������")]
    public float timeCooldown;

    /// <summary>
    /// ����� �������������
    /// </summary>
    [Min(0)]
    [Tooltip("����� �������������")]
    public float timeTargetMove;

    /// <summary>
    /// ����� ������ ������
    /// </summary>
    [Min(0)]
    [Tooltip("����� ������ ������")]
    public float timeCast;

    [EndColumnArea]
    [Header("������� ��� ������������� ������")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelGreen)]
    [Min(0)]
    public float mana;

    [Min(0)]
    public float stamina;

    [Min(0)]
    public float range;

    [Min(0)]
    public float maxAmountSkill;
    public bool pointCanBeTarget;
    public bool �anBePatrol;
    public TriggerType triggerTarget;

    [EndColumnArea]
    [Header("�������� ��������� ������")]
    [BeginColumnArea(areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelBlue)]
    public bool consumable;

    /// <summary>
    /// ������������ ������������
    /// </summary>
    public bool targetMove;

    /// <summary>
    /// ������������ �����
    /// </summary>
    public bool collective;

    [Min(1)]
    public int maxCountCatch;

    [SerializeField]
    public TriggerType triggerDanger;

    public Buff[] buffs;
    public Effect[] effects;

    [EndColumnArea]
    public string nameAnimation;
    public const float LIMIT_CLOSE_RANGE = 0.7f;
    #endregion Fields

    #region Methods

    public static bool OnTrigger(TriggerType trigger, Person initiator, Person target)
    {
        return trigger switch
        {
            TriggerType.enemy => IsEnemy(initiator, target),
            TriggerType.self => IsMe(initiator, target),
            TriggerType.friend => IsFriend(initiator, target),
            TriggerType.selfAndFriend => !IsEnemy(initiator, target),
            TriggerType.enemyAndFriend => !IsMe(initiator, target),
            TriggerType.selfAndEnemy => !IsFriend(initiator, target),
            TriggerType.all => true,
            _ => false,
        };
    }

    public static bool OnTrigger(TriggerType trigger, Army initiator, Army target)
    {
        return trigger switch
        {
            TriggerType.enemy => IsEnemy(initiator, target),
            TriggerType.self => IsMe(initiator, target),
            TriggerType.friend => IsFriend(initiator, target),
            TriggerType.selfAndFriend => !IsEnemy(initiator, target),
            TriggerType.enemyAndFriend => !IsMe(initiator, target),
            TriggerType.selfAndEnemy => !IsFriend(initiator, target),
            TriggerType.all => true,
            _ => false,
        };
    }

    public static bool IsMe(Person initiator, Person target) => initiator == target;

    public static bool IsEnemy(Person initiator, Person target) => target.status.sideID != initiator.status.sideID;

    public static bool IsFriend(Person initiator, Person target) => target.status.sideID == initiator.status.sideID && target != initiator;

    public static bool IsMe(Army initiator, Army target) => initiator == target;

    public static bool IsEnemy(Army initiator, Army target) => target.status.sideID != initiator.status.sideID;

    public static bool IsFriend(Army initiator, Army target) => target.status.sideID == initiator.status.sideID && target != initiator;

    public static (bool, bool, bool) GetTrigger(TriggerType trigger)
    {
        bool enemy = false;
        bool self = false;
        bool friend = false;

        switch (trigger)
        {
            case TriggerType.enemy:
                enemy = true;
                break;

            case TriggerType.self:
                self = true;
                break;

            case TriggerType.friend:
                friend = true;
                break;

            case TriggerType.selfAndFriend:
                self = true;
                friend = true;
                break;

            case TriggerType.enemyAndFriend:
                enemy = true;
                friend = true;
                break;

            case TriggerType.selfAndEnemy:
                self = true;
                enemy = true;
                break;

            case TriggerType.all:
                enemy = true;
                self = true;
                friend = true;
                break;
        }

        return (enemy, self, friend);
    }

    public void SetEffectsAndBuffs(Person initiator, Person target)
    {
        foreach (Effect effect in effects)
        {
            effect.Run(initiator, target, this);
        }

        foreach (Buff buff in buffs)
        {
            buff.Run(initiator, target);
        }
    }

    /// <summary>
    /// ���������� ������
    /// </summary>
    /// <param name="initiator">����������� �����</param>
    /// <param name="target">���� ������</param>
    public abstract void Run(Person initiator, Person target = null);
    /// <summary>
    /// ���������� ������
    /// </summary>
    /// <param name="initiator">����������� �����</param>
    /// <param name="target">���� ������</param>
    public abstract void Run(Person initiator, Vector3 target);
    /// <summary>
    /// ��������� ����������� ���������� ������
    /// </summary>
    /// <param name="initiator">����������� �����</param>
    /// <param name="target">���� ������</param>
    public virtual bool LimitRun(Person initiator, Vector3 target)
    {
        if (initiator == null)
            return false;
        // ���������, ����� �� �������� ������������ ��� ������
        if ((consumable && (initiator.amountSkill[this] - 1) < 0) || !initiator.CanUseSkill(this))
        {
            initiator.RemoveStateAnimation(nameAnimation);
            return false;
        }

        return LimitRangeRun(initiator, target);
    }

    /// <summary>
    /// ���������, ����� �� �������� ���������� �� ����� ���� �������
    /// </summary>
    /// <param name="initiator">��������</param>
    /// <param name="target">�����</param>
    /// <returns></returns>
    public virtual bool LimitRangeRun(Person initiator, Vector3 target, bool close = false)
    {
        float distance = Vector2.Distance(initiator.transform.position, target);
        if (target == null || (distance > range * (close ? LIMIT_CLOSE_RANGE : 1) && range != 0))
        {
            initiator.RemoveStateAnimation(nameAnimation);
            return false;
        }

        return true;
    }

    #endregion Methods
}
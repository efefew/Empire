#region

using System;
using System.Collections;
using System.Collections.Generic;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

/// <summary>
///     ��������������  ������ �������
/// </summary>
public class Status : MonoBehaviour
{
    #region Fields

    public Action<Skill, Person[]> OnRepeatUseSkillOnPersons;
    public Action<Skill, Vector3> OnRepeatUseSkillOnPoint;

    [FormerlySerializedAs("fraction")] public FractionBattlefield Fraction;
    [FormerlySerializedAs("sideID")] public ulong SideID;

    [FormerlySerializedAs("maxHealth")]
    [Header("�������� �������� ����������")]
    [BeginColumnArea(0.5f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelGreen)]
    [Min(0)]
    public float MaxHealth = 100;

    [FormerlySerializedAs("maxMana")] [Min(0)] public float MaxMana;

    [FormerlySerializedAs("maxStamina")] [Min(0)] public float MaxStamina = 100;

    [FormerlySerializedAs("maxMorality")] [Min(0)] public float MaxMorality = 100;

    [FormerlySerializedAs("regenHealth")] [Header("������������� �������� ����������")] [NewColumn(0.5f)] [Min(0)]
    public float RegenHealth = 1;

    [FormerlySerializedAs("regenMana")] [Min(0)] public float RegenMana = 1;

    [FormerlySerializedAs("regenStamina")] [Min(0)] public float RegenStamina = 1;

    [FormerlySerializedAs("regenMorality")] [Min(0)] public float RegenMorality = 1;
    
    [EndColumnArea]
    [Header("�����")]
    [BeginColumnArea(1f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelRed)]
    public float CreteChance;

    [FormerlySerializedAs("crit")] [Min(1)] public float Crit = 1;

    [FormerlySerializedAs("scaleGiveDamage")] [SerializeField] public DamageTypeDictionary ScaleGiveDamage = new();

    [FormerlySerializedAs("scaleTakeDamage")]
    [EndColumnArea]
    [Header("������")]
    [BeginColumnArea(1f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelBlue)]
    [SerializeField]
    public DamageTypeDictionary ScaleTakeDamage = new();

    [FormerlySerializedAs("shield")] public DamageTypeDictionary Shield = new();

    [FormerlySerializedAs("skills")]
    [EndColumnArea]
    [Header("������")]
    [BeginColumnArea(1f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.Bevel)]
    public Skill[] Skills;

    [FormerlySerializedAs("melee")] public Melee Melee;

    [FormerlySerializedAs("maxSpeed")] [EndColumnArea] [Min(0)] public float MaxSpeed = 3.5f;

    public Dictionary<Skill, float> TimersSkillReload = new();
    public Skill waitCastSkill;

    [FormerlySerializedAs("timerSkillCast")] [HideInInspector] public float TimerSkillCast;

    #endregion Fields

    #region Methods

    private IEnumerator IWaitCastSkill(Skill skill, Func<bool> expirationCondition)
    {
        waitCastSkill = skill;
        yield return new WaitUntil(expirationCondition);
        waitCastSkill = null;
    }

    private IEnumerator ITimerSkillCast(Skill skill)
    {
        TimerSkillCast = skill.TimeCast;
        while (TimerSkillCast > 0)
        {
            yield return new WaitForFixedUpdate();
            TimerSkillCast -= Time.fixedDeltaTime;
        }

        TimerSkillCast = 0;
    }

    private IEnumerator ITimerSkillReload(Skill skill, Person target)
    {
        TimersSkillReload.Add(skill, skill.TimeCooldown);
        Army army = target?.Army;
        while (TimersSkillReload[skill] > 0)
        {
            yield return new WaitForFixedUpdate();
            TimersSkillReload[skill] -= Time.fixedDeltaTime;
        }

        _ = TimersSkillReload.Remove(skill);

        if (target != null || (army != null && army.Persons.Count != 0))
            OnRepeatUseSkillOnPersons?.Invoke(skill, army ? army.Persons.ToArray() : new Person[1] { target });
        //else
        //OnPatrol?.Invoke(skill);
    }

    private IEnumerator ITimerSkillReload(Skill skill, Vector3 target)
    {
        TimersSkillReload.Add(skill, skill.TimeCooldown);
        while (TimersSkillReload[skill] > 0)
        {
            yield return new WaitForFixedUpdate();
            TimersSkillReload[skill] -= Time.fixedDeltaTime;
        }

        _ = TimersSkillReload.Remove(skill);

        OnRepeatUseSkillOnPoint?.Invoke(skill, target);
    }

    public void TimerSkillReload(Skill skill, Person target)
    {
        if (TimersSkillReload.ContainsKey(skill))
        {
            TimersSkillReload[skill] = skill.TimeCooldown;
            return;
        }

        if (skill.TimeCast > 0)
            _ = StartCoroutine(ITimerSkillCast(skill));
        _ = StartCoroutine(ITimerSkillReload(skill, target));
    }

    public void TimerSkillReload(Skill skill, Vector3 target)
    {
        if (TimersSkillReload.ContainsKey(skill))
        {
            TimersSkillReload[skill] = skill.TimeCooldown;
            return;
        }

        if (skill.TimeCast > 0)
            _ = StartCoroutine(ITimerSkillCast(skill));
        _ = StartCoroutine(ITimerSkillReload(skill, target));
    }

    public void WaitCastSkill(Skill skill, Func<bool> expirationCondition)
    {
        _ = StartCoroutine(IWaitCastSkill(skill, expirationCondition));
    }

    #endregion Methods
}
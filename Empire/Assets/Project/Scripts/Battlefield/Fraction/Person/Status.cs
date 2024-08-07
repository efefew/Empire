using System;
using System.Collections;
using System.Collections.Generic;

using AdvancedEditorTools.Attributes;

using UnityEngine;

/// <summary>
/// Характеристики  боевой единицы
/// </summary>
public class Status : MonoBehaviour
{
    public Action<Skill, Person[]> OnRepeatUseSkillOnPersons;
    public Action<Skill, Vector3> OnRepeatUseSkillOnPoint;
    public Action<Skill> OnPatrol;

    #region Fields

    public FractionBattlefield fraction;
    public ulong sideID;

    [Header("Максимум основных параметров")]
    [BeginColumnArea(columnWidth: 0.5f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelGreen)]
    [Min(0)]
    public float maxHealth = 100;
    [Min(0)]
    public float maxMana = 0;
    [Min(0)]
    public float maxStamina = 100;
    [Min(0)]
    public float maxMorality = 100;

    [Header("Востановление основных параметров")]
    [NewColumn(columnWidth: 0.5f)]
    [Min(0)]
    public float regenHealth = 1;
    [Min(0)]
    public float regenMana = 1;
    [Min(0)]
    public float regenStamina = 1;
    [Min(0)]
    public float regenMorality = 1;
    [EndColumnArea]

    [Header("Атака")]
    [BeginColumnArea(columnWidth: 1f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelRed)]
    public float critChance = 0;
    [Min(1)]
    public float crit = 1;
    [SerializeField]
    public DamageTypeDictionary scaleGiveDamage = new();
    [EndColumnArea]

    [Header("Защита")]
    [BeginColumnArea(columnWidth: 1f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.BevelBlue)]
    [SerializeField]
    public DamageTypeDictionary scaleTakeDamage = new();
    public DamageTypeDictionary shield = new();
    [EndColumnArea]

    [Header("Навыки")]
    [BeginColumnArea(columnWidth: 1f, areaStyle = LayoutStyle.None, columnStyle = LayoutStyle.Bevel)]
    public Skill[] skills;
    public Melee melee;
    [EndColumnArea]

    [Min(0)]
    public float maxSpeed = 3.5f;
    public Dictionary<Skill, float> timersSkillReload = new();
    public Skill waitCastSkill;
    [HideInInspector]
    public float timerSkillCast;

    #endregion Fields

    #region Methods

    public void TimerSkillReload(Skill skill, Person target)
    {
        if (timersSkillReload.ContainsKey(skill))
        {
            timersSkillReload[skill] = skill.timeCooldown;
            return;
        }

        if (skill.timeCast > 0)
            _ = StartCoroutine(ITimerSkillCast(skill));
        _ = StartCoroutine(ITimerSkillReload(skill, target));
    }
    public void TimerSkillReload(Skill skill, Vector3 target)
    {
        if (timersSkillReload.ContainsKey(skill))
        {
            timersSkillReload[skill] = skill.timeCooldown;
            return;
        }

        if (skill.timeCast > 0)
            _ = StartCoroutine(ITimerSkillCast(skill));
        _ = StartCoroutine(ITimerSkillReload(skill, target));
    }
    public void WaitCastSkill(Skill skill, Func<bool> expirationCondition) => _ = StartCoroutine(IWaitCastSkill(skill, expirationCondition));
    private IEnumerator IWaitCastSkill(Skill skill, Func<bool> expirationCondition)
    {
        waitCastSkill = skill;
        yield return new WaitUntil(expirationCondition);
        waitCastSkill = null;
    }
    private IEnumerator ITimerSkillCast(Skill skill)
    {
        timerSkillCast = skill.timeCast;
        while (timerSkillCast > 0)
        {
            yield return new WaitForFixedUpdate();
            timerSkillCast -= Time.fixedDeltaTime;
        }

        timerSkillCast = 0;
    }
    private IEnumerator ITimerSkillReload(Skill skill, Person target)
    {
        timersSkillReload.Add(skill, skill.timeCooldown);
        Army army = target?.army;
        while (timersSkillReload[skill] > 0)
        {
            yield return new WaitForFixedUpdate();
            timersSkillReload[skill] -= Time.fixedDeltaTime;
        }

        _ = timersSkillReload.Remove(skill);

        if (target != null || army != null)
            OnRepeatUseSkillOnPersons?.Invoke(skill, army ? army.persons.ToArray() : new Person[1] { target });
        else
            OnPatrol?.Invoke(skill);
    }
    private IEnumerator ITimerSkillReload(Skill skill, Vector3 target)
    {
        timersSkillReload.Add(skill, skill.timeCooldown);
        while (timersSkillReload[skill] > 0)
        {
            yield return new WaitForFixedUpdate();
            timersSkillReload[skill] -= Time.fixedDeltaTime;
        }

        _ = timersSkillReload.Remove(skill);

        OnRepeatUseSkillOnPoint?.Invoke(skill, target);
    }
    #endregion Methods
}
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// јрми€
/// </summary>
public partial class Army : MonoBehaviour, ICombatUnit
{
    #region Properties

    public bool StandStill { get; set; }

    #endregion Properties

    #region Fields

    [SerializeField]
    private Person warriorPrefab;

    private ConteinerButtonSkills conteinerSkill;
    private Battlefield battlefield;
    [SerializeField]
    [AdvancedEditorTools.Attributes.ReadOnly]
    private List<Person> personsCanRun = new();
    private bool firstCallWhenAllCanRun, cancelWaitCastSkill;
    public const float OFFSET_BETWEEN_ARMIES = 2f;
    public List<Person> persons = new();
    public StatusUI armyUI, armyGlobalUI;
    public Button buttonArmy;

    #endregion Fields

    #region Methods

    private void Start()
    {
        StandStill = false;
        battlefield = Battlefield.singleton;
        foreach (Skill skill in status.skills)
            skill.buttonSkillPrefab.Build(this, skill);
    }

    private void SetPositionArmy(Vector2 a, Vector2 b, int countWarriors)
    {
        anchors.SetPositionsHandler -= SetTargetArmy;
        anchors.SetPositionsHandler += SetTargetArmy;
        anchors.SetPositionA(a);
        anchors.SetPositionB(b);
        for (int i = 0; i < countWarriors; i++)
            persons[i].transform.position = persons[i].target.position;
    }

    public void BuildArmy(Vector2 a, Vector2 b, FractionBattlefield fraction, Button buttonArmy, StatusUI armyUI, StatusUI armyGlobalUI, ConteinerButtonSkills conteinerSkill)
    {
        this.armyUI = armyUI;
        this.armyGlobalUI = armyGlobalUI;
        this.conteinerSkill = conteinerSkill;

        BuildArmy(a, b, fraction, buttonArmy);
    }
    public void BuildArmy(Vector2 a, Vector2 b, FractionBattlefield fraction, Button buttonArmy)
    {
        this.buttonArmy = buttonArmy;
        status.fraction = fraction;
        status.sideID = fraction.sideID;

        _ = StartCoroutine(UIArmyUpdate());
        for (int i = 0; i < countWarriors; i++)
        {
            persons.Add(Instantiate(warriorPrefab, transform));
            persons.Last().OnDeadPerson += (Person person) =>
            {
                _ = personsCanRun.Remove(person);
            };
            persons.Last().Build(this);
        }

        SetPositionArmy(a, b, countWarriors);
    }

    public void AddSkillsUI()
    {
        foreach (Skill skill in status.skills)
            conteinerSkill.Add(this, skill);
    }

    public void RemoveSkillsUI()
    {
        foreach (Skill skill in status.skills)
            conteinerSkill.Remove(this, skill);
    }

    /// <summary>
    /// «апускает навык
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="targets">цель</param>
    public void UseSkill(Skill skill, params Person[] targets)
    {
        int idTarget = 0;
        if (StandStill)
        {

            bool allCantRun = true;
            for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            {
                if (persons[idPerson].stunCount == 0)
                {
                    if (persons[idPerson].CastRun(skill, targets?[idTarget]))
                        allCantRun = false;
                    idTarget++;
                    if (idTarget >= targets.Length)
                        idTarget = 0;
                }
            }

            if (!allCantRun)
            {
                battlefield.targetButtonSkill.OnArmySkillRun();
                conteinerSkill.Silence(skill.timeCast);
                status.TimerSkillReload(skill);
            }
        }
        else
        {
            anchors.SetPositionsHandler += CancelWaitCastSkill;
            conteinerSkill.OnClickAnyButtonSkills += CancelWaitCastSkill;
            cancelWaitCastSkill = false;

            battlefield.targetButtonSkill.waitCastSkill = true;
            firstCallWhenAllCanRun = true;
            personsCanRun.Clear();
            status.WaitCastSkill(skill, () => cancelWaitCastSkill || personsCanRun.Count >= persons.Count);
            for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            {
                Person person = persons[idPerson];
                Person target = targets?[idTarget];
                person.Ready = false;
                _ = person.Pursuit(target, ForceSkill(skill, person, target));
                idTarget++;
                if (idTarget >= targets.Length)
                    idTarget = 0;
            }
        }
    }
    public void CancelWaitCastSkill(Transform a, Transform b) => CancelWaitCastSkill();
    public void CancelWaitCastSkill()
    {
        if (!armyUI.toggle.isOn)
            return;
        conteinerSkill.OnClickAnyButtonSkills -= CancelWaitCastSkill;
        anchors.SetPositionsHandler -= CancelWaitCastSkill;
        cancelWaitCastSkill = true;
    }
    private void UpdateWaitPersonsCanRun(Skill skill)
    {
        if (!firstCallWhenAllCanRun || personsCanRun.Count < persons.Count)
            return;

        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            _ = persons[idPerson].CastRun(skill, persons[idPerson].LastPursuitTarget);

        battlefield.targetButtonSkill.OnArmySkillRun();
        conteinerSkill.Silence(skill.timeCast);
        status.TimerSkillReload(skill);
        firstCallWhenAllCanRun = false;
    }
    /// <summary>
    /// ¬ыполнение навыка с наступлением
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="person">инициатор</param>
    /// <param name="target">цель</param>
    /// <returns>функци€, котора€ будет использоватьс€ дл€ выполнени€ навыка</returns>
    private Func<bool> ForceSkill(Skill skill, Person person, Person target)
    {
        return () =>
        {
            // ≈сли достигнуты услови€ отмены ожидани€ выполнени€ навыка при переполнении очереди,
            // или персонажа не существует, или операци€ была отменена
            if (personsCanRun.Count >= persons.Count || person == null || cancelWaitCastSkill)
            {
                // ќбновление состо€ни€ персонажей, которые могут использовать навыки
                UpdateWaitPersonsCanRun(skill);
                return true;
            }

            // ≈сли цель не определена
            if (target == null)
            {
                // ≈сли персонаж готов использовать навык
                if (person.Ready)
                {
                    // ƒобавление персонажа в очередь
                    personsCanRun.Add(person);
                    person.Ready = false;
                }

                return true;
            }

            // ≈сли счетчик стана персонажа равен ожиданию выполнени€ и удовлетвор€ет услови€м диапазона навыка
            if (person.stunCount == (person.Ready ? 1 : 0) && skill.LimitRangeRun(person, target))
            {
                // ≈сли персонаж не готов к выполнению навыка
                if (!person.Ready)
                {
                    person.Ready = true;
                    personsCanRun.Add(person);

                    // «апуск процесса ожидани€ персонажем выполнени€ навыка (персонаж становитс€ в стан)
                    _ = person.Stun(() => target == null || !person.Ready || personsCanRun.Count >= persons.Count || cancelWaitCastSkill);
                }

                return false;
            }

            // ≈сли персонаж готов к выполнению навыка
            if (person.Ready)
            {
                // ”бираем персонажа из списка, тех кто может выполнить навык
                _ = personsCanRun.Remove(person);
                person.Ready = false;
            }

            return false;

        };
    }

    public void SetActive(bool on)
    {
        armyGlobalUI.toggle.SetIsOnWithoutNotify(on);
        for (int i = 0; i < persons.Count; i++)
            persons[i].target.gameObject.SetActive(on);
        anchors.SetActive(on);
    }

    /// <summary>
    /// »спользование навыка на цель
    /// </summary>
    /// <param name="target">цель</param>
    public void TargetForUseSkill(ICombatUnit target)
    {
        battlefield.OnSetTarget -= TargetForUseSkill;
        if (target.TryGetValueOtherType(out Person person))
            UseSkill(battlefield.targetButtonSkill.skillTarget, person);
        if (target.TryGetValueOtherType(out Army army))
            UseSkill(battlefield.targetButtonSkill.skillTarget, army.persons.ToArray());
    }
    #endregion Methods
}
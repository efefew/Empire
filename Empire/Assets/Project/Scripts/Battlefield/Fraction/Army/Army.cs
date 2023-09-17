using System;
using System.Collections.Generic;
using System.Linq;

using AdvancedEditorTools.Attributes;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// јрми€
/// </summary>
public partial class Army : MonoBehaviour, ICombatUnit
{
    #region Properties

    public bool Repeat { get; set; }
    public bool Stand { get; set; }

    #endregion Properties

    #region Fields

    [SerializeField]
    private Person warriorPrefab;

    private ConteinerButtonSkills conteinerSkill;
    private Battlefield battlefield;
    [SerializeField]
    [ReadOnly]
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
        Stand = false;
        firstMinDistance = true;
        battlefield = Battlefield.singleton;
        foreach (Skill skill in status.skills)
            skill.buttonSkillPrefab.Build(this, skill);
    }

    private void SetPositionArmy(Vector2 a, Vector2 b, int countWarriors)
    {
        anchors.OnSetPositions -= MoveArmy;
        anchors.OnSetPositions += MoveArmy;
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
        for (int id = 0; id < countWarriors; id++)
        {
            persons.Add(Instantiate(warriorPrefab, transform));
            persons.Last().OnDeadPerson += (Person person) =>
            {
                _ = personsCanRun.Remove(person);
            };
            persons.Last().name += $" {id}";
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
        status.OnRepeat -= UseSkill;
        if (targets.NotUnityNull().Count() == 0)
            return;
        if (Repeat)
            status.OnRepeat += UseSkill;

        int idTarget = 0;
        if (Stand)
        {

            bool allCantRun = true;
            for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            {
                if (persons[idPerson].stunCount == 0)
                {
                    if (persons[idPerson].CastRun(skill, targets[idTarget].army ? GetRandomPerson(targets[idTarget].army) : targets[idTarget]))
                        allCantRun = false;
                    idTarget++;
                    if (idTarget >= targets.Length)
                        idTarget = 0;
                }
            }

            if (!allCantRun)
            {
                _ = conteinerSkill.Silence(this, skill);
                _ = conteinerSkill.Reload(this, skill);
                status.TimerSkillReload(skill, targets.NotUnityNull().ToArray()[0]);
            }
        }
        else
        {
            anchors.OnSetPositions += CancelWaitCastSkill;
            conteinerSkill.OnClickAnyButtonSkills += CancelWaitCastSkill;
            cancelWaitCastSkill = false;

            if (conteinerSkill.Contains(this, skill, out ButtonSkill buttonSkill))
            {
                buttonSkill.waitCastSkill = true;
            }

            firstCallWhenAllCanRun = true;
            status.WaitCastSkill(skill, () => cancelWaitCastSkill || !firstCallWhenAllCanRun);
            for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            {
                Person person = persons[idPerson];
                Person target = targets[idTarget].army ? GetRandomPerson(targets[idTarget].army) : targets[idTarget];
                person.Ready = false;

                person.StopPursuit();
                person.armyPursuit = person.Pursuit(target, ForceSkill(skill, person, target, target.army));
                person.MoveUpdate();
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
        anchors.OnSetPositions -= CancelWaitCastSkill;
        cancelWaitCastSkill = true;
    }
    private void UpdateWaitPersonsCanRun(Skill skill, Army armyTarget)
    {
        Person randomTarget = GetRandomPerson(armyTarget);
        if (!firstCallWhenAllCanRun || personsCanRun.Count < persons.Count)
            return;

        bool allCantRun = true;
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
        {
            if (persons[idPerson].CastRun(skill, persons[idPerson].LastPursuitTarget != null ? persons[idPerson].LastPursuitTarget : randomTarget))
                allCantRun = false;
        }

        _ = conteinerSkill.Silence(this, skill);
        _ = conteinerSkill.Reload(this, skill);
        status.TimerSkillReload(skill, allCantRun ? null : randomTarget);
        firstCallWhenAllCanRun = false;
        personsCanRun.Clear();
    }

    /// <summary>
    /// ¬ыполнение навыка с наступлением
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="person">инициатор</param>
    /// <param name="target">цель</param>
    /// <returns>функци€, котора€ будет использоватьс€ дл€ выполнени€ навыка</returns>
    private Func<bool> ForceSkill(Skill skill, Person person, Person target, Army armyTarget)
    {
        return () =>
        {
            if (!firstCallWhenAllCanRun)
                return true;
            if (person.distracted)
                CancelForceSkill(person);
            // ≈сли достигнуты услови€ отмены ожидани€ выполнени€ навыка при переполнении очереди,
            // или персонажа не существует, или операци€ была отменена
            if (personsCanRun.Count >= persons.Count || person == null || cancelWaitCastSkill || person.distracted)
            {

                // ќбновление состо€ни€ персонажей, которые могут использовать навыки
                UpdateWaitPersonsCanRun(skill, armyTarget);
                return true;
            }

            // ≈сли цель не определена
            if (target == null)
            {
                target = GetRandomPerson(armyTarget);
                // ≈сли цель всЄ ещЄ  не определена
                if (target == null)
                {
                    CancelForceSkill(person);

                    return true;
                }

                person.Ready = false;
                _ = personsCanRun.Remove(person);

                person.SetTarget(target.transform);
                person.MoveUpdate();
                return false;
            }

            // ≈сли счетчик стана персонажа равен ожиданию выполнени€ и удовлетвор€ет услови€м диапазона навыка
            if (person.stunCount == (person.Ready ? 1 : 0) && skill.LimitRangeRun(person, target, close: true))
            {
                // ≈сли персонаж не готов к выполнению навыка
                if (!person.Ready)
                {
                    person.Ready = true;
                    personsCanRun.Add(person);

                    // «апуск процесса ожидани€ персонажем выполнени€ навыка (персонаж становитс€ в стан)
                    _ = person.Stun(() => target == null || !person.Ready || (personsCanRun.Count >= persons.Count) || cancelWaitCastSkill);
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

    private void CancelForceSkill(Person person)
    {
        // ≈сли персонаж готов использовать навык
        if (!personsCanRun.Contains(person))
        {
            // ƒобавление персонажа в очередь
            personsCanRun.Add(person);
        }

        person.Ready = false;
    }

    private static Person GetRandomPerson(Army armyTarget) => armyTarget == null || armyTarget.persons.Count == 0 ? null : armyTarget.persons[UnityEngine.Random.Range(0, armyTarget.persons.Count)];

    public void SetRepeat(bool on) => Repeat = on;
    public void SetStand(bool on) => Stand = on;
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
    [Button("MoveUpdate")]
    public void MoveUpdate()
    {
        for (int id = 0; id < persons.Count; id++)
            persons[id].MoveUpdate();
    }
    #endregion Methods
}
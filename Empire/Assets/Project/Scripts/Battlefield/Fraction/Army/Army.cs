#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdvancedEditorTools.Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#endregion
/// <summary>
/// Армия
/// </summary>
public partial class Army : MonoBehaviour
{
    public const float OFFSET_BETWEEN_ARMIES = 2f;
    private const float PATROL_DELAY = 0.1f;

    [FormerlySerializedAs("warriorPrefab")] [SerializeField]
    private Person _warriorPrefab;

    [FormerlySerializedAs("personsCanRun")] [SerializeField] [ReadOnly]
    private List<Person> _personsCanRun = new();

    [FormerlySerializedAs("persons")] public List<Person> Persons = new();
    [FormerlySerializedAs("armyUI")] public StatusUI ArmyUI;
    [FormerlySerializedAs("armyGlobalUI")] public StatusUI ArmyGlobalUI;
    [FormerlySerializedAs("buttonArmy")] public Button ButtonArmy;
    private Battlefield _battlefield;

    private ConteinerButtonSkills _containerSkill;

    private bool _firstCallWhenAllCanRun, _endWaitCastSkill;
    private Coroutine _patrolCoroutine;
    public bool Repeat { get; private set; }
    public bool Stand { get; private set; }
    public Skill PatrolSkill { get; private set; }

    private void Start()
    {
        Stand = false;
        firstMinDistance = true;
        _battlefield = Battlefield.Instance;
        foreach (Skill skill in status.Skills)
            skill.ButtonSkillPrefab.Build(this, skill);
        anchors.OnChangedPositions += (_, _) =>
        {
            TargetButtonPersonId = newTargetButtonPersonId;
            _battlefield.RemoveSkillAditionalUI();
            _battlefield.StopPatrol();
        };
    }

    private void SetPositionArmy(Vector2 a, Vector2 b, int warriors)
    {
        anchors.OnChangePositions -= MovePoints;
        anchors.OnChangePositions += MovePoints;
        anchors.OnChangedPositions -= MoveArmy;
        anchors.OnChangedPositions += MoveArmy;
        anchors.ChangePositionA(a);
        anchors.ChangePositionB(b, true);
        for (int i = 0; i < warriors; i++)
            Persons[i].transform.position = Persons[i].Target.position;
        MovePoints(anchors.A, anchors.B);
        TargetButtonPersonId = newTargetButtonPersonId;
    }

    private void PursuitUseSkill(Skill skill, Person[] targets)
    {
        int idTarget = 0;

        if (_containerSkill.Contains(this, skill, out ButtonSkill buttonSkill))
            buttonSkill.WaitCastSkill = true;

        _firstCallWhenAllCanRun = true;
        status.WaitCastSkill(skill, () => _endWaitCastSkill || !_firstCallWhenAllCanRun);
        bool melee = skill.TryGetComponent(out Melee _);
        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
        {
            Person person = Persons[idPerson];
            Person target = targets[idTarget].Army ? GetRandomPerson(targets[idTarget].Army) : targets[idTarget];
            person.Ready = false;

            person.StopPursuit();
            person.ArmyPursuit = person.Pursuit(target, ForceSkill(skill, person, target, target.Army, melee));
            person.MoveUpdate();
            idTarget++;
            if (idTarget >= targets.Length)
                idTarget = 0;
        }
    }

    private void PursuitUseSkill(Skill skill, Vector3 target)
    {
        if (_containerSkill.Contains(this, skill, out ButtonSkill buttonSkill))
            buttonSkill.WaitCastSkill = true;

        _firstCallWhenAllCanRun = true;
        status.WaitCastSkill(skill, () => _endWaitCastSkill || !_firstCallWhenAllCanRun);
        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
        {
            Person person = Persons[idPerson];
            person.Ready = false;

            person.StopPursuit();
            person.ArmyPursuit = person.Pursuit(target, ForceSkill(skill, person, target));
            person.MoveUpdate();
        }
    }

    private void StandUseSkill(Skill skill, Person[] targets)
    {
        int idTarget = 0;
        bool allCantRun = true;
        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
            if (Persons[idPerson].stunCount == 0)
            {
                if (Persons[idPerson].CastRun(skill,
                        targets[idTarget].Army ? GetRandomPerson(targets[idTarget].Army) : targets[idTarget]))
                    allCantRun = false;
                idTarget++;
                if (idTarget >= targets.Length)
                    idTarget = 0;
            }

        if (!allCantRun)
        {
            _ = _containerSkill.Reload(this, skill);
            status.TimerSkillReload(skill, targets.NotUnityNull().ToArray()[0]);
        }
        else
        {
            ForgetTargetArmy();
        }
    }

    private void StandUseSkill(Skill skill, Vector3 target)
    {
        bool allCantRun = true;
        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
            if (Persons[idPerson].stunCount == 0)
                if (Persons[idPerson].CastRun(skill, target))
                    allCantRun = false;

        if (!allCantRun)
        {
            _ = _containerSkill.Reload(this, skill);
            status.TimerSkillReload(skill, target);
        }
        else
        {
            ForgetTargetArmy();
        }
    }

    private void ForgetTargetArmy()
    {
        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
            Persons[idPerson].ArmyTarget = null;
    }

    private void UpdateWaitPersonsCanRun(Skill skill, Army armyTarget, bool melee)
    {
        Person randomTarget = GetRandomPerson(armyTarget);
        if (!_firstCallWhenAllCanRun || _personsCanRun.Count < Persons.Count)
            return;

        bool allCantRun = true;
        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
        {
            if (Persons[idPerson].CastRun(skill,
                    Persons[idPerson].LastPursuitTarget != null ? Persons[idPerson].LastPursuitTarget : randomTarget))
                allCantRun = false;
            if (!melee)
                Persons[idPerson].ArmyTarget = null;
        }

        _ = _containerSkill.Reload(this, skill);
        status.TimerSkillReload(skill, allCantRun ? null : randomTarget);
        _firstCallWhenAllCanRun = false;
        _personsCanRun.Clear();
    }

    private void UpdateWaitPersonsCanRun(Skill skill, Vector3 target)
    {
        if (!_firstCallWhenAllCanRun || _personsCanRun.Count < Persons.Count)
            return;

        for (int idPerson = 0; idPerson < Persons.Count; idPerson++)
            _ = Persons[idPerson].CastRun(skill, target);

        _ = _containerSkill.Reload(this, skill);
        status.TimerSkillReload(skill, target);
        _firstCallWhenAllCanRun = false;
        _personsCanRun.Clear();
    }

    /// <summary>
    /// Выполнение навыка с наступлением
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="person">инициатор</param>
    /// <param name="target">цель</param>
    /// <param name="armyTarget">Армия цели</param>
    /// <param name="melee">Ближний ли бой?</param>
    /// <returns>Функция, которая будет использоваться для выполнения навыка</returns>
    private Func<bool> ForceSkill(Skill skill, Person person, Person target, Army armyTarget, bool melee)
    {
        return () =>
        {
            if (person.distracted)
                CancelForceSkill(person);

            if (ChangeUpdateWaitPersonsCanRun(skill, person, armyTarget, melee))
                return true;
            // Если цель не определена
            if (target == null)
                return TrySetTarget(person, out target, armyTarget);

            if (HandleSkillExecution(skill, person, target.transform.position))
                return false;
            ReadyCheck(person);
            return false;
        };
    }

    /// <summary>
    /// Выполнение навыка с наступлением
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="person">инициатор</param>
    /// <param name="target">цель</param>
    /// <returns>Функция, которая будет использоваться для выполнения навыка</returns>
    private Func<bool> ForceSkill(Skill skill, Person person, Vector3 target)
    {
        return () =>
        {
            if (person.distracted)
                CancelForceSkill(person);

            if (ChangeUpdateWaitPersonsCanRun(skill, person, target))
                return true;

            if (HandleSkillExecution(skill, person, target))
                return false;
            ReadyCheck(person);
            return false;
        };
    }

    private void ReadyCheck(Person person)
    {
        if (!person.Ready) return; // Если персонаж готов к выполнению навыка
        // Убираем персонажа из списка, тех кто может выполнить навык
        _ = _personsCanRun.Remove(person);
        person.Ready = false;
    }

    private bool ChangeUpdateWaitPersonsCanRun(Skill skill, Person person, Army armyTarget, bool melee)
    {
        // Если достигнуты условия отмены ожидания выполнения навыка при переполнении очереди, или персонажа не существует, или операция была отменена
        if (_personsCanRun.Count < Persons.Count && person != null && !_endWaitCastSkill &&
            !person.distracted) return false;
        // Обновление состояния персонажей, которые могут использовать навыки
        UpdateWaitPersonsCanRun(skill, armyTarget, melee);
        _endWaitCastSkill = true;
        return true;

    }

    private bool TrySetTarget(Person person, out Person target, Army armyTarget)
    {
        target = GetRandomPerson(armyTarget);
        // Если цель всё ещё не определена
        if (!target)
        {
            CancelForceSkill(person);
            SetTargetArmy(null);
            return true;
        }

        person.LastPursuitTarget = target;
        person.SetTarget(target.transform);
        person.Ready = false;
        _ = _personsCanRun.Remove(person);

        //person.SetTarget(target.transform);
        person.MoveUpdate();
        return false;
    }

    private bool ChangeUpdateWaitPersonsCanRun(Skill skill, Person person, Vector3 target)
    {
        // Если достигнуты условия отмены ожидания выполнения навыка при переполнении очереди, или персонажа не существует, или операция была отменена
        if (_personsCanRun.Count < Persons.Count && person && !_endWaitCastSkill &&
            !person.distracted) return false;
        // Обновление состояния персонажей, которые могут использовать навыки
        UpdateWaitPersonsCanRun(skill, target);
        return true;

    }

    /// <summary>
    /// Управлять выполнением навыка
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="person">инициатор</param>
    /// <param name="target">цель</param>
    /// <returns></returns>
    private bool HandleSkillExecution(Skill skill, Person person, Vector3 target)
    {
        // Если счетчик стана персонажа равен ожиданию выполнения и удовлетворяет условиям диапазона навыка
        if (person.stunCount != (person.Ready ? 1 : 0) || !skill.LimitRangeRun(person, target, true)) return false;
        // Если персонаж не готов к выполнению навыка
        if (person.Ready) return true;
        person.Ready = true;
        _personsCanRun.Add(person);

        // Запуск процесса ожидания персонажем выполнения навыка (персонаж становится в стан)
        _ = person.Stun(() =>
            !person.Ready || _personsCanRun.Count >= Persons.Count || _endWaitCastSkill);

        return true;

    }

    private void CancelForceSkill(Person person)
    {
        // Если персонаж готов использовать навык
        if (!_personsCanRun.Contains(person))
            _personsCanRun.Add(person);// Добавление персонажа в очередь

        person.Ready = false;
    }

    private void SetTargetArmy(Army armyTarget)
    {
        for (int id = 0; id < Persons.Count; id++)
            Persons[id].ArmyTarget = armyTarget;
    }

    private void ListenCancelWaitCastSkill()
    {
        anchors.OnChangePositions -= CancelWaitCastSkill;
        anchors.OnChangePositions += CancelWaitCastSkill;
        _containerSkill.OnClickAnyButtonSkills -= CancelWaitCastSkill;
        _containerSkill.OnClickAnyButtonSkills += CancelWaitCastSkill;
    }

    private void ClearTargetUseSkill()
    {
        _battlefield.OnSetTargetArmy -= TargetForUseSkill;
        _battlefield.OnSetTargetPoint -= TargetForUseSkill;
    }

    private bool AnyEnemyInRange(out Army target, Skill skill)
    {
        for (int idEnemyFraction = 0; idEnemyFraction < _battlefield.Fractions.Length; idEnemyFraction++)
        {
            FractionBattlefield enemyFraction = _battlefield.Fractions[idEnemyFraction];
            bool itsEnemy = enemyFraction.SideID != status.Fraction.SideID;
            if (!itsEnemy) continue;
            for (int idEnemyArmy = 0; idEnemyArmy < enemyFraction.Armies.Count; idEnemyArmy++)
            {
                Army enemy = enemyFraction.Armies[idEnemyArmy];
                if (!EnemyInRange(enemy, skill)) continue;
                target = enemy;
                return true;
            }
        }

        target = null;
        return false;
    }

    private bool EnemyInRange(Army enemy, Skill skill)
    {
        float distance = Vector3.Distance(Persons[TargetButtonPersonId].transform.position,
            enemy.Persons[enemy.TargetButtonPersonId].transform.position);
        float range = skill.Range;
        return distance <= range;
    }

    public static Person GetRandomPerson(Army armyTarget)
    {
        return !armyTarget || armyTarget.Persons.Count == 0
            ? null
            : armyTarget.Persons[Random.Range(0, armyTarget.Persons.Count)];
    }

    public void BuildArmy(Vector2 a, Vector2 b, FractionBattlefield fraction, Button buttonArmy, StatusUI armyUI,
        StatusUI armyGlobalUI, ConteinerButtonSkills containerSkill)
    {
        ArmyUI = armyUI;
        ArmyGlobalUI = armyGlobalUI;
        _containerSkill = containerSkill;

        BuildArmy(a, b, fraction, buttonArmy);
    }

    public void BuildArmy(Vector2 a, Vector2 b, FractionBattlefield fraction, Button buttonArmy)
    {
        ButtonArmy = buttonArmy;
        status.Fraction = fraction;
        status.SideID = fraction.SideID;

        _ = StartCoroutine(UIArmyUpdate());
        for (int id = 0; id < countWarriors; id++)
        {
            Persons.Add(Instantiate(_warriorPrefab, transform));
            Persons.Last().OnDeadPerson += person => _personsCanRun.Remove(person);
            Persons.Last().name += $" {id}";
            Persons.Last().Build(this);
        }

        SetPositionArmy(a, b, countWarriors);
    }

    public void AddSkillsUI()
    {
        foreach (Skill skill in status.Skills)
            _containerSkill.Add(this, skill);
    }

    public void RemoveSkillsUI()
    {
        foreach (Skill skill in status.Skills)
            _containerSkill.Remove(this, skill);
    }

    /// <summary>
    /// Запускает навык
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="targets">цель</param>
    public void UseSkill(Skill skill, params Person[] targets)
    {
        if (!status.Skills.Contains(skill) || !targets.NotUnityNull().Any())
            return;
        status.OnRepeatUseSkillOnPersons -= UseSkill;
        if (Repeat)
            status.OnRepeatUseSkillOnPersons += UseSkill;

        ListenCancelWaitCastSkill();
        _endWaitCastSkill = false;

        SetTargetArmy(targets[0].Army);
        if (Stand)
            StandUseSkill(skill, targets);
        else
            PursuitUseSkill(skill, targets);
    }

    /// <summary>
    /// Запускает навык
    /// </summary>
    /// <param name="skill">навык</param>
    /// <param name="target">цель</param>
    private void UseSkill(Skill skill, Vector3 target)
    {
        if (!status.Skills.Contains(skill))
            return;
        status.OnRepeatUseSkillOnPoint -= UseSkill;
        if (Repeat)
            status.OnRepeatUseSkillOnPoint += UseSkill;

        ListenCancelWaitCastSkill();
        _endWaitCastSkill = false;

        if (Stand)
            StandUseSkill(skill, target);
        else
            PursuitUseSkill(skill, target);
    }

    private void CancelWaitCastSkill(Transform a, Transform b)
    {
        CancelWaitCastSkill(null);
    }

    private void CancelWaitCastSkill(ButtonSkill buttonSkill)
    {
        if (!ArmyUI.toggle.isOn)
            return;
        _containerSkill.OnClickAnyButtonSkills -= CancelWaitCastSkill;
        anchors.OnChangePositions -= CancelWaitCastSkill;
        status.OnRepeatUseSkillOnPersons -= UseSkill;
        status.OnRepeatUseSkillOnPoint -= UseSkill;
        _endWaitCastSkill = true;
        SetTargetArmy(null);
    }

    public void SetRepeat(bool on)
    {
        Repeat = on;
    }

    public void SetStand(bool on)
    {
        Stand = on;
    }

    /// <summary>
    /// Патрулировать
    /// </summary>
    public void StartPatrol(Skill skill)
    {
        if (_patrolCoroutine != null) return;
        PatrolSkill = skill;
        _patrolCoroutine = StartCoroutine(IPatrol(skill));
        _containerSkill.UpdatePatrolUI();
    }

    public void StopPatrol()
    {
        if (_patrolCoroutine == null) return;
        status.OnRepeatUseSkillOnPersons -= UseSkill;
        ForgetTargetArmy();
        StopCoroutine(_patrolCoroutine);
        _patrolCoroutine = null;
        PatrolSkill = null;
        _containerSkill.UpdatePatrolUI();
    }

    private IEnumerator IPatrol(Skill skill)
    {
        while (true)
        {
            if (!Persons[0].ArmyTarget && AnyEnemyInRange(out Army army, skill))
                UseSkill(skill, army.Persons.ToArray());

            yield return new WaitForSeconds(PATROL_DELAY);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public void SetActive(bool on)
    {
        ArmyGlobalUI.toggle.SetIsOnWithoutNotify(on);
        for (int i = 0; i < Persons.Count; i++)
            Persons[i].Target.gameObject.SetActive(on);
        anchors.SetActive(on);
    }

    /// <summary>
    /// Использование навыка на цель
    /// </summary>
    /// <param name="target">цель</param>
    public void TargetForUseSkill(Army target)
    {
        ClearTargetUseSkill();
        if (target.TryGetValueOtherType(out Person person))
            UseSkill(_battlefield._targetSkill, person);
        if (target.TryGetValueOtherType(out Army army))
            UseSkill(_battlefield._targetSkill, army.Persons.ToArray());
    }

    /// <summary>
    /// Использование навыка на цель
    /// </summary>
    /// <param name="target">цель</param>
    public void TargetForUseSkill(Vector3 target)
    {
        ClearTargetUseSkill();
        UseSkill(_battlefield._targetSkill, target);
    }

    /// <summary>
    /// Использование навыка патрулём
    /// </summary>
    public void TargetForUseSkill()
    {
        ClearTargetUseSkill();
        ListenCancelWaitCastSkill();
    }

    [Button("MoveUpdate")]
    public void MoveUpdate()
    {
        for (int id = 0; id < Persons.Count; id++)
            Persons[id].MoveUpdate();
    }
}
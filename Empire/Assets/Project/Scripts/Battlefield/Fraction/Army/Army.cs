#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdvancedEditorTools.Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#endregion

/// <summary>
///     �����
/// </summary>
public partial class Army : MonoBehaviour
{
    #region Properties

    public bool Repeat { get; set; }
    public bool Stand { get; set; }

    #endregion Properties

    #region Fields

    [SerializeField] private Person warriorPrefab;

    private ConteinerButtonSkills conteinerSkill;
    private Battlefield battlefield;

    [SerializeField] [ReadOnly] private List<Person> personsCanRun = new();

    private bool firstCallWhenAllCanRun, cancelWaitCastSkill;
    public const float OFFSET_BETWEEN_ARMIES = 2f;
    private const float PATROL_DELAY = 0.1f;
    public List<Person> persons = new();
    public StatusUI armyUI, armyGlobalUI;
    public Button buttonArmy;
    private Coroutine patrolCoroutine;
    public Skill PatrolSkill { get; private set; }

    #endregion Fields

    #region Methods

    private void Start()
    {
        Stand = false;
        firstMinDistance = true;
        battlefield = Battlefield.Singleton;
        foreach (Skill skill in status.skills)
            skill.buttonSkillPrefab.Build(this, skill);
        anchors.OnChangedPositions += (a, b) =>
        {
            TargetButtonPersonId = newTargetButtonPersonId;
            battlefield.RemoveSkillAditionalUI();
            battlefield.StopPatrol();
        };
    }

    private void SetPositionArmy(Vector2 a, Vector2 b, int countWarriors)
    {
        anchors.OnChangePositions -= MovePoints;
        anchors.OnChangePositions += MovePoints;
        anchors.OnChangedPositions -= MoveArmy;
        anchors.OnChangedPositions += MoveArmy;
        anchors.ChangePositionA(a);
        anchors.ChangePositionB(b, true);
        for (int i = 0; i < countWarriors; i++)
            persons[i].transform.position = persons[i].target.position;
        MovePoints(anchors.a, anchors.b);
        TargetButtonPersonId = newTargetButtonPersonId;
    }

    private void PursuitUseSkill(Skill skill, Person[] targets)
    {
        int idTarget = 0;

        if (conteinerSkill.Contains(this, skill, out ButtonSkill buttonSkill))
            buttonSkill.waitCastSkill = true;

        firstCallWhenAllCanRun = true;
        status.WaitCastSkill(skill, () => cancelWaitCastSkill || !firstCallWhenAllCanRun);
        bool melee = skill.TryGetComponent(out Melee meleeObj);
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
        {
            Person person = persons[idPerson];
            Person target = targets[idTarget].army ? GetRandomPerson(targets[idTarget].army) : targets[idTarget];
            person.Ready = false;

            person.StopPursuit();
            person.armyPursuit = person.Pursuit(target, ForceSkill(skill, person, target, target.army, melee));
            person.MoveUpdate();
            idTarget++;
            if (idTarget >= targets.Length)
                idTarget = 0;
        }
    }

    private void PursuitUseSkill(Skill skill, Vector3 target)
    {
        if (conteinerSkill.Contains(this, skill, out ButtonSkill buttonSkill))
            buttonSkill.waitCastSkill = true;

        firstCallWhenAllCanRun = true;
        status.WaitCastSkill(skill, () => cancelWaitCastSkill || !firstCallWhenAllCanRun);
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
        {
            Person person = persons[idPerson];
            person.Ready = false;

            person.StopPursuit();
            person.armyPursuit = person.Pursuit(target, ForceSkill(skill, person, target));
            person.MoveUpdate();
        }
    }

    private void StandUseSkill(Skill skill, Person[] targets)
    {
        int idTarget = 0;
        bool allCantRun = true;
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            if (persons[idPerson].stunCount == 0)
            {
                if (persons[idPerson].CastRun(skill,
                        targets[idTarget].army ? GetRandomPerson(targets[idTarget].army) : targets[idTarget]))
                    allCantRun = false;
                idTarget++;
                if (idTarget >= targets.Length)
                    idTarget = 0;
            }

        if (!allCantRun)
        {
            _ = conteinerSkill.Reload(this, skill);
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
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            if (persons[idPerson].stunCount == 0)
                if (persons[idPerson].CastRun(skill, target))
                    allCantRun = false;

        if (!allCantRun)
        {
            _ = conteinerSkill.Reload(this, skill);
            status.TimerSkillReload(skill, target);
        }
        else
        {
            ForgetTargetArmy();
        }
    }

    private void ForgetTargetArmy()
    {
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            persons[idPerson].armyTarget = null;
    }

    private void UpdateWaitPersonsCanRun(Skill skill, Army armyTarget, bool melee)
    {
        Person randomTarget = GetRandomPerson(armyTarget);
        if (!firstCallWhenAllCanRun || personsCanRun.Count < persons.Count)
            return;

        bool allCantRun = true;
        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
        {
            if (persons[idPerson].CastRun(skill,
                    persons[idPerson].LastPursuitTarget != null ? persons[idPerson].LastPursuitTarget : randomTarget))
                allCantRun = false;
            if (!melee)
                persons[idPerson].armyTarget = null;
        }

        _ = conteinerSkill.Reload(this, skill);
        status.TimerSkillReload(skill, allCantRun ? null : randomTarget);
        firstCallWhenAllCanRun = false;
        personsCanRun.Clear();
    }

    private void UpdateWaitPersonsCanRun(Skill skill, Vector3 target)
    {
        if (!firstCallWhenAllCanRun || personsCanRun.Count < persons.Count)
            return;

        for (int idPerson = 0; idPerson < persons.Count; idPerson++)
            _ = persons[idPerson].CastRun(skill, target);

        _ = conteinerSkill.Reload(this, skill);
        status.TimerSkillReload(skill, target);
        firstCallWhenAllCanRun = false;
        personsCanRun.Clear();
    }

    /// <summary>
    ///     ���������� ������ � ������������
    /// </summary>
    /// <param name="skill">�����</param>
    /// <param name="person">���������</param>
    /// <param name="target">����</param>
    /// <returns>�������, ������� ����� �������������� ��� ���������� ������</returns>
    /// <param name="melee"></param>
    /// <returns></returns>
    /// ���������� ������
    /// </returns>
    private Func<bool> ForceSkill(Skill skill, Person person, Person target, Army armyTarget, bool melee)
    {
        return () =>
        {
            if (person.distracted)
                CancelForceSkill(person);

            if (ChangeUpdateWaitPersonsCanRun(skill, person, armyTarget, melee))
                return true;
            // ���� ���� �� ����������
            if (target == null)
                return TrySetTarget(person, out target, armyTarget);

            if (HandleSkillExecution(skill, person, target.transform.position))
                return false;
            ReadyCheck(person);
            return false;
        };
    }

    /// <summary>
    ///     ���������� ������ � ������������
    /// </summary>
    /// <param name="skill">�����</param>
    /// <param name="person">���������</param>
    /// <param name="target">����</param>
    /// <returns>�������, ������� ����� �������������� ��� ���������� ������</returns>
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
        if (person.Ready) // ���� �������� ����� � ���������� ������
        {
            // ������� ��������� �� ������, ��� ��� ����� ��������� �����
            _ = personsCanRun.Remove(person);
            person.Ready = false;
        }
    }

    private bool ChangeUpdateWaitPersonsCanRun(Skill skill, Person person, Army armyTarget, bool melee)
    {
        // ���� ���������� ������� ������ �������� ���������� ������ ��� ������������ �������,  ��� ��������� �� ����������, ��� �������� ���� ��������
        if (personsCanRun.Count >= persons.Count || person == null || cancelWaitCastSkill || person.distracted)
        {
            // ���������� ��������� ����������, ������� ����� ������������ ������
            UpdateWaitPersonsCanRun(skill, armyTarget, melee);
            return true;
        }

        return false;
    }

    private bool TrySetTarget(Person person, out Person target, Army armyTarget)
    {
        target = GetRandomPerson(armyTarget);
        // ���� ���� �� ���  �� ����������
        if (target == null)
        {
            CancelForceSkill(person);
            SetTargetArmy(null);
            return true;
        }

        person.LastPursuitTarget = target;
        person.SetTarget(target.transform);
        person.Ready = false;
        _ = personsCanRun.Remove(person);

        //person.SetTarget(target.transform);
        person.MoveUpdate();
        return false;
    }

    private bool ChangeUpdateWaitPersonsCanRun(Skill skill, Person person, Vector3 target)
    {
        // ���� ���������� ������� ������ �������� ���������� ������ ��� ������������ �������, ��� ��������� �� ����������, ��� �������� ���� ��������
        if (personsCanRun.Count >= persons.Count || person == null || cancelWaitCastSkill || person.distracted)
        {
            // ���������� ��������� ����������, ������� ����� ������������ ������
            UpdateWaitPersonsCanRun(skill, target);
            return true;
        }

        return false;
    }

    /// <summary>
    ///     ��������� ����������� ������
    /// </summary>
    /// <param name="skill">�����</param>
    /// <param name="person">���������</param>
    /// <param name="target">����</param>
    /// <returns></returns>
    private bool HandleSkillExecution(Skill skill, Person person, Vector3 target)
    {
        // ���� ������� ����� ��������� ����� �������� ���������� � ������������� �������� ��������� ������
        if (person.stunCount == (person.Ready ? 1 : 0) && skill.LimitRangeRun(person, target, true))
        {
            // ���� �������� �� ����� � ���������� ������
            if (!person.Ready)
            {
                person.Ready = true;
                personsCanRun.Add(person);

                // ������ �������� �������� ���������� ���������� ������ (�������� ���������� � ����)
                _ = person.Stun(() =>
                    target == null || !person.Ready || personsCanRun.Count >= persons.Count || cancelWaitCastSkill);
            }

            return true;
        }

        return false;
    }

    private void CancelForceSkill(Person person)
    {
        // ���� �������� ����� ������������ �����
        if (!personsCanRun.Contains(person))
            // ���������� ��������� � �������
            personsCanRun.Add(person);

        person.Ready = false;
    }

    private void SetTargetArmy(Army armyTarget)
    {
        for (int id = 0; id < persons.Count; id++)
            persons[id].armyTarget = armyTarget;
    }

    private void ListenCancelWaitCastSkill()
    {
        anchors.OnChangePositions -= CancelWaitCastSkill;
        anchors.OnChangePositions += CancelWaitCastSkill;
        conteinerSkill.OnClickAnyButtonSkills -= CancelWaitCastSkill;
        conteinerSkill.OnClickAnyButtonSkills += CancelWaitCastSkill;
    }

    private void ClearTargetUseSkill()
    {
        battlefield.OnSetTargetArmy -= TargetForUseSkill;
        battlefield.OnSetTargetPoint -= TargetForUseSkill;
    }

    private bool AnyEnemyInRange(out Army target, Skill skill)
    {
        for (int idEnemyFraction = 0; idEnemyFraction < battlefield.Fractions.Length; idEnemyFraction++)
        {
            FractionBattlefield enemyFraction = battlefield.Fractions[idEnemyFraction];
            bool itsEnemy = enemyFraction.sideID != status.fraction.sideID;
            if (itsEnemy)
                for (int idEnemyArmy = 0; idEnemyArmy < enemyFraction.armies.Count; idEnemyArmy++)
                {
                    Army enemy = enemyFraction.armies[idEnemyArmy];
                    if (EnemyInRange(enemy, skill))
                    {
                        target = enemy;
                        return true;
                    }
                }
        }

        target = null;
        return false;
    }

    private bool EnemyInRange(Army enemy, Skill skill)
    {
        float distance = Vector3.Distance(persons[TargetButtonPersonId].transform.position,
            enemy.persons[enemy.TargetButtonPersonId].transform.position);
        float range = skill.range;
        return distance <= range;
    }

    public static Person GetRandomPerson(Army armyTarget)
    {
        return armyTarget == null || armyTarget.persons.Count == 0
            ? null
            : armyTarget.persons[Random.Range(0, armyTarget.persons.Count)];
    }

    public void BuildArmy(Vector2 a, Vector2 b, FractionBattlefield fraction, Button buttonArmy, StatusUI armyUI,
        StatusUI armyGlobalUI, ConteinerButtonSkills conteinerSkill)
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
            persons.Add(Instantiate(warriorPrefab, transform)); //������ ������� ����� ������� -_-
            persons.Last().OnDeadPerson += person => personsCanRun.Remove(person);
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
    ///     ��������� �����
    /// </summary>
    /// <param name="skill">�����</param>
    /// <param name="targets">����</param>
    public void UseSkill(Skill skill, params Person[] targets)
    {
        if (!status.skills.Contains(skill) || targets.NotUnityNull().Count() == 0)
            return;
        status.OnRepeatUseSkillOnPersons -= UseSkill;
        if (Repeat)
            status.OnRepeatUseSkillOnPersons += UseSkill;

        ListenCancelWaitCastSkill();
        cancelWaitCastSkill = false;

        SetTargetArmy(targets[0].army);
        if (Stand)
            StandUseSkill(skill, targets);
        else
            PursuitUseSkill(skill, targets);
    }

    /// <summary>
    ///     ��������� �����
    /// </summary>
    /// <param name="skill">�����</param>
    /// <param name="target">����</param>
    public void UseSkill(Skill skill, Vector3 target)
    {
        if (!status.skills.Contains(skill))
            return;
        status.OnRepeatUseSkillOnPoint -= UseSkill;
        if (Repeat)
            status.OnRepeatUseSkillOnPoint += UseSkill;

        ListenCancelWaitCastSkill();
        cancelWaitCastSkill = false;

        if (Stand)
            StandUseSkill(skill, target);
        else
            PursuitUseSkill(skill, target);
    }

    public void CancelWaitCastSkill(Transform a, Transform b)
    {
        CancelWaitCastSkill(null);
    }

    public void CancelWaitCastSkill(ButtonSkill buttonSkill)
    {
        if (!armyUI.toggle.isOn)
            return;
        conteinerSkill.OnClickAnyButtonSkills -= CancelWaitCastSkill;
        anchors.OnChangePositions -= CancelWaitCastSkill;
        status.OnRepeatUseSkillOnPersons -= UseSkill;
        status.OnRepeatUseSkillOnPoint -= UseSkill;
        cancelWaitCastSkill = true;
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
    ///     �������������
    /// </summary>
    public void StartPatrol(Skill skill)
    {
        if (patrolCoroutine == null)
        {
            PatrolSkill = skill;
            patrolCoroutine = StartCoroutine(IPatrol(skill));
            conteinerSkill.UpdatePatrolUI();
        }
    }

    public void StopPatrol()
    {
        if (patrolCoroutine != null)
        {
            status.OnRepeatUseSkillOnPersons -= UseSkill;
            ForgetTargetArmy();
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
            PatrolSkill = null;
            conteinerSkill.UpdatePatrolUI();
        }
    }

    private IEnumerator IPatrol(Skill skill)
    {
        while (true)
        {
            if (persons[0].armyTarget == null && AnyEnemyInRange(out Army army, skill))
                UseSkill(skill, army.persons.ToArray());

            yield return new WaitForSeconds(PATROL_DELAY);
        }
    }

    public void SetActive(bool on)
    {
        armyGlobalUI.toggle.SetIsOnWithoutNotify(on);
        for (int i = 0; i < persons.Count; i++)
            persons[i].target.gameObject.SetActive(on);
        anchors.SetActive(on);
    }

    /// <summary>
    ///     ������������� ������ �� ����
    /// </summary>
    /// <param name="target">����</param>
    public void TargetForUseSkill(Army target)
    {
        ClearTargetUseSkill();
        if (target.TryGetValueOtherType(out Person person))
            UseSkill(battlefield._targetSkill, person);
        if (target.TryGetValueOtherType(out Army army))
            UseSkill(battlefield._targetSkill, army.persons.ToArray());
    }

    /// <summary>
    ///     ������������� ������ �� ����
    /// </summary>
    /// <param name="target">����</param>
    public void TargetForUseSkill(Vector3 target)
    {
        ClearTargetUseSkill();
        UseSkill(battlefield._targetSkill, target);
    }

    /// <summary>
    ///     ������������� ������ �������
    /// </summary>
    public void TargetForUseSkill()
    {
        ClearTargetUseSkill();
        ListenCancelWaitCastSkill();
    }

    [Button("MoveUpdate")]
    public void MoveUpdate()
    {
        for (int id = 0; id < persons.Count; id++)
            persons[id].MoveUpdate();
    }

    #endregion Methods
}
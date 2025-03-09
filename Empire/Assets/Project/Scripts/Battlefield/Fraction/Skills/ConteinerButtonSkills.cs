#region

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class ConteinerButtonSkills : MonoBehaviour
{
    #region Fields

    private Transform tr;
    private float timerSkillCast;
    private Coroutine coroutine;
    public Action<ButtonSkill> OnClickAnyButtonSkills;
    public List<ButtonSkill> buttonSkills = new();

    #endregion Fields

    #region Methods

    private void Awake()
    {
        tr = transform;
    }

    private IEnumerator ITimerSkillCast()
    {
        while (timerSkillCast > 0)
        {
            yield return new WaitForFixedUpdate();
            timerSkillCast -= Time.fixedDeltaTime;
        }

        Silence(false);
        coroutine = null;
        timerSkillCast = 0;
    }

    private void ClickAnyButtonSkills(ButtonSkill buttonSkill)
    {
        OnClickAnyButtonSkills?.Invoke(buttonSkill);
    }

    private void AddTimerSkillCast(Army army)
    {
        Silence(army, army.status.timerSkillCast > timerSkillCast ? army.status.timerSkillCast : timerSkillCast);
    }

    /// <summary>
    ///     ���������� ������������� ������������
    /// </summary>
    /// <param name="on">�����������?</param>
    private void Silence(bool on)
    {
        for (int id = 0; id < buttonSkills.Count; id++)
            buttonSkills[id].Silence = on;
    }

    private bool Silence(Army initiator, float time)
    {
        bool exist = false;
        for (int id = 0; id < buttonSkills.Count; id++)
            if (buttonSkills[id].initiatorArmies.ContainsKey(initiator))
            {
                exist = true;
                break;
            }

        if (!exist)
            return false;
        if (time > 0)
        {
            Silence(true);
            timerSkillCast = time;
        }

        if (coroutine != null || time <= 0)
            return true;
        coroutine = StartCoroutine(ITimerSkillCast());
        return true;
    }

    public void Clear()
    {
        buttonSkills.Clear();
        tr.Clear();
    }

    public void UpdatePatrolUI()
    {
        for (int id = 0; id < buttonSkills.Count; id++)
            buttonSkills[id].UpdatePatrolUI();
    }

    public void Add(Army army, Skill skill)
    {
        int id = IndexOf(skill.buttonSkillPrefab);
        if (id == -1)
        {
            ButtonSkill buttonSkill = Instantiate(skill.buttonSkillPrefab, tr);
            buttonSkill.Build(army, skill);
            buttonSkill.Add(army);
            buttonSkill.button.onClick.AddListener(() => ClickAnyButtonSkills(buttonSkill));
            buttonSkills.Add(buttonSkill);
            AddTimerSkillCast(army);
            UpdatePatrolUI();
            return;
        }

        buttonSkills[id].Add(army);
        AddTimerSkillCast(army);
        UpdatePatrolUI();
    }

    /// <summary>
    ///     ������� ����� �� ����� � ����������.
    /// </summary>
    /// <param name="army">�����, �� ������� ���������� ������� �����.</param>
    /// <param name="skill">�����, ������� ����� �������.</param>
    public void Remove(Army army, Skill skill)
    {
        // �������� ������ ������ � ������ ������ �������
        int id = IndexOf(skill.buttonSkillPrefab);
        // ���� ������ ����� -1, ������ ����� �� ������, ������������
        if (id == -1)
            return;

        // ������� ����� �� ������ ����������� ������
        buttonSkills[id].Remove(army);

        // ���� ������ ����������� ������, ������� ������ ������ � ������� �� ������ ������
        if (buttonSkills[id].initiatorArmies.Count == 0)
        {
            buttonSkills[id].button.onClick.RemoveListener(() => ClickAnyButtonSkills(buttonSkills[id]));
            Destroy(buttonSkills[id].gameObject);
            _ = buttonSkills.Remove(buttonSkills[id]);
        }

        // ���� ������ ����� ������ �� ����� 0, ������ ���� ���� ������
        if (timerSkillCast == 0)
            return;
        // �������� ������ ����� � ������������� ��� ��� ���� ����������� �������
        timerSkillCast = 0;
        for (int idButtonSkill = 0; idButtonSkill < buttonSkills.Count; idButtonSkill++)
            foreach (var initiatorArmy in buttonSkills[idButtonSkill].initiatorArmies)
                AddTimerSkillCast(initiatorArmy.Key);

        UpdatePatrolUI();
    }

    public bool Reload(Army army, Skill skill)
    {
        Silence(army, skill);
        if (Contains(army, skill, out ButtonSkill buttonSkill))
        {
            buttonSkill.Reload();
            return true;
        }

        return false;
    }

    public bool Contains(Army army, Skill skill, out ButtonSkill buttonSkill)
    {
        buttonSkill = null;
        for (int id = 0; id < buttonSkills.Count; id++)
            if (buttonSkills[id].targetSkill == skill)
            {
                if (buttonSkills[id].initiatorArmies.ContainsKey(army))
                {
                    buttonSkill = buttonSkills[id];
                    return true;
                }

                return false;
            }

        return false;
    }

    public int IndexOf(ButtonSkill buttonSkill)
    {
        for (int id = 0; id < buttonSkills.Count; id++)
            if (buttonSkills[id].prefabID == buttonSkill.prefabID)
                return id;

        return -1;
    }

    public bool Silence(Army army, Skill skill)
    {
        if (Contains(army, skill, out ButtonSkill buttonSkill))
        {
            if (skill.timeCast > 0)
            {
                Silence(true);
                timerSkillCast = skill.timeCast;
                coroutine ??= StartCoroutine(ITimerSkillCast());
            }

            return true;
        }

        return false;
    }

    #endregion Methods
}
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class ConteinerButtonSkills : MonoBehaviour
{
    public Action OnClickAnyButtonSkills;
    #region Fields

    private Transform tr;
    public List<ButtonSkill> buttonSkills = new();
    private float timerSkillCast;
    private Coroutine coroutine;
    #endregion Fields

    #region Methods

    private void Awake() => tr = transform;
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
    public void Clear()
    {
        buttonSkills.Clear();
        tr.Clear();
    }
    private void ClickAnyButtonSkills() => OnClickAnyButtonSkills?.Invoke();
    public void Add(Army army, Skill skill)
    {
        int id = IndexOf(skill.buttonSkillPrefab);
        if (id == -1)
        {
            ButtonSkill buttonSkill = Instantiate(skill.buttonSkillPrefab, tr);
            buttonSkill.Build(army, skill);
            buttonSkill.Add(army);
            buttonSkill.button.onClick.AddListener(() => ClickAnyButtonSkills());
            buttonSkills.Add(buttonSkill);
            AddTimerSkillCast(army);
            return;
        }

        buttonSkills[id].Add(army);
        AddTimerSkillCast(army);
    }
    private void AddTimerSkillCast(Army army) => Silence(army.status.timerSkillCast > timerSkillCast ? army.status.timerSkillCast : timerSkillCast);
    /// <summary>
    /// ������� ����� �� ����� � ����������.
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
            buttonSkills[id].button.onClick.RemoveListener(() => ClickAnyButtonSkills());
            Destroy(buttonSkills[id].gameObject);
            _ = buttonSkills.Remove(buttonSkills[id]);
        }

        // ���� ������ ����� ������ �� ����� 0, ������ ���� ���� ������
        if (timerSkillCast == 0)
            return;
        // �������� ������ ����� � ������������� ��� ��� ���� ����������� �������
        timerSkillCast = 0;
        for (int idButtonSkill = 0; idButtonSkill < buttonSkills.Count; idButtonSkill++)
        {
            foreach (KeyValuePair<Army, UnityAction> initiatorArmy in buttonSkills[idButtonSkill].initiatorArmies)
                AddTimerSkillCast(initiatorArmy.Key);
        }
    }

    public int IndexOf(ButtonSkill buttonSkill)
    {
        for (int id = 0; id < buttonSkills.Count; id++)
        {
            if (buttonSkills[id].prefabID == buttonSkill.prefabID)
                return id;
        }

        return -1;
    }
    /// <summary>
    /// ���������� ������������� ������������
    /// </summary>
    /// <param name="on">�����������?</param>
    private void Silence(bool on)
    {
        for (int id = 0; id < buttonSkills.Count; id++)
            buttonSkills[id].Silence = on;
    }
    public void Silence(float time)
    {
        if (time > 0)
        {
            Silence(true);
            timerSkillCast = time;
        }

        if (coroutine != null || time <= 0)
            return;
        coroutine = StartCoroutine(ITimerSkillCast());
    }
    #endregion Methods
}
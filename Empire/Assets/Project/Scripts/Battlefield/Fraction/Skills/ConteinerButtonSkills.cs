using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

public class ConteinerButtonSkills : MonoBehaviour
{
    #region Fields

    private Transform tr;
    private float timerSkillCast;
    private Coroutine coroutine;
    public Action OnClickAnyButtonSkills;
    public List<ButtonSkill> buttonSkills = new();

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

    private void ClickAnyButtonSkills() => OnClickAnyButtonSkills?.Invoke();

    private void AddTimerSkillCast(Army army) => Silence(army, army.status.timerSkillCast > timerSkillCast ? army.status.timerSkillCast : timerSkillCast);

    /// <summary>
    /// Блокировка использования способностей
    /// </summary>
    /// <param name="on">блокировать?</param>
    private void Silence(bool on)
    {
        for (int id = 0; id < buttonSkills.Count; id++)
            buttonSkills[id].Silence = on;
    }

    private bool Silence(Army initiator, float time)
    {
        bool exist = false;
        for (int id = 0; id < buttonSkills.Count; id++)
        {
            if (buttonSkills[id].initiatorArmies.ContainsKey(initiator))
            {
                exist = true;
                break;
            }
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

    /// <summary>
    /// Удаляет навык из армии и интерфейса.
    /// </summary>
    /// <param name="army">Армия, из которой необходимо удалить навык.</param>
    /// <param name="skill">Навык, который нужно удалить.</param>
    public void Remove(Army army, Skill skill)
    {
        // Получаем индекс навыка в списке кнопок навыков
        int id = IndexOf(skill.buttonSkillPrefab);
        // Если индекс равен -1, значит навык не найден, возвращаемся
        if (id == -1)
            return;

        // Удаляем армию из списка инициаторов навыка
        buttonSkills[id].Remove(army);

        // Если список инициаторов пустой, удаляем кнопку навыка и удаляем из списка кнопок
        if (buttonSkills[id].initiatorArmies.Count == 0)
        {
            buttonSkills[id].button.onClick.RemoveListener(() => ClickAnyButtonSkills());
            Destroy(buttonSkills[id].gameObject);
            _ = buttonSkills.Remove(buttonSkills[id]);
        }

        // Если таймер каста навыка не равен 0, значит идет каст навыка
        if (timerSkillCast == 0)
            return;
        // Обнуляем таймер каста и перезапускаем его для всех инициаторов навыков
        timerSkillCast = 0;
        for (int idButtonSkill = 0; idButtonSkill < buttonSkills.Count; idButtonSkill++)
        {
            foreach (KeyValuePair<Army, UnityAction> initiatorArmy in buttonSkills[idButtonSkill].initiatorArmies)
                AddTimerSkillCast(initiatorArmy.Key);
        }
    }

    public bool Reload(Army army, Skill skill)
    {
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
        {
            if (buttonSkills[id].skillTarget == skill)
            {
                if (buttonSkills[id].initiatorArmies.ContainsKey(army))
                {
                    buttonSkill = buttonSkills[id];
                    return true;
                }

                return false;
            }
        }

        return false;
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
#region

using System;
using System.Collections;
using AdvancedEditorTools.Attributes;
using UnityEngine;

#endregion

public partial class Army : MonoBehaviour // �������������� �����
{
    #region Events

    public event Action<Army> OnDeadArmy;

    #endregion Events

    #region Fields

    private float healthArmy, manaArmy, staminaArmy, moralityArmy;
    public Status status;

    /// <summary>
    ///     ���������� ������ � ������
    /// </summary>
    public int countWarriors;

    #endregion Fields

    #region Methods

    private IEnumerator UIArmyUpdate()
    {
        yield return new WaitForFixedUpdate();

        while (true)
        {
            moralityArmy = 0;
            healthArmy = 0;
            manaArmy = 0;
            staminaArmy = 0;
            for (int i = 0; i < persons.Count; i++)
            {
                moralityArmy += persons[i].morality;
                healthArmy += persons[i].health;
                manaArmy += persons[i].mana;
                staminaArmy += persons[i].stamina;
            }

            if (!status.fraction.bot)
                UpdateStatusUI(armyUI, armyGlobalUI);
            // ���� �������� ����� ����� ������ 0, �������� ������� OnDeadArmy
            if (healthArmy == 0)
                OnDeadArmy?.Invoke(this);

            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateStatusUI(StatusUI statusUI)
    {
        statusUI.healthSlider.maxValue = persons.Count * status.maxHealth;
        statusUI.manaSlider.maxValue = persons.Count * status.maxMana;
        statusUI.staminaSlider.maxValue = persons.Count * status.maxStamina;
        statusUI.moralitySlider.maxValue = persons.Count * status.maxMorality;

        statusUI.manaSlider.value = manaArmy;
        statusUI.staminaSlider.value = staminaArmy;
        statusUI.moralitySlider.value = moralityArmy;
        statusUI.healthSlider.value = healthArmy;
        statusUI.countWarriors.text = persons.Count.ToString();
    }

    private void UpdateStatusUI(params StatusUI[] statusUI)
    {
        for (int idStatusUI = 0; idStatusUI < statusUI.Length; idStatusUI++)
            UpdateStatusUI(statusUI[idStatusUI]);
    }

    //[ContextMenu("Kill")]
    [Button("Kill", 15)]
    public void Kill()
    {
        for (int id = persons.Count - 1; id >= 0; id--)
            persons[id].Kill();
    }

    #endregion Methods
}
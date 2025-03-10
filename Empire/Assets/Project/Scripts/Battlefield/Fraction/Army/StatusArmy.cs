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
            for (int i = 0; i < Persons.Count; i++)
            {
                moralityArmy += Persons[i].morality;
                healthArmy += Persons[i].health;
                manaArmy += Persons[i].mana;
                staminaArmy += Persons[i].stamina;
            }

            if (!status.Fraction.Bot)
                UpdateStatusUI(ArmyUI, ArmyGlobalUI);
            // ���� �������� ����� ����� ������ 0, �������� ������� OnDeadArmy
            if (healthArmy == 0)
                OnDeadArmy?.Invoke(this);

            yield return new WaitForSeconds(1);
        }
    }

    private void UpdateStatusUI(StatusUI statusUI)
    {
        statusUI.healthSlider.maxValue = Persons.Count * status.MaxHealth;
        statusUI.manaSlider.maxValue = Persons.Count * status.MaxMana;
        statusUI.staminaSlider.maxValue = Persons.Count * status.MaxStamina;
        statusUI.moralitySlider.maxValue = Persons.Count * status.MaxMorality;

        statusUI.manaSlider.value = manaArmy;
        statusUI.staminaSlider.value = staminaArmy;
        statusUI.moralitySlider.value = moralityArmy;
        statusUI.healthSlider.value = healthArmy;
        statusUI.countWarriors.text = Persons.Count.ToString();
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
        for (int id = Persons.Count - 1; id >= 0; id--)
            Persons[id].Kill();
    }

    #endregion Methods
}
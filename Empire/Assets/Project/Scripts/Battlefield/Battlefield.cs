using System;
using System.Collections.Generic;

using UnityEngine;

using static Skill;

public class Battlefield : MonoBehaviour
{
    #region Events

    public event Action<ICombatUnit> OnSetTarget;

    #endregion Events

    #region Properties

    public static Battlefield singleton { get; private set; }

    #endregion Properties

    #region Fields

    [SerializeField]
    private Color selfColor, friendColor, enemyColor;

    [SerializeField]
    private FractionBattlefield[] fractions;

    internal ButtonSkill targetButtonSkill;

    public FractionBattlefield playerFraction;

    #endregion Fields

    #region Methods

    private void Awake()
    {
        InitializeSingleton();
        DeactiveAllArmies();
    }

    /// <summary>
    /// Активировать армию для нажатия
    /// </summary>
    /// <param name="army">армия</param>
    /// <param name="on">активировать?</param>
    private void SetActiveArmy(Army army, bool on)
    {
        army.buttonArmy.interactable = on;
        // Если это наша фракция, скрываем глобальный UI армии
        // (только в нашей фракции есть глобальный UI для выбора армии)
        if (playerFraction == army.status.fraction)
        {
            army.buttonArmy.gameObject.SetActive(on);
            army.armyGlobalUI.gameObject.SetActive(!on);
        }
    }

    /// <summary>
    /// Деактивировать все армии для нажатия
    /// </summary>
    private void DeactiveAllArmies()
    {
        List<Army> armies;
        for (int idFraction = 0; idFraction < fractions.Length; idFraction++)
        {
            armies = fractions[idFraction].armies;
            for (int idArmy = 0; idArmy < armies.Count; idArmy++)
                SetActiveArmy(armies[idArmy], false);
        }
    }

    public void InitializeSingleton()
    {
        if (singleton != null)
        {
            Debug.LogError("singleton > 1");
            return;
        }

        singleton = this;
    }

    /// <summary>
    /// Выбор цели
    /// </summary>
    /// <param name="target">цель</param>
    public void SetTargetArmy(ICombatUnit target)
    {
        OnSetTarget?.Invoke(target);
        //targetButtonSkill.OnArmySkillRun();

        DeactiveAllArmies();
    }

    /// <summary>
    /// Активировать армии для нажатия с ограничением в виде триггера
    /// </summary>
    /// <param name="trigger">триггер</param>
    /// <param name="armyInitiator">армия - инициатор</param>
    /// <returns>Активированые армии</returns>
    public void SetActiveArmies(TriggerType trigger, Army armyInitiator)
    {
        DeactiveAllArmies();

        for (int idFraction = 0; idFraction < fractions.Length; idFraction++)
        {
            for (int idArmy = 0; idArmy < fractions[idFraction].armies.Count; idArmy++)
            {
                if (OnTrigger(trigger, armyInitiator, fractions[idFraction].armies[idArmy]))
                    SetActiveArmy(fractions[idFraction].armies[idArmy], true);
            }
        }
    }

    public Color GetColorFraction(FractionBattlefield fraction) => fraction.sideID != playerFraction.sideID ? enemyColor : playerFraction == fraction ? selfColor : friendColor;

    #endregion Methods
}
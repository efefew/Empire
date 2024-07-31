using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static Skill;

public class Battlefield : MonoBehaviour
{
    #region Events

    public event Action<ICombatUnit> OnSetTargetArmy;
    public event Action<Vector3> OnSetTargetPoint;

    #endregion Events

    #region Properties

    public static Battlefield singleton { get; private set; }

    #endregion Properties

    #region Fields

    [SerializeField]
    private Color selfColor, friendColor, enemyColor;

    [SerializeField]
    private FractionBattlefield[] fractions;

    private bool pointTargetclicked;
    internal Skill targetSkill;

    public Vector3 screenPosition { get; private set; }
    public Vector3 worldPosition { get; private set; }
    public FractionBattlefield playerFraction;
    public Button pointTarget;
    #endregion Fields

    #region Methods
    private void OnGUI()
    {
        // Получаем основную камеру
        Camera c = Camera.main;

        // Получаем текущее событие мыши
        Event e = Event.current;

        // Получаем позицию мыши на экране
        Vector2 mousePos = new()
        {
            x = e.mousePosition.x,
            y = c.pixelHeight - e.mousePosition.y
        };

        // Создаем вектор 3D-позиции на экране
        // с учетом ближней плоскости отсечения
        screenPosition = new Vector3(mousePos.x, mousePos.y, c.nearClipPlane);

        // Преобразуем 2D-координаты на экране в 3D-координаты в мировом пространстве
        worldPosition = c.ScreenToWorldPoint(screenPosition);
    }
    private void Awake()
    {
        InitializeSingleton();
        DeactiveAllArmies();
        pointTarget.onClick.AddListener(() =>
        {
            SetTargetPoint(pointTarget.transform.position);
            pointTarget.gameObject.SetActive(false);
            // pointTargetclicked = true;
            targetSkill = null;
        });
        pointTarget.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (targetSkill == null || !targetSkill.pointCanBeTarget)
            return;
        if (!Input.GetKeyUp(KeyCode.Mouse0) || MyExtentions.IsPointerOverUI())
            return;
        CreatePointTarget(worldPosition);
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
    public void DeactiveAllArmies()
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
        OnSetTargetArmy?.Invoke(target);
        targetSkill = null;
        DeactiveAllArmies();
    }
    public void SetTargetPoint(Vector3 target)
    {
        OnSetTargetPoint?.Invoke(target);
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
    public void CreatePointTarget(Vector3 point)
    {
        if (pointTargetclicked)
        {
            pointTargetclicked = false;
            return;
        }

        pointTarget.gameObject.SetActive(true);
        pointTarget.transform.position = point;
    }
    public Color GetColorFraction(FractionBattlefield fraction) => fraction.sideID != playerFraction.sideID ? enemyColor : playerFraction == fraction ? selfColor : friendColor;

    #endregion Methods
}
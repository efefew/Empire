#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static Skill;

#endregion

public class Battlefield : MonoBehaviour
{
    #region Properties

    public static Battlefield Singleton { get; private set; }

    #endregion Properties

    #region Events

    public event Action<Skill> OnStartPatrol;
    public event Action OnStopPatrol;
    public event Action<Army> OnSetTargetArmy;

    public event Action<Vector3> OnSetTargetPoint;

    #endregion Events

    #region Fields

    private Vector3 ScreenPosition { get; set; }

    public Vector3 WorldPosition { get; private set; }

    [FormerlySerializedAs("selfColor")] [SerializeField] private Color _selfColor;
    [FormerlySerializedAs("friendColor")] [SerializeField] private Color _friendColor;
    [FormerlySerializedAs("enemyColor")] [SerializeField] private Color _enemyColor;

    internal Skill _targetSkill;
    [FormerlySerializedAs("fractions")] public FractionBattlefield[] Fractions;

    [FormerlySerializedAs("conteinerSkill")] public ConteinerButtonSkills ConteinerSkill;
    [FormerlySerializedAs("playerFraction")] public FractionBattlefield PlayerFraction;
    private bool _skillButtonsPopup;
    [FormerlySerializedAs("pointTarget")] public Button PointTarget;
    [FormerlySerializedAs("patrol")] public Button Patrol;
    [FormerlySerializedAs("toggleArmyGroup")] public Toggle ToggleArmyGroup;
    [FormerlySerializedAs("toggleStand")] public Toggle ToggleStand;
    [FormerlySerializedAs("toggleRepeat")] public Toggle ToggleRepeat;
    private Camera _c;

    public Battlefield(Vector3 screenPosition)
    {
        ScreenPosition = screenPosition;
    }

    #endregion Fields

    #region Methods

    private void Start()
    {
        _c = Camera.main;
    }

    private void OnGUI()
    {
        Event e = Event.current;

        Vector2 mousePos = new()
        {
            x = e.mousePosition.x,
            y = _c.pixelHeight - e.mousePosition.y
        };

        ScreenPosition = new Vector3(mousePos.x, mousePos.y, _c.nearClipPlane);

        WorldPosition = _c.ScreenToWorldPoint(ScreenPosition);
    }

    private void Awake()
    {
        InitializeSingleton();
        _skillButtonsPopup = false;
        DeactiveAllArmies();
        ToggleStand.onValueChanged.AddListener(on =>
        {
            if (!on) StopPatrol();
        });
        ToggleRepeat.onValueChanged.AddListener(on =>
        {
            if (!on) StopPatrol();
        });
        PointTarget.onClick.AddListener(() =>
        {
            SetTargetPoint(PointTarget.transform.position);
            PointTarget.gameObject.SetActive(false);
            _targetSkill = null;
        });
        PointTarget.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            RemoveSkillAditionalUI();
            return;
        }

        if (_targetSkill == null || !_targetSkill.pointCanBeTarget) return;

        if (!Input.GetKeyUp(KeyCode.Mouse0) || MyExtentions.IsPointerOverUI()) return;

        CreatePointTarget(WorldPosition);
    }

    /// <summary>
    ///     Активировать армию для нажатия
    /// </summary>
    /// <param name="army">армия</param>
    /// <param name="on">Активировать?</param>
    private void SetActiveArmy(Army army, bool on)
    {
        army.buttonArmy.interactable = on;

        // Если это наша фракция, скрываем глобальный UI армии
        // (только в нашей фракции есть глобальный UI для выбора армии)
        if (PlayerFraction == army.status.fraction)
        {
            army.buttonArmy.gameObject.SetActive(on);
            army.armyGlobalUI.gameObject.SetActive(!on);
        }
    }

    /// <summary>
    ///     Деактивировать все армии для нажатия
    /// </summary>
    public void DeactiveAllArmies()
    {
        List<Army> armies;
        for (int idFraction = 0; idFraction < Fractions.Length; idFraction++)
        {
            armies = Fractions[idFraction].armies;
            for (int idArmy = 0; idArmy < armies.Count; idArmy++) SetActiveArmy(armies[idArmy], false);
        }
    }

    public void InitializeSingleton()
    {
        if (Singleton != null)
        {
            Debug.LogError("singleton > 1");
            return;
        }

        Singleton = this;
    }

    /// <summary>
    ///     Выбор цели
    /// </summary>
    /// <param name="target">цель</param>
    public void SetTargetArmy(Army target)
    {
        OnSetTargetArmy?.Invoke(target);
        _targetSkill = null;
        DeactiveAllArmies();
    }

    /// <summary>
    ///     Выбор точки
    /// </summary>
    /// <param name="target">точка</param>
    public void SetTargetPoint(Vector3 target)
    {
        OnSetTargetPoint?.Invoke(target);
        DeactiveAllArmies();
    }

    /// <summary>
    ///     Патрулировать
    /// </summary>
    private void StartPatrol(Skill skill)
    {
        _targetSkill = null;
        DeactiveAllArmies();
        ToggleStand.isOn = true;
        ToggleRepeat.isOn = true;
        // реализовать индикатор на кнопке навыка
        // патрулировать
        OnStartPatrol?.Invoke(skill);
    }

    public void StopPatrol()
    {
        OnStopPatrol?.Invoke();
    }

    /// <summary>
    ///     Активировать армии для нажатия с ограничением в виде триггера
    /// </summary>
    /// <param name="trigger">триггер</param>
    /// <param name="armyInitiator">Армия - инициатор</param>
    /// <returns>Активированные армии</returns>
    public void ActiveArmies(TriggerType trigger, Army armyInitiator)
    {
        DeactiveAllArmies();

        for (int idFraction = 0; idFraction < Fractions.Length; idFraction++)
        for (int idArmy = 0; idArmy < Fractions[idFraction].armies.Count; idArmy++)
            if (OnTrigger(trigger, armyInitiator, Fractions[idFraction].armies[idArmy]))
                SetActiveArmy(Fractions[idFraction].armies[idArmy], true);
    }

    public void ActiveSkillButtons(Skill skill)
    {
        if (!(skill.collective || skill.сanBePatrol)) return;

        _skillButtonsPopup = true;
        if (skill.collective)
        {
        }

        if (skill.сanBePatrol)
        {
            Patrol.gameObject.SetActive(true);
            Patrol.onClick.RemoveAllListeners();
            Patrol.onClick.AddListener(() => StartPatrol(skill));
        }
    }

    public void RemoveSkillAditionalUI()
    {
        if (!_skillButtonsPopup)
        {
            if (Patrol.gameObject.activeSelf) Patrol.gameObject.SetActive(false);
        }
        else
        {
            _skillButtonsPopup = false;
        }

        if (PointTarget.gameObject.activeSelf && !MyExtentions.IsPointerOverUI(PointTarget.gameObject))
        {
            _targetSkill = null;
            DeactiveAllArmies();
            PointTarget.gameObject.SetActive(false);
        }
    }

    public void CreatePointTarget(Vector3 point)
    {
        DeactiveAllArmies();
        PointTarget.gameObject.SetActive(true);
        PointTarget.transform.position = point;
    }

    public Color GetColorFraction(FractionBattlefield fraction)
    {
        return fraction.sideID != PlayerFraction.sideID
            ? _enemyColor
            : PlayerFraction == fraction
                ? _selfColor
                : _friendColor;
    }

    #endregion Methods
}
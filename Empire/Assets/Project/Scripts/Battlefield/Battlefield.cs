using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static Skill;

public class Battlefield : MonoBehaviour
{
    #region Events

    public event Action<Skill> OnStartPatrol;
    public event Action OnStopPatrol;
    public event Action<Army> OnSetTargetArmy;

    public event Action<Vector3> OnSetTargetPoint;

    #endregion Events

    #region Properties

    public static Battlefield singleton { get; private set; }

    #endregion Properties

    #region Fields

    public Vector3 screenPosition { get; private set; }

    public Vector3 worldPosition { get; private set; }

    [SerializeField]
    private Color selfColor, friendColor, enemyColor;

    internal Skill targetSkill;
    public FractionBattlefield[] fractions;

    public ConteinerButtonSkills conteinerSkill;
    public FractionBattlefield playerFraction;
    private bool skillButtonsPopup;
    public Button pointTarget, patrol;
    public Toggle toggleArmyGroup, toggleStand, toggleRepeat;

    #endregion Fields

    #region Methods

    private void OnGUI()
    {
        Camera c = Camera.main;
        Event e = Event.current;

        Vector2 mousePos = new()
        {
            x = e.mousePosition.x,
            y = c.pixelHeight - e.mousePosition.y
        };

        screenPosition = new Vector3(mousePos.x, mousePos.y, c.nearClipPlane);

        worldPosition = c.ScreenToWorldPoint(screenPosition);
    }

    private void Awake()
    {
        InitializeSingleton();
        skillButtonsPopup = false;
        DeactiveAllArmies();
        toggleStand.onValueChanged.AddListener((bool on) =>
        {
            if (!on)
                StopPatrol();
        });
        toggleRepeat.onValueChanged.AddListener((bool on) =>
        {
            if (!on)
                StopPatrol();
        });
        pointTarget.onClick.AddListener(() =>
        {
            SetTargetPoint(pointTarget.transform.position);
            pointTarget.gameObject.SetActive(false);
            targetSkill = null;
        });
        pointTarget.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            RemoveSkillAditionalUI();
            return;
        }

        if (targetSkill == null || !targetSkill.pointCanBeTarget)
            return;

        if (!Input.GetKeyUp(KeyCode.Mouse0) || MyExtentions.IsPointerOverUI())
            return;

        CreatePointTarget(worldPosition);
    }

    /// <summary>
    /// ������������ ����� ��� �������
    /// </summary>
    /// <param name="army">�����</param>
    /// <param name="on">������������?</param>
    private void SetActiveArmy(Army army, bool on)
    {
        army.buttonArmy.interactable = on;

        // ���� ��� ���� �������, �������� ���������� UI �����
        // (������ � ����� ������� ���� ���������� UI ��� ������ �����)
        if (playerFraction == army.status.fraction)
        {
            army.buttonArmy.gameObject.SetActive(on);
            army.armyGlobalUI.gameObject.SetActive(!on);
        }
    }

    /// <summary>
    /// �������������� ��� ����� ��� �������
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
    /// ����� ����
    /// </summary>
    /// <param name="target">����</param>
    public void SetTargetArmy(Army target)
    {
        OnSetTargetArmy?.Invoke(target);
        targetSkill = null;
        DeactiveAllArmies();
    }

    /// <summary>
    /// ����� �����
    /// </summary>
    /// <param name="target">�����</param>
    public void SetTargetPoint(Vector3 target)
    {
        OnSetTargetPoint?.Invoke(target);
        DeactiveAllArmies();
    }

    /// <summary>
    /// �������������
    /// </summary>
    public void StartPatrol(Skill skill)
    {
        targetSkill = null;
        DeactiveAllArmies();
        toggleStand.isOn = true;
        toggleRepeat.isOn = true;
        // ����������� �������� �� ������ ������
        // �������������
        OnStartPatrol.Invoke(skill);
    }
    public void StopPatrol() => OnStopPatrol?.Invoke();
    /// <summary>
    /// ������������ ����� ��� ������� � ������������ � ���� ��������
    /// </summary>
    /// <param name="trigger">�������</param>
    /// <param name="armyInitiator">����� - ���������</param>
    /// <returns>������������� �����</returns>
    public void ActiveArmies(TriggerType trigger, Army armyInitiator)
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
    public void ActiveSkillButtons(Skill skill)
    {
        if (!(skill.collective || skill.�anBePatrol))
            return;
        skillButtonsPopup = true;
        if (skill.collective)
        { }

        if (skill.�anBePatrol)
        {
            patrol.gameObject.SetActive(true);
            patrol.onClick.RemoveAllListeners();
            patrol.onClick.AddListener(() => StartPatrol(skill));
        }
    }
    public void RemoveSkillAditionalUI()
    {
        if (!skillButtonsPopup)
        {
            if (patrol.gameObject.activeSelf)
                patrol.gameObject.SetActive(false);
        }
        else
        {
            skillButtonsPopup = false;
        }

        if (pointTarget.gameObject.activeSelf && !MyExtentions.IsPointerOverUI(pointTarget.gameObject))
        {
            targetSkill = null;
            DeactiveAllArmies();
            pointTarget.gameObject.SetActive(false);
        }
    }

    public void CreatePointTarget(Vector3 point)
    {
        DeactiveAllArmies();
        pointTarget.gameObject.SetActive(true);
        pointTarget.transform.position = point;
    }

    public Color GetColorFraction(FractionBattlefield fraction) => fraction.sideID != playerFraction.sideID ? enemyColor : playerFraction == fraction ? selfColor : friendColor;

    #endregion Methods
}
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
        // �������� �������� ������
        Camera c = Camera.main;

        // �������� ������� ������� ����
        Event e = Event.current;

        // �������� ������� ���� �� ������
        Vector2 mousePos = new()
        {
            x = e.mousePosition.x,
            y = c.pixelHeight - e.mousePosition.y
        };

        // ������� ������ 3D-������� �� ������
        // � ������ ������� ��������� ���������
        screenPosition = new Vector3(mousePos.x, mousePos.y, c.nearClipPlane);

        // ����������� 2D-���������� �� ������ � 3D-���������� � ������� ������������
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
    /// ������������ ����� ��� ������� � ������������ � ���� ��������
    /// </summary>
    /// <param name="trigger">�������</param>
    /// <param name="armyInitiator">����� - ���������</param>
    /// <returns>������������� �����</returns>
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
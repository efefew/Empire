using System;
using System.Collections.Generic;

using AdvancedEditorTools.Attributes;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������� �� ���� �����
/// </summary>
[RequireComponent(typeof(PointsAB))]
public class FractionBattlefield : MonoBehaviour
{
    #region Properties

    public PointsAB mainAB { get; private set; }

    #endregion Properties

    #region Fields

    [SerializeField]
    private Transform conteinerGlobal, conteinerArmy;

    [SerializeField]
    private ToggleGroup conteinerToggle;

    private Battlefield battlefield;

    [ReadOnly]
    public Bot bot;

    public ulong sideID;

    [SerializeField]
    public List<Army> armies = new();

    public List<ArmyInformation> armiesInfo;

    public Transform start, end;

    #endregion Fields

    #region Methods

    //public float angle;
    private void Awake()
    {
        mainAB = GetComponent<PointsAB>();
        bot ??= GetComponent<Bot>();
        conteinerArmy = transform;
        battlefield = Battlefield.singleton;
        BuildFraction(start.position, end.position);
    }

    private void BuildFraction(Vector2 a, Vector2 b)
    {
        int countArmy = armiesInfo.Count;
        float distance = Mathf.Max(0, (Vector2.Distance(a, b) - (Army.OFFSET_BETWEEN_ARMIES * (countArmy - 1))) / countArmy);
        Transform point = new GameObject("point").transform;
        point.position = a;
        point.LookAt2D(b);

        if (!bot)
        {
            mainAB.conteinerToggle = conteinerToggle;
            battlefield.toggleArmyGroup.onValueChanged.AddListener((bool on) => mainAB.Group(on));
            //toggleStand.onValueChanged.AddListener((bool on) =>  );
        }

        for (int id = 0; id < armiesInfo.Count; id++)
        {
            Army army = Instantiate(armiesInfo[id].armyPack.army, conteinerArmy);
            army.name += $" {id}";
            armies.Add(army);
            mainAB.childrensAB.Add(army.anchors);
            army.anchors.parentAB = mainAB;
            army.OnDeadArmy += DeadArmy;
            Button buttonArmy = Instantiate(armiesInfo[id].armyPack.buttonArmy, conteinerGlobal);
            buttonArmy.GetComponent<Image>().color = battlefield.GetColorFraction(this);
            buttonArmy.onClick.AddListener(() => battlefield.SetTargetArmy(army));

            a = point.position;
            point.position += point.right * distance;
            b = point.position;
            BuildArmy(a, b, armiesInfo[id], army, buttonArmy);

            point.position += point.right * Army.OFFSET_BETWEEN_ARMIES;
        }
    }

    private void BuildArmy(Vector2 a, Vector2 b, ArmyInformation armyInfo, Army army, Button buttonArmy)
    {
        if (bot)
        {
            army.BuildArmy(a, b, this, buttonArmy);
            return;
        }

        StatusUI armyUI = Instantiate(armyInfo.armyPack.armyUI, conteinerToggle.transform);
        StatusUI armyGlobalUI = Instantiate(armyInfo.armyPack.armyGlobalUI, conteinerGlobal);
        armyGlobalUI.background.color = battlefield.GetColorFraction(this);

        Toggle toggle = armyUI.toggle;
        toggle.group = conteinerToggle;
        toggle.onValueChanged.AddListener((bool on) =>
        {
            if (on == true)
            {
                army.AddSkillsUI();
                battlefield.toggleRepeat.SetIsOnWithoutNotify(army.Repeat);
                battlefield.toggleStand.SetIsOnWithoutNotify(army.Stand);
                battlefield.DeactiveAllArmies();
                battlefield.toggleRepeat.onValueChanged.AddListener(army.SetRepeat);
                battlefield.toggleStand.onValueChanged.AddListener(army.SetStand);
                army.SetActive(true);
            }
            else
            {
                army.RemoveSkillsUI();
                battlefield.toggleRepeat.onValueChanged.RemoveListener(army.SetRepeat);
                battlefield.toggleStand.onValueChanged.RemoveListener(army.SetStand);
                army.SetActive(false);
            }
        });
        armyGlobalUI.toggle.onValueChanged.AddListener((bool on) => toggle.isOn = !toggle.isOn);

        army.BuildArmy(a, b, this, buttonArmy, armyUI, armyGlobalUI, battlefield.conteinerSkill);
    }

    private void DeadArmy(Army army)
    {
        army.OnDeadArmy -= DeadArmy;
        if (army.armyUI)
            Destroy(army.armyUI.gameObject);
        if (army.armyGlobalUI)
            Destroy(army.armyGlobalUI.gameObject);
        if (army.buttonArmy)
            Destroy(army.buttonArmy.gameObject);
        _ = armies.Remove(army);
        //Destroy(army);
    }

    [Button("Kill", 15)]
    public void Kill()
    {
        for (int id = armies.Count - 1; id >= 0; id--)
            armies[id].Kill();
    }

    #endregion Methods
}

[Serializable]
public class ArmyInformation
{
    #region Fields

    public List<Buff> buffs;
    public PackArmy armyPack;

    #endregion Fields
}
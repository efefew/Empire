#region

using System;
using System.Collections.Generic;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

/// <summary>
///     ������� �� ���� �����
/// </summary>
[RequireComponent(typeof(PointsAb))]
public class FractionBattlefield : MonoBehaviour
{
    private PointsAb MainAb { get; set; }
    
    [FormerlySerializedAs("conteinerGlobal")] [SerializeField] private Transform _containerGlobal;
    [FormerlySerializedAs("conteinerArmy")] [SerializeField] private Transform _containerArmy;
    [FormerlySerializedAs("conteinerToggle")] [SerializeField] private ToggleGroup _containerToggle;

    private Battlefield _battlefield;

    [FormerlySerializedAs("bot")] [ReadOnly] public Bot Bot;

    [FormerlySerializedAs("sideID")] public ulong SideID;

    [FormerlySerializedAs("armies")] [SerializeField] public List<Army> Armies = new();

    [FormerlySerializedAs("armiesInfo")] public List<ArmyInformation> ArmiesInfo;

    [FormerlySerializedAs("start")] public Transform Start;
    [FormerlySerializedAs("end")] public Transform End;

    //public float angle;
    private void Awake()
    {
        MainAb = GetComponent<PointsAb>();
        Bot ??= GetComponent<Bot>();
        _containerArmy = transform;
        _battlefield = Battlefield.Instance;
        BuildFraction(Start.position, End.position);
    }

    private void BuildFraction(Vector2 a, Vector2 b)
    {
        int countArmy = ArmiesInfo.Count;
        float distance = Mathf.Max(0,
            (Vector2.Distance(a, b) - Army.OFFSET_BETWEEN_ARMIES * (countArmy - 1)) / countArmy);
        Transform point = new GameObject("point").transform;
        point.position = a;
        point.LookAt2D(b);

        if (!Bot)
        {
            MainAb.ContainerToggle = _containerToggle;
            _battlefield.ToggleArmyGroup.onValueChanged.AddListener(on => MainAb.Group(on));
            //toggleStand.onValueChanged.AddListener((bool on) =>  );
        }

        for (int id = 0; id < ArmiesInfo.Count; id++)
        {
            Army army = Instantiate(ArmiesInfo[id].ArmyContent.Army, _containerArmy);
            army.name += $" {id}";
            Armies.Add(army);
            MainAb.ChildrenAb.Add(army.anchors);
            army.anchors.ParentAb = MainAb;
            army.OnDeadArmy += DeadArmy;
            Button buttonArmy = Instantiate(ArmiesInfo[id].ArmyContent.ButtonArmy, _containerGlobal);
            buttonArmy.GetComponent<Image>().color = _battlefield.GetColorFraction(this);
            buttonArmy.onClick.AddListener(() => _battlefield.SetTargetArmy(army));

            a = point.position;
            point.position += point.right * distance;
            b = point.position;
            BuildArmy(a, b, ArmiesInfo[id], army, buttonArmy);

            point.position += point.right * Army.OFFSET_BETWEEN_ARMIES;
        }
    }

    private void BuildArmy(Vector2 a, Vector2 b, ArmyInformation armyInfo, Army army, Button buttonArmy)
    {
        if (Bot)
        {
            army.BuildArmy(a, b, this, buttonArmy);
            return;
        }

        StatusUI armyUI = Instantiate(armyInfo.ArmyContent.ArmyUI, _containerToggle.transform);
        StatusUI armyGlobalUI = Instantiate(armyInfo.ArmyContent.ArmyGlobalUI, _containerGlobal);
        armyGlobalUI.background.color = _battlefield.GetColorFraction(this);

        Toggle toggle = armyUI.toggle;
        toggle.group = _containerToggle;
        toggle.onValueChanged.AddListener(on =>
        {
            if (on)
            {
                army.AddSkillsUI();
                _battlefield.ToggleRepeat.SetIsOnWithoutNotify(army.Repeat);
                _battlefield.ToggleStand.SetIsOnWithoutNotify(army.Stand);
                _battlefield.DeactiveAllArmies();
                _battlefield.ToggleRepeat.onValueChanged.AddListener(army.SetRepeat);
                _battlefield.ToggleStand.onValueChanged.AddListener(army.SetStand);
                army.SetActive(true);
            }
            else
            {
                army.RemoveSkillsUI();
                _battlefield.ToggleRepeat.onValueChanged.RemoveListener(army.SetRepeat);
                _battlefield.ToggleStand.onValueChanged.RemoveListener(army.SetStand);
                army.SetActive(false);
            }
        });
        armyGlobalUI.toggle.onValueChanged.AddListener(on => toggle.isOn = !toggle.isOn);

        army.BuildArmy(a, b, this, buttonArmy, armyUI, armyGlobalUI, _battlefield.ConteinerSkill);
    }

    private void DeadArmy(Army army)
    {
        army.OnDeadArmy -= DeadArmy;
        army.anchors.enabled = false;
        if (army.ArmyUI)
            Destroy(army.ArmyUI.gameObject);
        if (army.ArmyGlobalUI)
            Destroy(army.ArmyGlobalUI.gameObject);
        if (army.ButtonArmy)
            Destroy(army.ButtonArmy.gameObject);
        _ = Armies.Remove(army);
        //Destroy(army);
    }

    [Button("Kill", 15)]
    public void Kill()
    {
        for (int id = Armies.Count - 1; id >= 0; id--)
            Armies[id].Kill();
    }
}

[Serializable]
public class ArmyInformation
{
    #region Fields

    [FormerlySerializedAs("buffs")] public List<Buff> Buffs;
    public ArmyContent ArmyContent;

    #endregion Fields
}
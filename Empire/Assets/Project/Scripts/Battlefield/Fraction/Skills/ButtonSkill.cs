using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSkill : MonoBehaviour
{
    #region Properties

    internal Skill skillTarget { get; private set; }
    internal Button button { get; private set; }
    internal int prefabID { get; private set; }

    #endregion Properties

    #region Fields

    private Battlefield battlefield;
    private float timerSkillReload;
    public bool waitCastSkill;
    [SerializeField]
    private Image imageLoad;
    [SerializeField]
    private TMP_Text textLoad;

    public Dictionary<Army, UnityAction> initiatorArmies = new();
    private bool silence;

    public bool Silence
    {
        get => silence;
        set
        {
            button.enabled = !value;
            silence = value;
        }
    }

    #endregion Fields

    #region Methods

    private void Awake() => button = GetComponent<Button>();

    private void Start() => battlefield = Battlefield.singleton;

    private void FixedUpdate()
    {
        if (waitCastSkill)//ПОМЕНЯТЬ НА СОБЫТИЯ!!!
        {
            waitCastSkill = false;
            foreach (KeyValuePair<Army, UnityAction> initiatorArmy in initiatorArmies)
                CheckWaitCastSkill(initiatorArmy.Key);
            button.enabled = !waitCastSkill;
            imageLoad.fillAmount = waitCastSkill ? 1 : 0;
            textLoad.text = waitCastSkill ? "∞" : "";
            return;
        }

        if (timerSkillReload.Timer())
            return;

        UpdateColdownSkill();
    }

    private void UpdateColdownSkill()
    {
        button.enabled = !Silence && timerSkillReload == 0;
        imageLoad.fillAmount = skillTarget.timeCooldown == 0 ? 0 : timerSkillReload / skillTarget.timeCooldown;
        textLoad.text = timerSkillReload == 0 ? "" : System.Math.Round(timerSkillReload, 1).ToString();
    }
    private void CheckWaitCastSkill(Army army)
    {
        if (army.status.waitCastSkill == skillTarget)
            waitCastSkill = true;
    }
    private void AddTimerSkillReload(Army army)
    {
        if (army.status.timersSkillReload.ContainsKey(skillTarget))
            timerSkillReload = army.status.timersSkillReload[skillTarget] > timerSkillReload ? army.status.timersSkillReload[skillTarget] : timerSkillReload;
    }

    public void Build(Army army, Skill skillTarget)
    {
        this.skillTarget = skillTarget;
        AddTimerSkillReload(army);
        CheckWaitCastSkill(army);
        prefabID = skillTarget.buttonSkillPrefab.GetInstanceID();
    }
    /// <summary>
    /// Вызывается при выборе цели для реализации навыка
    /// </summary>
    /// <param name="army">Армия - цель</param>
    public void OnArmySkillRun() => timerSkillReload = skillTarget.timeCooldown;

    public void Add(Army army)
    {
        AddTimerSkillReload(army);
        CheckWaitCastSkill(army);

        UnityAction skillRunner = () =>
        {
            battlefield.OnSetTarget += army.TargetForUseSkill;
            battlefield.targetButtonSkill = this;
            battlefield.SetActiveArmies(skillTarget.triggerTarget, army);
        };
        initiatorArmies.Add(army, skillRunner);
        button.onClick.AddListener(skillRunner);
    }

    public void Remove(Army army)
    {
        battlefield.OnSetTarget -= army.TargetForUseSkill;
        UnityAction skillRunner = initiatorArmies.First((KeyValuePair<Army, UnityAction> initiatorArmy) => army == initiatorArmy.Key).Value;

        _ = initiatorArmies.Remove(army);
        button.onClick.RemoveListener(skillRunner);

        if (timerSkillReload == 0)
            return;
        timerSkillReload = 0;
        foreach (KeyValuePair<Army, UnityAction> initiatorArmy in initiatorArmies)
        {
            AddTimerSkillReload(initiatorArmy.Key);
            CheckWaitCastSkill(initiatorArmy.Key);
        }

        UpdateColdownSkill();
    }

    #endregion Methods

}
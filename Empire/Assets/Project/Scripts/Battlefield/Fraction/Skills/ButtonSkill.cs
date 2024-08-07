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

    internal Skill targetSkill { get; private set; }
    internal Button button { get; private set; }
    internal int prefabID { get; private set; }

    public bool Silence
    {
        get => silence;
        set
        {
            button.enabled = !value;
            silence = value;
        }
    }

    #endregion Properties

    #region Fields

    private Battlefield battlefield;
    private float timerSkillReload;

    [SerializeField]
    private Image imageLoad, imagePatrol;

    [SerializeField]
    private TMP_Text textLoad;

    private bool silence;
    public bool waitCastSkill, firstClick;
    public Dictionary<Army, UnityAction> initiatorArmies = new();

    #endregion Fields

    #region Methods

    private void Awake() => button = GetComponent<Button>();

    private void Start()
    {
        battlefield = Battlefield.singleton;
        firstClick = true;
        button.onClick.AddListener(() =>
        {
            imagePatrol.fillAmount = 1;
            imagePatrol.gameObject.SetActive(!firstClick);
            if (!firstClick)
                battlefield.SetPatrol();
            firstClick = !firstClick;
        });
        battlefield.conteinerSkill.OnClickAnyButtonSkills += (ButtonSkill buttonSkill) =>
        {
            if (buttonSkill != this)
                firstClick = true;
        };
    }

    private void FixedUpdate()
    {
        UpdateWaitCastSkill();

        if (waitCastSkill || timerSkillReload.Timer())
            return;

        UpdateColdownSkill();
    }

    private void UpdateWaitCastSkill()//.ПОМЕНЯТЬ НА СОБЫТИЯ!!!
    {
        waitCastSkill = false;
        foreach (KeyValuePair<Army, UnityAction> initiatorArmy in initiatorArmies)
            CheckWaitCastSkill(initiatorArmy.Key);

        button.enabled = !waitCastSkill;
        imageLoad.fillAmount = waitCastSkill ? 1 : 0;
        textLoad.text = waitCastSkill ? "∞" : "";
    }

    private void UpdateColdownSkill()//.ПОМЕНЯТЬ НА СОБЫТИЯ!!!
    {
        button.enabled = !Silence && timerSkillReload == 0;
        imageLoad.fillAmount = targetSkill.timeCooldown == 0 ? 0 : timerSkillReload / targetSkill.timeCooldown;
        textLoad.text = timerSkillReload == 0 ? "" : System.Math.Round(timerSkillReload, 1).ToString();
    }

    private void CheckWaitCastSkill(Army army)
    {
        if (army.status.waitCastSkill == targetSkill)
            waitCastSkill = true;
    }

    private void AddTimerSkillReload(Army army)
    {
        if (army.status.timersSkillReload.ContainsKey(targetSkill))
            timerSkillReload = army.status.timersSkillReload[targetSkill] > timerSkillReload ? army.status.timersSkillReload[targetSkill] : timerSkillReload;
    }

    public void Build(Army army, Skill skillTarget)
    {
        this.targetSkill = skillTarget;
        AddTimerSkillReload(army);
        CheckWaitCastSkill(army);
        prefabID = skillTarget.buttonSkillPrefab.GetInstanceID();
    }

    /// <summary>
    /// Перезарядка
    /// </summary>
    public void Reload() => timerSkillReload = targetSkill.timeCooldown;

    public void Add(Army army)
    {
        AddTimerSkillReload(army);
        CheckWaitCastSkill(army);

        UnityAction skillRunner = () =>
        {
            battlefield.OnSetTargetArmy += army.TargetForUseSkill;
            battlefield.OnSetTargetPoint += army.TargetForUseSkill;
            battlefield.OnSetPatrol += army.TargetForUseSkill;
            battlefield.targetSkill = targetSkill;
            battlefield.ActiveArmies(targetSkill.triggerTarget, army);
        };
        initiatorArmies.Add(army, skillRunner);
        button.onClick.AddListener(skillRunner);
    }

    public void Remove(Army army)
    {
        battlefield.OnSetTargetArmy -= army.TargetForUseSkill;
        battlefield.OnSetTargetPoint -= army.TargetForUseSkill;
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
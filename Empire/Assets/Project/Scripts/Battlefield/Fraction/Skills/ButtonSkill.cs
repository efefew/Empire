#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

[RequireComponent(typeof(TMP_Text))]
[RequireComponent(typeof(Button))]
public class ButtonSkill : MonoBehaviour
{
    [FormerlySerializedAs("imageLoad")] [SerializeField]
    private Image _imageLoad;

    [FormerlySerializedAs("imagePatrol")] [SerializeField]
    private Image _imagePatrol;

    [FormerlySerializedAs("textLoad")] [SerializeField]
    private TMP_Text _textLoad;

    [FormerlySerializedAs("waitCastSkill")]
    public bool WaitCastSkill;

    private Battlefield _battlefield;
    Coroutine _reloadCoroutine;

    private bool _silence;
    private float _timerSkillReload;
    public Dictionary<Army, UnityAction> InitiatorArmies = new();
    internal Skill TargetSkill { get; private set; }
    internal Button Button { get; private set; }
    internal int PrefabID { get; private set; }

    public bool Silence
    {
        get => _silence;
        set
        {
            Button.enabled = !value;
            _silence = value;
        }
    }

    private float TimerSkillReload
    {
        get => _timerSkillReload;
        set
        {
            _reloadCoroutine ??= StartCoroutine(IReload());
            _timerSkillReload = value;
        }
    }

    #region Methods

    private void Awake()
    {
        Button = GetComponent<Button>();
    }
    private IEnumerator IReload()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            UpdateWaitCastSkill();
            if (WaitCastSkill)
                continue;
            if (_timerSkillReload.Timer())
            {
                ButtonActive(true);
                _reloadCoroutine = null;
                yield break;
            }

            UpdateColDownSkill();
        }
    }

    private void UpdateWaitCastSkill()
    {
        WaitCastSkill = false;
        foreach (var initiatorArmy in InitiatorArmies)
            CheckWaitCastSkill(initiatorArmy.Key);

        ButtonActive(WaitCastSkill);
    }

    private void ButtonActive(bool active)
    {
        Button.enabled = active;
        _imageLoad.fillAmount = !active ? 1 : 0;
        _textLoad.text = !active ? "∞" : "";
    }

    private void UpdateColDownSkill()
    {
        Button.enabled = !Silence && TimerSkillReload == 0;
        _imageLoad.fillAmount = TargetSkill.TimeCooldown == 0 ? 0 : TimerSkillReload / TargetSkill.TimeCooldown;
        _textLoad.text = TimerSkillReload == 0 ? "" : $"{Math.Round(TimerSkillReload, 1)}";
    }

    private void CheckWaitCastSkill(Army army)
    {
        if (army.status.waitCastSkill == TargetSkill)
            WaitCastSkill = true;
    }

    private void AddTimerSkillReload(Army army)
    {
        if (army.status.TimersSkillReload.ContainsKey(TargetSkill))
            TimerSkillReload = army.status.TimersSkillReload[TargetSkill] > TimerSkillReload
                ? army.status.TimersSkillReload[TargetSkill]
                : TimerSkillReload;
    }

    public void Build(Army army, Skill skillTarget)
    {
        _battlefield = Battlefield.Instance;
        TargetSkill = skillTarget;
        AddTimerSkillReload(army);
        CheckWaitCastSkill(army);
        PrefabID = skillTarget.ButtonSkillPrefab.GetInstanceID();
    }

    /// <summary>
    ///     Перезарядка
    /// </summary>
    public void Reload()
    {
        TimerSkillReload = TargetSkill.TimeCooldown;
    }

    public void Add(Army army)
    {
        AddTimerSkillReload(army);
        CheckWaitCastSkill(army);

        UnityAction skillRunner = () =>
        {
            _battlefield.OnSetTargetArmy -= army.TargetForUseSkill;
            _battlefield.OnSetTargetPoint -= army.TargetForUseSkill;
            _battlefield.OnSetTargetArmy += army.TargetForUseSkill;
            _battlefield.OnSetTargetPoint += army.TargetForUseSkill;
            _battlefield._targetSkill = TargetSkill;
            _battlefield.ActiveArmies(TargetSkill.TriggerTarget, army);
            _battlefield.ActiveSkillButtons(TargetSkill);
            _battlefield.StopPatrol();
        };
        _battlefield.OnStartPatrol -= army.StartPatrol;
        _battlefield.OnStopPatrol -= army.StopPatrol;
        _battlefield.OnStartPatrol += army.StartPatrol;
        _battlefield.OnStopPatrol += army.StopPatrol;
        InitiatorArmies.Add(army, skillRunner);
        Button.onClick.AddListener(skillRunner);
    }

    public void UpdatePatrolUI()
    {
        bool anyArmyPatrol = false;
        foreach (var army in InitiatorArmies)
            if (TargetSkill == army.Key.PatrolSkill)
            {
                _imagePatrol.fillAmount = 1f;
                anyArmyPatrol = true;
            }
            else if (anyArmyPatrol)
            {
                _imagePatrol.fillAmount = 1 / 2f;
                break;
            }

        _imagePatrol.gameObject.SetActive(anyArmyPatrol);
    }

    public void Remove(Army army)
    {
        _battlefield.OnSetTargetArmy -= army.TargetForUseSkill;
        _battlefield.OnSetTargetPoint -= army.TargetForUseSkill;
        UnityAction skillRunner = InitiatorArmies.First(initiatorArmy => army == initiatorArmy.Key).Value;

        _battlefield.OnStartPatrol -= army.StartPatrol;
        _battlefield.OnStopPatrol -= army.StopPatrol;
        _ = InitiatorArmies.Remove(army);
        Button.onClick.RemoveListener(skillRunner);

        if (TimerSkillReload == 0)
            return;
        TimerSkillReload = 0;
        foreach (var initiatorArmy in InitiatorArmies)
        {
            AddTimerSkillReload(initiatorArmy.Key);
            CheckWaitCastSkill(initiatorArmy.Key);
        }

        UpdateColDownSkill();
    }

    #endregion Methods
}
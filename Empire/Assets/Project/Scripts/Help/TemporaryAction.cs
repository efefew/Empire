п»їusing System;
using System.Collections;

using UnityEngine;

/// <summary>
/// РІСЂРµРјРµРЅРЅРѕРµ РґРµР№СЃС‚РІРёРµ
/// </summary>
public partial class TemporaryAction : MonoBehaviour // СЏ С…Р· РєР°Рє СЌС‚Рѕ СЂРµР°Р»РёР·РѕРІР°С‚СЊ
{
    #region Methods

    /// <summary>
    /// РўР°Р№РјРµСЂ
    /// </summary>
    /// <param name="time">Р·РЅР°С‡РµРЅРёРµ С‚Р°Р№РјРµСЂР°</param>
    /// <returns></returns>
    private IEnumerator Timer(float time)
    {
        if (time <= 0)
            yield break;
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// Р”РµР»Р°С‚СЊ РїРѕРєР° СѓСЃР»РѕРІРёРµ РѕРєРѕРЅС‡Р°РЅРёСЏ РґРµР№СЃС‚РІРёСЏ (С„СѓРЅРєС†РёСЏ) РЅРµ РІС‹РїРѕР»РЅРµРЅРѕ
    /// </summary>
    /// <param name="expirationCondition">СѓСЃР»РѕРІРёРµ РѕРєРѕРЅС‡Р°РЅРёСЏ РґРµР№СЃС‚РІРёСЏ (С„СѓРЅРєС†РёСЏ)</param>
    /// <param name="startAction">РґРµР№СЃС‚РІРёРµ</param>
    /// <param name="endAction">РѕРєРѕРЅС‡Р°РЅРёРµ РґРµР№СЃС‚РІРёСЏ</param>
    /// <returns></returns>
    private IEnumerator IDo(Func<bool> expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters)
    {
        startAction?.Invoke(parameters);
        yield return new WaitUntil(expirationCondition);
        endAction?.Invoke(parameters);
    }

    /// <summary>
    /// Р”РµР»Р°С‚СЊ РїРѕРєР° СѓСЃР»РѕРІРёРµ РѕРєРѕРЅС‡Р°РЅРёСЏ РґРµР№СЃС‚РІРёСЏ (СЃС‡РµС‚С‡РёРє) РЅРµ РІС‹РїРѕР»РЅРµРЅРѕ
    /// </summary>
    /// <param name="expirationCondition">СѓСЃР»РѕРІРёРµ РѕРєРѕРЅС‡Р°РЅРёСЏ РґРµР№СЃС‚РІРёСЏ (СЃС‡РµС‚С‡РёРє)</param>
    /// <param name="startAction">РґРµР№СЃС‚РІРёРµ</param>
    /// <param name="endAction">РѕРєРѕРЅС‡Р°РЅРёРµ РґРµР№СЃС‚РІРёСЏ</param>
    private IEnumerator IDo(IEnumerator expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters)
    {
        startAction?.Invoke(parameters);
        yield return StartCoroutine(expirationCondition);
        endAction?.Invoke(parameters);
    }

    /// <summary>
    /// РЎРѕР·РґР°С‘С‚ РґРµР№СЃС‚РІРёРµ РґРѕ РѕРїСЂРµРґРµР»С‘РЅРЅРѕРіРѕ РјРѕРјРµРЅС‚Р°
    /// </summary>
    /// <param name="expirationCondition">РѕРїСЂРµРґРµР»С‘РЅРЅС‹Р№ РјРѕРјРµРЅС‚</param>
    /// <param name="startAction">РґРµР№СЃС‚РІРёРµ</param>
    /// <param name="endAction">РѕРєРѕРЅС‡Р°РЅРёРµ РґРµР№СЃС‚РІРёСЏ</param>
    public void Do(Func<bool> expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(expirationCondition, startAction, endAction, parameters));

    /// <summary>
    /// РЎРѕР·РґР°С‘С‚ РґРµР№СЃС‚РІРёРµ РґРѕ РѕРїСЂРµРґРµР»С‘РЅРЅРѕРіРѕ РјРѕРјРµРЅС‚Р°
    /// </summary>
    /// <param name="expirationCondition">РѕРїСЂРµРґРµР»С‘РЅРЅС‹Р№ РјРѕРјРµРЅС‚</param>
    /// <param name="startAction">РґРµР№СЃС‚РІРёРµ</param>
    /// <param name="endAction">РѕРєРѕРЅС‡Р°РЅРёРµ РґРµР№СЃС‚РІРёСЏ</param>
    public void Do(IEnumerator expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(expirationCondition, startAction, endAction, parameters));

    /// <summary>
    /// РЎРѕР·РґР°С‘С‚ РґРµР№СЃС‚РІРёРµ РґРѕ РѕРїСЂРµРґРµР»С‘РЅРЅРѕРіРѕ РјРѕРјРµРЅС‚Р°
    /// </summary>
    /// <param name="expirationCondition">РѕРїСЂРµРґРµР»С‘РЅРЅС‹Р№ РјРѕРјРµРЅС‚</param>
    /// <param name="startAction">РґРµР№СЃС‚РІРёРµ</param>
    /// <param name="endAction">РѕРєРѕРЅС‡Р°РЅРёРµ РґРµР№СЃС‚РІРёСЏ</param>
    public void Do(Condition expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(expirationCondition.GetCondition(), startAction, endAction, parameters));

    /// <summary>
    /// РЎРѕР·РґР°С‘С‚ РґРµР№СЃС‚РІРёРµ РЅР° РІСЂРµРјСЏ
    /// </summary>
    /// <param name="time">РІСЂРµРјСЏ</param>
    /// <param name="expirationCondition">РґРµР№СЃС‚РІРёРµ</param>
    /// <param name="endAction">РѕРєРѕРЅС‡Р°РЅРёРµ РґРµР№СЃС‚РІРёСЏ</param>
    public void Do(float time, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(Timer(time), startAction, endAction, parameters));

    #endregion Methods
}
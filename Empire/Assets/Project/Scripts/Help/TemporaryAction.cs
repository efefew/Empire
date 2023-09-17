using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// временное действие
/// </summary>
public partial class TemporaryAction : MonoBehaviour // я хз как это реализовать
{
    #region Methods

    /// <summary>
    /// Таймер
    /// </summary>
    /// <param name="time">значение таймера</param>
    /// <returns></returns>
    private IEnumerator Timer(float time)
    {
        if (time <= 0)
            yield break;
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// Делать пока условие окончания действия (функция) не выполнено
    /// </summary>
    /// <param name="expirationCondition">условие окончания действия (функция)</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    /// <returns></returns>
    private IEnumerator IDo(Func<bool> expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters)
    {
        startAction?.Invoke(parameters);
        yield return new WaitUntil(expirationCondition);
        endAction?.Invoke(parameters);
    }

    /// <summary>
    /// Делать пока условие окончания действия (счетчик) не выполнено
    /// </summary>
    /// <param name="expirationCondition">условие окончания действия (счетчик)</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    private IEnumerator IDo(IEnumerator expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters)
    {
        startAction?.Invoke(parameters);
        yield return StartCoroutine(expirationCondition);
        endAction?.Invoke(parameters);
    }

    /// <summary>
    /// Создаёт действие до определённого момента
    /// </summary>
    /// <param name="expirationCondition">определённый момент</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(Func<bool> expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(expirationCondition, startAction, endAction, parameters));

    /// <summary>
    /// Создаёт действие до определённого момента
    /// </summary>
    /// <param name="expirationCondition">определённый момент</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(IEnumerator expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(expirationCondition, startAction, endAction, parameters));

    /// <summary>
    /// Создаёт действие до определённого момента
    /// </summary>
    /// <param name="expirationCondition">определённый момент</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(Condition expirationCondition, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(expirationCondition.GetCondition(), startAction, endAction, parameters));

    /// <summary>
    /// Создаёт действие на время
    /// </summary>
    /// <param name="time">время</param>
    /// <param name="expirationCondition">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(float time, Action<object[]> startAction, Action<object[]> endAction, object[] parameters = null) => StartCoroutine(IDo(Timer(time), startAction, endAction, parameters));

    #endregion Methods
}
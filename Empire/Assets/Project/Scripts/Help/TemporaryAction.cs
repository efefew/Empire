using System;
using System.Collections;

using UnityEngine;
/// <summary>
/// временное действие
/// </summary>
public partial class TemporaryAction : MonoBehaviour // я хз как это реализовать
{
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
    private IEnumerator IDo(Func<bool> expirationCondition, Action startAction, Action endAction)
    {
        startAction?.Invoke();
        yield return new WaitUntil(expirationCondition);
        endAction?.Invoke();
    }
    /// <summary>
    /// Делать пока условие окончания действия (счетчик) не выполнено
    /// </summary>
    /// <param name="expirationCondition">условие окончания действия (счетчик)</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    private IEnumerator IDo(IEnumerator expirationCondition, Action startAction, Action endAction)
    {
        startAction?.Invoke();
        yield return StartCoroutine(expirationCondition);
        endAction?.Invoke();
    }

    /// <summary>
    /// Создаёт действие до определённого момента
    /// </summary>
    /// <param name="expirationCondition">определённый момент</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(Func<bool> expirationCondition, Action startAction, Action endAction) => StartCoroutine(IDo(expirationCondition, startAction, endAction));

    /// <summary>
    /// Создаёт действие до определённого момента
    /// </summary>
    /// <param name="coroutineStun">определённый момент</param>
    /// <param name="startAction">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(IEnumerator coroutineStun, Action startAction, Action endAction) => StartCoroutine(IDo(coroutineStun, startAction, endAction));

    /// <summary>
    /// Создаёт действие на время
    /// </summary>
    /// <param name="time">время</param>
    /// <param name="expirationCondition">действие</param>
    /// <param name="endAction">окончание действия</param>
    public void Do(float time, Action expirationCondition, Action endAction) => StartCoroutine(IDo(Timer(time), expirationCondition, endAction));
}

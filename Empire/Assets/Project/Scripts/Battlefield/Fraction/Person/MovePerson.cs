using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(AgentMove))]
public partial class Person : MonoBehaviour// Мобильность существа
{
    #region Delegates

    public delegate bool EndStunHandler();

    #endregion Delegates

    #region Enums

    /// <summary>
    /// Тип преследования цели
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        /// Вынужденая
        /// </summary>
        forced,

        /// <summary>
        /// Приказ
        /// </summary>
        command,

        /// <summary>
        /// Закончить то, что начал
        /// </summary>
        finish
    }

    #endregion Enums

    #region Properties

    public int stunCount { get; private set; }
    public Person LastPursuitTarget { get; private set; }

    #endregion Properties

    #region Fields

    private const float CAMERA_VISIBLE_DISTANCE = 1230f;
    private const float UPDATE_MOVE_SECONDS = 0.5f;
    private bool isStoped, rightDirection;
    private Vector3 scaleDefault;
    public Dictionary<Transform, TargetType> targets = new();

    public AgentMove agentMove;

    #endregion Fields

    #region Methods

    private IEnumerator Timer(float time)
    {
        if (time <= 0)
            yield break;
        yield return new WaitForSeconds(time);
    }

    private IEnumerator IStun(Func<bool> funcStun)
    {
        stunCount++;
        yield return new WaitUntil(funcStun);
        stunCount--;
    }

    private IEnumerator IStun(IEnumerator coroutineStun)
    {
        stunCount++;
        yield return StartCoroutine(coroutineStun);
        stunCount--;
    }

    private IEnumerator IPursuit(Person target, Func<bool> funcTarget)
    {
        if (target == null)
            yield break;
        LastPursuitTarget = target;
        agentMove.tempTarget = target.transform;
        yield return new WaitUntil(funcTarget);
        agentMove.tempTarget = null;
    }

    private IEnumerator IPursuit(Person target, IEnumerator coroutineTarget)
    {
        if (target == null)
            yield break;

        LastPursuitTarget = target;
        agentMove.tempTarget = target.transform;
        yield return StartCoroutine(coroutineTarget);
        agentMove.tempTarget = null;
    }

    private IEnumerator IMoveUpdate()
    {
        while (true)
        {
            if (health <= 0)
                yield break;
            yield return new WaitForSeconds(UPDATE_MOVE_SECONDS);
            if (speedScale < 1)
            {

            }

            if (isStoped != agentMove.UpdateAgent(ref rightDirection, stunCount > 0, speedScale * status.maxSpeed * (stamina / status.maxStamina)))
            {
                isStoped = !isStoped;
                ChangeStateAnimation(isStoped ? idleState : walkState);
            }

            if (isStoped)
                continue;
            transform.localScale = scaleDefault.X(scaleDefault.x * (rightDirection ? 1 : -1));
            animator.transform.position = animator.transform.position.Z(Mathf.Clamp(transform.position.y, -CAMERA_VISIBLE_DISTANCE, CAMERA_VISIBLE_DISTANCE));
        }
    }

    /// <summary>
    /// Оглушает до определённого момента
    /// </summary>
    /// <param name="funcStun">определённый момент</param>
    public Coroutine Stun(Func<bool> funcStun) => StartCoroutine(IStun(funcStun));

    /// <summary>
    /// Оглушает до определённого момента
    /// </summary>
    /// <param name="coroutineStun">определённый момент</param>
    public Coroutine Stun(IEnumerator coroutineStun) => StartCoroutine(IStun(coroutineStun));

    /// <summary>
    /// Оглушает на время
    /// </summary>
    /// <param name="endStun">время</param>
    public Coroutine Stun(float time) => StartCoroutine(IStun(Timer(time)));

    /// <summary>
    /// Преследует до определённого момента
    /// </summary>
    /// <param name="funcTarget">определённый момент</param>
    public Coroutine Pursuit(Person target, Func<bool> funcTarget) => StartCoroutine(IPursuit(target, funcTarget));

    /// <summary>
    /// Преследует до определённого момента
    /// </summary>
    /// <param name="coroutineTarget">определённый момент</param>
    public Coroutine Pursuit(Person target, IEnumerator coroutineTarget) => StartCoroutine(IPursuit(target, coroutineTarget));

    /// <summary>
    /// Преследует на время
    /// </summary>
    /// <param name="endStun">время</param>
    public Coroutine Pursuit(Person target, float time) => StartCoroutine(IPursuit(target, Timer(time)));

    #endregion Methods
}
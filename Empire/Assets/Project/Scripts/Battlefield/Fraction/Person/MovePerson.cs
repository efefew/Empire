#region

using System;
using System.Collections;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

[RequireComponent(typeof(AgentMove))]
public partial class Person : MonoBehaviour // ����������� ��������
{
    #region Enums

    /// <summary>
    ///     ��� ������������� ����
    /// </summary>
    public enum TargetType
    {
        /// <summary>
        ///     ����������
        /// </summary>
        forced,

        /// <summary>
        ///     ������
        /// </summary>
        command,

        /// <summary>
        ///     ��������� ��, ��� �����
        /// </summary>
        finish
    }

    #endregion Enums

    #region Properties

    public int stunCount { get; private set; }
    public Person LastPursuitTarget { get; set; }

    #endregion Properties

    #region Fields

    private const float CAMERA_VISIBLE_DISTANCE = 1230f;
    private const float UPDATE_MOVE = 0.1f, UPDATE_STOP_STATUS = 0.05f;
    private bool _isStoped, _rightDirection;
    private Vector3 _scaleDefault;

    //public Dictionary<Transform, TargetType> targets = new();
    public Coroutine ArmyPursuit;
    [FormerlySerializedAs("armyTarget")] public Army ArmyTarget;

    [FormerlySerializedAs("agentMove")] public AgentMove AgentMove;

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

    private IEnumerator IPursuit(Vector3 target, Func<bool> funcTarget)
    {
        SetTarget(target);
        yield return new WaitUntil(funcTarget);
        AgentMove.TempPointTarget = null;
        MoveUpdate();
    }

    private IEnumerator IPursuit(Vector3 target, IEnumerator coroutineTarget)
    {
        SetTarget(target);
        yield return StartCoroutine(coroutineTarget);
        AgentMove.TempPointTarget = null;
        MoveUpdate();
    }

    private IEnumerator IPursuit(Person target, Func<bool> funcTarget)
    {
        if (!target)
            yield break;
        LastPursuitTarget = target;
        SetTarget(target.transform);
        yield return new WaitUntil(funcTarget);
        AgentMove.TempTarget = null;
        MoveUpdate();
    }

    private IEnumerator IPursuit(Person target, IEnumerator coroutineTarget)
    {
        if (target == null)
            yield break;

        LastPursuitTarget = target;
        SetTarget(target.transform);
        yield return StartCoroutine(coroutineTarget);
        AgentMove.TempTarget = null;
        MoveUpdate();
    }

    private IEnumerator IStopStatusUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(UPDATE_STOP_STATUS);

            _isStoped = (!AgentMove.Agent.isOnNavMesh || AgentMove.Agent.remainingDistance <= AgentMove.MIN_DISTANCE) &&
                       stunCount == 0;
            ChangeStateAnimation(_isStoped ? idleState : walkState);

            if (_isStoped) continue;
            if (AgentMove.Agent.path.corners.Length >= 2)
                //agentMove.UpdateLine();
                _rightDirection = AgentMove.Agent.path.corners[1].x - transform.position.x > 0;

            transform.localScale = _scaleDefault.X(_scaleDefault.x * (_rightDirection ? 1 : -1));
            animator.transform.position = animator.transform.position.Z(Mathf.Clamp(transform.position.y,
                -CAMERA_VISIBLE_DISTANCE, CAMERA_VISIBLE_DISTANCE));
        }
    }

    private IEnumerator IMoveUpdate()
    {
        do
        {
            yield return new WaitForSeconds(UPDATE_MOVE);
            if (!AgentMove.TempTarget && !Army.Stand)
                AgentMove.TempTarget = ArmyTarget ? Army.GetRandomPerson(ArmyTarget)?.transform : null;

            AgentMove.UpdateAgent(stunCount > 0, speedScale * Status.MaxSpeed * (stamina / Status.MaxStamina));
        } while (AgentMove.TempTarget);
    }

    [Button("MoveUpdate")]
    public void MoveUpdate()
    {
        StartCoroutine(IMoveUpdate());
    }

    public void SetTarget(Transform target)
    {
        AgentMove.TempTarget = target;
        MoveUpdate();
    }

    public void SetTarget(Vector3 target)
    {
        AgentMove.TempPointTarget = target;
        MoveUpdate();
    }

    /// <summary>
    ///     �������� �� ������������ �������
    /// </summary>
    /// <param name="funcStun">����������� ������</param>
    public Coroutine Stun(Func<bool> funcStun)
    {
        return StartCoroutine(IStun(funcStun));
    }

    /// <summary>
    ///     �������� �� ������������ �������
    /// </summary>
    /// <param name="coroutineStun">����������� ������</param>
    public Coroutine Stun(IEnumerator coroutineStun)
    {
        return StartCoroutine(IStun(coroutineStun));
    }

    /// <summary>
    ///     �������� �� �����
    /// </summary>
    /// <param name="endStun">�����</param>
    public Coroutine Stun(float time)
    {
        return StartCoroutine(IStun(Timer(time)));
    }

    /// <summary>
    ///     ���������� �� ������������ �������
    /// </summary>
    /// <param name="funcTarget">����������� ������</param>
    public Coroutine Pursuit(Person target, Func<bool> funcTarget)
    {
        return StartCoroutine(IPursuit(target, funcTarget));
    }

    /// <summary>
    ///     ���������� �� ������������ �������
    /// </summary>
    /// <param name="coroutineTarget">����������� ������</param>
    public Coroutine Pursuit(Person target, IEnumerator coroutineTarget)
    {
        return StartCoroutine(IPursuit(target, coroutineTarget));
    }

    /// <summary>
    ///     ���������� �� ������������ �������
    /// </summary>
    /// <param name="funcTarget">����������� ������</param>
    public Coroutine Pursuit(Vector3 target, Func<bool> funcTarget)
    {
        return StartCoroutine(IPursuit(target, funcTarget));
    }

    /// <summary>
    ///     ���������� �� ������������ �������
    /// </summary>
    /// <param name="coroutineTarget">����������� ������</param>
    public Coroutine Pursuit(Vector3 target, IEnumerator coroutineTarget)
    {
        return StartCoroutine(IPursuit(target, coroutineTarget));
    }

    /// <summary>
    ///     ���������� �� �����
    /// </summary>
    /// <param name="endStun">�����</param>
    public Coroutine Pursuit(Person target, float time)
    {
        return StartCoroutine(IPursuit(target, Timer(time)));
    }

    public void StopPursuit()
    {
        if (ArmyPursuit == null)
            return;
        StopCoroutine(ArmyPursuit);
    }

    #endregion Methods
}
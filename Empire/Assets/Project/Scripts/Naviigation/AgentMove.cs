#region

using NavMeshPlus.Extensions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

#endregion

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class AgentMove : MonoBehaviour
{
    public const float
        MIN_DISTANCE = 0.1f;

    private const float
        MIN_DISTANCE_ENEMY_CONTACT = 1f;

    [FormerlySerializedAs("target")] [SerializeField] private Transform _target;

    [FormerlySerializedAs("tempTarget")] public Transform TempTarget;
    private LineRenderer _line;
    public Vector3? TempPointTarget;

    [FormerlySerializedAs("agent")] [HideInInspector] public NavMeshAgent Agent;


    private void Awake()
    {
        TempPointTarget = null;
        Agent = GetComponent<NavMeshAgent>();
        _line = _target.GetChild(0).GetComponent<LineRenderer>();
    }

    public void UpdateAgent(bool stun, float speed)
    {
        Agent.speed = speed;
        if (!Agent.isOnNavMesh)
        {
            transform.position = _target.position;
            return;
        }

        Agent.isStopped = stun;
        Vector3 point = TempPointTarget ?? _target.position;
        point = TempTarget ? TempTarget.position : point;

        _ = Agent.SetDestination(point);
        Agent.stoppingDistance = TempTarget ? MIN_DISTANCE_ENEMY_CONTACT : MIN_DISTANCE;
    }

    public void UpdateLine()
    {
        if (!_target.gameObject.activeSelf)
            return;
        var corners = Agent.path.corners;
        if (corners.Length < 2)
            return;
        _line.positionCount = corners.Length;
        _line.SetPositions(corners);
    }

}
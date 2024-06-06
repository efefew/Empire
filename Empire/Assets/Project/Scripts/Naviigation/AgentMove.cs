using NavMeshPlus.Extensions;

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class AgentMove : MonoBehaviour
{
    #region Fields

    public const float
        MIN_DISTANCE = 0.1f,
        MIN_DISTANCE_ENEMY_CONTACT = 1f;
    [SerializeField]
    private Transform target;
    public Transform tempTarget;
    private LineRenderer line;
    public Vector3? tempPointTarget;
    [HideInInspector]
    public NavMeshAgent agent;

    #endregion Fields

    #region Methods
    private void Awake()
    {
        tempPointTarget = null;
        agent = GetComponent<NavMeshAgent>();
        line = target.GetChild(0).GetComponent<LineRenderer>();
    }

    /// <summary>
    /// Обновление пути существа
    /// </summary>
    public void UpdateAgent(bool stun, float speed)
    {
        agent.speed = speed;
        if (!agent.isOnNavMesh)
        {
            transform.position = target.position;
            return;
        }

        agent.isStopped = stun;
        Vector3 point = tempPointTarget == null ? target.position : (Vector3)tempPointTarget;
        point = tempTarget ? tempTarget.position : point;

        _ = agent.SetDestination(point);
        agent.stoppingDistance = tempTarget ? MIN_DISTANCE_ENEMY_CONTACT : MIN_DISTANCE;
    }
    public void UpdateLine()
    {
        if (!target.gameObject.activeSelf)
            return;
        Vector3[] corners = agent.path.corners;
        if (corners.Length < 2)
            return;
        line.positionCount = corners.Length;
        line.SetPositions(corners);
    }
    #endregion Methods
}
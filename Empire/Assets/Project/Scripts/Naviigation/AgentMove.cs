using AdvancedEditorTools.Attributes;

using NavMeshPlus.Extensions;

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class AgentMove : MonoBehaviour
{
    #region Fields

    public const float MIN_DISTANCE = 0.2f;

    [SerializeField]
    private Transform target;
    [ReadOnly]
    public Transform tempTarget;
    [ReadOnly]
    public Vector3? tempPointTarget;
    [HideInInspector]
    public NavMeshAgent agent;

    #endregion Fields

    #region Methods
    private void Awake()
    {
        tempPointTarget = null;
        agent = GetComponent<NavMeshAgent>();
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

        if (stun)
        {
            agent.isStopped = true;
            return;
        }

        Vector3 point = tempPointTarget == null ? target.position : (Vector3)tempPointTarget;
        point = tempTarget ? tempTarget.position : point;
        Move(point);
    }

    private void Move(Vector3 point)
    {
        _ = agent.SetDestination(point);
        agent.isStopped = agent.remainingDistance < MIN_DISTANCE;
    }

    #endregion Methods
}
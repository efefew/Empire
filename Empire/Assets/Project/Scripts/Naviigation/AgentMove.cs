using NavMeshPlus.Extensions;

using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AgentOverride2d))]
public class AgentMove : MonoBehaviour
{
    #region Fields

    private const float MIN_DISTANCE = 0.1f;

    [SerializeField]
    private Transform target;
    [HideInInspector]
    public Transform tempTarget;
    [HideInInspector]
    public NavMeshAgent agent;

    #endregion Fields

    #region Methods
    private void Awake() => agent = GetComponent<NavMeshAgent>();
    /// <summary>
    /// Обновление пути существа
    /// </summary>
    /// <returns>существо стоит?</returns>
    public bool UpdateAgent(ref bool rightDirection, bool stun, float speed)
    {
        agent.speed = speed;
        if (!agent.isOnNavMesh)
        {
            transform.position = target.position;
            return rightDirection;
        }

        if (stun)
        {
            agent.isStopped = true;
            return rightDirection;
        }

        return tempTarget ? Move(tempTarget, ref rightDirection) : Move(target, ref rightDirection);
    }

    private bool Move(Transform target, ref bool rightDirection)
    {
        _ = agent.SetDestination(target.position);
        agent.isStopped = agent.remainingDistance < MIN_DISTANCE;
        if (agent.path.corners.Length >= 2)
            rightDirection = agent.path.corners[1].x - transform.position.x > 0;
        return agent.isStopped;
    }

    #endregion Methods
}
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
    [HideInInspector]
    public NavMeshAgent agent;

    #endregion Fields

    #region Methods
    private void Awake() => agent = GetComponent<NavMeshAgent>();

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

        Move(tempTarget ? tempTarget : target);
    }

    private void Move(Transform target)
    {
        _ = agent.SetDestination(target.position);
        agent.isStopped = agent.remainingDistance < MIN_DISTANCE;
    }

    #endregion Methods
}
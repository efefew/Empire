#region

using UnityEngine;

#endregion

public partial class Person : MonoBehaviour // �������� ��������
{
    #region Fields

    [SerializeField] private Animator animator;

    private string currentStateAnimation;
    private uint currentPriorityStateAnimation;
    public string idleState = "idle", walkState = "walk", runState = "run", hitState = "hit", deadState = "dead";

    #endregion Fields

    #region Methods

    public void ChangeStateAnimation(string newStateAnimation, uint newPriorityStateAnimation = 0)
    {
        if (currentStateAnimation == newStateAnimation || currentPriorityStateAnimation > newPriorityStateAnimation)
            return;
        animator.Play(newStateAnimation);
        currentStateAnimation = newStateAnimation;
        currentPriorityStateAnimation = newPriorityStateAnimation;
    }

    public void RemoveStateAnimation(string newStateAnimation)
    {
        if (currentStateAnimation == newStateAnimation)
        {
            animator.Play(idleState);
            currentStateAnimation = idleState;
            currentPriorityStateAnimation = 0;
        }
    }

    #endregion Methods
}
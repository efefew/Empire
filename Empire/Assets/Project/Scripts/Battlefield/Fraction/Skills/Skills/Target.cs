#region

using UnityEngine;

#endregion

[AddComponentMenu("Skill/Target")]
public class Target : Skill
{
    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target.transform.position))
            return;

        if (Consumable)
            initiator.amountSkill[this]--;

        if (OnTrigger(TriggerTarget, initiator, target))
            SetEffectsAndBuffs(initiator, target);
    }

    public override void Run(Person initiator, Vector3 target)
    {
        Debug.LogError("��� ����������� �� ����� ���� ���������� �� �����");
    }

    #endregion Methods
}
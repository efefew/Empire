using UnityEngine;

[AddComponentMenu("Skill/Target")]
public class Target : Skill
{
    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target.transform.position))
            return;

        if (consumable)
            initiator.amountSkill[this]--;

        if (OnTrigger(triggerTarget, initiator, target))
            SetEffectsAndBuffs(initiator, target);
    }
    public override void Run(Person initiator, Vector3 target) => Debug.LogError("Эта способность не может быть направлена на точку");

    #endregion Methods
}
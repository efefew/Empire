using UnityEngine;

[AddComponentMenu("Skill/Target")]
public class Target : Skill
{
    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target))
            return;

        if (consumable)
            amountSkill--;

        if (OnTrigger(triggerTarget, initiator, target))
            SetEffectsAndBuffs(initiator, target);
    }

    #endregion Methods
}
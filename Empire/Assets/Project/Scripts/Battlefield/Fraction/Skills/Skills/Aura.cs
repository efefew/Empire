using UnityEngine;

[AddComponentMenu("Skill/Aura")]
public class Aura : Skill
{
    #region Fields

    public Person auraTarget;

    [Min(0)]
    public float duration;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        if (consumable && (amountSkill - 1) < 0)
        {
            return;
        }
    }

    #endregion Methods
}
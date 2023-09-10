using UnityEngine;

[AddComponentMenu("Skill/Target")]
public class Target : Skill
{
    #region Fields

    [SerializeField]
    private int countTarget = 1;

    [Min(0)]
    public new float range;

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
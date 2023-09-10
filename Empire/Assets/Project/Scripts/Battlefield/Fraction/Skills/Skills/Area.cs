using UnityEngine;

[AddComponentMenu("Skill/Area")]
public class Area : Skill
{
    #region Fields

    [Min(0)]
    public new float range;

    [Min(0)]
    public float duration;

    /// <summary>
    /// Промахи не тратят статы
    /// </summary>
    public bool canMiss;

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
using UnityEngine;

using Zelude;

[AddComponentMenu("Skill/Aura")]
public class Aura : Skill
{
    #region Fields

    public AuraObject aura;
    [MinMaxSlider(1, 100, "maxCountAura", "Count Aura")]
    [SerializeField]
    public int minCountAura;

    [HideInInspector]
    public int maxCountAura;
    /// <summary>
    /// Промежуток
    /// </summary>
    [Min(0.001f)]
    public float gap;

    /// <summary>
    /// Частота
    /// </summary>
    public uint frequency;
    [Min(0)]
    public float radius;
    [Min(0)]
    public float scatter;
    ///// <summary>
    ///// Стоять при использовании навыка?
    ///// </summary>
    //public bool stun;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target.transform.position) || target == null)
            return;

        if (consumable)
            initiator.amountSkill[this]--;

        for (int i = 0; i < Random.Range(minCountAura, maxCountAura); i++)
        {
            AuraObject aura = Instantiate(this.aura, new Vector2(transform.position.x + Random.Range(0f, scatter), transform.position.y + Random.Range(0f, scatter)), Quaternion.Euler(0, 0, Random.Range(0f, 360f)), initiator.transform.parent);
            aura.Build(initiator, this, target);
        }
    }
    public override void Run(Person initiator, Vector3 target) => Debug.LogError("Эта способность не может быть направлена на точку");
    #endregion Methods
}
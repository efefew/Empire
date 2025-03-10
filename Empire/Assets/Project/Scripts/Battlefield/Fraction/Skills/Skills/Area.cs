#region

using UnityEngine;
using Zelude;

#endregion

[AddComponentMenu("Skill/Area")]
public class Area : Skill
{
    #region Fields

    public AreaObject area;

    [MinMaxSlider(1, 100, "maxCountAura", "Count Area")] [SerializeField]
    public int minCountArea;

    [HideInInspector] public int maxCountArea;

    /// <summary>
    ///     ����������
    /// </summary>
    [Min(0.001f)] public float gap;

    [Min(0)] public float radius;

    [Min(0)] public float scatter;

    /// <summary>
    ///     �������
    /// </summary>
    public uint frequency;

    ///// <summary>
    ///// ������ ��� ������������� ������?
    ///// </summary>
    //public bool stun;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target.transform.position))
            return;

        if (Consumable)
            initiator.amountSkill[this]--;

        for (int i = 0; i < Random.Range(minCountArea, maxCountArea); i++)
        {
            AreaObject area = Instantiate(
                this.area,
                new Vector2(target.transform.position.x + Random.Range(0f, scatter),
                    target.transform.position.y + Random.Range(0f, scatter)),
                Quaternion.Euler(0, 0, Random.Range(0f, 360f)),
                initiator.transform.parent);
            area.Build(initiator, this);
        }
    }

    public override void Run(Person initiator, Vector3 target)
    {
        if (!PointCanBeTarget)
            return;
        if (!LimitRun(initiator, target))
            return;

        if (Consumable)
            initiator.amountSkill[this]--;

        for (int i = 0; i < Random.Range(minCountArea, maxCountArea); i++)
        {
            AreaObject area = Instantiate(
                this.area,
                new Vector2(target.x + Random.Range(0f, scatter), target.y + Random.Range(0f, scatter)),
                Quaternion.Euler(0, 0, Random.Range(0f, 360f)),
                initiator.transform.parent);
            area.Build(initiator, this);
        }
    }

    #endregion Methods
}
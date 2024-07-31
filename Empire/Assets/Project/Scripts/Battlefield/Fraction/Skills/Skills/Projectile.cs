using UnityEngine;

using Zelude;

[AddComponentMenu("Skill/Projectile")]
public class Projectile : Skill
{
    #region Fields

    [MinMaxSlider(1, 100, "maxCountProjectile", "Count Projectile")]
    [SerializeField]
    public int minCountProjectile;

    [HideInInspector]
    public int maxCountProjectile;

    public ProjectileObject projectile;

    [Min(0)]
    public float timeDanger, timeDead;

    [Min(0)]
    public float speed;

    public float offset;
    /// <summary>
    /// разброс
    /// </summary>
    [Range(0f, 360f)]
    public float scatter;

    public bool targetPerson;

    #endregion Fields

    #region Methods

    private void SpawnPrjectile(Person initiator, Person target)
    {
        ProjectileObject projectile = Instantiate(this.projectile, initiator.transform.parent);

        projectile.transform.position = initiator.transform.position + (initiator.transform.up * offset);
        projectile.transform.LookAt2D(target.transform.position);
        projectile.transform.eulerAngles = projectile.transform.eulerAngles.Z(projectile.transform.eulerAngles.z + Random.Range(-scatter, scatter));
        projectile.Build(initiator, this, target);
    }
    private void SpawnPrjectile(Person initiator, Vector3 target)
    {
        ProjectileObject projectile = Instantiate(this.projectile, initiator.transform.parent);

        projectile.transform.position = initiator.transform.position + (initiator.transform.up * offset);
        projectile.transform.LookAt2D(target);
        projectile.transform.eulerAngles = projectile.transform.eulerAngles.Z(projectile.transform.eulerAngles.z + Random.Range(-scatter, scatter));
        projectile.Build(initiator, this);
    }
    public override void Run(Person initiator, Person target = null)
    {
        if (target == null || !LimitRun(initiator, target.transform.position))
            return;

        if (consumable)
            initiator.amountSkill[this]--;

        for (int i = 0; i < Random.Range(minCountProjectile, maxCountProjectile); i++)
            SpawnPrjectile(initiator, target);
    }

    public override void Run(Person initiator, Vector3 target)
    {
        if (targetPerson || !pointCanBeTarget)
            return;
        if (!LimitRun(initiator, target))
            return;

        if (consumable)
            initiator.amountSkill[this]--;

        for (int i = 0; i < Random.Range(minCountProjectile, maxCountProjectile); i++)
            SpawnPrjectile(initiator, target);
    }

    #endregion Methods
}
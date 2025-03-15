#region

using System.Collections;
using UnityEngine;
using static Skill;

#endregion

public class ProjectileObject : MonoBehaviour
{
    #region Fields

    [SerializeField] private ProjectileObject nextProjectile;

    private Vector3 targetPoint;
    private Person targetPerson, initiator;

    [SerializeField] private float timeAnimationDead;

    private Transform tr;
    private bool danger;
    private Projectile skill;
    public string animationDead;
    private int countCatch;

    #endregion Fields

    #region Methods

    private void Start()
    {
        tr = transform;
        _ = StartCoroutine(LifeProjectile());
    }

    private IEnumerator LifeProjectile()
    {
        yield return new WaitForSeconds(skill.TimeDanger);
        danger = true;
        yield return new WaitForSeconds(Mathf.Max(skill.TimeDead - timeAnimationDead, 0));
        _ = StartCoroutine(DestroySelf());
    }

    private IEnumerator DestroySelf()
    {
        //animation
        yield return new WaitForSeconds(timeAnimationDead);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!danger)
            return;

        if (col.transform.TryGetComponent(out Person target))
        {
            if (initiator.Army && target.Army)
            {
                if (!OnTrigger(skill.TriggerDanger, initiator.Army, target.Army))
                    return;
            }
            else
            {
                if (!OnTrigger(skill.TriggerDanger, initiator, target))
                    return;
            }

            skill.SetEffectsAndBuffs(initiator, target);
            if (skill.MaxCountCatch > 0)
                countCatch--;
            if (countCatch <= 0)
                _ = StartCoroutine(DestroySelf());
            return;
        }

        _ = StartCoroutine(DestroySelf());
    }

    private void FixedUpdate()
    {
        tr.position += tr.right * skill.Speed;
    }

    public void Build(Person initiator, Projectile skill, Person targetPerson = null)
    {
        this.targetPerson = targetPerson;
        this.initiator = initiator;
        this.skill = skill;
        countCatch = skill.MaxCountCatch;
    }

    #endregion Methods
}
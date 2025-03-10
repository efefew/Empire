#region

using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

#endregion

[RequireComponent(typeof(TemporaryAction))]
public partial class Person : MonoBehaviour
{

    public Army Army { get; private set; }

    public Status Status { get; private set; }

    /// <summary>
    ///     ����� �� ������������� �����
    /// </summary>
    public bool Ready { get; set; }

    public TemporaryAction TemporaryBuff { get; private set; }

    private const int WAIT_MELEE = 1;

    [FormerlySerializedAs("target")] public Transform Target;

    #region Methods

    private void Awake()
    {
        TemporaryBuff = GetComponent<TemporaryAction>();
        _scaleDefault = transform.localScale;
    }
    private void DeadPerson(Person person)
    {
        if (person != this)
            return;
        OnDeadPerson -= DeadPerson;
        if (Army)
            _ = Army.Persons.Remove(this);
        ChangeStateAnimation(deadState, uint.MaxValue);
        if (transform.childCount > 0)
        {
            if (transform.GetChild(0).GetComponent<SpriteRenderer>())
                transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder -= 1;
            transform.GetChild(0).SetParent(transform.parent);
        }

        Destroy(Target.gameObject);
        Destroy(gameObject);
    }

    private IEnumerator MeleeUpdate()
    {
        while (true)
        {
            if (!Status.Melee)
            {
                yield return new WaitForSeconds(WAIT_MELEE);
                continue;
            }

            yield return new WaitForSeconds(Status.Melee.TimeCooldown);
            if (health == 0)
                yield break;
            // ���������, �� �������� �� ��
            if (stunCount == 0)
                Status.Melee.Run(this);
        }
    }

    private IEnumerator ICastRun(Skill skill, Person target = null)
    {
        _ = Stun(skill.TimeCast);
        yield return new WaitForSeconds(skill.TimeCast);
        skill.Run(this, target);
    }

    private IEnumerator ICastRun(Skill skill, Vector3 target)
    {
        _ = Stun(skill.TimeCast);
        yield return new WaitForSeconds(skill.TimeCast);
        skill.Run(this, target);
    }

    public void Build(Army army)
    {
        this.Army = army;
        Build(army.status);
    }

    private void Build(Status status)
    {
        this.Status = status;
        for (int i = 0; i < status.Skills.Length; i++)
            amountSkill.Add(status.Skills[i], status.Skills[i].MaxAmountSkill);

        Target.SetParent(transform.parent, true);
        health = status.MaxHealth;
        stamina = status.MaxStamina;
        mana = status.MaxMana;
        morality = status.MaxMorality;
        _ = StartCoroutine(MeleeUpdate());
        _ = StartCoroutine(IRegenUpdate());
        _ = StartCoroutine(IStopStatusUpdate());
        OnDeadPerson += DeadPerson;
    }

    public bool CastRun(Skill skill, Person target)
    {
        if (!skill.LimitRun(this, target.transform.position)) return false;
        if (skill.TryGetComponent(out Melee melee) && !melee.canMiss)
        {
            Status.Melee = melee;
            if (!melee.canMiss)
                ChangeStateAnimation(skill.NameAnimation, 1);
        }
        else
        {
            ChangeStateAnimation(skill.NameAnimation, 1);
        }

        _ = StartCoroutine(ICastRun(skill, target));
        return true;

    }

    public bool CastRun(Skill skill, Vector3 target)
    {
        if (!skill.LimitRun(this, target)) return false;
        if (skill.TryGetComponent(out Melee melee) && !melee.canMiss)
        {
            Status.Melee = melee;
            if (!melee.canMiss)
                ChangeStateAnimation(skill.NameAnimation, 1);
        }
        else
        {
            ChangeStateAnimation(skill.NameAnimation, 1);
        }

        _ = StartCoroutine(ICastRun(skill, target));
        return true;

    }

    #endregion Methods
}
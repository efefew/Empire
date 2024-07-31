using System.Collections;

using UnityEngine;

[RequireComponent(typeof(TemporaryAction))]
public partial class Person : MonoBehaviour, ICombatUnit
{
    #region Properties

    public Army army { get; set; }

    public Status status { get; private set; }
    public bool Repeat { get; set; }
    public bool Stand { get; set; }

    /// <summary>
    /// ����� �� ������������� �����
    /// </summary>
    public bool Ready { get; set; }

    public TemporaryAction temporaryBuff { get; set; }

    private const int WAIT_MELEE = 1;

    #endregion Properties

    #region Fields

    private Battlefield battlefield;
    public Transform target;

    #endregion Fields

    #region Methods

    private void Awake()
    {
        temporaryBuff = GetComponent<TemporaryAction>();
        scaleDefault = transform.localScale;
    }

    private void Start() => battlefield = Battlefield.singleton;

    private void DeadPerson(Person person)
    {
        if (person != this)
            return;
        OnDeadPerson -= DeadPerson;
        if (army)
            _ = army.persons.Remove(this);
        ChangeStateAnimation(deadState, uint.MaxValue);
        if (transform.childCount > 0)
        {
            if (transform.GetChild(0).GetComponent<SpriteRenderer>())
                transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder -= 1;
            transform.GetChild(0).SetParent(transform.parent);
        }

        Destroy(target.gameObject);
        Destroy(gameObject);
    }

    private IEnumerator MeleeUpdate()
    {
        while (true)
        {
            if (status.melee == null)
            {
                yield return new WaitForSeconds(WAIT_MELEE);
                continue;
            }

            yield return new WaitForSeconds(status.melee.timeCooldown);
            if (health == 0)
                yield break;
            // ���������, �� �������� �� ��
            if (stunCount == 0)
                status.melee.Run(this);
        }
    }

    /// <summary>
    /// ��������� �����
    /// </summary>
    /// <param name="skill">�����</param>
    private void UseSkill(Skill skill, Person target)
    {
        if (stunCount != 0)
            return;
        if (CastRun(skill, target))
        {
            //conteinerSkill.Silence(skill.timeCast);
            status.TimerSkillReload(skill, target);
        }
    }

    private IEnumerator ICastRun(Skill skill, Person target = null)
    {
        _ = Stun(skill.timeCast);
        yield return new WaitForSeconds(skill.timeCast);
        skill.Run(this, target);
    }

    private IEnumerator ICastRun(Skill skill, Vector3 target)
    {
        _ = Stun(skill.timeCast);
        yield return new WaitForSeconds(skill.timeCast);
        skill.Run(this, target);
    }

    public void Build(Army army)
    {
        this.army = army;
        Build(army.status);
    }

    public void Build(Status status)
    {
        this.status = status;
        for (int i = 0; i < status.skills.Length; i++)
            amountSkill.Add(status.skills[i], status.skills[i].maxAmountSkill);

        target.SetParent(transform.parent, true);
        health = status.maxHealth;
        stamina = status.maxStamina;
        mana = status.maxMana;
        morality = status.maxMorality;
        _ = StartCoroutine(MeleeUpdate());
        _ = StartCoroutine(IRegenUpdate());
        _ = StartCoroutine(IStopStatusUpdate());
        OnDeadPerson += DeadPerson;
    }

    /// <summary>
    /// ��������� �����
    /// </summary>
    /// <param name="target">����</param>
    public void TargetForUseSkill(ICombatUnit target)
    {
        battlefield.OnSetTargetArmy -= TargetForUseSkill;
        battlefield.OnSetTargetPoint -= TargetForUseSkill;
        if (target.TryGetValueOtherType(out Person person))
            UseSkill(battlefield.targetSkill, person);
        if (target.TryGetValueOtherType(out Army army))
            UseSkill(battlefield.targetSkill, army.persons[0]);

    }

    /// <summary>
    /// ��������� �����
    /// </summary>
    /// <param name="target">����</param>
    public void TargetForUseSkill(Vector3 target)
    {
        battlefield.OnSetTargetArmy -= TargetForUseSkill;
        battlefield.OnSetTargetPoint -= TargetForUseSkill;
        //.����������� UseSkill(battlefield.targetSkill, target);
    }

    public bool CastRun(Skill skill, Person target)
    {
        if (skill.LimitRun(this, target.transform.position))
        {
            if (skill.TryGetComponent(out Melee melee) && !melee.canMiss)
            {
                status.melee = melee;
                if (!melee.canMiss)
                    ChangeStateAnimation(skill.nameAnimation, 1);
            }
            else
            {
                ChangeStateAnimation(skill.nameAnimation, 1);
            }

            _ = StartCoroutine(ICastRun(skill, target));
            return true;
        }

        return false;
    }

    public bool CastRun(Skill skill, Vector3 target)
    {
        if (skill.LimitRun(this, target))
        {
            if (skill.TryGetComponent(out Melee melee) && !melee.canMiss)
            {
                status.melee = melee;
                if (!melee.canMiss)
                    ChangeStateAnimation(skill.nameAnimation, 1);
            }
            else
            {
                ChangeStateAnimation(skill.nameAnimation, 1);
            }

            _ = StartCoroutine(ICastRun(skill, target));
            return true;
        }

        return false;
    }

    #endregion Methods
}
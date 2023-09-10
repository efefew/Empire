using System.Collections;

using UnityEngine;

/// <summary>
/// Существо
/// </summary>
public partial class Person : MonoBehaviour, ICombatUnit
{
    #region Properties

    public Army army { get; private set; }

    public Status status { get; private set; }
    public bool StandStill { get; set; }
    /// <summary>
    /// готов ли испольлзовать навык
    /// </summary>
    public bool Ready { get; set; }

    #endregion Properties

    #region Fields

    private Battlefield battlefield;
    public Transform target;

    #endregion Fields

    #region Methods

    private void Awake() => scaleDefault = transform.localScale;

    private void Start() => battlefield = Battlefield.singleton;

    private void DeadPerson(Person person)
    {
        if (person != this)
            return;
        OnDeadPerson -= DeadPerson;
        if (army)
            _ = army.persons.Remove(this);
        //agentMove.agent.isStopped = true;
        ChangeStateAnimation(deadState, uint.MaxValue);
        if (transform.childCount > 0)
        {
            if (transform.GetChild(0).GetComponent<SpriteRenderer>())
                transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder -= 1;
            transform.GetChild(0).SetParent(transform.parent);
        }

        Destroy(target.gameObject);
        Destroy(gameObject);

        //gameObject.SetActive(false);
    }

    private IEnumerator SkillUpdate(Skill skill)
    {
        while (true)
        {
            yield return new WaitForSeconds(skill.timeCooldown);
            if (health == 0)
                yield break;
            // Проверяем, не оглушены ли мы
            if (stunCount == 0)
                skill.Run(this);
        }
    }

    /// <summary>
    /// Запускает навык
    /// </summary>
    /// <param name="skill">навык</param>
    private void UseSkill(Skill skill, Person target)
    {
        if (stunCount != 0)
            return;
        if (CastRun(skill, target))
        {
            //conteinerSkill.Silence(skill.timeCast);
            status.TimerSkillReload(skill);
        }
    }

    private IEnumerator ICastRun(Skill skill, Person target = null)
    {
        Stun(skill.timeCast);
        yield return new WaitForSeconds(skill.timeCast);
        skill.Run(this, target);
    }

    public void Build(Army army)
    {
        this.army = army;
        Build(army.status);
        OnDeadPerson += DeadPerson;
    }

    public void Build(Status status)
    {
        this.status = status;
        target.SetParent(transform.parent, true);
        health = status.maxHealth;
        stamina = status.maxStamina;
        mana = status.maxMana;
        morality = status.maxMorality;
        if (status.permanentSkills.Length > 0)
        {
            for (int id = 0; id < status.permanentSkills.Length; id++)
                _ = StartCoroutine(SkillUpdate(status.permanentSkills[id]));
        }

        _ = StartCoroutine(IMoveUpdate());
        _ = StartCoroutine(RegenUpdate());
        OnDeadPerson += DeadPerson;
    }

    /// <summary>
    /// Запускает навык
    /// </summary>
    /// <param name="target">цель</param>
    public void TargetForUseSkill(ICombatUnit target)
    {
        battlefield.OnSetTarget -= TargetForUseSkill;
        if (target.TryGetValueOtherType(out Person person))
            UseSkill(battlefield.targetButtonSkill.skillTarget, person);
        if (target.TryGetValueOtherType(out Army army))
            UseSkill(battlefield.targetButtonSkill.skillTarget, army.persons[0]);
    }
    public bool CastRun(Skill skill, Person target)
    {
        if (skill.TryGetComponent(out Melee melee) && !melee.canMiss)
            ChangeStateAnimation(skill.nameAnimation, 1);
        else
            ChangeStateAnimation(skill.nameAnimation, 1);

        if (skill.LimitRun(this, target))
        {
            _ = StartCoroutine(ICastRun(skill, target));
            return true;
        }

        return false;
    }

    #endregion Methods
}
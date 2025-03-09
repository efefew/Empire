#region

using System;
using System.Collections;
using System.Collections.Generic;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using static Attack;
using Random = UnityEngine.Random;

#endregion

public partial class Person : MonoBehaviour // �������������� ��������
{
    #region Delegates

    public delegate void OnDamageHandler(Person friend, Person enemy, DamageType type, Skill attackType, float damage);

    #endregion Delegates

    #region Events

    public event Action<Person> OnDeadPerson;

    public event OnDamageHandler OnDamageTaken;

    public event OnDamageHandler OnDamageGiven;

    #endregion Events

    #region Properties

    public float health { get; private set; }
    public float mana { get; private set; }
    public float stamina { get; private set; }
    public float morality { get; private set; }

    [Min(0)] public float speedScale = 1;

    public bool repeat;
    public bool needTarget = true;
    public bool collective;
    public bool distracted;

    #endregion Properties

    #region Fields

    private const float MIN_PRECENT_WALK_STAMINA = 80f;
    private const float UPDATE_REGEN = 1.5f;

    /// <summary>
    ///     ���������� �������
    /// </summary>
    public List<Buff> buffs;

    public Dictionary<Skill, float> amountSkill = new();

    [ReadOnly] public Melee tempMelee;

    #endregion Fields

    #region Methods

    private IEnumerator IRegenUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(UPDATE_REGEN);

            // ���� �������� ������� ����� ������ 0, �������� ������� OnDeadArmy
            if (health == 0)
            {
                OnDeadPerson?.Invoke(this);
                yield break;
            }

            health += status.regenHealth;
            health = Mathf.Clamp(health, 0, status.maxHealth);

            mana += status.regenMana;
            mana = Mathf.Clamp(mana, 0, status.maxMana);
            if (!isStoped && stamina >= status.maxStamina * MIN_PRECENT_WALK_STAMINA / 100f)
            {
                stamina -= status.regenStamina / 10;
                stamina = Mathf.Clamp(stamina, status.maxStamina * MIN_PRECENT_WALK_STAMINA / 100f,
                    stamina + status.regenStamina / 10);
            }
            else
            {
                stamina += status.regenStamina;
                stamina = Mathf.Clamp(stamina, 0, status.maxStamina);
            }

            morality += status.regenMorality;
            morality = Mathf.Clamp(morality, 0, status.maxMorality);
        }
    }

    /// <summary>
    ///     ��������� �����
    /// </summary>
    /// <param name="enemy">���, ��� ������� ����</param>
    /// <param name="type">��� �����</param>
    /// <param name="damage">����</param>
    public void TakeDamage(Person enemy, DamageType type, Skill attackType, float damage)
    {
        // ���������, ���� �� � ����� ��������������� ����� ��� ������� ���� �����
        if (status.scaleTakeDamage.ContainsKey(type))
            damage *= status.scaleTakeDamage[type];

        // ���������, ���� �� � ����� ��� ��� ������� ���� �����
        if (status.shield.ContainsKey(type) && status.shield[type] + damage != 0)
            damage = damage * damage / (status.shield[type] + damage);

        // �������� ������� OnDamageTaken, ����� ��������� � ���������� �����
        OnDamageTaken?.Invoke(this, enemy, type, attackType, damage);

        // �������� �������� ������� �� ������ ���� �����
        health += type == DamageType.Healing ? damage : -damage;

        // ������������ �������� ������� ������������ ���������
        health = Mathf.Clamp(health, 0, status.maxHealth);

        // ���� �������� ������� ����� ������ 0, �������� ������� OnDeadArmy
        if (health == 0)
            OnDeadPerson?.Invoke(this);
    }

    /// <summary>
    ///     ��������� �����
    /// </summary>
    /// <param name="enemy">���, ���� ������� ����</param>
    /// <param name="attacks">�����</param>
    /// <param name="skill">����� �����</param>
    public void GiveDamage(Person enemy, DamageTypeDictionary attacks, Skill skill)
    {
        foreach (var attack in attacks)
        {
            // �����������, ��� ���� �������� ������������� ���������
            float damage = Mathf.Abs(attack.Value);

            // ���������, ���� �� � ����� ��������������� ����� ��� ������� ���� �����
            if (status.scaleGiveDamage.ContainsKey(attack.Key))
                damage *= status.scaleGiveDamage[attack.Key];

            // ���������, �������� �� ����� ����������� ������ �� ������ ����� ������������ ����� �����
            damage = Random.Range(0, 100f) <= status.critChance ? damage * status.crit : damage;

            // �������� ������� OnDamageGiven, ����� ��������� ��� � ��������� �����
            OnDamageGiven?.Invoke(this, enemy, attack.Key, skill, damage);

            // ������� ���� �����
            enemy.TakeDamage(this, attack.Key, skill, damage);
        }
    }

    public bool CanUseSkill(Skill skill)
    {
        // ���������, ���������� �� � ��� ������� � ���� ��� �����
        if (stamina < skill.stamina || mana < skill.mana)
            return false;

        // �������� ��������� ������� � ���� ��� ���������� ������
        stamina -= skill.stamina;
        mana -= skill.mana;
        return true;
    }

    public void ReturnUseSkill(Skill skill)
    {
        stamina += skill.stamina;
        mana += skill.mana;
    }

    [Button("Kill", 15)]
    public void Kill()
    {
        TakeDamage(null, DamageType.Absolute, new Melee(), status.maxHealth);
    }

    #endregion Methods
}
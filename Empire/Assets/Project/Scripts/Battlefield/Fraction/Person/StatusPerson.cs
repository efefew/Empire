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

    private const float MIN_PRECENT_WALK_STAMINA = 80f;
    private const float UPDATE_REGEN = 1.5f;

    /// <summary>
    ///     ���������� �������
    /// </summary>
    public List<Buff> buffs;

    public Dictionary<Skill, float> amountSkill = new();

    [ReadOnly] public Melee tempMelee;

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

            health += Status.RegenHealth;
            health = Mathf.Clamp(health, 0, Status.MaxHealth);

            mana += Status.RegenMana;
            mana = Mathf.Clamp(mana, 0, Status.MaxMana);
            if (!_isStoped && stamina >= Status.MaxStamina * MIN_PRECENT_WALK_STAMINA / 100f)
            {
                stamina -= Status.RegenStamina / 10;
                stamina = Mathf.Clamp(stamina, Status.MaxStamina * MIN_PRECENT_WALK_STAMINA / 100f,
                    stamina + Status.RegenStamina / 10);
            }
            else
            {
                stamina += Status.RegenStamina;
                stamina = Mathf.Clamp(stamina, 0, Status.MaxStamina);
            }

            morality += Status.RegenMorality;
            morality = Mathf.Clamp(morality, 0, Status.MaxMorality);
        }
    }

    /// <summary>
    ///     ��������� �����
    /// </summary>
    /// <param name="enemy">���, ��� ������� ����</param>
    /// <param name="type">��� �����</param>
    /// <param name="attackType"></param>
    /// <param name="damage">����</param>
    public void TakeDamage(Person enemy, DamageType type, Skill attackType, float damage)
    {
        if (Status.ScaleTakeDamage.TryGetValue(type, out float value))
            damage *= value;
        
        if (Status.Shield.ContainsKey(type) && Status.Shield[type] + damage != 0)
            damage = damage * damage / (Status.Shield[type] + damage);
        
        OnDamageTaken?.Invoke(this, enemy, type, attackType, damage);
        health += type == DamageType.Healing ? damage : -damage;
        health = Mathf.Clamp(health, 0, Status.MaxHealth);
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
            if (Status.ScaleGiveDamage.TryGetValue(attack.Key, out float value))
                damage *= value;

            // ���������, �������� �� ����� ����������� ������ �� ������ ����� ������������ ����� �����
            damage = Random.Range(0, 100f) <= Status.CreteChance ? damage * Status.Crit : damage;

            // �������� ������� OnDamageGiven, ����� ��������� ��� � ��������� �����
            OnDamageGiven?.Invoke(this, enemy, attack.Key, skill, damage);

            // ������� ���� �����
            enemy.TakeDamage(this, attack.Key, skill, damage);
        }
    }

    public bool CanUseSkill(Skill skill)
    {
        // ���������, ���������� �� � ��� ������� � ���� ��� �����
        if (stamina < skill.Stamina || mana < skill.Mana)
            return false;

        // �������� ��������� ������� � ���� ��� ���������� ������
        stamina -= skill.Stamina;
        mana -= skill.Mana;
        return true;
    }

    public void ReturnUseSkill(Skill skill)
    {
        stamina += skill.Stamina;
        mana += skill.Mana;
    }

    [Button("Kill", 15)]
    public void Kill()
    {
        TakeDamage(null, DamageType.Absolute, new Melee(), Status.MaxHealth);
    }

    #endregion Methods
}
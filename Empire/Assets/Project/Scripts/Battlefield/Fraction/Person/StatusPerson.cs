using System;
using System.Collections;
using System.Collections.Generic;

using AdvancedEditorTools.Attributes;

using UnityEngine;

using static Attack;

public partial class Person : MonoBehaviour // Характеристики существа
{
    #region Events

    public event Action<Person> OnDeadPerson;

    public event OnDamageHandler OnDamageTaken;

    public event OnDamageHandler OnDamageGiven;

    #endregion Events

    #region Delegates

    public delegate void OnDamageHandler(Person friend, Person enemy, DamageType type, Skill attackType, float damage);

    #endregion Delegates

    #region Properties

    public float health { get; private set; }
    public float mana { get; private set; }
    public float stamina { get; private set; }
    public float morality { get; private set; }

    [Min(0)]
    public float speedScale = 1;

    public bool repeat = false;
    public bool needTarget = true;
    public bool collective = false;
    public bool distracted = false;
    public bool stopDistracted = false;
    #endregion Properties

    #region Fields

    private const float MIN_PRECENT_WALK_STAMINA = 80f;
    private const float UPDATE_REGEN = 1.5f;

    /// <summary>
    /// Наложенные эффекты
    /// </summary>
    public List<Buff> buffs;
    public Dictionary<Skill, float> amountSkill = new();

    [ReadOnly]
    public Melee tempMelee;
    #endregion Fields

    #region Methods
    private IEnumerator IRegenUpdate()
    {
        while (true)
        {
            yield return new WaitForSeconds(UPDATE_REGEN);

            // Если здоровье персоны стало равным 0, вызываем событие OnDeadArmy
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
                stamina = Mathf.Clamp(stamina, status.maxStamina * MIN_PRECENT_WALK_STAMINA / 100f, stamina + (status.regenStamina / 10));
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
    /// Получение урона
    /// </summary>
    /// <param name="enemy">тот, кто наносит урон</param>
    /// <param name="type">тип урона</param>
    /// <param name="damage">урон</param>
    public void TakeDamage(Person enemy, DamageType type, Skill attackType, float damage)
    {
        // Проверяем, есть ли у армии масштабирование урона для данного типа урона
        if (status.scaleTakeDamage.ContainsKey(type))
            damage *= status.scaleTakeDamage[type];

        // Проверяем, есть ли у армии щит для данного типа урона
        if (status.shield.ContainsKey(type) && status.shield[type] + damage != 0)
            damage = damage * damage / (status.shield[type] + damage);

        // Вызываем событие OnDamageTaken, чтобы уведомить о полученном уроне
        OnDamageTaken?.Invoke(this, enemy, type, attackType, damage);

        // Изменяем здоровье персоны на основе типа урона
        health += type == DamageType.Healing ? damage : -damage;

        // Ограничиваем здоровье персоны максимальным значением
        health = Mathf.Clamp(health, 0, status.maxHealth);

        // Если здоровье персоны стало равным 0, вызываем событие OnDeadArmy
        if (health == 0)
            OnDeadPerson?.Invoke(this);
    }

    /// <summary>
    /// Нанесение урона
    /// </summary>
    /// <param name="enemy">тот, кому наносят урон</param>
    /// <param name="attacks">атаки</param>
    /// <param name="skill">навык атаки</param>
    public void GiveDamage(Person enemy, DamageTypeDictionary attacks, Skill skill)
    {
        foreach (KeyValuePair<DamageType, float> attack in attacks)
        {
            // Гарантируем, что урон является положительным значением
            float damage = Mathf.Abs(attack.Value);

            // Проверяем, есть ли у армии масштабирование урона для данного типа урона
            if (status.scaleGiveDamage.ContainsKey(attack.Key))
                damage *= status.scaleGiveDamage[attack.Key];

            // Проверяем, является ли атака критическим ударом на основе шанса критического удара армии
            damage = UnityEngine.Random.Range(0, 100f) <= status.critChance ? damage * status.crit : damage;

            // Вызываем событие OnDamageGiven, чтобы уведомить его о нанесённом уроне
            OnDamageGiven?.Invoke(this, enemy, attack.Key, skill, damage);

            // Наносим урон врагу
            enemy.TakeDamage(this, attack.Key, skill, damage);
        }
    }

    public bool CanUseSkill(Skill skill)
    {
        // Проверяем, достаточно ли у нас стамины и маны для атаки
        if (stamina < skill.stamina || mana < skill.mana)
            return false;

        // Вычитаем стоимость стамины и маны для выполнения навыка
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
    public void Kill() => TakeDamage(null, DamageType.Absolute, new Melee(), status.maxHealth);

    #endregion Methods
}
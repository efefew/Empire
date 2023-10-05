using System;

using UnityEngine;

using static Attack;

[AddComponentMenu("Effect/Attack")]
public class Attack : Effect
{
    #region Enums

    /// <summary>
    /// Тип повреждения
    /// </summary>
    public enum DamageType
    {
        [Tooltip("Тип повреждения от пореза")]
        /// <summary>
        /// Тип повреждения от пореза
        /// </summary>
        Сutting,

        [Tooltip("Тип повреждения от прокалывания")]
        /// <summary>
        /// Тип повреждения от прокалывания
        /// </summary>
        Pricking,

        [Tooltip("Тип повреждения от удара")]
        /// <summary>
        /// Тип повреждения от удара
        /// </summary>
        Punch,

        [Tooltip("Тип повреждения от взрыва")]
        /// <summary>
        /// Тип повреждения от взрыва
        /// </summary>
        Explosion,

        [Tooltip("Тип повреждения от выстрела")]
        /// <summary>
        /// Тип повреждения от выстрела
        /// </summary>
        Shooting,

        [Tooltip("Тип повреждения от перегрева")]
        /// <summary>
        /// Тип повреждения от перегрева
        /// </summary>
        Overheat,

        [Tooltip("Тип повреждения от обморожения")]
        /// <summary>
        /// Тип повреждения от обморожения
        /// </summary>
        Frostbite,

        [Tooltip("Тип повреждения от электрического разряда")]
        /// <summary>
        /// Тип повреждения от электрического разряда
        /// </summary>
        ElectricShock,

        [Tooltip("Тип повреждения от растворения в кислоте")]
        /// <summary>
        /// Тип повреждения от растворения в кислоте
        /// </summary>
        DissolutionInAcid,

        [Tooltip("Тип повреждения от яда")]
        /// <summary>
        /// Тип повреждения от яда
        /// </summary>
        Poison,

        [Tooltip("Тип лечения")]
        /// <summary>
        /// Тип лечения
        /// </summary>
        Healing,

        [Tooltip("Тип повреждения от чистой магии")]
        /// <summary>
        /// Тип повреждения от чистой магии
        /// </summary>
        ClearMagic,

        [Tooltip("Тип абсолютного повреждения (например, отдельный тип для оружия, которое наносит неизбежный урон)")]
        /// <summary>
        /// Тип абсолютного повреждения (например, отдельный тип для оружия, которое наносит неизбежный урон)
        /// </summary>
        Absolute
    }

    #endregion Enums

    #region Fields

    [SerializeField]
    public DamageTypeDictionary attacks = new();

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target, Skill skill) => initiator.GiveDamage(target, attacks, skill);

    #endregion Methods
}

[Serializable]
public class DamageTypeDictionary : SerializableDictionary<DamageType, float>
{ }
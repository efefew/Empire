#region

using System;
using UnityEngine;
using UnityEngine.Serialization;
using static Attack;

#endregion

[AddComponentMenu("Effect/Attack")]
public class Attack : Effect
{
    #region Enums

    /// <summary>
    ///     Тип повреждения
    /// </summary>
    public enum DamageType
    {
        [Tooltip("Тип повреждения от пореза")] 
        Сutting,

        [Tooltip("Тип повреждения от прокалывания")]
        Pricking,

        [Tooltip("Тип повреждения от удара")] 
        Punch,

        [Tooltip("Тип повреждения от взрыва")] 
        Explosion,

        [Tooltip("Тип повреждения от выстрела")]
        Shooting,

        [Tooltip("Тип повреждения от перегрева")]
        Overheat,

        [Tooltip("Тип повреждения от обморожения")]
        Frostbite,

        [Tooltip("Тип повреждения от электрического разряда")]
        ElectricShock,

        [Tooltip("Тип повреждения от растворения в кислоте")]
        DissolutionInAcid,

        [Tooltip("Тип повреждения от яда")] Poison,

        [Tooltip("Тип лечения")]
        Healing,

        [Tooltip("Тип повреждения от чистой магии")]
        ClearMagic,

        [Tooltip("Тип абсолютного повреждения (например, отдельный тип для оружия, которое наносит неизбежный урон)")]
        Absolute
    }

    #endregion Enums

    #region Fields

    [FormerlySerializedAs("attacks")] [SerializeField] public DamageTypeDictionary Attacks = new();

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target, Skill skill)
    {
        initiator.GiveDamage(target, Attacks, skill);
    }

    #endregion Methods
}

[Serializable]
public class DamageTypeDictionary : SerializableDictionary<DamageType, float>
{
}
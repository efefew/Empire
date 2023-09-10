using System;

using UnityEngine;

using static Attack;

[AddComponentMenu("Skill Effect/Attack")]
public class Attack : Effect
{
    #region Enums

    /// <summary>
    /// ��� �����������
    /// </summary>
    public enum DamageType
    {
        [Tooltip("��� ����������� �� ������")]
        /// <summary>
        /// ��� ����������� �� ������
        /// </summary>
        �utting,

        [Tooltip("��� ����������� �� ������������")]
        /// <summary>
        /// ��� ����������� �� ������������
        /// </summary>
        Pricking,

        [Tooltip("��� ����������� �� �����")]
        /// <summary>
        /// ��� ����������� �� �����
        /// </summary>
        Punch,

        [Tooltip("��� ����������� �� ������")]
        /// <summary>
        /// ��� ����������� �� ������
        /// </summary>
        Explosion,

        [Tooltip("��� ����������� �� ��������")]
        /// <summary>
        /// ��� ����������� �� ��������
        /// </summary>
        Shooting,

        [Tooltip("��� ����������� �� ���������")]
        /// <summary>
        /// ��� ����������� �� ���������
        /// </summary>
        Overheat,

        [Tooltip("��� ����������� �� �����������")]
        /// <summary>
        /// ��� ����������� �� �����������
        /// </summary>
        Frostbite,

        [Tooltip("��� ����������� �� �������������� �������")]
        /// <summary>
        /// ��� ����������� �� �������������� �������
        /// </summary>
        ElectricShock,

        [Tooltip("��� ����������� �� ����������� � �������")]
        /// <summary>
        /// ��� ����������� �� ����������� � �������
        /// </summary>
        DissolutionInAcid,

        [Tooltip("��� ����������� �� ���")]
        /// <summary>
        /// ��� ����������� �� ���
        /// </summary>
        Poison,

        [Tooltip("��� �������")]
        /// <summary>
        /// ��� �������
        /// </summary>
        Healing,

        [Tooltip("��� ����������� �� ������ �����")]
        /// <summary>
        /// ��� ����������� �� ������ �����
        /// </summary>
        ClearMagic,

        [Tooltip("��� ����������� ����������� (��������, ��������� ��� ��� ������, ������� ������� ���������� ����)")]
        /// <summary>
        /// ��� ����������� ����������� (��������, ��������� ��� ��� ������, ������� ������� ���������� ����)
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
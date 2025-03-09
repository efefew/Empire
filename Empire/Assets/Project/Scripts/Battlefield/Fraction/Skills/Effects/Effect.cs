#region

using UnityEngine;

#endregion

/// <summary>
///     ������������ ������
/// </summary>
public abstract class Effect : MonoBehaviour
{
    #region Methods

    /// <summary>
    ///     ��������� ������
    /// </summary>
    /// <param name="initiator">���������</param>
    /// <param name="target">����</param>
    public abstract void Run(Person initiator, Person target, Skill skill);

    #endregion Methods
}
using UnityEngine;

/// <summary>
/// Моментальный эффект
/// </summary>
public abstract class Effect : MonoBehaviour
{
    #region Methods

    /// <summary>
    /// Запустить эффект
    /// </summary>
    /// <param name="initiator">инициатор</param>
    /// <param name="target">цель</param>
    public abstract void Run(Person initiator, Person target, Skill skill);

    #endregion Methods
}
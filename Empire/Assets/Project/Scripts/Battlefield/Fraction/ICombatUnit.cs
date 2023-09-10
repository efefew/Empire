/// <summary>
/// Боевая единица
/// </summary>
public interface ICombatUnit
{
    /// <summary>
    /// Стоять на месте
    /// </summary>
    bool StandStill { get; set; }

    /// <summary>
    /// Запускает навык
    /// </summary>
    /// <param name="target">цель</param>
    abstract void TargetForUseSkill(ICombatUnit target);
}

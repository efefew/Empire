/// <summary>
/// Боевая единица
/// </summary>
public interface ICombatUnit
{
    /// <summary>
    /// Повторять использование навыка
    /// </summary>
    bool Repeat { get; set; }
    /// <summary>
    /// Стоять на месте при использовании навыка
    /// </summary>
    bool Stand { get; set; }

    /// <summary>
    /// Запускает навык
    /// </summary>
    /// <param name="target">цель</param>
    abstract void TargetForUseSkill(ICombatUnit target);
}

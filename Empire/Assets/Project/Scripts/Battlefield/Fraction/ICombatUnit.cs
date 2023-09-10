/// <summary>
/// ������ �������
/// </summary>
public interface ICombatUnit
{
    /// <summary>
    /// ������ �� �����
    /// </summary>
    bool StandStill { get; set; }

    /// <summary>
    /// ��������� �����
    /// </summary>
    /// <param name="target">����</param>
    abstract void TargetForUseSkill(ICombatUnit target);
}

/// <summary>
/// ������ �������
/// </summary>
public interface ICombatUnit
{
    /// <summary>
    /// ��������� ������������� ������
    /// </summary>
    bool Repeat { get; set; }
    /// <summary>
    /// ������ �� ����� ��� ������������� ������
    /// </summary>
    bool Stand { get; set; }

    /// <summary>
    /// ��������� �����
    /// </summary>
    /// <param name="target">����</param>
    abstract void TargetForUseSkill(ICombatUnit target);
}

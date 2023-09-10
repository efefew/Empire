using UnityEngine;

[AddComponentMenu("Skill/Melee")]
public class Melee : Skill
{
    #region Fields

    /// <summary>
    /// ������� �� ������ �����
    /// </summary>
    public bool canMiss;

    #endregion Fields

    #region Methods

    public override bool LimitRangeRun(Person initiator, Person target = null) => Physics2D.OverlapCircleAll(initiator.transform.position, range, LayerMask.GetMask("Person")).Length > 0;

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target))
            return;
        // ������� ��� ���������� � ������� �������� ������
        Collider2D[] colliders2D = Physics2D.OverlapCircleAll(initiator.transform.position, range, LayerMask.GetMask("Person"));

        // ������� �����, ���������� �������
        int countCatch = 0;
        for (int i = 0; i < colliders2D.Length; i++)
        {
            if (!colliders2D[i].GetComponent<Person>())
                continue;
            target = colliders2D[i].GetComponent<Person>();
            // ���� � ���� ��� ��������, ��������� � ��������� ����
            if (target.health <= 0)
                continue;
            // ������� ���� � ��������� ������� ������
            if (OnTrigger(triggerTarget, initiator, target))
                SetEffectsAndBuffs(initiator, target, ref countCatch);
            // ���� ������ ����� ��������� ���� ��������� � ���� ���� ��������, �������� ������ �������� ��������� �� ���� ����� �� ��������� �����
            if (targetMove && countCatch > 0)
            {

                initiator.Pursuit(target, timeTargetMove);
            }
            // ���� ���������� ���������� ����� �������� ������������� �������� � ��� �������� �� ����� 0, �� ���������� ���� �� ����������
            if (countCatch >= maxCountCatch && maxCountCatch != 0)
                break;
        }
        // ���� �� ���� �������� �� ���� ���� � ������ ����� ������������, �� �������� ������ ������������ � ������������� ������ � �������� ���������. �����, �������� ������ �������� �������� ������.
        if (countCatch == 0 && canMiss)
        {
            initiator.ReturnUseSkill(this);
            initiator.RemoveStateAnimation(nameAnimation);
        }
        else
        {
            if (consumable)
                amountSkill--;
            if (countCatch != 0)
                initiator.ChangeStateAnimation(nameAnimation, 1);
        }
    }

    #endregion Methods
}
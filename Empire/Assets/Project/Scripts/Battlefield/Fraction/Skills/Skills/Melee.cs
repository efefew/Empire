#region

using System.Collections;
using UnityEngine;

#endregion

[AddComponentMenu("Skill/Melee")]
public class Melee : Skill
{
    #region Fields

    /// <summary>
    ///     ������� �� ������ �����
    /// </summary>
    public bool canMiss;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        //if (target != null && !LimitRun(initiator, target))
        //    return;
        // ������� ��� ���������� � ������� �������� ������
        var colliders2D = Physics2D.OverlapCircleAll(initiator.transform.position, range, LayerMask.GetMask("Person"));

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
            {
                countCatch++;
                SetEffectsAndBuffs(initiator, target);
            }

            // ���� ������ ����� ��������� ���� ��������� � ���� ���� ��������, �������� ������ �������� ��������� �� ���� ����� �� ��������� �����
            if (targetMove && countCatch > 0) _ = initiator.Pursuit(target, ITargetMove(initiator, timeTargetMove));
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
                initiator.amountSkill[this]--;
            if (countCatch != 0)
                initiator.ChangeStateAnimation(nameAnimation, 1);
        }
    }

    public override void Run(Person initiator, Vector3 target)
    {
        Debug.LogError("��� ����������� �� ����� ���� ���������� �� �����");
    }

    private IEnumerator ITargetMove(Person initiator, float time)
    {
        if (time <= 0)
            yield break;
        initiator.distracted = true;
        yield return new WaitForSeconds(time);
        initiator.distracted = false;
    }

    #endregion Methods
}
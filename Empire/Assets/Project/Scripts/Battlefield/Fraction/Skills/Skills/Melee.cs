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
        var colliders2D = Physics2D.OverlapCircleAll(initiator.transform.position, Range, LayerMask.GetMask("Person"));

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
            if (OnTrigger(TriggerTarget, initiator, target))
            {
                countCatch++;
                SetEffectsAndBuffs(initiator, target);
            }

            // ���� ������ ����� ��������� ���� ��������� � ���� ���� ��������, �������� ������ �������� ��������� �� ���� ����� �� ��������� �����
            if (TargetMove && countCatch > 0) _ = initiator.Pursuit(target, ITargetMove(initiator, TimeTargetMove));
            // ���� ���������� ���������� ����� �������� ������������� �������� � ��� �������� �� ����� 0, �� ���������� ���� �� ����������
            if (countCatch >= MaxCountCatch && MaxCountCatch != 0)
                break;
        }

        // ���� �� ���� �������� �� ���� ���� � ������ ����� ������������, �� �������� ������ ������������ � ������������� ������ � �������� ���������. �����, �������� ������ �������� �������� ������.
        if (countCatch == 0 && canMiss)
        {
            initiator.ReturnUseSkill(this);
            initiator.RemoveStateAnimation(NameAnimation);
        }
        else
        {
            if (Consumable)
                initiator.amountSkill[this]--;
            if (countCatch != 0)
                initiator.ChangeStateAnimation(NameAnimation, 1);
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
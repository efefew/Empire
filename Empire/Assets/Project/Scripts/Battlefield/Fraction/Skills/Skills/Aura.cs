using System.Collections;

using UnityEngine;

[AddComponentMenu("Skill/Aura")]
public class Aura : Skill
{
    #region Fields

    public Person auraTarget;

    /// <summary>
    /// ����������
    /// </summary>
    [Min(0.001f)]
    public float gap;

    /// <summary>
    /// �������
    /// </summary>
    public uint frequency;

    /// <summary>
    /// ������ ��� ������������� ������?
    /// </summary>
    public bool stun;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null) => _ = stun ? initiator.Stun(IRun(initiator, target)) : StartCoroutine(IRun(initiator, target));

    public IEnumerator IRun(Person initiator, Person target)
    {
        for (int ID = 0; ID < frequency; ID++)
        {
            if (!LimitRun(initiator, target))
                yield break;

            if (consumable)
                amountSkill--;
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
                {
                    countCatch++;
                    SetEffectsAndBuffs(initiator, target);
                }
                // ���� ���������� ���������� ����� �������� ������������� �������� � ��� �������� �� ����� 0, �� ���������� ���� �� ����������
                if (countCatch >= maxCountCatch && maxCountCatch != 0)
                    break;
            }

            yield return new WaitForSeconds(gap);
        }
    }

    #endregion Methods
}
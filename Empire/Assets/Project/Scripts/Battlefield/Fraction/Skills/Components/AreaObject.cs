#region

using System.Collections;
using UnityEngine;

#endregion

public class AreaObject : MonoBehaviour
{
    private Person initiator;
    private Area skill;
    private Transform tr;

    private void Start()
    {
        tr = transform;
    }

    public void Build(Person initiator, Area skill)
    {
        this.initiator = initiator;
        this.skill = skill;
        _ = StartCoroutine(IRun());
    }

    private IEnumerator IRun()
    {
        Person target;
        for (int ID = 0; ID < skill.frequency; ID++)
        {
            // ������� ��� ���������� � ������� �������� ������
            var colliders2D = Physics2D.OverlapCircleAll(tr.position, skill.radius, LayerMask.GetMask("Person"));
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
                if (Skill.OnTrigger(skill.TriggerTarget, initiator, target))
                {
                    countCatch++;
                    skill.SetEffectsAndBuffs(initiator, target);
                }

                // ���� ���������� ���������� ����� �������� ������������� �������� � ��� �������� �� ����� 0, �� ���������� ���� �� ����������
                if (countCatch >= skill.MaxCountCatch && skill.MaxCountCatch != 0)
                    yield break;
            }

            yield return new WaitForSeconds(skill.gap);
        }
    }
}
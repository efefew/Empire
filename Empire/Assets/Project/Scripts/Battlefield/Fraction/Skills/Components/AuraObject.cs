#region

using System.Collections;
using UnityEngine;

#endregion

public class AuraObject : MonoBehaviour
{
    private Person initiator, target;
    private Aura skill;
    private Transform tr;

    private void Start()
    {
        tr = transform;
    }

    public void Build(Person initiator, Aura skill, Person target)
    {
        this.initiator = initiator;
        this.target = target;
        this.skill = skill;
        _ = StartCoroutine(IRun());
    }

    private IEnumerator IRun()
    {
        Person targetInAura;
        for (int ID = 0; ID < skill.frequency; ID++)
        {
            // ������� ��� ���������� � ������� �������� ������
            var colliders2D =
                Physics2D.OverlapCircleAll(target.transform.position, skill.radius, LayerMask.GetMask("Person"));
            // ������� �����, ���������� �������
            int countCatch = 0;
            for (int i = 0; i < colliders2D.Length; i++)
            {
                if (!colliders2D[i].GetComponent<Person>())
                    continue;

                targetInAura = colliders2D[i].GetComponent<Person>();
                // ���� � ���� ��� ��������, ��������� � ��������� ����
                if (targetInAura.health <= 0)
                    continue;
                // ������� ���� � ��������� ������� ������
                if (Skill.OnTrigger(skill.triggerTarget, initiator, targetInAura))
                {
                    countCatch++;
                    skill.SetEffectsAndBuffs(initiator, targetInAura);
                }

                // ���� ���������� ���������� ����� �������� ������������� �������� � ��� �������� �� ����� 0, �� ���������� ���� �� ����������
                if (countCatch >= skill.maxCountCatch && skill.maxCountCatch != 0)
                    yield break;
            }

            yield return new WaitForSeconds(skill.gap);
        }
    }
}
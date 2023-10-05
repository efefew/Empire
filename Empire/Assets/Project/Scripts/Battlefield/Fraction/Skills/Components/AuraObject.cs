using System.Collections;

using UnityEngine;

public class AuraObject : MonoBehaviour
{
    private Transform tr;
    private Aura skill;
    private Person initiator, target;
    private void Start() => tr = transform;
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
            // Находим все коллайдеры в радиусе действия умения
            Collider2D[] colliders2D = Physics2D.OverlapCircleAll(target.transform.position, skill.radius, LayerMask.GetMask("Person"));
            // Счетчик целей, пораженных умением
            int countCatch = 0;
            for (int i = 0; i < colliders2D.Length; i++)
            {
                if (!colliders2D[i].GetComponent<Person>())
                    continue;

                targetInAura = colliders2D[i].GetComponent<Person>();
                // Если у цели нет здоровья, переходим к следующей цели
                if (targetInAura.health <= 0)
                    continue;
                // Наносим урон и применяем эффекты умения
                if (Skill.OnTrigger(skill.triggerTarget, initiator, targetInAura))
                {
                    countCatch++;
                    skill.SetEffectsAndBuffs(initiator, targetInAura);
                }
                // Если количество пораженных целей достигло максимального значения и это значение не равно 0, то оставшиеся цели не поражаются
                if (countCatch >= skill.maxCountCatch && skill.maxCountCatch != 0)
                    yield break;
            }

            yield return new WaitForSeconds(skill.gap);
        }
    }
}

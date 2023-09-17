using System.Collections;

using UnityEngine;

[AddComponentMenu("Skill/Area")]
public class Area : Skill
{
    #region Fields

    public Person auraTarget;

    /// <summary>
    /// Промежуток
    /// </summary>
    [Min(0.001f)]
    public float gap;

    /// <summary>
    /// Частота
    /// </summary>
    public uint frequency;

    /// <summary>
    /// Стоять при использовании навыка?
    /// </summary>
    public bool stun;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null) => _ = stun ? initiator.Stun(IRun(initiator, target, target.transform.position)) : StartCoroutine(IRun(initiator, target, target.transform.position));

    public IEnumerator IRun(Person initiator, Person target, Vector3 point)
    {
        for (int ID = 0; ID < frequency; ID++)
        {
            if (!LimitRun(initiator, target))
                yield break;

            if (consumable)
                amountSkill--;
            // Находим все коллайдеры в радиусе действия умения
            Collider2D[] colliders2D = Physics2D.OverlapCircleAll(point, range, LayerMask.GetMask("Person"));
            // Счетчик целей, пораженных умением
            int countCatch = 0;
            for (int i = 0; i < colliders2D.Length; i++)
            {
                if (!colliders2D[i].GetComponent<Person>())
                    continue;
                target = colliders2D[i].GetComponent<Person>();
                // Если у цели нет здоровья, переходим к следующей цели
                if (target.health <= 0)
                    continue;
                // Наносим урон и применяем эффекты умения
                if (OnTrigger(triggerTarget, initiator, target))
                {
                    countCatch++;
                    SetEffectsAndBuffs(initiator, target);
                }
                // Если количество пораженных целей достигло максимального значения и это значение не равно 0, то оставшиеся цели не поражаются
                if (countCatch >= maxCountCatch && maxCountCatch != 0)
                    break;
            }

            yield return new WaitForSeconds(gap);
        }
    }

    #endregion Methods
}
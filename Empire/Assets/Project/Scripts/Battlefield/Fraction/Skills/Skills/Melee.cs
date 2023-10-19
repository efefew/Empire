using System.Collections;

using UnityEngine;

[AddComponentMenu("Skill/Melee")]
public class Melee : Skill
{
    #region Fields

    /// <summary>
    /// Промахи не тратят статы
    /// </summary>
    public bool canMiss;

    #endregion Fields

    #region Methods

    public override void Run(Person initiator, Person target = null)
    {
        //if (target != null && !LimitRun(initiator, target))
        //    return;
        // Находим все коллайдеры в радиусе действия умения
        Collider2D[] colliders2D = Physics2D.OverlapCircleAll(initiator.transform.position, range, LayerMask.GetMask("Person"));

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
            // Если умение может заставить цель двигаться и цель была поражена, персонаж игрока начинает следовать за этой целью на некоторое время
            if (targetMove && countCatch > 0)
            {
                _ = initiator.Pursuit(target, ITargetMove(initiator, timeTargetMove));
            }
            // Если количество пораженных целей достигло максимального значения и это значение не равно 0, то оставшиеся цели не поражаются
            if (countCatch >= maxCountCatch && maxCountCatch != 0)
                break;
        }
        // Если не была поражена ни одна цель и умение может промахнуться, то персонаж игрока возвращается к использованию умения и анимация удаляется. Иначе, персонаж игрока начинает анимацию умения.
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
    public override void Run(Person initiator, Vector3 target) => Debug.LogError("Эта способность не может быть направлена на точку");
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
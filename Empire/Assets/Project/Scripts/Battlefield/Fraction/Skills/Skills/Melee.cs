using UnityEngine;

[AddComponentMenu("Skill/Melee")]
public class Melee : Skill
{
    #region Fields

    /// <summary>
    /// ѕромахи не трат€т статы
    /// </summary>
    public bool canMiss;

    #endregion Fields

    #region Methods

    public override bool LimitRangeRun(Person initiator, Person target = null) => Physics2D.OverlapCircleAll(initiator.transform.position, range, LayerMask.GetMask("Person")).Length > 0;

    public override void Run(Person initiator, Person target = null)
    {
        if (!LimitRun(initiator, target))
            return;
        // Ќаходим все коллайдеры в радиусе действи€ умени€
        Collider2D[] colliders2D = Physics2D.OverlapCircleAll(initiator.transform.position, range, LayerMask.GetMask("Person"));

        // —четчик целей, пораженных умением
        int countCatch = 0;
        for (int i = 0; i < colliders2D.Length; i++)
        {
            if (!colliders2D[i].GetComponent<Person>())
                continue;
            target = colliders2D[i].GetComponent<Person>();
            // ≈сли у цели нет здоровь€, переходим к следующей цели
            if (target.health <= 0)
                continue;
            // Ќаносим урон и примен€ем эффекты умени€
            if (OnTrigger(triggerTarget, initiator, target))
                SetEffectsAndBuffs(initiator, target, ref countCatch);
            // ≈сли умение может заставить цель двигатьс€ и цель была поражена, персонаж игрока начинает следовать за этой целью на некоторое врем€
            if (targetMove && countCatch > 0)
            {

                initiator.Pursuit(target, timeTargetMove);
            }
            // ≈сли количество пораженных целей достигло максимального значени€ и это значение не равно 0, то оставшиес€ цели не поражаютс€
            if (countCatch >= maxCountCatch && maxCountCatch != 0)
                break;
        }
        // ≈сли не была поражена ни одна цель и умение может промахнутьс€, то персонаж игрока возвращаетс€ к использованию умени€ и анимаци€ удал€етс€. »наче, персонаж игрока начинает анимацию умени€.
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
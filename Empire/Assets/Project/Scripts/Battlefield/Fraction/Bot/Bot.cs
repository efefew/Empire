#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Skill;

#endregion

[RequireComponent(typeof(FractionBattlefield))]
public class Bot : MonoBehaviour
{
    private FractionBattlefield myFraction;
    private PointsAb pointsAB;

    private void Awake()
    {
        myFraction = GetComponent<FractionBattlefield>();
        pointsAB = GetComponent<PointsAb>();
    }

    private void Start()
    {
        MoveArmy(myFraction.Armies[2], new Vector2(3, 3), new Vector2(5, 8));
    }

    private IEnumerator IMoveArmy(Army army, Vector2 a, Vector2 b)
    {
        yield return new WaitUntil
        (() => army.Persons.All(person => person.AgentMove.Agent.isOnNavMesh));

        if (!myFraction.Armies.Contains(army))
            yield break;
        army.anchors.ChangePositionA(a);
        army.anchors.ChangePositionB(b);
        army.anchors.ChangedPositions();
    }
    private void UseSkillArmy(Army bot, Army target, Skill skill)
    {
        if(!myFraction.Armies.Contains(bot)) return;
        if(!bot.status.Skills.Contains(skill)) return;
        bot.UseSkill(skill, target.Persons.ToArray());
    }
    private void UseSkillArmy(Army bot, Army target, SkillType skillType)
    {
        if(!myFraction.Armies.Contains(bot))
            return;
        List<Skill> skills = bot.status.Skills.Where(s => s.Type == skillType).ToList();
        Skill skill = skills[Random.Range(0, skills.Count)];
        UseSkillArmy(bot, target, skill);
    }
    private void MoveArmy(Army army, Vector2 a, Vector2 b)
    {
        StartCoroutine(IMoveArmy(army, a, b));
    }
}
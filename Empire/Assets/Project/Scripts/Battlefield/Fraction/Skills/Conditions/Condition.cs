using System;
using System.Collections;

using UnityEngine;

public abstract class Condition : MonoBehaviour
{
    [SerializeField]
    public bool conditionFunction { get; private set; }
    public virtual IEnumerator GetConditionEnumerator() => null;
    public virtual Func<bool> GetConditionFunction() => null;

}

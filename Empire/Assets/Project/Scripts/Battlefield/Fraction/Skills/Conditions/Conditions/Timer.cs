using System.Collections;

using UnityEngine;

[AddComponentMenu("Condition/Timer")]
public class Timer : Condition
{
    #region Fields

    [Min(0)]
    [SerializeField]
    private float time;

    #endregion Fields

    #region Methods

    private IEnumerator IGetCondition()
    {
        yield return new WaitForSeconds(time);
    }

    public override IEnumerator GetCondition() => IGetCondition();

    #endregion Methods
}
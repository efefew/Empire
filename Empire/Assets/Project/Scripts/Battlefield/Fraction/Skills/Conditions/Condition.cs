using System.Collections;

using UnityEngine;

public abstract class Condition : MonoBehaviour
{
    #region Methods

    public abstract IEnumerator GetCondition();

    #endregion Methods
}
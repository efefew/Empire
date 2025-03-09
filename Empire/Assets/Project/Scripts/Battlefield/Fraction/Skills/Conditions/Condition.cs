#region

using System.Collections;
using UnityEngine;

#endregion

public abstract class Condition : MonoBehaviour
{
    #region Methods

    public abstract IEnumerator GetCondition();

    #endregion Methods
}
#region

using UnityEngine;

#endregion

public class World : MonoBehaviour
{
    #region Methods

    [ContextMenu("CreateWorld")]
    public void CreateWorld()
    {
    }

    #endregion Methods

    #region Fields

    [SerializeField] private City[] cities;

    [SerializeField] private Fraction[] fractions;

    #endregion Fields
}
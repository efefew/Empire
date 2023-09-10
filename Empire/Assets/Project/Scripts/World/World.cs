using UnityEngine;

public class World : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private City[] cities;

    [SerializeField]
    private Fraction[] fractions;

    #endregion Fields

    #region Methods

    [ContextMenu("CreateWorld")]
    public void CreateWorld()
    {
    }

    #endregion Methods
}
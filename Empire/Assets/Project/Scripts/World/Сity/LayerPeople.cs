п»їusing UnityEngine;

public class LayerPeople : MonoBehaviour
{
    #region Fields

    public string nameLayerPeople;

    [SerializeField]
    public LayerPeopleDictionary nameLayersPeople = new();

    #endregion Fields
}

public class LayerPeopleDictionary : SerializableDictionary<string, float>
{ }
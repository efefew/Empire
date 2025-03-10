using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "ArmyContent", menuName = "Scriptable Objects/ArmyContent")]
public class ArmyContent : ScriptableObject
{
    [field: SerializeField]
    public Person Person { get; private set; }
    [field: SerializeField]
    public StatusUI ArmyUI{get; private set;}
    [field: SerializeField]
    public StatusUI ArmyGlobalUI{get; private set;}
    [field: SerializeField]
    public Army Army{get; private set;}
    [field: SerializeField]
    public Button ButtonArmy{get; private set;}
}

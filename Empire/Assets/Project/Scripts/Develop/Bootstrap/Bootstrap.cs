#region

using UnityEngine;

#endregion

public class Bootstrap : MonoBehaviour
{
    [Interface(typeof(IBootstrap))] [SerializeField]
    private MonoBehaviour[] _bootstraps;

    private void Start()
    {
        for (int id = 0; id < _bootstraps.Length; id++) (_bootstraps[id] as IBootstrap).StartBootstrap();
    }
}
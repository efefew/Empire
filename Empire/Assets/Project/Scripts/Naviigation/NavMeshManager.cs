#region

using System.Collections;
using NavMeshPlus.Components;
using UnityEngine;

#endregion

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshManager : MonoBehaviour
{
    #region Fields

    [SerializeField] private NavMeshSurface Surface2D;

    #endregion Fields

    #region Methods

    private void Start()
    {
        StartCoroutine(IBuild());
    }

    [ContextMenu("Build")]
    public IEnumerator IBuild()
    {
        yield return new WaitForEndOfFrame();
        _ = Surface2D.BuildNavMeshAsync();
    }

    #endregion Methods

    //void Update()
    //{
    //    Surface2D.UpdateNavMesh(Surface2D.navMeshData);
    //}
}
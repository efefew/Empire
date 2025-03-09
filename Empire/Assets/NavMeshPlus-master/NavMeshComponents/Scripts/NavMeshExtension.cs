using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshPlus.Extensions
{
    public abstract class NavMeshExtension : MonoBehaviour
    {
        private NavMeshSurface m_navMeshOwner;
        public int Order { get; protected set; }

        public NavMeshSurface NavMeshSurfaceOwner
        {
            get
            {
                if (m_navMeshOwner == null)
                    m_navMeshOwner = GetComponent<NavMeshSurface>();
                return m_navMeshOwner;
            }
        }

        protected virtual void Awake()
        {
            ConnectToVcam(true);
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDestroy()
        {
            ConnectToVcam(false);
        }

        public virtual void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources,
            NavMeshBuilderState navNeshState)
        {
        }

        public virtual void CalculateWorldBounds(NavMeshSurface surface, List<NavMeshBuildSource> sources,
            NavMeshBuilderState navNeshState)
        {
        }

        public virtual void PostCollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources,
            NavMeshBuilderState navNeshState)
        {
        }
#if UNITY_EDITOR
        [DidReloadScripts]
        private static void OnScriptReload()
        {
            var extensions = Resources.FindObjectsOfTypeAll(
                typeof(NavMeshExtension)) as NavMeshExtension[];
            foreach (NavMeshExtension e in extensions)
                e.ConnectToVcam(true);
        }
#endif
        protected virtual void ConnectToVcam(bool connect)
        {
            if (connect && NavMeshSurfaceOwner == null)
                Debug.LogError("NevMeshExtension requires a NavMeshSurface component");
            if (NavMeshSurfaceOwner != null)
            {
                if (connect)
                    NavMeshSurfaceOwner.NevMeshExtensions.Add(this, Order);
                else
                    NavMeshSurfaceOwner.NevMeshExtensions.Remove(this);
            }
        }
    }
}
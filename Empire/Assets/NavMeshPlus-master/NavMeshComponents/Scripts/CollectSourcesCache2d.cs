using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshPlus.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/NavMesh CacheSources2d", 30)]
    public class CollectSourcesCache2d : NavMeshExtension
    {
        private Dictionary<Object, NavMeshBuildSource> _lookup;
        private Bounds _sourcesBounds;

        private NavMeshBuilder2dState _state;
        public bool IsDirty { get; protected set; }

        public int SourcesCount => Cache.Count;
        public int CahcheCount => _lookup.Count;

        public List<NavMeshBuildSource> Cache { get; private set; }

        protected override void Awake()
        {
            _lookup = new Dictionary<Object, NavMeshBuildSource>();
            Cache = new List<NavMeshBuildSource>();
            IsDirty = false;
            Order = -1000;
            _sourcesBounds = new Bounds();
            base.Awake();
        }

        protected override void OnDestroy()
        {
            _state?.Dispose();
            base.OnDestroy();
        }

        public bool AddSource(GameObject gameObject, NavMeshBuildSource source)
        {
            bool res = _lookup.ContainsKey(gameObject);
            if (res) return UpdateSource(gameObject);
            Cache.Add(source);
            _lookup.Add(gameObject, source);
            IsDirty = true;
            return true;
        }

        public bool UpdateSource(GameObject gameObject)
        {
            bool res = _lookup.ContainsKey(gameObject);
            if (res)
            {
                IsDirty = true;
                NavMeshBuildSource source = _lookup[gameObject];
                int idx = Cache.IndexOf(source);
                if (idx >= 0)
                {
                    source.transform = Matrix4x4.TRS(gameObject.transform.position, gameObject.transform.rotation,
                        gameObject.transform.lossyScale);
                    Cache[idx] = source;
                    _lookup[gameObject] = source;
                }
            }

            return res;
        }

        public bool RemoveSource(GameObject gameObject)
        {
            bool res = _lookup.ContainsKey(gameObject);
            if (res)
            {
                IsDirty = true;
                NavMeshBuildSource source = _lookup[gameObject];
                _lookup.Remove(gameObject);
                Cache.Remove(source);
            }

            return res;
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            IsDirty = false;
            return NavMeshBuilder.UpdateNavMeshDataAsync(data, NavMeshSurfaceOwner.GetBuildSettings(), Cache,
                _sourcesBounds);
        }

        public AsyncOperation UpdateNavMesh()
        {
            return UpdateNavMesh(NavMeshSurfaceOwner.navMeshData);
        }

        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources,
            NavMeshBuilderState navMeshState)
        {
            _lookup.Clear();
            IsDirty = false;
            _state?.Dispose();
            _state = navMeshState.GetExtraState<NavMeshBuilder2dState>(false);
            _state.lookupCallback = LookupCallback;
        }

        private void LookupCallback(Object component, NavMeshBuildSource source)
        {
            if (component == null) return;
            _lookup.Add(component, source);
        }

        public override void PostCollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources,
            NavMeshBuilderState navNeshState)
        {
            _sourcesBounds = navNeshState.worldBounds;
            Cache = sources;
        }
    }
}
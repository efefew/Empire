﻿using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

namespace NavMeshPlus.Extensions
{
    [ExecuteAlways]
    [AddComponentMenu("Navigation/NavMesh RootSources2d", 30)]
    public class RootSources2d : NavMeshExtension
    {
        [SerializeField] private List<GameObject> _rootSources;

        public List<GameObject> RooySources
        {
            get => _rootSources;
            set => _rootSources = value;
        }

        protected override void Awake()
        {
            Order = -1000;
            base.Awake();
        }

        public override void CollectSources(NavMeshSurface surface, List<NavMeshBuildSource> sources,
            NavMeshBuilderState navNeshState)
        {
            navNeshState.roots = _rootSources;
        }
    }
}
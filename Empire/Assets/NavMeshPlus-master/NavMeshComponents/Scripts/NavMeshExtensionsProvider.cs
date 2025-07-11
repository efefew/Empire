﻿using System.Collections.Generic;

namespace NavMeshPlus.Extensions
{
    public interface INavMeshExtensionsProvider
    {
        int Count { get; }
        NavMeshExtension this[int index] { get; }
        void Add(NavMeshExtension extension, int order);
        void Remove(NavMeshExtension extension);
    }

    internal class NavMeshExtensionMeta
    {
        public NavMeshExtension extension;
        public int order;

        public NavMeshExtensionMeta(int order, NavMeshExtension extension)
        {
            this.order = order;
            this.extension = extension;
        }
    }

    internal class NavMeshExtensionsProvider : INavMeshExtensionsProvider
    {
        private static Comparer<NavMeshExtensionMeta> Comparer =
            Comparer<NavMeshExtensionMeta>.Create((x, y) => x.order > y.order ? 1 : x.order < y.order ? -1 : 0);

        private List<NavMeshExtensionMeta> _extensions = new();
        public NavMeshExtension this[int index] => _extensions[index].extension;

        public int Count => _extensions.Count;

        public void Add(NavMeshExtension extension, int order)
        {
            NavMeshExtensionMeta meta = new(order, extension);
            int at = _extensions.BinarySearch(meta, Comparer);
            if (at < 0)
            {
                _extensions.Add(meta);
                _extensions.Sort(Comparer);
            }
            else
            {
                _extensions.Insert(at, meta);
            }
        }

        public void Remove(NavMeshExtension extension)
        {
            _extensions.RemoveAll(x => x.extension = extension);
        }
    }
}
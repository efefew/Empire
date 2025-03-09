using System;
using System.Collections.Generic;
using UnityEngine;

namespace NavMeshPlus.Extensions
{
    public class NavMeshBuilderState : IDisposable
    {
        private bool _disposed;
        private CompositeDisposable disposable;
        private Dictionary<Type, object> mExtraState;
        public IEnumerable<GameObject> roots;
        public Bounds worldBounds;
        public Matrix4x4 worldToLocal;

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public T GetExtraState<T>(bool dispose = true) where T : class, new()
        {
            if (mExtraState == null)
            {
                mExtraState = new Dictionary<Type, object>();
                disposable = new CompositeDisposable();
            }

            if (!mExtraState.TryGetValue(typeof(T), out object extra))
            {
                extra = mExtraState[typeof(T)] = new T();
                if (dispose) disposable.Add(extra);
            }

            return extra as T;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
                // TODO: dispose managed state (managed objects).
                disposable?.Dispose();

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }

    internal class CompositeDisposable : IDisposable
    {
        private bool _disposed;
        private List<IDisposable> extraStates = new();

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public void Add(IDisposable dispose)
        {
            extraStates.Add(dispose);
        }

        public void Add(object dispose)
        {
            if (dispose is IDisposable) extraStates.Add((IDisposable)dispose);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                foreach (IDisposable item in extraStates) item?.Dispose();
                extraStates.Clear();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }
}
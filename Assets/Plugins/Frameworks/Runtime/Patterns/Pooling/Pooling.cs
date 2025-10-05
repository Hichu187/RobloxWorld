using System.Collections.Generic;
using UnityEngine;

namespace Hichu
{
    public abstract class Pooling<TItem, TPool>
        where TPool : Pooling<TItem, TPool>, new()
    {
        private static TPool s_instance;
        public static TPool instance => s_instance ?? (s_instance = new TPool());
        public static void SetInstance(TPool inst) => s_instance = inst;

        protected readonly Queue<TItem> _pool = new();
        protected int _created;
        public int CountInactive => _pool.Count;
        public int CountCreated => _created;

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = Create();
                Release(item);
            }
        }

        public TItem Get()
        {
            if (_pool.Count > 0)
            {
                var item = _pool.Dequeue();
                OnGet(item);
                return item;
            }

            var created = Create();
            OnGet(created);
            return created;
        }

        public void Release(TItem item)
        {
            if (item == null) return;
            OnRelease(item);
            _pool.Enqueue(item);
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var it = _pool.Dequeue();
                DestroyItem(it);
            }
            _created = 0;
            OnClear();
        }

        protected virtual TItem Create()
        {
            var item = CreateNew();
            _created++;
            return item;
        }

        protected abstract TItem CreateNew();
        protected abstract void OnGet(TItem item);
        protected abstract void OnRelease(TItem item);
        protected virtual void DestroyItem(TItem item) { }
        protected virtual void OnClear() { }
    }

    public abstract class MonoPool<TComp, TPool> : Pooling<TComp, TPool>
        where TComp : Component
        where TPool : MonoPool<TComp, TPool>, new()
    {
        public TComp prefab;
        public Transform parent;

        public void Configure(TComp prefab, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
        }

        protected override TComp CreateNew()
        {
            if (prefab == null)
                throw new System.InvalidOperationException($"[{GetType().Name}] Prefab chưa được cấu hình.");

            var inst = Object.Instantiate(prefab, parent);
            inst.gameObject.SetActive(false);
            return inst;
        }

        protected override void OnGet(TComp item)
        {
            if (item) item.gameObject.SetActive(true);
        }

        protected override void OnRelease(TComp item)
        {
            if (item) item.gameObject.SetActive(false);
        }

        protected override void DestroyItem(TComp item)
        {
            if (item) Object.Destroy(item.gameObject);
        }
    }
}

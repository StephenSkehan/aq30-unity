using System.Collections.Generic;
using UnityEngine;

namespace AQ.App
{
    public class SimplePool<T> where T : Component
    {
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly T _prefab;
        private readonly Transform _parent;

        public SimplePool(T prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        public T Get()
        {
            if (_stack.Count > 0)
            {
                var t = _stack.Pop();
                t.gameObject.SetActive(true);
                return t;
            }
            return Object.Instantiate(_prefab, _parent);
        }

        public void Release(T t)
        {
            if (t == null) return;
            t.gameObject.SetActive(false);
            t.transform.SetParent(_parent, false);
            _stack.Push(t);
        }
    }
}
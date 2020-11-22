using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SensorToolkit
{
    public class ObjectCache<T>
    {
        Stack<T> cache;

        public ObjectCache() : this(10) { }
        public ObjectCache(int startSize)
        {
            cache = new Stack<T>();
            for (int i = 0; i < startSize; i++) { cache.Push(create()); }
        }

        public T Get()
        {
            if (cache.Count > 0) return cache.Pop();
            else return create();
        }

        public virtual void Dispose(T obj)
        {
            cache.Push(obj);
        }

        protected virtual T create()
        {
            return System.Activator.CreateInstance<T>();
        }
    }

    public class ListCache<T> : ObjectCache<List<T>>
    {
        public override void Dispose(List<T> obj)
        {
            obj.Clear();
            base.Dispose(obj);
        }
    }
}

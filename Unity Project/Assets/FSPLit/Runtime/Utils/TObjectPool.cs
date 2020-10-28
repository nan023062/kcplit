using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nave.FSPLit
{
    /// <summary>
    /// 数据对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TObjectPool<T> where T : new()
    {
        private Queue<T> _pool = new Queue<T>();

        public void ResetPool()
        {
            _pool.Clear();
        }

        public T Spawn()
        {
            if (_pool.Count > 0)
            {
                return _pool.Dequeue();
            }
            else
            {
                return new T();
            }
        }

        public void Unspawn(T obj)
        {
            _pool.Enqueue(obj);
        }
    }
}

using System;
using System.Collections.Generic;

namespace ROC.Core.Pool
{
    public class ObjectPool<T> where T : class
    {
        private readonly Func<T> _createFunc;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnDestroy;
        private readonly int _maxSize;
        
        private readonly Stack<T> _pool;
        
        public ObjectPool(
            Func<T> createFunc, 
            Action<T> actionOnGet = null, 
            Action<T> actionOnRelease = null, 
            Action<T> actionOnDestroy = null, 
            int defaultCapacity = 10,
            int maxSize = 10000)
        {
            _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDestroy = actionOnDestroy;
            _maxSize = maxSize;
            
            _pool = new Stack<T>(defaultCapacity);
        }
        
        public T Get()
        {
            T item;
            
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                item = _createFunc();
            }
            
            _actionOnGet?.Invoke(item);
            return item;
        }
        
        public void Release(T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
                
            _actionOnRelease?.Invoke(item);
            
            if (_pool.Count < _maxSize)
            {
                _pool.Push(item);
            }
            else
            {
                _actionOnDestroy?.Invoke(item);
            }
        }
        
        public void Clear()
        {
            if (_actionOnDestroy != null)
            {
                foreach (T item in _pool)
                {
                    _actionOnDestroy(item);
                }
            }
            
            _pool.Clear();
        }
    }
} 
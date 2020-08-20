using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheGameNet.Utils
{
    public class ObjectPool<T> where T : class //new()
    {

        private readonly int CONST_MaxElementForRecycle;

        private T[] _objects = new T[0];
        private int _objIndex = 0;

        private T _oneObject = null;

        

        private long _countNews = 0;
        private long _countRecycled = 0;
        private long _countRecycledWasted = 0;

        private Func<T> _createInstance;
        public ObjectPool( Func<T> createInstance)
            : this(10, createInstance)
        {

        }

        public ObjectPool(int maxElementsForRecycle, Func<T> createInstance)
        {
            CONST_MaxElementForRecycle = maxElementsForRecycle;
            _objects = new T[CONST_MaxElementForRecycle];
            
            _createInstance = createInstance;
        }

        public string GetBaseStat()
        {
            return $"new:{_countNews} recycle:{_countRecycled} recy waste:{_countRecycledWasted}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetNewOrRecycle()
        {
            //return Instance.Invoke();
            if (_oneObject == null)
            {
                if (_objIndex == 0)
                {
                    _countNews++;
                    return _createInstance.Invoke();// Instance.Invoke();
                }
                else
                {
                    
                    var retObject = _objects[_objIndex-1];
                    _objects[_objIndex-1] = null;
                    _objIndex--;

                    if (_objIndex > 0)
                    {
                        _objIndex--;
                        _oneObject = _objects[_objIndex];
                        _objects[_objIndex] = null;
                    }

                    return retObject;
                }
            }
            else
            {
                T inst = _oneObject;
                _oneObject = null;
                return inst;
            }

            
            
        }

        public void PutForRecycle(T[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                PutForRecycle(objects[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutForRecycle(T pobject)
        {
            if (_oneObject == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                _oneObject = pobject;
                _countRecycled++;
            }
            else
            {
                if (_objIndex < this.CONST_MaxElementForRecycle)
                {
                    _countRecycled++;
                    _objects[_objIndex] = pobject;
                    _objIndex++;
                }
                else
                {
                    _countRecycledWasted++;
                }
            }

        }

        public void Clear()
        {
                _objIndex = 0;
                _countRecycledWasted = 0;
                _countRecycled = 0;
                _countNews = 0;
        }


    }

    
}

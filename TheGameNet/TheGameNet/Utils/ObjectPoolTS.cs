using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Utils
{
    public class ObjectPoolTS<T> where T : new()
    {

        private readonly int CONST_MaxElementForRecycle;

        private T[] _objects = new T[0];
        private int _objIndex = 0;

        FastLock _fastLock = new FastLock();

        private long _countNews = 0;
        private long _countRecycled = 0;
        private long _countRecycledWasted = 0;

        public ObjectPoolTS()
            : this(10)
        {

        }

        public ObjectPoolTS(int maxElementsForRecycle)
        {
            CONST_MaxElementForRecycle = maxElementsForRecycle;
            _objects = new T[CONST_MaxElementForRecycle];
        }

        public string GetBaseStat()
        {
            return $"new:{_countNews} recycle:{_countRecycled} recy waste:{_countRecycledWasted}";
        }

        public T GetNewOrRecycle()
        {
            {
                using (_fastLock.Lock())
                {
                    if (_objIndex > 0)
                    {
                        _objIndex--;
                        return _objects[_objIndex];
                    }
                }

            }

            _countNews++;
            return Instance.Invoke();// new T();
        }

        public static readonly Func<T> Instance =
     Expression.Lambda<Func<T>>(Expression.New(typeof(T))).Compile();

        public void PutForRecycle(T[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                PutForRecycle(objects[i]);
            }
        }

        public void PutForRecycle(T pobject)
        {
            using (_fastLock.Lock())
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
            using (_fastLock.Lock())
            {
                _objIndex = 0;
                _countRecycledWasted = 0;
                _countRecycled = 0;
                _countNews = 0;
            }
        }


    }
}

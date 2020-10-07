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
    public class ObjectPool_LocalThread<T> where T : class //new()
    {
        private ThreadLocal<InternalPool> _internalPool;

        public ObjectPool_LocalThread()
            : this(10)
        {

        }

        public ObjectPool_LocalThread(int maxElementsForRecycle)
        {
            _internalPool = new ThreadLocal<InternalPool>(() => { return new InternalPool(maxElementsForRecycle); });
        }

        public string GetBaseStat()
        {
            var ip = _internalPool.Value;
            return $"new:{ip.CountNews} recycle:{ip.CountRecycled} recy waste:{ip.CountRecycledWasted}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetNewOrRecycle()
        {
            var ip = _internalPool.Value;

            T inst = ip.OneObject;
            if (inst == null )
            {
                
                    if (ip.ObjIndex > 0)
                    {
                    ip.ObjIndex--;
                        var retObject = ip.Objects[ip.ObjIndex];
                    ip.Objects[ip.ObjIndex] = null;

                        if (ip.ObjIndex > 0 && ip.OneObject == null)
                        {
                        ip.ObjIndex--;
                        ip.OneObject = ip.Objects[ip.ObjIndex];
                        ip.Objects[ip.ObjIndex] = null;
                        }

                        return retObject;
                    }


                ip.CountNews++;
                return Instance.Invoke();
            }

            ip.OneObject = null;
            return inst;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PutForRecycle(T pobject)
        {
            var ip = _internalPool.Value;

            if (ip.OneObject == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                ip.OneObject = pobject;
                
            ip.CountRecycled++;
            }
            else
            {
                //using (_fastLock.Lock())

                if (ip.ObjIndex < ip.CONST_MaxElemForRecycle)
                {
                    ip.CountRecycled++;
                    ip.Objects[ip.ObjIndex] = pobject;
                    ip.ObjIndex++;
                }
                else
                {
                    ip.CountRecycledWasted++;
                }
            }

        }

        //public void Clear()
        //{
        //    using (_fastLock.Lock())
        //    {
        //        _objIndex = 0;
        //        _countRecycledWasted = 0;
        //        _countRecycled = 0;
        //        _countNews = 0;
        //    }
        //}

        private class InternalPool
        {
            public readonly int CONST_MaxElemForRecycle;
            public T[] Objects = new T[0];
            public int ObjIndex = 0;

            public T OneObject = null;



            public long CountNews = 0;
            public long CountRecycled = 0;
            public long CountRecycledWasted = 0;

            public InternalPool()
            {

            }

            public InternalPool(int maxElemForRecycle)
            {
                CONST_MaxElemForRecycle = maxElemForRecycle;
                Objects = new T[maxElemForRecycle];
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace BonesLib.Utils
{
    public class FixList<T>
    {
        private T [] Data;
        public int Length;

        public int MaxSize => Data.Length;

        public FixList(int size)
            :this(new T[size])
        {
            
        }

        public FixList(T [] data)
        {
            Data = data;
            Length = 0;
        }

        public void Init(T [] data)
        {
            Data = data;
            Length = 0;
        }

        public void Clear()
        {
            Length = 0;
        }

        public ref T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { 
                    if (i >= Length)
                    throw new Exception("not alllowed");
                    return ref Data[i]; }
            //set { Data[i] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int i)
        {
            if (i >= Length)
                throw new Exception("not alllowed");
            return ref Data[i];

        }

        public Span<T> GetSpan() => Data.AsSpan(0, Length);

        public bool IsFull => Length == Data.Length;

        public void CopyTo(FixList<T> data)
        {
            if (data.MaxSize < this.Length)
                throw new Exception("not allow");

            data.Length = this.Length;

            Array.Copy(this.Data,data.Data,this.Length);
        }

        public void Add(T data)
        {
            if (Data.Length == Length)
            {
                throw new IndexOutOfRangeException();
            }

            Data[Length] = data;
            Length++;
        }

        public T Pop()
        {
            T result = Data[Length - 1];
            RemoveLast();
            return result;
        }

        public void RemoveLast()
        {
            if (Length == 0) return;

            Length--;
        }

        public void Remove(int index)
        {
            if (index >= Length)
            {
                throw new IndexOutOfRangeException();
            }

            for (int i = index; i < Length - 1; i++)
            {
                Data[i] = Data[i + 1];
            }

            Length--;
        }

    }
}

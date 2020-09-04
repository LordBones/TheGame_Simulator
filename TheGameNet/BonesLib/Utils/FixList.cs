using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.Utils
{
    public class FixList<T>
    {
        private T [] Data;
        public int Length;


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

        public ref T this[int i]
        {
            get { return ref Data[i]; }
            //set { Data[i] = value; }
        }

        public Span<T> GetSpan() => Data.AsSpan(0, Length);

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

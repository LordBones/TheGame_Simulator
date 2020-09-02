using System;
using System.Collections.Generic;
using System.Text;

namespace BonesLib.Utils
{
    public ref struct FixListSpan<T>
    {
        private Span<T> Data;
        public int Length;

        public FixListSpan(Span<T> data)
        {
            Data = data;
            Length = 0;
        }

        public void Init(Span<T> data)
        {
            Data = data;
            Length = 0;
        }

        public ref T this[int i]
        {
            get { return ref Data[i]; }
            //set { Data[i] = value; }
        }

        public Span<T> GetSpan() => Data.Slice(0, Length);

        public void Add(T data)
        {
            if(Data.Length == Length)
            {
                throw new IndexOutOfRangeException();
            }

            Data[Length] = data;
            Length++;
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

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return ref Data[i]; }
            //set { Data[i] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get(int i)
        {
            if (i >= Length)
                throw new Exception("not alllowed");
            return ref Data[i];

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

        public int IndexOf(T data)
        {
            for(int i =0; i < Length; i++)
            {
                if (Data[i].Equals(data))
                    return i;
            }

            return -1;
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

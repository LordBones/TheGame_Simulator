using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TheGameNet.Utils
{
    
    class ChunkArrayCreator
    {
        static ChunkArrayCreator _default = new ChunkArrayCreator();


        private byte[] _array;
        private ushort _startIndex;
        readonly int _maxBufferSize;
        

        /// <summary>
        /// it is good to have value under 85000 for better gcc
        /// </summary>
        /// <param name="maxBufferSize"></param>
        public ChunkArrayCreator(int maxBufferSize = ushort.MaxValue-1)
        {
            _maxBufferSize = maxBufferSize;
            _array = new byte[_maxBufferSize];
        }


        public static ChunkArrayCreator Default => _default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegmentExSmall_Struct<byte> GetChunk(ushort sizeChunk)
        {
            if (_startIndex + sizeChunk > _maxBufferSize)
            {
                AllocNewBuffer();
            }

            var result = new ArraySegmentExSmall_Struct<byte>(_array, _startIndex, sizeChunk);

            _startIndex += sizeChunk;
            return result;
        }

        private void AllocNewBuffer()
        {
            _array = new byte[_maxBufferSize];
            _startIndex = 0;
        }
    }
}

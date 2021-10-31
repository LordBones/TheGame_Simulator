using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Utils;

namespace TheGameNet.Core.QLearning
{




    class QTable
    {
        const float CONST_EmptyElement = float.NegativeInfinity;
        const int maxActionSize = 1024;
        private float[][] _HashQTable;
        private int _hashActionSize;
        private float _defaultValue;

        public QTable(int hashTableSize, int maxActionLength, float defaultValue)
        {
            _hashActionSize = maxActionLength;
            _HashQTable = new float[hashTableSize][];

            for (int i = 0; i < _HashQTable.Length; i++)
            {
                _HashQTable[i] = new float[_hashActionSize];

                for (int k = 0; k < _hashActionSize; k++)
                    _HashQTable[i][k] = CONST_EmptyElement;
            }

            _defaultValue = defaultValue;
        }


        public int CreateKey_IndexState(ArraySegmentEx_Struct<byte> state)
        {
            //  _hash.Compute()
            //  xxHashSharp.xxHash.CalculateHash(state,)


            //ulong resLong = Standart.Hash.xxHash.xxHash64.ComputeHash(state.Array.AsSpan(state.Offset, state.Count), state.Count);

            //byte [] result = _shaProvider.ComputeHash(state.Array, state.Offset, state.Count);

            //uint tmp = (uint)((result[0] << 24) | (result[1] << 16) | (result[2] << 8) | (result[3]));
            //uint tmp = (uint)resLong;

            uint tmp = (uint)HashDepot.XXHash.Hash64(state.Array.AsSpan(state.Offset, state.Count));
            //uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            tmp &= 0x7fffffff;

            return (int)tmp % _HashQTable.Length;
        }

        public int CreateKey_IndexState(ReadOnlySpan<byte> state)
        {
            //  _hash.Compute()
            //  xxHashSharp.xxHash.CalculateHash(state,)


            //ulong resLong = Standart.Hash.xxHash.xxHash64.ComputeHash(state.Array.AsSpan(state.Offset, state.Count), state.Count);

            //byte [] result = _shaProvider.ComputeHash(state.Array, state.Offset, state.Count);

            //uint tmp = (uint)((result[0] << 24) | (result[1] << 16) | (result[2] << 8) | (result[3]));
            //uint tmp = (uint)resLong;

            //uint tmp = (uint)HashDepot.XXHash.Hash64(state);
            //uint tmp = (uint)FastHash(state);
            uint tmp = HashDepot.XXHash.Hash32(state);
            // tmp &= 0x7fffffff;

            return (int)(tmp % _HashQTable.Length);
        }

        public int CreateKey_IndexAction(Span<byte> action)
        {


            uint tmp = (uint)HashDepot.XXHash.Hash64(action);

            //uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            tmp &= 0x7fffffff;

            return (int)tmp % _hashActionSize;
        }

        public int CreateKey_IndexAction(ArraySegmentEx_Struct<byte> action)
        {


            uint tmp = (uint)HashDepot.XXHash.Hash64(action.Array.AsSpan(action.Offset, action.Count));
            //uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            tmp &= 0x7fffffff;

            return (int)tmp % _hashActionSize;
        }



        public bool HasValue(int indexState, int actionState)
        {
            return _HashQTable[indexState][actionState] != CONST_EmptyElement;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public (float val, bool empty) Get_detectIfExist(int indexState, int actionState)
        {
            var dicActions = _HashQTable[indexState];


            if (dicActions[actionState] != CONST_EmptyElement)
            {
                return (dicActions[actionState], true);
            }

            return (_defaultValue, false);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]

        public float Get(int indexState, int actionState)
        {
            var dicActions = _HashQTable[indexState];

            if (dicActions[actionState] == CONST_EmptyElement)
            {
                return _defaultValue;
            }

            return dicActions[actionState];
        }

        public (float val, int state) Get_Highest(int indexState, ReadOnlySpan<int> actionStates)
        {
            float biggestVal = float.MinValue;
            int state = -1;

            var dicActions = _HashQTable[indexState];

            for (int i = 0; i < actionStates.Length; i++)
            {
                float val = dicActions[actionStates[i]];
                if (val != CONST_EmptyElement)
                {
                    if (biggestVal < val)
                    {
                        biggestVal = val;
                        state = actionStates[i];
                    }
                }
                //else
                //{
                //    state = actionStates[i];
                //    biggestVal = float.MaxValue;
                //}
            }

            return (biggestVal, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]

        public void Set(int indexState, int actionState, float value)
        {
            var actions = _HashQTable[indexState];
            actions[actionState] = value;
        }

        public void PrintTable(TextWriter tw)
        {
            var forHeader = GetAllRows().OrderBy(x => x).ToArray();

            int skip = 0;
            int take = 10;

            do
            {
                if (skip + take > forHeader.Length)
                {
                    take = forHeader.Length - skip;
                }

                Span<int> header = forHeader.AsSpan(skip, take);
                skip += take;


                PrintTableHeader(tw, header);
                PrintTableData(tw, header);

                tw.WriteLine();
            } while (skip < forHeader.Length);
        }



        private void PrintTableHeader(TextWriter tw, Span<int> rows)
        {
            tw.Write($"{"",10};");

            foreach (var row in rows)
            {
                tw.Write($"{row.ToString("X"),10};");
            }

            tw.Write("\n");
        }

        

        private void PrintTableData(TextWriter tw, Span<int> rows)
        {
            StringBuilder sb = new StringBuilder(8096);
            const int CONST_Flush = 8024;

            for (int i = 0; i < _HashQTable.Length; i++)
            {
                if (sb.Length > CONST_Flush)
                {
                    foreach (var item in sb.GetChunks())
                    {
                        tw.Write(item.Span);
                    }

                    sb.Clear();
                }

                sb.AppendFormat("{0,10};", i);


                var data = _HashQTable[i];
                foreach (var col in rows)
                {
                    if (data[col] != CONST_EmptyElement)
                    {
                        sb.AppendFormat("{0,10:0.###};", data[col]);
                    }
                    else
                    {
                        sb.Append($"{"",10};");
                    }

                    if (sb.Length > CONST_Flush)
                    {

                        foreach (var item in sb.GetChunks())
                        {
                            tw.Write(item.Span);
                        }

                        sb.Clear();
                    }
                }

                sb.AppendLine();

            }

            foreach (var item in sb.GetChunks())
            {
                tw.Write(item.Span);
            }
            //sb.Clear();
        }


        /* private void PrintTableData2(TextWriter tw, Span<int> rows)
         {

             for (int i = 0; i < _HashQTable.Length; i++)
             {
                 if (sb.Length > 2048)
                 {
                     tw.Write(sb.ToString());
                     sb.Clear();
                 }

                 sb.Append($"{i,10};");

                 var data = _HashQTable[i];
                 foreach (var col in rows)
                 {
                     if (data[col] != CONST_EmptyElement)
                     {

                         sb.AppendFormat("{0,10};", data[col].ToString("0.###"));
                     }
                     else
                     {
                         sb.Append($"{"",10};");
                     }
                 }

                 sb.AppendLine();

             }

             tw.Write(sb.ToString());
         }
        */
        private int[] GetAllRows()
        {
            HashSet<int> result = new HashSet<int>(400);
            foreach (var row in _HashQTable)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if (row[i] == CONST_EmptyElement)
                        continue;

                    result.Add(i);
                }

            }

            return result.ToArray();
        }


    }
}

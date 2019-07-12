using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Utils;

namespace TheGameNet.Core.QLearning
{
    class QTable
    {
        private Dictionary<int, float>[] _HashQTable;
        private int _hashActionSize;

        public QTable(int hashTableSize, int hashActionSize)
        {
            _hashActionSize = hashActionSize;
            _HashQTable = new Dictionary<int,float>[hashTableSize];

            for(int i = 0; i < _HashQTable.Length; i++)
            {
                _HashQTable[i] = new Dictionary<int, float>();
            }
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

            return (int) tmp % _HashQTable.Length;
        }

        public int CreateKey_IndexAction(ArraySegmentEx_Struct<byte> action)
        {
            

            uint tmp = (uint)HashDepot.XXHash.Hash64(action.Array.AsSpan(action.Offset, action.Count));
            //uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            tmp &= 0x7fffffff;

            return (int)tmp % _hashActionSize;
        }


        public float Get(int indexState, int actionState)
        {
            var dicActions = _HashQTable[indexState];

            if(dicActions.TryGetValue(actionState, out float val))
            {
                return val;
            }

            return 0.0f;
        }

        public void Set(int indexState, int actionState, float value)
        {
            var dicActions = _HashQTable[indexState];
            if (dicActions.ContainsKey(actionState))
            {
                dicActions[actionState] = value;
                
            }
            else
            {
                dicActions.Add(actionState, value);
            }
        }

        public void PrintTable(TextWriter tw)
        {
            var header = GetAllRows();
            PrintTableHeader(tw, header);
            PrintTableData(tw, header);
        }



        private void PrintTableHeader(TextWriter tw, HashSet<int> rows)
        {
            tw.Write($"{"",10};");

            foreach(var row in rows)
            {
                tw.Write($"{row.ToString("x"), 8};");
            }

            tw.WriteLine();
        }

        private  void PrintTableData(TextWriter tw, HashSet<int> rows)
        {
            HashSet<int> result = new HashSet<int>();
            for(int i = 0;i < _HashQTable.Length;i++ )
            {
                tw.Write($"{i.ToString(""),10};");

                var data = _HashQTable[i];
                foreach (var col in rows)
                {
                    if (data.ContainsKey(col))
                    {
                        tw.Write($"{data[col].ToString("0.###"),8};");
                    }
                    else
                    {
                        tw.Write($"{"",8};");
                    }
                }
                tw.WriteLine();
            }

            
        }

        private HashSet<int> GetAllRows()
        {
            HashSet<int> result = new HashSet<int>();
            foreach (var row in _HashQTable)
            {
                foreach(var col in row)
                {
                    result.Add(col.Key);
                }
            }

            return result;
        }
    }
}

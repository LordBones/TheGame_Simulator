using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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
        private float _defaultValue;

        public QTable(int hashTableSize, int hashActionSize, float defaultValue)
        {
            _hashActionSize = hashActionSize;
            _HashQTable = new Dictionary<int,float>[hashTableSize];

            for(int i = 0; i < _HashQTable.Length; i++)
            {
                _HashQTable[i] = new Dictionary<int, float>();
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

            return (int) tmp % _HashQTable.Length;
        }

        public int CreateKey_IndexState(Span<byte> state)
        {
            //  _hash.Compute()
            //  xxHashSharp.xxHash.CalculateHash(state,)


            //ulong resLong = Standart.Hash.xxHash.xxHash64.ComputeHash(state.Array.AsSpan(state.Offset, state.Count), state.Count);

            //byte [] result = _shaProvider.ComputeHash(state.Array, state.Offset, state.Count);

            //uint tmp = (uint)((result[0] << 24) | (result[1] << 16) | (result[2] << 8) | (result[3]));
            //uint tmp = (uint)resLong;

            uint tmp = (uint)HashDepot.XXHash.Hash64(state);
            //uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            tmp &= 0x7fffffff;

            return (int)tmp % _HashQTable.Length;
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
            return _HashQTable[indexState].Keys.Contains(actionState);
        }

        public (float val, bool empty) Get_detectIfExist(int indexState, int actionState)
        {
            var dicActions = _HashQTable[indexState];

            if(dicActions.TryGetValue(actionState, out float val))
            {
                return (val,true);
            }

            return (_defaultValue, false);
        }

        public float Get(int indexState, int actionState)
        {
            var dicActions = _HashQTable[indexState];

            if (dicActions.TryGetValue(actionState, out float val))
            {
                return val ;
            }

            return _defaultValue;
        }

        public (float val, int state) Get_Highest(int indexState, Span<int> actionStates)
        {
            float biggestVal = float.MinValue;
            int state = -1;

            var dicActions = _HashQTable[indexState];

            for(int i = 0;i < actionStates.Length; i++)
            {

                if (dicActions.TryGetValue(actionStates[i], out float val))
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

            foreach(var row in rows)
            {
                tw.Write($"{row.ToString("X"), 10};");
            }

            tw.Write("\n");
        }

        StringBuilder sb = new StringBuilder(4096);
        char[] buffer = new char[1024];
        private  void PrintTableData(TextWriter tw, Span<int> rows)
        {
            

            for(int i = 0;i < _HashQTable.Length;i++ )
            {
                if(sb.Length > 512)
                {
                    sb.CopyTo(0, buffer, 0, sb.Length);
                    tw.Write(buffer, 0, sb.Length);
                    sb.Clear();
                }
                
                sb.Append($"{i.ToString(),10};");

                var data = _HashQTable[i];
                foreach (var col in rows)
                {
                    if (data.TryGetValue(col,out float qValue))
                    {
                        
                        sb.AppendFormat("{0,10};", qValue.ToString("0.###"));
                    }
                    else
                    {
                        sb.Append($"{"",10};");
                    }

                    if (sb.Length > 512)
                    {
                        sb.CopyTo(0, buffer, 0, sb.Length);
                        tw.Write(buffer, 0, sb.Length);
                        sb.Clear();
                    }
                }

                sb.AppendLine();
                
            }

            sb.CopyTo(0, buffer, 0, sb.Length);
            tw.Write(buffer, 0, sb.Length);
            sb.Clear();
            //tw.Write(sb.ToString());
        }

        private void PrintTableData2(TextWriter tw, Span<int> rows)
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
                    if (data.TryGetValue(col, out float qValue))
                    {
                        
                        sb.AppendFormat("{0,10};", qValue.ToString("0.###"));
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

        private int [] GetAllRows()
        {
            HashSet<int> result = new HashSet<int>(400);
            foreach (var row in _HashQTable)
            {
                foreach(var col in row)
                {
                    result.Add(col.Key);
                }
            }

            return result.ToArray();
        }
    }
}

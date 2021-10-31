using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.QLearning;
using TheGameNet.Utils;

namespace TheGameNet.Core
{
    internal static class QLearning_HashTransform
    {
        static byte[] arrayForHashState = new byte[320];

        public static int QLearning_StateIndex2(QTable qTable, BoardMini boardMini, GameBoard board, List<byte> handCards)
        {
            Array.Clear(arrayForHashState, 0, arrayForHashState.Length);

            //Span<byte> arrayForHashState = stackalloc byte[120];

            int arrayForHashIndex = 0;

            for (int i = 0; i < 4; i++)
            {
                arrayForHashState[arrayForHashIndex] =
                 (byte)boardMini.CardPlaceholders[i].Get_TopCard();
                //(byte)((boardMini.CardPlaceholders[i].Get_CardDiff_ToEnd(boardMini.CardPlaceholders[i].Get_TopCard())));
                arrayForHashIndex++;

            }

            /* for (int i = 0; i < 4; i++)
             {
                 arrayForHashState[arrayForHashIndex] =
                  (byte)boardMini.CardPlaceholders[i].Get_TopCard();
                 //(byte)((boardMini.CardPlaceholders[i].Get_CardDiff_ToEnd(boardMini.CardPlaceholders[i].Get_TopCard())));
                 //arrayForHashIndex++;

             }

             arrayForHashIndex += 200;*/

            // arrayForHashState[arrayForHashIndex++] = (byte)board.AvailableCards.Count;

            for (int i = 0; i < handCards.Count; i++)
            {
                arrayForHashState[arrayForHashIndex + handCards[i]] = 1;
                arrayForHashIndex++;
            }
            arrayForHashIndex += 100;

            //for (int i = 0; i < 10; i++)
            //{
            //    if (handCards.Count > i)
            //    {
            //        arrayForHash[arrayForHashIndex] = handCards[i];
            //        arrayForHashIndex++;
            //    }
            //}

            //arrayForHashState[arrayForHashIndex] = (byte)board.Count_AllRemaindPlayCards;// (byte)boardMini.CountPossiblePlay();
            arrayForHashIndex++;
            var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashState, 0, arrayForHashIndex);
            return qTable.CreateKey_IndexState(dataForHash);


        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization )]
        public static int QLearning_StateIndex(QTable qTable, BoardMini boardMini, GameBoard board, ReadOnlySpan<byte> handCards, byte playerId)
        {
            //Array.Clear(arrayForHashState, 0, arrayForHashState.Length);
            //var arrayrent = ArrayPool<byte>.Shared.Rent(205);
            //Span<byte> arrayForHashState = arrayrent.AsSpan(0, 205);
            Span<byte> arrayForHashState = stackalloc byte[205];
            //arrayForHashState.Fill(0);
            int arrayForHashIndex = 0;


            for (int i = 0; i < 4; i++)
            {
                var cph = boardMini.CardPlaceholders[i];
                int index = 0;
                if (cph.UpDirection)
                {
                    index = 100;
                }

                arrayForHashState[arrayForHashIndex + index + cph.Get_TopCard()] = 1;
                // (byte)boardMini.CardPlaceholders[i].Get_TopCard();
                //(byte)((boardMini.CardPlaceholders[i].Get_CardDiff_ToEnd(boardMini.CardPlaceholders[i].Get_TopCard())));
                //arrayForHashIndex++;

            }

            //arrayForHashState[0] = (byte)boardMini.CardPlaceholders[0].Get_TopCard();
            //arrayForHashState[1] = (byte)boardMini.CardPlaceholders[1].Get_TopCard();
            //arrayForHashState[2] = (byte)boardMini.CardPlaceholders[2].Get_TopCard();
            //arrayForHashState[3] = (byte)boardMini.CardPlaceholders[3].Get_TopCard();

            //arrayForHashIndex += 4;
            arrayForHashIndex += 200;
            //arrayForHashState[4] = (byte)board.Get_PlayerBoardData(playerId).CountNeedPlayCard;
            //arrayForHashIndex ++;
            //Encode_CompactHandCars(arrayForHashState, handCards, ref arrayForHashIndex);

            // arrayForHashState[arrayForHashIndex++] = (byte)board.AvailableCards.Count;

            //for (int i = 0; i < handCards.Count; i++)
            //{
            //    arrayForHashState[arrayForHashIndex + handCards[i]] = 1;
            //    arrayForHashIndex++;
            //}
            //arrayForHashIndex += 100;

            //arrayForHashState[arrayForHashIndex] = (byte)board.Count_AllRemaindPlayCards;// (byte)boardMini.CountPossiblePlay();
            arrayForHashIndex++;
            // var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashState, 0, arrayForHashIndex);
            // return qTable.CreateKey_IndexState(dataForHash);

            int result = qTable.CreateKey_IndexState(arrayForHashState.Slice(0, arrayForHashIndex));
           // ArrayPool<byte>.Shared.Return(arrayrent,false);
            return result;
        }

        public static int QLearning_StateIndex3(QTable qTable, BoardMini boardMini, GameBoard board, Span<byte> handCards, byte playerId)
        {
            //Array.Clear(arrayForHashState, 0, arrayForHashState.Length);

            Span<byte> arrayForHashState = stackalloc byte[5];
            arrayForHashState.Fill(0);
            int arrayForHashIndex = 0;

            arrayForHashState[0] = (byte)boardMini.CardPlaceholders[0].Get_TopCard();
            arrayForHashState[1] = (byte)boardMini.CardPlaceholders[1].Get_TopCard();
            arrayForHashState[2] = (byte)boardMini.CardPlaceholders[2].Get_TopCard();
            arrayForHashState[3] = (byte)boardMini.CardPlaceholders[3].Get_TopCard();

            arrayForHashIndex += 4;

            //arrayForHashState[4] = (byte)board.Get_PlayerBoardData(playerId).CountNeedPlayCard;
            //arrayForHashIndex ++;
            //Encode_CompactHandCars(arrayForHashState, handCards, ref arrayForHashIndex);

            // arrayForHashState[arrayForHashIndex++] = (byte)board.AvailableCards.Count;

            //for (int i = 0; i < handCards.Count; i++)
            //{
            //    arrayForHashState[arrayForHashIndex + handCards[i]] = 1;
            //    arrayForHashIndex++;
            //}
            //arrayForHashIndex += 100;

            //arrayForHashState[arrayForHashIndex] = (byte)board.Count_AllRemaindPlayCards;// (byte)boardMini.CountPossiblePlay();
            arrayForHashIndex++;
            // var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashState, 0, arrayForHashIndex);
            // return qTable.CreateKey_IndexState(dataForHash);

            return qTable.CreateKey_IndexState(arrayForHashState.Slice(0, arrayForHashIndex));
        }

        private static void Encode_CompactHandCars(Span<byte> arrayForHashState, List<byte> handCards, ref int arrayForHashIndex)
        {
            for (int i = 0; i < handCards.Count; i++)
            {
                int index = handCards[i] >> 3;
                int bitPos = handCards[i] & 0x07;

                arrayForHashState[arrayForHashIndex + index] |= (byte)(1 << (bitPos));

            }
            arrayForHashIndex += 16;
        }

        public static byte[] arrayForHashAction = new byte[10];
        public static int QLearning_ActionIndex(QTable qTable, BoardMini boardMini, ref  MoveToPlay moveToPlay)
        {

            return moveToPlay.Card + (moveToPlay.DeckIndex * 100);
            Span<byte> arrayForHashAction = stackalloc byte[3];
            //arrayForHashAction.Fill(0);
            //Array.Clear(arrayForHashAction,0,arrayForHashAction.Length);
            int arrayForHashIndex = 0;
            //arrayForHashAction[arrayForHashIndex++] = (byte)((moveToPlay.IsNotMove) ? 1 : 0);

            arrayForHashAction[arrayForHashIndex++] = moveToPlay.Card; //(byte) boardMini.CardPlaceholders[moveToPlay.DeckIndex].Get_CardDiff_ToEnd(moveToPlay.Card);
            arrayForHashAction[arrayForHashIndex++] = (byte)moveToPlay.DeckIndex;

            // var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashAction, 0, arrayForHashIndex);
            //return qTable.CreateKey_IndexAction(arrayForHashAction.AsSpan(0,arrayForHashIndex) );
            return qTable.CreateKey_IndexAction(arrayForHashAction);
        }

        public static int QLearning_ActionIndex2(QTable qTable, BoardMini boardMini, MoveToPlay moveToPlay)
        {
            Array.Clear(arrayForHashAction, 0, arrayForHashAction.Length);
            int arrayForHashIndex = 0;
            arrayForHashAction[arrayForHashIndex++] = (byte)((moveToPlay.IsNotMove) ? 1 : 0);

            arrayForHashAction[arrayForHashIndex++] = moveToPlay.Card; //(byte) boardMini.CardPlaceholders[moveToPlay.DeckIndex].Get_CardDiff_ToEnd(moveToPlay.Card);
            arrayForHashAction[arrayForHashIndex++] = (byte)moveToPlay.DeckIndex;

            // var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashAction, 0, arrayForHashIndex);
            return qTable.CreateKey_IndexAction(arrayForHashAction.AsSpan(0, arrayForHashIndex));
        }

    }
}

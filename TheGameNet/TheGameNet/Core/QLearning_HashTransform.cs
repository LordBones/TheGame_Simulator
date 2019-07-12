using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.QLearning;
using TheGameNet.Utils;

namespace TheGameNet.Core
{
    internal static class QLearning_HashTransform
    {
        static byte[] arrayForHashState = new byte[105];

        public static int QLearning_StateIndex(QTable qTable, BoardMini boardMini, List<byte> handCards)
        {
            Array.Clear(arrayForHashState, 0, arrayForHashState.Length);

            
            int arrayForHashIndex = 0;

            for (int i = 0; i < 4; i++)
            {
                arrayForHashState[arrayForHashIndex] =
                 //(byte)boardMini.CardPlaceholders[i].Get_TopCard();
                 (byte)((boardMini.CardPlaceholders[i].Get_CardDiff_ToEnd(boardMini.CardPlaceholders[i].Get_TopCard())));
                arrayForHashIndex++;

            }

            //for (int i = 0; i < handCards.Count; i++)
            //{
            //    arrayForHashState[arrayForHashIndex + handCards[i]] = 1;
            //        //arrayForHashIndex++;
            //}
            //arrayForHashIndex+=100;

            //for (int i = 0; i < 10; i++)
            //{
            //    if (handCards.Count > i)
            //    {
            //        arrayForHash[arrayForHashIndex] = handCards[i];
            //        arrayForHashIndex++;
            //    }
            //}

            //arrayForHash[arrayForHashIndex] = (byte)boardMini.CountNeedPlayCard;

            var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashState, 0, arrayForHashIndex);
            return qTable.CreateKey_IndexState(dataForHash);
        }

        public static byte[] arrayForHashAction = new byte[10];
        public static int QLearning_ActionIndex(QTable qTable, BoardMini boardMini, MoveToPlay moveToPlay)
        {
            Array.Clear(arrayForHashAction,0,arrayForHashAction.Length);
            int arrayForHashIndex = 0;
            arrayForHashAction[arrayForHashIndex++] = (byte)((moveToPlay.IsNotMove) ? 1 : 0);

            arrayForHashAction[arrayForHashIndex++] = (byte) boardMini.CardPlaceholders[moveToPlay.DeckIndex].Get_CardDiff_ToEnd(moveToPlay.Card);
            arrayForHashAction[arrayForHashIndex++] = (byte)moveToPlay.DeckIndex;

            var dataForHash = new ArraySegmentEx_Struct<byte>(arrayForHashAction, 0, arrayForHashIndex);
            return qTable.CreateKey_IndexAction(dataForHash);
        }

    }
}

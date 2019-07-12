using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.GameBoardMini_Solver;

namespace TheGameNet.Core
{
    internal class BoardMini
    {
        public CardPlaceholderLight[] CardPlaceholders = new CardPlaceholderLight[4];

        public sbyte CountNeedPlayCard;




        public bool CanPlay(byte indexPlaceholder, byte card)
        {
            return CardPlaceholders[indexPlaceholder].CanPlaceCard(card);
        }

        public void ApplyCard(byte indexPlaceholder, byte card)
        {
            CardPlaceholders[indexPlaceholder].PlaceCard(card);
            CountNeedPlayCard--;
        }

        public BoardMini Clone()
        {
            BoardMini result = new BoardMini();

            for(int i = 0;i < CardPlaceholders.Length;i++)
            {
                result.CardPlaceholders[i] = CardPlaceholders[i].Clone();
            }

            result.CountNeedPlayCard = this.CountNeedPlayCard;

            return result;
        }

        public int CountPossiblePlay()
        {
            int result = 0;
            for(int i=0;i < CardPlaceholders.Length; i++)
            {
                result += CardPlaceholders[i].Get_CardDiff_ToEnd(CardPlaceholders[i].Get_TopCard());
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.GameBoardMini_Solver;
using TheGameNet.Utils;

namespace TheGameNet.Core
{
    internal class BoardMini
    {
        public CardPlaceholderLight[] CardPlaceholders = new CardPlaceholderLight[4];

        public sbyte CountNeedPlayCard;



        public BoardMini()
        {
           

        }

        public bool WasInited()
        {
            return CardPlaceholders[0] != null
                && CardPlaceholders[1] != null
                && CardPlaceholders[2] != null
                && CardPlaceholders[3] != null;
        }

        public void InitBy(BoardMini boardMini)
        {
            for (int i = 0; i < this.CardPlaceholders.Length; i++)
            {
                var cph = boardMini.CardPlaceholders[i];
                if (cph as CardPlaceholderLight_Up != null)
                {
                    this.CardPlaceholders[i] = new CardPlaceholderLight_Up(cph.Get_TopCard());
                }
                else
                {
                    this.CardPlaceholders[i] = new CardPlaceholderLight_Down(cph.Get_TopCard());
                }
            }
        }

        public bool CanPlay(byte indexPlaceholder, byte card)
        {
            return CardPlaceholders[indexPlaceholder].CanPlaceCard(card);
        }

        public void ApplyCard(sbyte indexPlaceholder, byte card)
        {
            CardPlaceholders[indexPlaceholder].PlaceCard(card);
            CountNeedPlayCard--;
        }

        public BoardMini Clone()
        {
            BoardMini result = new BoardMini();
            result.InitBy(this);
            return Clone(result);
        }

        public BoardMini Clone(BoardMini result)
        {
        
            for(int i = 0;i < CardPlaceholders.Length;i++)
            {


                result.CardPlaceholders[i] = CardPlaceholders[i].Clone();
            }

            result.CountNeedPlayCard = this.CountNeedPlayCard;

            return result;
        }

        

        public void CopyFrom(BoardMini result)
        {

            for (int i = 0; i < CardPlaceholders.Length; i++)
            {
                bool isDown = CardPlaceholders[i] as CardPlaceholderLight_Down != null;
                bool isUp = CardPlaceholders[i] as CardPlaceholderLight_Up != null;
                bool isResultDown = result.CardPlaceholders[i] as CardPlaceholderLight_Down != null;
                bool isResultUp = result.CardPlaceholders[i] as CardPlaceholderLight_Up != null;


                if (!((isDown && isResultDown) || (isResultUp && isResultUp)))
                    throw new Exception("fail");

                CardPlaceholders[i].PlaceCard(result.CardPlaceholders[i].Get_TopCard());
            }

            this.CountNeedPlayCard = result.CountNeedPlayCard;

            //return result;
        }

        public void RecyclePlaceholders(ObjectPool<CardPlaceholderLight_Down> _cpLightDown_Pool, ObjectPool<CardPlaceholderLight_Up> _cpLightUp_Pool)
        {
            for (int i = 0; i < CardPlaceholders.Length; i++)
            {
                if (CardPlaceholders[i].UpDirection)
                {
                    _cpLightUp_Pool.PutForRecycle((CardPlaceholderLight_Up)CardPlaceholders[i]);
                }
                else
                {
                    _cpLightDown_Pool.PutForRecycle((CardPlaceholderLight_Down)CardPlaceholders[i]);

                }
                CardPlaceholders[i] = null;
            }
        }

        public int CountPossiblePlay()
        {
            int result = 0;
            for(int i=0;i < CardPlaceholders.Length; i++)
            {
                int kk = CardPlaceholders[i].Get_CardDiff_ToEnd(CardPlaceholders[i].Get_TopCard());
                result += kk;
            }

            return result;
        }

        public int CountPossiblePlayMinMax()
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            int result = 0;
            for (int i = 0; i < CardPlaceholders.Length; i++)
            {
                int kk = CardPlaceholders[i].Get_CardDiff_ToEnd(CardPlaceholders[i].Get_TopCard());
                if (kk < min) min = kk;
                if (kk > max) max = kk;
                
            }

            result = min ;
            return result;
        }
    }
}

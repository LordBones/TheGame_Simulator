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

        public BoardMini Clone(BoardMini result, ObjectPoolTS<CardPlaceholderLight_Down> _cpLightDown_Pool, ObjectPoolTS<CardPlaceholderLight_Up> _cpLightUp_Pool)
        {

            for (int i = 0; i < CardPlaceholders.Length; i++)
            {
                if (_cpLightDown_Pool != null && _cpLightUp_Pool != null)
                {
                    if (CardPlaceholders[i].UpDirection)
                    {
                        result.CardPlaceholders[i] = CardPlaceholders[i].Clone(_cpLightUp_Pool.GetNewOrRecycle());
                    }
                    else
                    {
                        result.CardPlaceholders[i] = CardPlaceholders[i].Clone(_cpLightDown_Pool.GetNewOrRecycle());
                    }
                }
                else
                {
                    result.CardPlaceholders[i] = CardPlaceholders[i].Clone();
                }
            }

            result.CountNeedPlayCard = this.CountNeedPlayCard;

            return result;
        }

        public void RecyclePlaceholders(ObjectPoolTS<CardPlaceholderLight_Down> _cpLightDown_Pool, ObjectPoolTS<CardPlaceholderLight_Up> _cpLightUp_Pool)
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

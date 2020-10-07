using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.GameBoardMini
{
    public struct CardPlaceholderULight
    {
        private const int CONST_Down_InitCard = 100;
        private const int CONST_Down_EndCard = 2;
        private const int CONST_Up_InitCard = 1;
        private const int CONST_Up_EndCard = 99;

        private bool _upDirection;
        private byte _cardPlaceholder;

        public byte Get_TopCard() => _cardPlaceholder;
        public bool UpDirection => _upDirection;

        public CardPlaceholderULight(bool upDirection)
        {
            _upDirection = upDirection;
            _cardPlaceholder = 0;
            Clear();
        }

        public CardPlaceholderULight Clone()
        {
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanPlaceCard(byte card)
        {
            if (_upDirection)
            {
                byte peekCard = _cardPlaceholder;

                if (peekCard < card || card + 10 == peekCard) return true;

                return false;
            }
            else
            {
                byte peekCard = _cardPlaceholder;

                if (peekCard > card || card == peekCard + 10) return true;

                return false;
            }
        }

        public void PlaceCard(byte card)
        {
            this._cardPlaceholder = card;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get_CardDiff(byte myCard)
        {
            if (_upDirection)
            {
                int peekCard = this.GetPeakCard();

                return (int)myCard - peekCard;
            }
            else
            {
                int peekCard = this.GetPeakCard();

                return peekCard - (int)myCard;

                
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  int Get_CardDiff_FromBase(byte myCard)
        {
            if (_upDirection)
            {
                return (int)myCard - CONST_Up_InitCard;
            }
            else
            {
                return CONST_Down_InitCard - (int)myCard;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get_CardDiff_ToEnd(byte myCard)
        {
            if (_upDirection)
            {
                return CONST_Up_EndCard - myCard;
            }
            else
            {
                return myCard - CONST_Up_EndCard;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Get_CardJump(byte myCard)
        {
            if (_upDirection)
            {
                return (byte)(myCard + 10);
            }
            else
            {
                if (myCard < 11) return myCard;
                else return (byte)(myCard - 10);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            if (_upDirection)
            {
                this._cardPlaceholder = CONST_Up_InitCard;
            }
            else
            {
                this._cardPlaceholder = CONST_Down_InitCard;
            }
        }

        public byte GetPeakCard()
        {
            return _cardPlaceholder;
        }
    }
}

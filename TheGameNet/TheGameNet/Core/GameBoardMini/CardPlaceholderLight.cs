using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.GameBoardMini_Solver
{
    internal abstract class CardPlaceholderLight
    {
        protected bool _upDirection;
        protected byte _cardPlaceholder;


        public byte Get_TopCard() => _cardPlaceholder;

        public bool UpDirection => _upDirection;

        public abstract CardPlaceholderLight Clone();
        public abstract CardPlaceholderLight Clone(CardPlaceholderLight cph);

        public abstract bool CanPlaceCard(byte card);

        public abstract void PlaceCard(byte card);

        public abstract int Get_CardDiff(byte myCard);


        public abstract int Get_CardDiff_FromBase(byte myCard);

        public abstract int Get_CardDiff_ToEnd(byte myCard);



        public abstract byte Get_CardJump(byte myCard);


        
        public abstract void Clear();

        protected abstract byte GetPeakCard();

        public abstract void UpdatePhantomState();
    }

    internal class CardPlaceholderLight_Down : CardPlaceholderLight
    {
        private const int CONST_InitCard = 100;
        private const int CONST_EndCard = 2;

        public CardPlaceholderLight_Down()
        {
            this._upDirection = false;
            this.Clear();
        }

        public CardPlaceholderLight_Down(byte initCard) : this()
        {
            this._cardPlaceholder = initCard;
        }



        public override void PlaceCard(byte card)
        {
            this._cardPlaceholder =card;
        }

        public override int Get_CardDiff(byte myCard)
        {
            int peekCard = this.GetPeakCard();

            return peekCard - (int)myCard;

        }

        public override int Get_CardDiff_FromBase(byte myCard)
        {
            return CONST_InitCard - (int)myCard;
        }

        public override int Get_CardDiff_ToEnd(byte myCard)
        {
            return myCard - CONST_EndCard;
        }

        public override bool CanPlaceCard(byte card)
        {
            byte peekCard = _cardPlaceholder;

            if (peekCard > card || card == peekCard + 10) return true;

            return false;
        }

        public override byte Get_CardJump(byte myCard)
        {
            if (myCard < 11) return myCard;
            else return (byte)(myCard - 10);
        }

        public override void Clear()
        {
            this._cardPlaceholder = CONST_InitCard;
        }

        protected override byte GetPeakCard()
        {
            return _cardPlaceholder;
        }

        public override void UpdatePhantomState()
        {
        }

        public override CardPlaceholderLight Clone()
        {
            CardPlaceholderLight_Down result = new CardPlaceholderLight_Down();
            return Clone(result);
        }

        public override CardPlaceholderLight Clone(CardPlaceholderLight cph)
        {
            CardPlaceholderLight_Down placeholder = cph as CardPlaceholderLight_Down;
            if (placeholder == null) throw new Exception("nesmi");

            placeholder.PlaceCard(_cardPlaceholder);
            return placeholder;
        }

        

        public override string ToString()
        {
            return $"top: {_cardPlaceholder}, Down";
        }
    }


    internal class CardPlaceholderLight_Up : CardPlaceholderLight
    {
        private const int CONST_InitCard = 1;
        private const int CONST_EndCard = 99;


        public CardPlaceholderLight_Up(byte initCard):this()
        {
            this._cardPlaceholder = initCard; 
        }

        public CardPlaceholderLight_Up()
        {
            this._upDirection = true;
            this.Clear();
        }



        public override void PlaceCard(byte card)
        {
            this._cardPlaceholder= card;
        }

        public override int Get_CardDiff(byte myCard)
        {
            int peekCard = this.GetPeakCard();

            return (int)myCard - peekCard;

        }

        public override int Get_CardDiff_FromBase(byte myCard)
        {
            return (int)myCard - CONST_InitCard;
        }

        public override int Get_CardDiff_ToEnd(byte myCard)
        {
            return CONST_EndCard - myCard ;
        }

        public override bool CanPlaceCard(byte card)
        {
            byte peekCard = _cardPlaceholder;

            if (peekCard < card || card + 10 == peekCard) return true;

            return false;
        }

        public override byte Get_CardJump(byte myCard)
        {
            return (byte)(myCard + 10);
        }

        public override void Clear()
        {
            this._cardPlaceholder=CONST_InitCard;
        }

        protected override byte GetPeakCard()
        {
            return _cardPlaceholder;
        }

        public override void UpdatePhantomState()
        {
        }
        public override CardPlaceholderLight Clone()
        {
            CardPlaceholderLight_Up result = new CardPlaceholderLight_Up();
            
            return Clone(result);
        }

        public override CardPlaceholderLight Clone(CardPlaceholderLight cph)
        {
            CardPlaceholderLight_Up placeholder = cph as CardPlaceholderLight_Up;
            if (placeholder == null) throw new Exception("nesmi");

            placeholder.PlaceCard(_cardPlaceholder);
            return placeholder;
        }


        public override string ToString()
        {
            return $"top: {_cardPlaceholder}, Up";
        }
    }
}

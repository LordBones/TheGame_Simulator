using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core
{

    public class CardPlaceholder_DownDirection : CardPlaceholder
    {

        private int InitCard = 100;

        public CardPlaceholder_DownDirection()
        {
            this._upDirection = false;
            this.Clear();
        }

        public void Set_InitCard(int card)
        {
            InitCard = card;
        }

        public override void PlaceCard(byte card)
        {
            this._cardPlaceholder.Push(card);
        }

        public override int Get_CardDiff(byte myCard)
        {
            int peekCard = this.GetPeakCard();

            return peekCard - (int)myCard;

        }

        public override int Get_CardDiff_FromBase(byte myCard)
        {
            int peekCard = this.GetPeakCard();

            return InitCard - (int)myCard;
        }

        public override bool CanPlaceCard(byte card)
        {
            byte peekCard = _cardPlaceholder.Peek();

            if (peekCard > card || card == peekCard + 10) return true;

            return false;
        }

        public override byte Get_CardJump(byte myCard)
        {
            if (myCard < 11) return myCard;
            else return (byte)(myCard - 10);
        }

        public override void ValidateStack()
        {
            byte[] cards = this._cardPlaceholder.ToArray();


            for (int i = cards.Length - 2; i >= 0; i--)
            {
                byte upper = cards[i + 1];
                byte lower = cards[i];
                if (!(upper > lower || lower == upper + 10))
                {
                    throw new Exception();
                }
            }

        }

        public override void Clear()
        {
            this._cardPlaceholder.Clear();
            
            this._cardPlaceholder.Push((byte)InitCard);
        }

        protected override byte GetPeakCard()
        {
            return _cardPlaceholder.Peek();
        }

        public override void UpdatePhantomState()
        {
        }
    }

    public class CardPlaceholder_DownDirection_Smart : CardPlaceholder_DownDirection
    {

        private byte _phantomPeakCard;

        private readonly PlayedCards _playedCards; 

        public CardPlaceholder_DownDirection_Smart(PlayedCards playedCards) :base()
        {
            _playedCards = playedCards;
            _phantomPeakCard = this._cardPlaceholder.Peek();
        }

        public override void PlaceCard(byte card)
        {
            this._cardPlaceholder.Push(card);

            UpdatePhantom();
        }

        public override void Clear()
        {
            base.Clear();

            _phantomPeakCard = this._cardPlaceholder.Peek();
        }

        private void UpdatePhantom()
        {
            byte tmpPhantomCard = this._cardPlaceholder.Peek();

            while (_playedCards.WasPlayed((byte)(tmpPhantomCard - 1)))
            {
                tmpPhantomCard--;
            } 

            _phantomPeakCard = tmpPhantomCard;
        }

        
        protected override byte GetPeakCard()
        {
            return _phantomPeakCard;
        }

        public override void UpdatePhantomState()
        {
            UpdatePhantom();
        }
    }

}

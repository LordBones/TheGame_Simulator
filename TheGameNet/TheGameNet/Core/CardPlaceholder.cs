using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core
{
    public abstract class CardPlaceholder
    {
        protected bool _upDirection;
        protected Stack<byte> _cardPlaceholder = new Stack<byte>();

        public bool IsUpDirection => _upDirection;
        public byte Get_TopCard() => _cardPlaceholder.Peek();

        public abstract bool CanPlaceCard(byte card);

        public abstract void PlaceCard(byte card);

        public abstract int Get_CardDiff(byte myCard);


        public abstract int Get_CardDiff_FromBase(byte myCard);


        public abstract byte Get_CardJump(byte myCard);


        public abstract void ValidateStack();

        public abstract void Clear();

        protected abstract byte GetPeakCard();

        public abstract void UpdatePhantomState();
        
    }

}

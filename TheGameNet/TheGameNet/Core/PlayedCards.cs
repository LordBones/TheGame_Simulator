using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core
{
    public class PlayedCards
    {
        private byte [] _cards ;
        private byte _count;

        public PlayedCards()
        {
            _cards = new byte[14];
            _count = 0;
        }
        

        public int Count => _count;

        public void Clear()
        {
             _cards.AsSpan().Fill(0);
            _count = 0;
        }

        public bool WasPlayed(byte card)
        {
            int index = card >> 3;
            int offset = card & 7;
            return (_cards[index] & (1<<offset)) > 0;
        }

        public void AddCard_IfNotExist(byte card)
        {
            int index = card >> 3;
            int offset = card & 7;
            _cards[index] |= (byte)(1 << offset);
        }
    }

    
}

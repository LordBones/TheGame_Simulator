using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core
{
    public class PlayedCards
    {
        private HashSet<byte> _cards;

        public PlayedCards()
        {
            _cards = new HashSet<byte>();
        }

        public int Count => _cards.Count;

        public void Clear()
        {
            _cards.Clear();
        }

        public bool WasPlayed(byte card)
        {
            return _cards.Contains(card);
        }

        public void AddCard_IfNotExist(byte card)
        {
            _cards.Add(card);
        }
    }
}

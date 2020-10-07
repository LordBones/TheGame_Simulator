using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core
{
    class DecksTestBatch
    {
        FixList<byte[]> _decks;
        public enum eStrategy { Fixed, Incremental}

        private int _deckCounter = 0;
        private DeckGenerator _deckGen;

        public Span<byte[]> Decks { get { return _decks.GetSpan(); } }

        public DecksTestBatch(int maxDeckCounts, DeckGenerator deckGen)
        {
            _decks = new FixList<byte[]>(maxDeckCounts);
            _deckGen = deckGen;
        }

        public bool Is_Full { get { return _decks.IsFull; } }

        public void Refresh_Decks()
        {
            _decks.Clear();

            for(int i=0; i < _decks.MaxSize; i++)
            {
                _decks.Add(_deckGen.Get_CreatedSuffledDeck());
            }
        }

        public bool Add_Deck()
        {
            if (_decks.IsFull) return false;

            _decks.Add(_deckGen.Get_CreatedSuffledDeck());
            return true;
        }

        public void Clear()
        {
            _decks.Clear();
        }

        public void Execute_Step_Strategy()
        {

        }

        private void Strategy_Fixed()
        {

        }
    }
}

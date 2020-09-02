using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Utils;

namespace TheGameNet.Core
{
    class DeckGenerator
    {
        private int _maxCard;
        private RandomGen _rng;

        public DeckGenerator(int maxCard, int randomSeed)
        {
            _rng = new RandomGen(randomSeed);
            _maxCard = maxCard;
        }

        public  byte[] Get_CreatedSuffledDeck()
        {

            int countCards = _maxCard - 2;
            List<byte> result = new List<byte>(countCards);

            List<byte> forSuffle = new List<byte>(countCards);
            for (byte i = 2; i < _maxCard; i++)
            {
                forSuffle.Add(i);
            }

            //           Random rnd = new Random((int)DateTime.Now.Ticks);

            while (forSuffle.Count > 0)
            {
                int index = _rng.GetRandomNumber(0, forSuffle.Count);
                byte card = forSuffle[index];
                result.Add(card);
                forSuffle.Remove(card);
            }

            return result.ToArray();
        }
    }
}

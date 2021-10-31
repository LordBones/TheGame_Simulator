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

        public int CardMaxCount => _maxCard - 2;

        public DeckGenerator(int maxCard, int randomSeed)
        {
            _rng = new RandomGen(randomSeed);
            _maxCard = maxCard;
        }





        public byte[] Get_CreatedSuffledDeck()
        {
            byte[] result = new byte[CardMaxCount];
            Get_CreatedSuffledDeck(result.AsSpan());
            return result;
        }

        public void Get_CreatedSuffledDeck(Span<byte> result)
        {
            if (result.Length != CardMaxCount)
                throw new Exception("forbiden");





            int index = 0;
            for (byte i = 2; i < _maxCard; i++, index++)
            {
                result[index] = i;
            }

            int endIndex = result.Length - 1;

            while (endIndex > 0)
            {
                int tmpIndex = _rng.GetRandomNumber(0, endIndex + 1);

                // swap
                result[tmpIndex] ^= result[endIndex];
                result[endIndex] ^= result[tmpIndex];
                result[tmpIndex] ^= result[endIndex];

                endIndex--;
            }
        }

        public byte[] Get_CreatedSuffledDeck2()
        {

            int countCards = _maxCard - 2;

            Span<byte> result_Span = stackalloc byte[countCards];
            FixListSpan<byte> result = new FixListSpan<byte>(result_Span);

            //List<byte> result = new List<byte>(countCards);

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

            return result.GetSpan().ToArray();
        }
    }
}

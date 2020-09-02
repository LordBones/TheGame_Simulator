using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    public class Player_Dumb : Player
    {
        public Player_Dumb()
        {

        }

        public Player_Dumb(string name) : base(name)
        {
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {

        }

        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);


            return possibleToPlay[0];
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {

            return new MoveToPlay(0, -1);
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {

        }

        public override void EndGame(GameBoard board, Span<byte> handCards)
        {

        }
    }
}

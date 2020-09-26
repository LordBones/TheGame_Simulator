using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    public class Player_Soliter_MinHarm : Player
    {

        public Player_Soliter_MinHarm() : base()
        {

        }
        public Player_Soliter_MinHarm(string name) : base(name)
        {
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {

        }


        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMinHarm_Move(possibleToPlay, board);

            forMove = Get_JumpFromHandToPlay(board, handCards, forMove);

            return forMove;
        }



        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {
            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMinHarm_Move(possibleToPlay, board);

            if (!forMove.IsNotMove && board.Get_PH_ULight(forMove.DeckIndex).Get_CardDiff(forMove.Card) < 2)
            {
                forMove = Get_JumpFromHandToPlay(board, handCards, forMove);
                return forMove;
            }

            return new MoveToPlay(0, -1);
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {

        }

        public override void EndGame(GameBoard board, Span<byte> handCards)
        {

        }


        private MoveToPlay Get_JumpFromHandToPlay(GameBoard board, Span<byte> handCards, MoveToPlay forMove)
        {
            byte cardJump = board.Get_PH_ULight(forMove.DeckIndex).Get_CardJump(forMove.Card);

            int index = handCards.IndexOf(cardJump);

            if (index < 0)
            {
                return forMove;
            }
            else
            {
                return new MoveToPlay(cardJump, forMove.DeckIndex);
            }
        }

        private MoveToPlay GetMinHarm_Move(List<MoveToPlay> moves, GameBoard board)
        {
            int bestDiff = int.MaxValue;
            MoveToPlay bestMove = new MoveToPlay(0, -1);

            for (int i = 0; i < moves.Count; i++)
            {
                MoveToPlay move = moves[i];
                int diff = board.Get_PH_ULight(move.DeckIndex).Get_CardDiff(move.Card);

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestMove = move;
                }
            }

            return bestMove;
        }
    }
}

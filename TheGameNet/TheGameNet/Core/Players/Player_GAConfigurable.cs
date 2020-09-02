using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    public class Player_GAConfigurable : Player
    {
        public class Configuration
        {
            public short[] valueFirstCards = new short[10];
            public short valueOfAllOthers = 0;
            public short valueJump = 0;
        }

        private Configuration _configuration = new Configuration();

        public Configuration ValueConfiguration { get { return _configuration; } }

        public Player_GAConfigurable()
        {
            Init_DefaultConfiguration();
        }

        public Player_GAConfigurable(string name) : base(name)
        {
            Init_DefaultConfiguration();
        }

        private void Init_DefaultConfiguration()
        {
            for(int i = 0;i< _configuration.valueFirstCards.Length; i++)
            {
                _configuration.valueFirstCards[i] = (short)(i);
                _configuration.valueJump = -10;
                _configuration.valueOfAllOthers = 30;
            }
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {
            
        }

        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {
            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMostValued_Move(possibleToPlay, board,handCards);

            

            return forMove;
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {
            return new MoveToPlay(0, -1);
        }

        public override void EndGame(GameBoard board, Span<byte> handCards)
        {
           
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {
           
        }


        private bool Has_JumpFromHandToPlay(GameBoard board, Span<byte> handCards, MoveToPlay forMove)
        {
            byte cardJump = board.CardPlaceholders[forMove.DeckIndex].Get_CardJump(forMove.Card);

            int index = handCards.IndexOf(cardJump);

            if (index < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private MoveToPlay GetMostValued_Move(List<MoveToPlay> moves, GameBoard board, Span<byte> handCards)
        {
            int bestMoveValue = int.MaxValue;
            MoveToPlay bestMove = new MoveToPlay(0, -1);

            for (int i = 0; i < moves.Count; i++)
            {
                MoveToPlay move = moves[i];
                int diff = board.CardPlaceholders[move.DeckIndex].Get_CardDiff(move.Card);

                short moveValue = ComputeValue_FromDiff(diff);

                if (Has_JumpFromHandToPlay(board, handCards, move))
                {
                    moveValue += 10;
                }

                if (moveValue < bestMoveValue)
                {
                    bestMoveValue = moveValue;
                    bestMove = move;
                }
            }

            return bestMove;
        }

        private short ComputeValue_FromDiff(int diff)
        {
            if(diff>-1 && diff < this._configuration.valueFirstCards.Length)
            {
                return this._configuration.valueFirstCards[diff];
            }
            else if(diff >= this._configuration.valueFirstCards.Length)
            {
                return this._configuration.valueOfAllOthers;
            }
            else
            {
                return this._configuration.valueJump;
            }
        }
    }
}

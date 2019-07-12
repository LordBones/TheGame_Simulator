using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    public class Player_Team_MinHarm_2 : Player
    {

        public Player_Team_MinHarm_2() : base()
        {

        }
        public Player_Team_MinHarm_2(string name) : base(name)
        {
        }

        public override void StartPlay(GameBoard board, List<byte> handCards)
        {

        }


        public override MoveToPlay Decision_CardToPlay(GameBoard board, List<byte> handCards)
        {

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMinHarm_Move(possibleToPlay, board);

            forMove = Get_JumpFromHandToPlay(board, handCards, forMove);

            return forMove;
        }



        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, List<byte> handCards)
        {
            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMinHarm_Move(possibleToPlay, board);

            if (!forMove.IsNotMove && board.CardPlaceholders[forMove.DeckIndex].Get_CardDiff(forMove.Card) < 2)
            {
                forMove = Get_JumpFromHandToPlay(board, handCards, forMove);
                return forMove;
            }

            return new MoveToPlay(0, -1);
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, List<byte> handCards, bool isEndOfGame)
        {

        }

        public override void EndGame(GameBoard board, List<byte> handCards)
        {

        }

        private MoveToPlay Get_JumpFromHandToPlay(GameBoard board, List<byte> handCards, MoveToPlay forMove)
        {
            byte cardJump = board.CardPlaceholders[forMove.DeckIndex].Get_CardJump(forMove.Card);

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

        short?[] deckBestHints = new short?[4];

        private MoveToPlay GetMinHarm_Move(List<MoveToPlay> moves, GameBoard board)
        {


            deckBestHints[0] = board.CardPlaceholdersHints[0].FindNearestDiff(this.Id);
            deckBestHints[1] = board.CardPlaceholdersHints[1].FindNearestDiff(this.Id);
            deckBestHints[2] = board.CardPlaceholdersHints[2].FindNearestDiff(this.Id);
            deckBestHints[3] = board.CardPlaceholdersHints[3].FindNearestDiff(this.Id);

            int bestDiff = int.MaxValue;
            MoveToPlay bestMove = new MoveToPlay(0, -1);

            int teamBestDiff = int.MaxValue;
            MoveToPlay teamBestMove = new MoveToPlay(0, -1);


            for (int i = 0; i < moves.Count; i++)
            {
                MoveToPlay move = moves[i];
                int diff = board.CardPlaceholders[move.DeckIndex].Get_CardDiff(move.Card);

                byte cardJump = board.CardPlaceholders[move.DeckIndex].Get_CardJump(move.Card);
                bool cardHashJump = move.Card != cardJump && board.Get_HasCard(board.Players_Cards[this.Id], cardJump);

                short? cardHint = deckBestHints[move.DeckIndex];

                if (cardHint.HasValue)
                {
                    if (diff < cardHint.Value)
                    {
                        if (diff < teamBestDiff)
                        {
                            teamBestDiff = diff;
                            teamBestMove = move;
                        }
                    }
                }

                //int evaluatedCardHint = Evaluate_CardHint(cardHint);

                //int computedDiff = diff + evaluatedCardHint;

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestMove = move;
                }
            }

            if (teamBestMove.IsNotMove)
            {
                return bestMove;
            }
            else
            {
                return teamBestMove;
            }

        }

        private int Evaluate_CardHint(RangeHint? cardHint)
        {
            if (!cardHint.HasValue) return 0;
            else if (cardHint.Value == RangeHint.Exact) return 200;
            else if (cardHint.Value == RangeHint.Closer) return 400;

            else return 600;

        }
    }

    public class Player_Team_MinHarm : Player
    {

        public Player_Team_MinHarm() : base()
        {

        }
        public Player_Team_MinHarm(string name) : base(name)
        {
        }

        public override void StartPlay(GameBoard board, List<byte> handCards)
        {

        }

        public override MoveToPlay Decision_CardToPlay(GameBoard board, List<byte> handCards)
        {

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMinHarm_Move(possibleToPlay, board);

            forMove = Get_JumpFromHandToPlay(board, handCards, forMove);

            return forMove;
        }



        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, List<byte> handCards)
        {
            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            MoveToPlay forMove = GetMinHarm_Move(possibleToPlay, board);

            if (!forMove.IsNotMove && board.CardPlaceholders[forMove.DeckIndex].Get_CardDiff(forMove.Card) < 2)
            {
                forMove = Get_JumpFromHandToPlay(board, handCards, forMove);
                return forMove;
            }

            return new MoveToPlay(0, -1);
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, List<byte> handCards, bool isEndOfGame)
        {

        }

        public override void EndGame(GameBoard board, List<byte> handCards)
        {

        }

        private MoveToPlay Get_JumpFromHandToPlay(GameBoard board, List<byte> handCards, MoveToPlay forMove)
        {
            byte cardJump = board.CardPlaceholders[forMove.DeckIndex].Get_CardJump(forMove.Card);

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

        RangeHint?[] deckBestHints = new RangeHint?[4];

        private MoveToPlay GetMinHarm_Move(List<MoveToPlay> moves, GameBoard board)
        {


            deckBestHints[0] = board.CardPlaceholdersHints[0].FindNearest(this.Id);
            deckBestHints[1] = board.CardPlaceholdersHints[1].FindNearest(this.Id);
            deckBestHints[2] = board.CardPlaceholdersHints[2].FindNearest(this.Id);
            deckBestHints[3] = board.CardPlaceholdersHints[3].FindNearest(this.Id);

            int bestDiff = int.MaxValue;
            MoveToPlay bestMove = new MoveToPlay(0, -1);

            int teamBestDiff = int.MaxValue;
            MoveToPlay teamBestMove = new MoveToPlay(0, -1);


            for (int i = 0; i < moves.Count; i++)
            {
                MoveToPlay move = moves[i];
                int diff = board.CardPlaceholders[move.DeckIndex].Get_CardDiff(move.Card);

                RangeHint? cardHint = deckBestHints[move.DeckIndex];

                if (cardHint.HasValue)
                {
                    RangeHint myCardHint = GameBoard.DiffToRangeHint(diff);

                    if (myCardHint < cardHint.Value)
                    {
                        if (diff < teamBestDiff)
                        {
                            teamBestDiff = diff;
                            teamBestMove = move;
                        }
                    }
                }

                //int evaluatedCardHint = Evaluate_CardHint(cardHint);

                //int computedDiff = diff + evaluatedCardHint;

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestMove = move;
                }
            }

            if (teamBestMove.IsNotMove)
            {
                return bestMove;
            }
            else
            {
                return teamBestMove;
            }


        }

        private int Evaluate_CardHint(RangeHint? cardHint)
        {
            if (!cardHint.HasValue) return 0;
            else if (cardHint.Value == RangeHint.Exact) return 200;
            else if (cardHint.Value == RangeHint.Closer) return 400;

            else return 600;

        }
    }
}

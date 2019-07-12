using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.GameBoardMini
{
    internal class DeepSearch_BoardMini
    {
        private BoardMini _initGameState;
        private byte[] _startCardsInHand;
        private int _maxLevel;
        private int _countCardsInDeck;

        private Stack<NodeState> _stackState = new Stack<NodeState>();

        public DeepSearch_BoardMini(BoardMini stateGame, byte [] cardsInHand, int maxLevel, int countCardsInDeck)
        {
            _startCardsInHand = cardsInHand;
            _initGameState = stateGame;
            _maxLevel = maxLevel;
            _countCardsInDeck = countCardsInDeck;
        }

        public (sbyte placeholderId, byte card, byte level) GetBestNextCard()
        {
            NodeState startNode = new NodeState();
            startNode.GameState = _initGameState.Clone();
            startNode.RootMove = new Move(-1, 0);
            startNode.cardsInHand = new byte[_startCardsInHand.Length];
            startNode.Level = 0;
            _startCardsInHand.CopyTo(startNode.cardsInHand,0);

            _stackState.Clear();
            _stackState.Push(startNode);

            var nodeState = FindBestNextCard();


            return (placeholderId: nodeState.RootMove.Placeholder, card: nodeState.RootMove.Card, level: nodeState.Level);
        }

        private NodeState FindBestNextCard()
        {
            NodeState result = new NodeState();
            result.RootMove = new Move(-1,0);

            do
            {
                NodeState startNode = _stackState.Pop();

                result = GetBestNodeState(result, startNode);


                if (startNode.Level < _maxLevel)
                {
                    int generatedLevels = GenerateNextValidMoves(startNode);

                    //if(generatedLevels == 0)
                    //{
                    //    result = GetBestNodeState(result, startNode);
                    //}
                }
                else
                {
                   // result = GetBestNodeState(result, startNode);
                }



            } while (_stackState.Count > 0);

            return result;
        }

        private NodeState GetBestNodeState(NodeState currentBest, NodeState newNodeState)
        {
            if (currentBest.RootMove.Placeholder < 0) return newNodeState;

            /*
            int kk = (currentBest.GameState.CountPossiblePlay() - this._countCardsInDeck );

            //if (currentBest.cardsInHand.Length == newNodeState.cardsInHand.Length )
            {
                if(kk < (newNodeState.GameState.CountPossiblePlay() - -this._countCardsInDeck))
                {
                    return newNodeState;
                }
            }
            */
            int kk = (currentBest.GameState.CountPossiblePlay()- this._countCardsInDeck + currentBest.Level );


            if (kk < (newNodeState.GameState.CountPossiblePlay()- this._countCardsInDeck + newNodeState.Level))
            {
                return newNodeState;
            }

            //if (currentBest.cardsInHand.Length > newNodeState.cardsInHand.Length) return newNodeState;

            return currentBest;
        }

        private int GenerateNextValidMoves(NodeState currentState)
        {
            int countValidMoves = 0;
            var currentPlaceholders = currentState.GameState.CardPlaceholders;
            var currentCardsInHand = currentState.cardsInHand;

            for (byte i = 0; i < currentPlaceholders.Length; i++)
            {
                for (int c = 0; c < currentCardsInHand.Length; c++)
                {
                    byte card = currentCardsInHand[c];
                    if (currentState.GameState.CanPlay((byte)i, card))
                    {
                        BoardMini nextCurrentState = currentState.GameState.Clone();
                        nextCurrentState.ApplyCard(i, card);

                        NodeState startNode = new NodeState();
                        startNode.GameState = nextCurrentState;
                        startNode.RootMove = (currentState.RootMove.Placeholder < 0)? new Move((sbyte)i, card): currentState.RootMove;
                        startNode.cardsInHand =
                            Remove_CardInHand(currentCardsInHand, card);
                            //currentCardsInHand.Where(x => x != card).ToArray();
                        startNode.Level = (byte)(currentState.Level + 1);

                        //var ferda = Remove_CardInHand(currentCardsInHand, card);
                        //var ferda2 = currentCardsInHand.Where(x => x != card).ToArray();

                        //if (!ferda.SequenceEqual(ferda2))
                        //{
                        //    Console.Error.WriteLine("Fail");
                        //}

                        _stackState.Push(startNode);
                        countValidMoves++;
                    }
                }
            }

            return countValidMoves;
        }


        private static byte[] emptybyteField = new byte[0];
        private byte [] Remove_CardInHand(byte [] cards, byte removeCard)
        {
            int sizeField = cards.Length;
            if (Array.IndexOf(cards,removeCard) >= 0) sizeField--;

            //if (sizeField == 0) return emptybyteField;

            byte[] result = new byte[sizeField];
            int resultIndex = 0;
            for(int i =0; i < cards.Length; i++)
            {
                if(cards[i] != removeCard)
                {
                    result[resultIndex] = cards[i];
                    resultIndex++;
                }
            }

            return result;
        }
    }

    struct NodeState
    {
        public BoardMini GameState;
        public Move RootMove;
        public byte[] cardsInHand;
        public byte Level;
    }

    struct Move
    {
        public sbyte Placeholder;
        public byte Card;

        public Move(sbyte placeholder, byte card)
        {
            this.Card = card;
            this.Placeholder = placeholder;
        }
    }


}

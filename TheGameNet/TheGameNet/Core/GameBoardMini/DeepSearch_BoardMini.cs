using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private HashSet<CardsPlayed> _cardsPlayedLookup;

        private Stack<NodeState> _stackState = new Stack<NodeState>();

        public DeepSearch_BoardMini(BoardMini stateGame, byte [] cardsInHand, int maxLevel, int countCardsInDeck)
        {
            _startCardsInHand = cardsInHand;
            _initGameState = stateGame;
            _maxLevel = maxLevel;
            _countCardsInDeck = countCardsInDeck;
            _cardsPlayedLookup = new HashSet<CardsPlayed>();
        }

        public (sbyte placeholderId, byte card, byte level) GetBestNextCard(byte minLevel)
        {
            NodeState startNode = new NodeState();
            startNode.GameState = _initGameState.Clone();
            startNode.RootMove = new Move(-1, 0);
            startNode.cardsInHand = new byte[_startCardsInHand.Length];
            startNode.Level = 0;
            _startCardsInHand.CopyTo(startNode.cardsInHand,0);

            _stackState.Clear();
            _stackState.Push(startNode);
            _cardsPlayedLookup.Clear();

            var nodeState = FindBestNextCard(minLevel);


            return (placeholderId: nodeState.RootMove.Placeholder, card: nodeState.RootMove.Card, level: nodeState.Level);
        }

        private NodeState FindBestNextCard(byte minlevel)
        {
            NodeState result = new NodeState();
            result.RootMove = new Move(-1,0);
            result.StateEvaluation = int.MinValue;

            int iteration = 0;
            do
            {
                iteration++;
                NodeState startNode = _stackState.Pop();

                //   result = GetBestNodeState(result, startNode);

                if (startNode.Level == _maxLevel)
                {
                    result = GetBestNodeState(result, startNode);
                }
                else
                {
                    if (minlevel >= startNode.Level )
                    {
                        int generatedLevels = GenerateNextValidMoves(result, startNode);

                        if (generatedLevels == 0)
                        {
                            result = GetBestNodeState(result, startNode);
                        }
                    }
                    else // || startNode.Level < _maxLevel
                    {
                        if (IsWorthGenerateNextMoves(result, startNode))
                        {
                            GenerateNextValidMoves(result, startNode);
                        }

                        result = GetBestNodeState(result, startNode);
                    }
                }
                
                


            } while (_stackState.Count > 0);


            Trace.WriteLine($"{iteration}");
            return result;
        }

        private bool IsWorthGenerateNextMoves(NodeState currentBest, NodeState newNodeState)
        {
            return currentBest.StateEvaluation <= newNodeState.StateEvaluation -  (newNodeState.GetCardsLeftForPlay() * 10);
        }

        private NodeState GetBestNodeState(NodeState currentBest, NodeState newNodeState)
        {
            if (currentBest.RootMove.Placeholder < 0) return newNodeState;

           
            int kk = (currentBest.StateEvaluation );


            if (kk-1 < (newNodeState.StateEvaluation))
            {
                return newNodeState;
            }

            //if (currentBest.cardsInHand.Length > newNodeState.cardsInHand.Length) return newNodeState;

            return currentBest;
        }

        private int GenerateNextValidMoves(NodeState currentBestState, NodeState currentState)
        {
            

            int countValidMoves = 0;
            var currentPlaceholders = currentState.GameState.CardPlaceholders;

            var currentCardsInHand = currentState.cardsInHand;
            var cardsRemoved = currentState.CardsRemoved;

            for (byte i = 0; i < currentPlaceholders.Length; i++)
            {
                for (int c = 0; c < currentCardsInHand.Length; c++)
                {
                    if (cardsRemoved.IsCardRemoved(c)) continue;

                    byte card = currentCardsInHand[c];

                    var tmpCanPlayed = currentState.CardPlayed;
                    tmpCanPlayed.Set(i, c);

                    if (currentState.GameState.CanPlay((byte)i, card) //&& !_cardsPlayedLookup.Contains(tmpCanPlayed)
                        )
                    {
                        BoardMini nextCurrentState = currentState.GameState.Clone();
                        nextCurrentState.ApplyCard(i, card);

                        NodeState startNode = new NodeState();
                        startNode.GameState = nextCurrentState;
                        startNode.RootMove = (currentState.RootMove.Placeholder < 0)? new Move((sbyte)i, card): currentState.RootMove;
                        startNode.cardsInHand = currentCardsInHand;
                        //Remove_CardInHand(currentCardsInHand, card);
                        //currentCardsInHand.Where(x => x != card).ToArray();
                        startNode.CardsRemoved = cardsRemoved;
                        startNode.CardsRemoved.Set(c);
                        startNode.StateEvaluation = startNode.GameState.CountPossiblePlay() + currentState.Level;
                        startNode.Level = (byte)(currentState.Level + 1);
                        startNode.CardPlayed = tmpCanPlayed;
                        //_cardsPlayedLookup.Add(tmpCanPlayed);
                        

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

        
    }

    struct CardsRemoved
    {
        public ushort State;

        public void Set(int cardhandIndex)
        {
            State |= (ushort)(1 << (cardhandIndex));
        }

        public bool IsCardRemoved(int cardHandIndex)
        {
            return (State & (ushort)(1 << (cardHandIndex))) != 0;
        }
    }

    struct CardsPlayed : IEquatable<CardsPlayed>
    {
        public ulong Deck1;
        public ulong Deck2;
        public ulong Deck3;
        public ulong Deck4;

        public byte Deck1Count;
        public byte Deck2Count;
        public byte Deck3Count;
        public byte Deck4Count;


        public override bool Equals(object obj)
        {
            return Equals((CardsPlayed)obj );
           
        }

        public override int GetHashCode()
        {
            return (Deck1, Deck1Count, Deck2, Deck2Count, Deck3, Deck3Count, Deck4, Deck4Count).GetHashCode();
        }

        public bool Equals(CardsPlayed other)
        {
            return Deck1 == other.Deck1 & Deck2 == other.Deck2 &
                Deck3 == other.Deck3 & Deck4 == other.Deck4 &
                Deck1Count == other.Deck1Count & Deck2Count == other.Deck2Count &
                Deck3Count == other.Deck3Count & Deck4Count == other.Deck4Count;
        }

        public void Set(int deck, int cardhandIndex)
        {
            if (deck == 0) SetIndexForSet(cardhandIndex, ref Deck1Count, ref Deck1);
            else if (deck == 1) SetIndexForSet(cardhandIndex, ref Deck2Count, ref Deck2);
            else if (deck == 2) SetIndexForSet(cardhandIndex, ref Deck3Count, ref Deck3);
            else if (deck == 3) SetIndexForSet(cardhandIndex, ref Deck4Count, ref Deck4);
            else throw new NotImplementedException();
        }

        private void SetIndexForSet(int cardIndex, ref byte deckCount, ref ulong deck)
        {
            deck |= (ulong)(cardIndex & 0x0f) << deckCount;
            deckCount++;
        }
    }

    struct NodeState
    {
        public BoardMini GameState;
        public Move RootMove;
        public byte[] cardsInHand;
        public byte Level;
        public int StateEvaluation ;
        public CardsPlayed CardPlayed;
        public CardsRemoved CardsRemoved;

        public int GetCardsLeftForPlay()
        {
            int result = 0;
            for(int i =0;i< cardsInHand.Length; i++)
            {
                if (!CardsRemoved.IsCardRemoved(i)) result++;
            }

            return result;
        }
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.GameBoardMini_Solver;
using TheGameNet.Utils;

namespace TheGameNet.Core.GameBoardMini
{
    internal class DeepSearch_BoardMini
    {
        private static ObjectPoolTS<CardsPlayed> _cardsPlayedPool = new ObjectPoolTS<CardsPlayed>(100);
        private static ObjectPoolTS<NodeState> _nodeStatePool = new ObjectPoolTS<NodeState>(100);
        private static ObjectPoolTS<BoardMini> _boarMiniPool = new ObjectPoolTS<BoardMini>(100);
        private static ObjectPoolTS<CardPlaceholderLight_Down> _cpLightDown_Pool = new ObjectPoolTS<CardPlaceholderLight_Down>(200);
        private static ObjectPoolTS<CardPlaceholderLight_Up> _cpLightUp_Pool = new ObjectPoolTS<CardPlaceholderLight_Up>(200);


        private BoardMini _initGameState;
        private byte[] _startCardsInHand;
        private int _maxLevel;
        private int _countCardsInDeck;
        private HashSet<CardsPlayed> _cardsPlayedLookup;

        private Stack<NodeState> _stackState = new Stack<NodeState>();

        private MoveSequence _moveSequence = new MoveSequence(32);
        private MoveSequence _bestMoveSequence = new MoveSequence(32);
        private int _bestStateEvaluation = -1;


        public DeepSearch_BoardMini()
        {
            _cardsPlayedLookup = new HashSet<CardsPlayed>();
        }
        public DeepSearch_BoardMini(BoardMini stateGame, byte[] cardsInHand, int maxLevel, int countCardsInDeck)
        {
            _startCardsInHand = cardsInHand;
            _initGameState = stateGame;
            _maxLevel = maxLevel;
            _countCardsInDeck = countCardsInDeck;
            _cardsPlayedLookup = new HashSet<CardsPlayed>();
        }

        public void Init(BoardMini stateGame, byte[] cardsInHand, int maxLevel, int countCardsInDeck)
        {
            _startCardsInHand = cardsInHand;
            _initGameState = stateGame;
            _maxLevel = maxLevel;
            _countCardsInDeck = countCardsInDeck;
            _cardsPlayedLookup.Clear();
        }

      
        public MoveSequence GetBestNextCard(byte minLevel)
        {
            NodeState startNode = new NodeState();
            startNode.GameState = _initGameState.Clone();
            startNode.Level = 0;
            startNode.CardPlayed = new CardsPlayed();
           
            startNode.CardsInHand = new CardsInHand((byte)_startCardsInHand.Length);
            startNode.CardsInHand.Count = (byte)_startCardsInHand.Length;
            Buffer.BlockCopy(_startCardsInHand, 0, startNode.CardsInHand.Cards.Array, startNode.CardsInHand.Cards.Offset, _startCardsInHand.Length);
            //Buffer.BlockCopy(_startCardsInHand, 0, startNode.CardsInHand.Cards, 0, _startCardsInHand.Length);


            _stackState.Clear();
            _stackState.Push(startNode);
            _moveSequence = new MoveSequence(32);
            _bestMoveSequence = new MoveSequence(32);
            _cardsPlayedLookup.Clear();
            _bestStateEvaluation = int.MinValue;

            FindBestNextCard(minLevel);

            var result = new MoveSequence(_bestMoveSequence.MoveCount);
            _bestMoveSequence.CopyTo(result);


            //_stackState.Clear();
            //_cardsPlayedLookup.Clear();


            return result;
        }

        private void FindBestNextCard(byte minlevel)
        {


            int iteration = 0;
            do
            {
                iteration++;
                NodeState startNode = _stackState.Pop();

                if (startNode.Level != 0)
                {
                    _moveSequence.SetMove(startNode.Level, startNode.CurrentMove);
                }





                //   result = GetBestNodeState(result, startNode);
                bool foundBetterScore = WasFoundBetterScore(minlevel, startNode.Level, startNode.StateEvaluation);
                if (foundBetterScore)
                {
                    UpdateBestState(startNode.StateEvaluation);
                }

                if (startNode.Level < _maxLevel)
                {
                    if (minlevel >= startNode.Level)
                    {
                        int generatedLevels = GenerateNextValidMoves(startNode);
                    }
                    else
                    {
                        if (IsWorthGenerateNextMoves(startNode, foundBetterScore))
                        {
                            GenerateNextValidMoves(startNode);
                        }
                    }
                }


                startNode.GameState.RecyclePlaceholders(_cpLightDown_Pool,_cpLightUp_Pool);
                _boarMiniPool.PutForRecycle(startNode.GameState);
                startNode.Clear();
                _nodeStatePool.PutForRecycle(startNode);

            } while (_stackState.Count > 0);


            //Trace.WriteLine($"{iteration}");

            //Trace.WriteLine($"{_cpLightUp_Pool.GetBaseStat()}");


        }

        private bool IsWorthGenerateNextMoves(NodeState newNodeState, bool foundBetterScore)
        {
            if (_bestStateEvaluation == int.MinValue) return true;

            // return foundBetterScore;

            return false;
            //return currentBest.StateEvaluation <= newNodeState.StateEvaluation;
            //-  (newNodeState.GetCardsLeftForPlay() * 10);
        }


        private bool WasFoundBetterScore(byte minLevel, byte level, int stateEval)
        {
            byte currBestLevel = _bestMoveSequence.MoveCount;
            int currBestStateEval = _bestStateEvaluation;

            if (currBestLevel == 0) return true;


            bool currBestUnderLevel = currBestLevel < minLevel;
            bool underLevel = level < minLevel;

            if (currBestUnderLevel && !underLevel) return true;

            if (currBestUnderLevel && underLevel)
            {
                if (currBestLevel < level) return true;
                else if (currBestLevel == level) return currBestStateEval < stateEval;
                else return false;
            }

            if (!currBestUnderLevel && underLevel) return false;

            if (!currBestUnderLevel && !underLevel)
            {
                return currBestStateEval < stateEval;
            }

            throw new Exception("nesmi nastat");
        }

        private void UpdateBestState(int newStateEval)
        {
            _moveSequence.CopyTo(_bestMoveSequence);
            //Trace.WriteLine($"Progress best: {_bestMoveSequence.ToString()}");
            _bestStateEvaluation = newStateEval;
        }

        
        private int GenerateNextValidMoves(NodeState currentState)
        {


            int countValidMoves = 0;
         
            var currentCardsInHand = currentState.CardsInHand;
            var gameState = currentState.GameState;


            for (ushort c = 0; c < currentCardsInHand.Count; c++)
            {
                
                byte card = currentCardsInHand.Cards[c];

                for (byte p = 0; p < 4; p++)
                {
                    if (gameState.CanPlay(p, card))
                    {
                        GenerateNextValidMove(currentState, card, p);
                        countValidMoves++;
                    }
                }
                //countValidMoves += GenerateNextValidMove(currentState, card, 0);
                //countValidMoves += GenerateNextValidMove(currentState, card, 1);
                //countValidMoves += GenerateNextValidMove(currentState, card, 2);
                //countValidMoves += GenerateNextValidMove(currentState, card, 3);
            }

            return countValidMoves;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GenerateNextValidMove(NodeState currentState, byte card, byte i)
        {
         
            //if (currentState.GameState.CanPlay(i, card)
            //    )
            {

                var tmpCanPlayed = _cardsPlayedPool.GetNewOrRecycle();
                currentState.CardPlayed.CopyTo(tmpCanPlayed);
                tmpCanPlayed.Set(i, card);

                if (_cardsPlayedLookup.Contains(tmpCanPlayed))
                {
                    _cardsPlayedPool.PutForRecycle(tmpCanPlayed);
                   
                    return 0;
                }


                BoardMini nextCurrentState = _boarMiniPool.GetNewOrRecycle();
                nextCurrentState = currentState.GameState.Clone(nextCurrentState,_cpLightDown_Pool,_cpLightUp_Pool);
                nextCurrentState.ApplyCard((sbyte)i, card);


                NodeState startNode = _nodeStatePool.GetNewOrRecycle();
                startNode.GameState = nextCurrentState;
                //Remove_CardInHand(currentCardsInHand, card);
                //currentCardsInHand.Where(x => x != card).ToArray();
                startNode.CardsInHand = CreateCopy(ref currentState.CardsInHand);
                startNode.CardsInHand.RemoveCard(card);
                startNode.CurrentMove = new Move((sbyte)i, card);

                startNode.Level = (byte)(currentState.Level + 1);
                startNode.CardPlayed = tmpCanPlayed;

                startNode.StateEvaluation = startNode.GameState.CountPossiblePlay() +
                    //+ currentState.Level*10 + 
                    tmp(startNode.GameState,ref startNode.CardsInHand);
                _cardsPlayedLookup.Add(tmpCanPlayed);


                //var ferda = Remove_CardInHand(currentCardsInHand, card);
                //var ferda2 = currentCardsInHand.Where(x => x != card).ToArray();

                //if (!ferda.SequenceEqual(ferda2))
                //{
                //    Console.Error.WriteLine("Fail");
                //}

                _stackState.Push(startNode);
                return 1;
            }
            return 0;
        }

        private CardsInHand CreateCopy(ref CardsInHand cih)
        {
            CardsInHand result = new CardsInHand(cih.Count);
            result.Count = cih.Count;

            cih.Cards.CopyTo(ref result.Cards, cih.Count);
            //Buffer.BlockCopy(cih.Cards.Array, cih.Cards.Offset, result.Cards.Array, result.Cards.Offset, cih.Count);
            return result;
        }

        private int tmp(BoardMini gameState,ref CardsInHand cardsInHand)
        {
            int countValidMoves = 0;

            var currentPlaceholders = gameState.CardPlaceholders;

            for (ushort c = 0; c < cardsInHand.Count; c++)
            {
                byte card = cardsInHand.Cards[c];

                if (gameState.CanPlay(0, card)) countValidMoves++;
                if (gameState.CanPlay(1, card)) countValidMoves++;
                if (gameState.CanPlay(2, card)) countValidMoves++;
                if (gameState.CanPlay(3, card)) countValidMoves++;

                //for (byte i = 0; i < currentPlaceholders.Length; i++)
                //{
                //    if (gameState.CanPlay(i, card)) countValidMoves++;
                //}
            }

            return countValidMoves;
        }
    }

    struct CardsInHand
    {
        public ArraySegmentExSmall_Struct<byte> Cards;

        public byte Count;

        public CardsInHand(byte maxSize)
        {
            Count = 0;
            Cards = ChunkArrayCreator.Default.GetChunk(maxSize);// new byte[maxSize]; 
        }

        public void RemoveCard(byte card)
        {
            ushort index = 0;
            byte found = 0;
            for(byte i = 0;i < Count; i++)
            {
                if(Cards[i] == card)
                {
                    found++;
                }
                else
                {
                    Cards[index] = Cards[i];
                    index++;
                }
            }

            Count -= found;
        }

        
    }

    class CardsPlayed : IComparable<CardsPlayed>, IEquatable<CardsPlayed>
    {
        private const int CONST_CountDecks = 4;

        private const int CONST_MaxPlayedCards = 10;
        public ArraySegmentExSmall_Struct<byte> HistoryCardsPlayed;
        public ArraySegmentExSmall_Struct<byte>  HistoryCardsPlayedCounts;

        public CardsPlayed()
        {

            this.HistoryCardsPlayed = ChunkArrayCreator.Default.GetChunk(CONST_MaxPlayedCards * CONST_CountDecks);
            HistoryCardsPlayedCounts = ChunkArrayCreator.Default.GetChunk(CONST_CountDecks);
        }


        public void CopyTo(CardsPlayed dest)
        {
            this.HistoryCardsPlayed.CopyTo(ref dest.HistoryCardsPlayed);
            this.HistoryCardsPlayedCounts.CopyTo(ref dest.HistoryCardsPlayedCounts);

            //Buffer.BlockCopy(this.HistoryCardsPlayed.Array, this.HistoryCardsPlayed.Offset, dest.HistoryCardsPlayed.Array, dest.HistoryCardsPlayed.Offset, this.HistoryCardsPlayed.Count);
            //Buffer.BlockCopy(this.HistoryCardsPlayedCounts.Array, this.HistoryCardsPlayedCounts.Offset, dest.HistoryCardsPlayedCounts.Array, dest.HistoryCardsPlayedCounts.Offset, this.HistoryCardsPlayedCounts.Count);

        }

        public CardsPlayed Clone()
        {
            var result = new CardsPlayed();

            CopyTo(result);
            
            return result;
        }

        public override bool Equals(object obj)
        {
            return Equals((CardsPlayed)obj);

        }

        public override int GetHashCode()
        {
            //uint tmp = (uint)HashDepot.XXHash.Hash64(this.HistoryCardsPlayed);
            ////uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            //tmp &= 0x7fffffff;

            //return (int)tmp ;


            int sum = 0;
            for (ushort i = 0; i < 
                this.HistoryCardsPlayed.Count
                ; i++)
            {
                sum *= 31;
                sum += this.HistoryCardsPlayed[i];
            }
            return sum;
        }

        public bool Equals(CardsPlayed other)
        {

            return
                CardsPlayed.CompareByteArray(ref this.HistoryCardsPlayed, ref other.HistoryCardsPlayed) == 0 &&
                CardsPlayed.CompareByteArray(ref this.HistoryCardsPlayedCounts,ref other.HistoryCardsPlayedCounts) == 0;
        }

        public void Set(int deck, byte card)
        {
            ushort indexStart = (ushort)(deck * CONST_MaxPlayedCards);

            this.HistoryCardsPlayed[(ushort)(indexStart + this.HistoryCardsPlayedCounts[(ushort)deck])] = card;
            this.HistoryCardsPlayedCounts[(ushort)deck]++;
        }

        public int CompareTo(CardsPlayed other)
        {
            int cmp = CardsPlayed.CompareByteArray(ref this.HistoryCardsPlayed,ref other.HistoryCardsPlayed);
            if (cmp != 0) return cmp;

            return CardsPlayed.CompareByteArray(ref this.HistoryCardsPlayedCounts,ref other.HistoryCardsPlayedCounts);

            //if (
            // this.HistoryCardsPlayed.SequenceEqual(other.HistoryCardsPlayed) &&
            //this.HistoryCardsPlayedCounts.SequenceEqual(other.HistoryCardsPlayedCounts))
            //    return 0;

            //return 1;

        }

        public static int CompareByteArray(byte[] data, byte[] data2)
        {
            if (data.Length != data2.Length) return data.Length - data2.Length;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != data2[i]) return data[i] - data2[i];
            }

            return 0;
        }

        public static int CompareByteArray(ref ArraySegmentExSmall_Struct<byte> data,ref ArraySegmentExSmall_Struct<byte> data2)
        {
            if (data.Count != data2.Count) return data.Count - data.Count;

            for (ushort i = 0; i < data.Count; i++)
            {
                if (data[i] != data2[i]) return data[i] - data2[i];
            }

            return 0;
        }

        
    }


    class NodeState
    {
        public BoardMini GameState;
        public byte Level;
        public int StateEvaluation;
        public CardsPlayed CardPlayed;
        public CardsInHand CardsInHand;
        public Move CurrentMove;

        public void Clear()
        {
            CardPlayed = null;
            GameState = null;
        }
    }


    public class MoveSequence
    {
        public Move[] Moves;
        public byte MoveCount;

        public MoveSequence(int maxMovesDepth)
        {
            Moves = new Move[maxMovesDepth];
            MoveCount = 0;
        }

        public void SetMove(byte level, Move move)
        {
            if (level == 0) throw new Exception();
            if (level > MoveCount + 1) { throw new Exception(); }

            MoveCount = level;
            Moves[MoveCount - 1] = move;
        }

        public void CopyTo(MoveSequence moveSequence)
        {
            Array.Copy(Moves, moveSequence.Moves, MoveCount);
            moveSequence.MoveCount = MoveCount;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < MoveCount; i++)
            {
                sb.AppendFormat("[{0},{1}],", Moves[i].Placeholder.ToString(), Moves[i].Card.ToString());
            }

            return sb.ToString();
        }
    }


    public struct Move
    {
        public sbyte Placeholder;
        public byte Card;

        public Move(sbyte placeholder, byte card)
        {
            this.Card = card;
            this.Placeholder = placeholder;
        }

        public override string ToString()
        {
            return $"p:{Placeholder.ToString()} card:{Card.ToString()}";
        }
    }


}

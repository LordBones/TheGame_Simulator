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
        private static ObjectPool<CardsPlayed> _cardsPlayedPool = new ObjectPool<CardsPlayed>(200, () => { return new CardsPlayed(); });
        private static ObjectPool<NodeState> _nodeStatePool = new ObjectPool<NodeState>(200, () => { return new NodeState(); });
        private static ObjectPool<BoardMini> _boarMiniPool = new ObjectPool<BoardMini>(200, () => { return new BoardMini(); });
        //private static ObjectPoolTS<CardPlaceholderLight_Down> _cpLightDown_Pool = new ObjectPoolTS<CardPlaceholderLight_Down>(200);
        //private static ObjectPoolTS<CardPlaceholderLight_Up> _cpLightUp_Pool = new ObjectPoolTS<CardPlaceholderLight_Up>(200);

        private static ChunkArrayCreator _cardsPlayedLookup_chunkCreator = new ChunkArrayCreator();

        private BoardMini _initGameState;
        private ArraySegmentExSmall_Struct<byte> _startCardsInHand;
        private int _maxLevel;
        private int _countCardsInDeck;
        //private HashSet<CardsPlayed> _cardsPlayedLookup;
        private HashSet<ArraySegmentExSmall_Struct<byte>> _cardsPlayedLookup;


        private Stack<NodeState> _stackState = new Stack<NodeState>(200);

        private MoveSequence _moveSequence = new MoveSequence(11);
        private MoveSequence _bestMoveSequence = new MoveSequence(11);
        private EvaluateState<int, int> _bestStateEvaluation = new EvaluateState<int, int>(-1,-1);

        private const int CONST_Treshold_CardDiff_NotGetMoves = 5;
        public DeepSearch_BoardMini()
        {
            _cardsPlayedLookup = new HashSet<ArraySegmentExSmall_Struct<byte>>(new ASESSComparer());
        }
        public DeepSearch_BoardMini(BoardMini stateGame,in ArraySegmentExSmall_Struct<byte> cardsInHand, int maxLevel, int countCardsInDeck)
        {
            _startCardsInHand = cardsInHand;
            _initGameState = stateGame;
            _maxLevel = maxLevel;
            _countCardsInDeck = countCardsInDeck;
            _cardsPlayedLookup = new HashSet<ArraySegmentExSmall_Struct<byte>>(new ASESSComparer());
        }

        public void Init(BoardMini stateGame,in ArraySegmentExSmall_Struct<byte> cardsInHand, int maxLevel, int countCardsInDeck)
        {
            _startCardsInHand = cardsInHand;
            _initGameState = stateGame;
            _maxLevel = maxLevel;
            _countCardsInDeck = countCardsInDeck;
            _cardsPlayedLookup.Clear();
        }

      
        public MoveSequence GetBestNextCard(byte minLevel)
        {
            NodeState startNode = _nodeStatePool.GetNewOrRecycle(); //new NodeState();

            BoardMini nextCurrentState = _boarMiniPool.GetNewOrRecycle();

            if (!nextCurrentState.WasInited())
            {
                nextCurrentState.InitBy(_initGameState);
            }
            nextCurrentState.CopyFrom(_initGameState);

            var cpl = _cardsPlayedPool.GetNewOrRecycle();
            cpl.Clear();

            startNode.GameState = nextCurrentState;
            startNode.Level = 0;
            startNode.CardPlayed = cpl;// new CardsPlayed();
            startNode.IsNotWorthGenNextMove = false;
            startNode.CardsInHand = new CardsInHand((byte)_startCardsInHand.Count);
            startNode.CardsInHand.Count = (byte)_startCardsInHand.Count;
            _startCardsInHand.CopyTo(startNode.CardsInHand.Cards);
            //Buffer.BlockCopy(_startCardsInHand., 0, startNode.CardsInHand.Cards.Array, startNode.CardsInHand.Cards.Offset, _startCardsInHand.Count);
            //Buffer.BlockCopy(_startCardsInHand, 0, startNode.CardsInHand.Cards, 0, _startCardsInHand.Length);


            _stackState.Clear();



            //_stackState.Push(startNode);
            _moveSequence.Clear();// = new MoveSequence(16);
            _bestMoveSequence.Clear();// = new MoveSequence(16);
            _cardsPlayedLookup.Clear();
            _bestStateEvaluation.Val = int.MinValue;
            _bestStateEvaluation.Val2 = int.MinValue;

            GenerateNextValidMoves(startNode);

            _boarMiniPool.PutForRecycle(startNode.GameState);
            _cardsPlayedPool.PutForRecycle(startNode.CardPlayed);
            startNode.Clear();
            _nodeStatePool.PutForRecycle(startNode);

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
            while (_stackState.Count > 0)
            {
                iteration++;
                NodeState startNode = _stackState.Pop();

                _moveSequence.SetMove(startNode.Level,in startNode.CurrentMove);
             

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
                        //if (!startNode.IsNotWorthGenNextMove)
                        {
                            int generatedLevels = GenerateNextValidMoves(startNode);
                        }
                    }
                    else
                    {
                        if (IsWorthGenerateNextMoves(startNode, foundBetterScore))
                        {
                            GenerateNextValidMoves(startNode);
                        }
                    }
                }


                _cardsPlayedPool.PutForRecycle(startNode.CardPlayed); 
                //startNode.GameState.RecyclePlaceholders(_cpLightDown_Pool,_cpLightUp_Pool);
                _boarMiniPool.PutForRecycle(startNode.GameState);
                startNode.Clear();
                _nodeStatePool.PutForRecycle(startNode);

            } ;


            //Trace.WriteLine($"{iteration}");

            //Trace.WriteLine($"{_cpLightUp_Pool.GetBaseStat()}");


        }

        private bool IsWorthGenerateNextMoves(NodeState newNodeState, bool foundBetterScore)
        {
            //if (newNodeState.IsNotWorthGenNextMove) return false;

            if (_bestStateEvaluation.Val == int.MinValue) return true;

            //return foundBetterScore;

            return false;
            //return currentBest.StateEvaluation <= newNodeState.StateEvaluation;
            //-  (newNodeState.GetCardsLeftForPlay() * 10);
        }


        private bool WasFoundBetterScore(byte minLevel, byte level, EvaluateState<int, int> stateEval)
        {
            byte currBestLevel = _bestMoveSequence.MoveCount;
            EvaluateState<int, int> currBestStateEval = _bestStateEvaluation;

            if (currBestLevel == 0) return true;


            bool currBestUnderLevel = currBestLevel < minLevel;
            bool underLevel = level < minLevel;

            if (currBestUnderLevel && !underLevel) return true;

            if (currBestUnderLevel && underLevel)
            {
                if (currBestLevel < level) return true;
                else if (currBestLevel == level) return currBestStateEval.CompareTo(stateEval)<0;
                else return false;
            }

            if (!currBestUnderLevel && underLevel) return false;

            if (!currBestUnderLevel && !underLevel)
            {
                return currBestStateEval.CompareTo(stateEval) < 0;
            }

            throw new Exception("nesmi nastat");
        }

        private void UpdateBestState(EvaluateState<int,int> newStateEval)
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

            var ccihCount = currentCardsInHand.Count;

            var ccihCards = currentCardsInHand.Cards;
            for (ushort c = 0; c < ccihCount; c++)
            {
                
                byte card = ccihCards[c];

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


                var chunk = _cardsPlayedLookup_chunkCreator.GetChunk((ushort)tmpCanPlayed.HistoryCardsPlayed.Count);
                tmpCanPlayed.HistoryCardsPlayed.CopyTo(chunk, tmpCanPlayed.HistoryCardsPlayed.Count);
                

                if (_cardsPlayedLookup.Contains(chunk))
                {
                    _cardsPlayedPool.PutForRecycle(tmpCanPlayed);
                   
                    return 0;
                }


                BoardMini nextCurrentState = _boarMiniPool.GetNewOrRecycle();

                if (!nextCurrentState.WasInited())
                {
                    nextCurrentState.InitBy(currentState.GameState);
                }
                nextCurrentState.CopyFrom(currentState.GameState);
                nextCurrentState.ApplyCard((sbyte)i, card);


                NodeState startNode = _nodeStatePool.GetNewOrRecycle();
                startNode.GameState = nextCurrentState;
                //Remove_CardInHand(currentCardsInHand, card);
                //currentCardsInHand.Where(x => x != card).ToArray();
                startNode.CardsInHand = CreateCopy2(in currentState.CardsInHand,ref startNode.CardsInHand);
                startNode.CardsInHand.RemoveCard(card);
                startNode.CurrentMove = new Move((sbyte)i, card);

                startNode.Level = (byte)(currentState.Level + 1);
                startNode.CardPlayed = tmpCanPlayed;
                startNode.IsNotWorthGenNextMove = false;
                startNode.StateEvaluation = 
                    new EvaluateState<int, int>(
                        
                        
                    startNode.GameState.CountPossiblePlay()
                    ,
                    tmp(startNode.GameState, startNode.CardsInHand)
                    
                    );
                _cardsPlayedLookup.Add(chunk);


                if (currentState.GameState.CardPlaceholders[i].Get_CardDiff(card) > CONST_Treshold_CardDiff_NotGetMoves)
                    startNode.IsNotWorthGenNextMove = true;

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

        private CardsInHand CreateCopy(in CardsInHand cih)
        {
            CardsInHand result = new CardsInHand(cih.Count);
            result.Count = cih.Count;

            cih.Cards.CopyTo(result.Cards, cih.Count);
            //Buffer.BlockCopy(cih.Cards.Array, cih.Cards.Offset, result.Cards.Array, result.Cards.Offset, cih.Count);
            return result;
        }

        private CardsInHand CreateCopy2(in CardsInHand cih, ref CardsInHand cihNew)
        {

            CardsInHand result = (cihNew.Cards!= null && cihNew.Cards.Length >= cih.Count)?
                cihNew: new CardsInHand(cih.Count);
            result.Count = cih.Count;

            //cih.Cards.CopyTo(result.Cards,0, cih.Count);
            Buffer.BlockCopy(cih.Cards, 0, result.Cards, 0, cih.Count);
            //Buffer.BlockCopy(cih.Cards.Array, cih.Cards.Offset, result.Cards.Array, result.Cards.Offset, cih.Count);
            return result;
        }

        private int tmp(BoardMini gameState,in CardsInHand cardsInHand)
        {
            int countValidMoves = 0;

            //var currentPlaceholders = gameState.CardPlaceholders;

            var ccihCount = cardsInHand.Count;

            var ccihCards = cardsInHand.Cards;

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
        public readonly byte [] Cards;

        public byte Count;

        public CardsInHand(byte maxSize)
        {
            Count = 0;
            Cards = new byte[maxSize];
        }

        public void RemoveCard(byte card)
        {
            ushort index = 0;
            byte found = 0;
            for (byte i = 0; i < Count; i++)
            {
                var cardI = Cards[i];
                if (cardI == card)
                {
                    found++;
                }
                else
                {
                    Cards[index] = cardI;
                    index++;
                }
            }

            Count -= found;
        }


    }

    struct CardsInHand2
    {
        public readonly ArraySegmentExSmall_Struct<byte> Cards;

        public byte Count;

        public CardsInHand2(byte maxSize)
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
                ref var cardI = ref Cards.Get(i);
                if(cardI == card)
                {
                    found++;
                }
                else
                {
                    Cards[index] = cardI;
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

        private static ChunkArrayCreator chunkCreator = new ChunkArrayCreator();

        public CardsPlayed()
        {

            this.HistoryCardsPlayed = chunkCreator.GetChunk(CONST_MaxPlayedCards * CONST_CountDecks);
            HistoryCardsPlayedCounts = chunkCreator.GetChunk(CONST_CountDecks);
        }

        public void Clear()
        {
            for (int i = 0; i < HistoryCardsPlayed.Count; i++) {
                HistoryCardsPlayed[i] = 0;
            }

            for (int i = 0; i < HistoryCardsPlayedCounts.Count; i++)
            {
                HistoryCardsPlayedCounts[i] = 0;
            }
        }


        public void CopyTo(CardsPlayed dest)
        {
            this.HistoryCardsPlayed.CopyTo(dest.HistoryCardsPlayed);
            this.HistoryCardsPlayedCounts.CopyTo(dest.HistoryCardsPlayedCounts);

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
                CardsPlayed.CompareByteArray(in this.HistoryCardsPlayed, in other.HistoryCardsPlayed) == 0 &&
                CardsPlayed.CompareByteArray(in this.HistoryCardsPlayedCounts,in other.HistoryCardsPlayedCounts) == 0;
        }

        public void Set(int deck, byte card)
        {
            ushort indexStart = (ushort)(deck * CONST_MaxPlayedCards);

            this.HistoryCardsPlayed[(ushort)(indexStart + this.HistoryCardsPlayedCounts[(ushort)deck])] = card;
            this.HistoryCardsPlayedCounts[(ushort)deck]++;
        }

        public int CompareTo(CardsPlayed other)
        {
            int cmp = CardsPlayed.CompareByteArray(in this.HistoryCardsPlayed,in other.HistoryCardsPlayed);
            if (cmp != 0) return cmp;

            return CardsPlayed.CompareByteArray(in this.HistoryCardsPlayedCounts,in other.HistoryCardsPlayedCounts);

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

        public static int CompareByteArray(in ArraySegmentExSmall_Struct<byte> data,in ArraySegmentExSmall_Struct<byte> data2)
        {
            if (data.Count != data2.Count) return data.Count - data.Count;

            for (ushort i = 0; i < data.Count; i++)
            {
                ref byte d1 = ref data.Get(i);
                ref byte d2 = ref data2.Get(i);
                if (d1 != d2) return data[i] - data2[i];
            }

            return 0;
        }

        
    }


    class NodeState
    {
        public BoardMini GameState;
        public byte Level;
        public EvaluateState<int,int> StateEvaluation;
        public CardsPlayed CardPlayed;
        public CardsInHand CardsInHand;
        public Move CurrentMove;
        public bool IsNotWorthGenNextMove;

        public void Clear()
        {
            
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

        public void SetMove(byte level,in Move move)
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

        public void Clear()
        {
            MoveCount = 0;
        }
    }


    public readonly struct Move
    {
        public readonly sbyte Placeholder;
        public readonly byte Card;

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

    class ASESSComparer : IEqualityComparer<ArraySegmentExSmall_Struct<byte>>
    {
        public bool Equals(ArraySegmentExSmall_Struct<byte> x, ArraySegmentExSmall_Struct<byte> y)
        {
            return CardsPlayed.CompareByteArray(in x,in  y) == 0;
        }

        public int GetHashCode(ArraySegmentExSmall_Struct<byte> obj)
        {
            //uint tmp = (uint)HashDepot.XXHash.Hash64(this.HistoryCardsPlayed);
            ////uint tmp = HashDepot.XXHash.Hash32(state.Array.AsSpan(state.Offset, state.Count));
            //tmp &= 0x7fffffff;

            //return (int)tmp ;


            int sum = 0;
            for (ushort i = 0; i <
                obj.Count
                ; i++)
            {
                sum *= 31;
                sum += obj[i];
            }
            return sum;
        }
    }

    
}

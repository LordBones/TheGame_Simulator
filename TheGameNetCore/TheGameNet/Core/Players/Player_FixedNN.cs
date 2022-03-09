using BonesLib.FixedForwardNN;
using BonesLib.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    internal class Player_FixedNN : Player
    {
        private FixedForwardNN _fnn;

        private BoardMini _tmpBoardMini = null;

        private float _alphaLearning = 0.001f;
        private float _chooseRandomPolicy = 0.05f;
        private float _moment = 0.9f;
        private float _lastReward;
        private ArrayWrapper<float> _lastBestNNInputs;

        private HistoryItem[] _qHistoryMoves = new HistoryItem[100];
        private int _qHistoryMovesIndex = 0;

        public bool TeachingEnable = true;
        

        public FixedForwardNN Fnn { get => _fnn; set => _fnn = value; }

        struct HistoryItem
        {
            public ArrayWrapper<float> _NNInputs;
            public float Reward;
        }

        public Player_FixedNN()
        {
            Init();
        }

        public Player_FixedNN(string name) : base(name)
        {
            Init();
        }

        private void Init()
        {
            //_fnn = new FlexibleForwardNN(105, 98 * 4);
            //var topology = new short[] { 1 };
            //_fnn.Layers.Init(105, 98 * 4, topology.Sum(x => x) + 800, 30000);


            _fnn = new FixedForwardNN(9, 1);
            //var topology = new short[] { 60,30,20,10 };
            var topology = new short[] { 10,10,10, 5 };
            _fnn.SetTopology(topology);
            _fnn.InitBaseWeights();
            //_fnn.SetTopology(topology);

        }

        public override void StartGame(GameBoard board)
        {
            _qHistoryMovesIndex = 0;
            _tmpBoardMini = (_tmpBoardMini != null) ? board.CreateBoardMini(this.Id, _tmpBoardMini) : board.CreateBoardMini(this.Id);
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {
           
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {
            if (_lastBestNNInputs.Count == 0) return;
            if (!TeachingEnable) return;

            _qHistoryMoves[_qHistoryMovesIndex] = new HistoryItem() { Reward = _lastReward, _NNInputs=_lastBestNNInputs};
            _qHistoryMovesIndex++;
        }

        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {
            var boardMini = board.CreateBoardMini(this.Id, _tmpBoardMini);

            Span<MoveToPlay> flsa = stackalloc MoveToPlay[40];
            FixListSpan<MoveToPlay> possibleToPlay = new FixListSpan<MoveToPlay>(flsa);
            board.Get_PossibleToPlay(handCards, ref possibleToPlay);

            (float reward, ArrayWrapper<float> bestNNInputs, MoveToPlay move) bestAction = Get_BestAction_Best(boardMini,board.Count_AllRemaindPlayCards,ref possibleToPlay);

            _lastBestNNInputs = bestAction.bestNNInputs;
            _lastReward = bestAction.reward;

            return bestAction.move;
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {
            _lastBestNNInputs = new ArrayWrapper<float>();
            return new MoveToPlay(0, -1);
        }

        public override void EndGame(GameBoard board, Span<byte> handCards)
        {
            if (_lastBestNNInputs.Count == 0) return;
            if (!TeachingEnable) return;

            Span<float> rewards = stackalloc  float[1];
            float realReward =  (98 - (board.Count_AllRemaindPlayCards)) / 100.0f;
            rewards[0] = realReward;

            float coef = 1.0f;
            float coefStep =
                //0.9f;
                1.0f / (98 - (board.Count_AllRemaindPlayCards));

            for (int i = _qHistoryMovesIndex-1; i >=0;  i--)
            {
               // rewards[0] = (_qHistoryMovesIndex - i) / 100.0f;
                var move = _qHistoryMoves[i];

                move._NNInputs.AsSpan().CopyTo(_fnn.Inputs);
                _fnn.Evaluate();
                _fnn.BackPropagate(_alphaLearning, rewards,_moment, coef);
                coef-=coefStep;
                //coef *= coefStep;

            }

        }


        (float reward, ArrayWrapper<float> bestNNInputs, MoveToPlay move) Get_BestAction_Best(BoardMini boardMini,int countRemaindCards, ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            

            MoveToPlay moveBest = new MoveToPlay();
            ArrayWrapper<float> bestNNInputs = new ArrayWrapper<float>();

            ArrayWrapper<float> tmpArray = Helper_GetPooledArray<float>(_fnn.Inputs.Length);

            if (TeachingEnable && _chooseRandomPolicy > RandomGen.Default.GetRandomNumberDouble())
            {

                var indexPosToPlay = RandomGen.Default.GetRandomNumber(0, possibleToPlay.Length);

                PrepareFNNInputs(tmpArray.AsSpan(), boardMini, countRemaindCards, ref possibleToPlay[indexPosToPlay]);
                tmpArray.AsSpan().CopyTo(_fnn.Inputs);

                _fnn.Evaluate();
                float outputVal = _fnn.Outputs[0];

                moveBest = possibleToPlay[indexPosToPlay];
                bestNNInputs.FreePooledArray();
                bestNNInputs = tmpArray;
                tmpArray = Helper_GetPooledArray<float>(_fnn.Inputs.Length);
                rewardBest = outputVal;

            }
            else
            {
                //  Trace.Write($"{qGameStateIndex,7} - {possibleToPlay.Count,2} line: ");
                for (int i = 0; i < possibleToPlay.Length; i++)
                {
                    PrepareFNNInputs(tmpArray.AsSpan(), boardMini, countRemaindCards, ref possibleToPlay[i]);
                    tmpArray.AsSpan().CopyTo(_fnn.Inputs);

                    _fnn.Evaluate();
                    float outputVal = _fnn.Outputs[0];

                    if (rewardBest < outputVal)
                    {
                        moveBest = possibleToPlay[i];
                        bestNNInputs.FreePooledArray();
                        bestNNInputs = tmpArray;
                        tmpArray = Helper_GetPooledArray<float>(_fnn.Inputs.Length);
                        rewardBest = outputVal;
                    }
                }
            }


            return (reward: rewardBest, bestNNInputs: bestNNInputs, move: moveBest);
        }

        private void PrepareFNNInputs(Span<float> inputs, BoardMini boardMini,int countRemainsCards, ref MoveToPlay moveToPlay)
        {
            inputs.Clear();
            //inputs[0] = moveToPlay.Card / 100.0f;
            //inputs[1] = boardMini.CardPlaceholders[moveToPlay.DeckIndex].Get_TopCard() / 100.0f;
            //inputs[2] = (boardMini.CardPlaceholders[moveToPlay.DeckIndex].UpDirection) ? 1.0f : 0.01f;
            //inputs[3] = (boardMini.CardPlaceholders[moveToPlay.DeckIndex].UpDirection) ? 0.01f : 1.0f;
            //inputs[4] = boardMini.CountNeedPlayCard / 3.0f;
            //inputs[5] = countRemainsCards / 100.0f;

            inputs[0] = boardMini.CardPlaceholders[0].Get_TopCard() / 100.0f;
            inputs[1] = boardMini.CardPlaceholders[1].Get_TopCard() / 100.0f;
            inputs[2] = boardMini.CardPlaceholders[2].Get_TopCard() / 100.0f;
            inputs[3] = boardMini.CardPlaceholders[3].Get_TopCard() / 100.0f;
            

            inputs[4+ moveToPlay.DeckIndex] = moveToPlay.Card / 100.0f;
            inputs[8] = countRemainsCards / 100.0f;
        }

        private static ArrayWrapper<T> Helper_GetPooledArray<T>(int lenght)
        {
            return new ArrayWrapper<T>(ArrayPool<T>.Shared.Rent(lenght), lenght);
        }

        struct ArrayWrapper<T>
        {
            public T[] Data;
            public int Count;

            public Span<T> AsSpan()
            {
                return Data.AsSpan(0,Count);
            }

            public ArrayWrapper(T [] data, int count)
            {
                Data = data;
                Count = count;
            }

            public void FreePooledArray()
            {
                if (Count > 0 && Data != null)
                {
                    ArrayPool<T>.Shared.Return(Data);
                    Data = null;
                }
            }
        }
    }
}

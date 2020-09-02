using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.QLearning;
using TheGameNet.Utils;

namespace TheGameNet.Core.Players
{
    internal class Player_DoubleQLearning : Player
    {
        private QTable _qTableA;
        private QTable _qTableB;
        private int _switchCounter = 0;
        private QNode _lastQNode;
        private QLearningCompute _qLearningCompute = new QLearningCompute(0.3f, 0.5f);


        struct QNode
        {
            public int StateIndex;
            public int ActionIndex;

            public QNode(int stateIndex, int actionIndex)
            {
                StateIndex = stateIndex;
                ActionIndex = actionIndex;

            }

            public static QNode Default
            {
                get
                {
                    return new QNode(-1, -1);
                }
            }

            public bool IsDefault()
            {
                return StateIndex < 0;
            }

        }
        public Player_DoubleQLearning()
        {
            Init();
        }

        public Player_DoubleQLearning(string name) : base(name)
        {
            Init();
        }

        private void Init()
        {
            _qTableA = new QTable(2000-1, 399, 0.0f);
            _qTableB = new QTable(2000-1, 399, 0.0f);

            _lastQNode = new QNode(0, 0);// QNode.Default;
        }

        public void PrintQTableA(TextWriter tw)
        {
            _qTableA.PrintTable(tw);
        }

        public void PrintQTableB(TextWriter tw)
        {
            _qTableB.PrintTable(tw);
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {
            _lastQNode = new QNode(0, 0);
        }


        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {
            Swap_QTable();
            var boardMini = board.CreateBoardMini(this.Id);

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTableA, boardMini,board, handCards, this.Id);

            var currentBestMove = Get_QLearning_BestActionForPlay(boardMini, qGameStateIndex, possibleToPlay);


            if (currentBestMove.move.IsNotMove)
            {
                throw new Exception();
            }

            _lastQNode = new QNode(qGameStateIndex, currentBestMove.qActionIndex);

            return currentBestMove.move;
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {
            _lastQNode = QNode.Default;
            return new MoveToPlay(0, -1);
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {
            if (_lastQNode.IsDefault()) return;

            var boardMini = board.CreateBoardMini(this.Id);

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTableA, boardMini,board, handCards, this.Id);

            var currentBestMove = Get_QLearning_BestAction(boardMini, qGameStateIndex, possibleToPlay);


            float currentReward =  QLearning_CurrentReward(board,boardMini, handCards)-100;
            float qCurrentReward = _qTableA.Get(_lastQNode.StateIndex, _lastQNode.ActionIndex);

            float newReward = _qLearningCompute.Q_Compute(qCurrentReward, currentReward, currentBestMove.reward);

            _qTableA.Set(_lastQNode.StateIndex, _lastQNode.ActionIndex, newReward);
        }

        public override void EndGame(GameBoard board, Span<byte> handCards)
        {
            if (_lastQNode.IsDefault()) return;

            var boardMini = board.CreateBoardMini(this.Id);


            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTableA, boardMini,board, handCards, this.Id);

            float currentReward = -1.0f;// -board.Count_AllRemaindPlayCards; //QLearning_CurrentReward(board,handCards);
            float qCurrentReward = _qTableA.Get(_lastQNode.StateIndex, _lastQNode.ActionIndex);

            float newReward = _qLearningCompute.Q_Compute(qCurrentReward, currentReward, -1.0f);

            _qTableA.Set(_lastQNode.StateIndex, _lastQNode.ActionIndex, newReward);
        }


        private void Swap_QTable()
        {
            _switchCounter++;
            _switchCounter &= 0x3;
            if (_switchCounter == 0)
            {
                var tmp = _qTableA;
                _qTableA = _qTableB;
                _qTableB = tmp;
            }
            
        }

        private (int qActionIndex, MoveToPlay move) Get_QLearning_BestActionForPlay(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            MoveToPlay moveBest = new MoveToPlay();

            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTableA, boardMini, possibleToPlay[i]);

                if (!_qTableA.HasValue(qGameStateIndex, qActionIndex))
                {
                    qActionIndexBest = qActionIndex;
                    moveBest = possibleToPlay[i];

                    break;
                }

                float reward = _qTableB.Get(qGameStateIndex, qActionIndex);
                float reward2 = _qTableA.Get(qGameStateIndex, qActionIndex);

                if (reward + reward2 > rewardBest)
                {
                    rewardBest = reward + reward2;
                    qActionIndexBest = qActionIndex;
                    moveBest = possibleToPlay[i];
                }
            }

            if (rewardBest == float.MinValue) rewardBest = 0;

            return ( qActionIndex: qActionIndexBest, move: moveBest);
        }

        private (float reward, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            MoveToPlay moveBest = new MoveToPlay();

            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTableB, boardMini, possibleToPlay[i]);

                float reward = _qTableB.Get(qGameStateIndex, qActionIndex);

                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                    moveBest = possibleToPlay[i];
                }
            }

            if (rewardBest == float.MinValue) rewardBest = 0;

            return (reward: rewardBest, qActionIndex: qActionIndexBest, move: moveBest);
        }

        private int QLearning_CurrentReward(GameBoard board, BoardMini boardMini, Span<byte> handCards)
        {
            int sum = 0;
            //foreach (var item in boardMini.CardPlaceholders)
            //{
            //    sum += item.Get_CardDiff_ToEnd(item.Get_TopCard());
            //}
            //return sum;
            return (((98 - board.Count_AllRemaindPlayCards)));
        }


       
    }
}

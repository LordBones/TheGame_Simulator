using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.GameBoardMini;
using TheGameNet.Core.QLearning;
using TheGameNet.Utils;

namespace TheGameNet.Core.Players
{
    internal class Player_QLearning : Player
    {
        private QTable _qTable;
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
        public Player_QLearning()
        {
            Init();
        }

        public Player_QLearning(string name) : base(name)
        {
            Init();
        }

        private void Init()
        {
            _qTable = new QTable(1023, 399);
            _lastQNode = new QNode(0, 0);// QNode.Default;
        }

        public override void StartPlay(GameBoard board, List<byte> handCards)
        {
            _lastQNode = new QNode(0, 0);
        }


        public override MoveToPlay Decision_CardToPlay(GameBoard board, List<byte> handCards)
        {
            var boardMini = board.CreateBoardMini(this.Id);

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini, handCards);

            var currentBestMove = Get_QLearning_BestAction(boardMini, qGameStateIndex, possibleToPlay);


            if (currentBestMove.move.IsNotMove)
            {
                throw new Exception();
            }

            _lastQNode = new QNode(qGameStateIndex, currentBestMove.qActionIndex);

            return currentBestMove.move;
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, List<byte> handCards)
        {
            _lastQNode = QNode.Default;
            return new MoveToPlay(0, -1);
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, List<byte> handCards, bool isEndOfGame)
        {
            if (_lastQNode.IsDefault()) return;

            var boardMini = board.CreateBoardMini(this.Id);

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini, handCards);

            //var currentBestMove = Get_QLearning_BestAction(boardMini, qGameStateIndex, possibleToPlay);

            float featureReward = Get_QLearning_FeatureReward(boardMini, qGameStateIndex, possibleToPlay);

            float currentReward = 0;// QLearning_CurrentReward(board, handCards)/10;
            float qCurrentReward = _qTable.Get(_lastQNode.StateIndex, _lastQNode.ActionIndex);

            float newReward = _qLearningCompute.Q_Compute(qCurrentReward, currentReward, featureReward);

            _qTable.Set(_lastQNode.StateIndex, _lastQNode.ActionIndex,newReward);
        }

        public void PrintQTable(TextWriter tw)
        {
            _qTable.PrintTable(tw);
        }

        public override void EndGame(GameBoard board, List<byte> handCards)
        {
            if (_lastQNode.IsDefault()) return;

            var boardMini = board.CreateBoardMini(this.Id);


            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini, handCards);

            float currentReward = 
                -board.Count_AllRemaindPlayCards;
             //QLearning_CurrentReward(board, handCards); 
            float qCurrentReward = _qTable.Get(_lastQNode.StateIndex, _lastQNode.ActionIndex);

            float newReward = _qLearningCompute.Q_Compute(qCurrentReward, currentReward, 0.0f);

            _qTable.Set(_lastQNode.StateIndex, _lastQNode.ActionIndex, currentReward);
        }



        private (float reward, int qActionIndex, MoveToPlay move)  Get_QLearning_BestAction(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            MoveToPlay moveBest = new MoveToPlay();

            for(int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, possibleToPlay[i]);

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);

                if(reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                    moveBest = possibleToPlay[i];
                }
            }

            if (rewardBest == float.MinValue) rewardBest = 0;

            return (reward: rewardBest, qActionIndex: qActionIndexBest, move:moveBest);

        }

        private float Get_QLearning_FeatureReward(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;

            float rewardSum = 0.0f;
            int rewardCount = 0;
            int qActionIndexBest = 0;
            

            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, possibleToPlay[i]);

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);

                rewardSum += reward;
                rewardCount++;

                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                }
            }



            if (rewardBest == float.MinValue) rewardBest = 0;

            //if(rewardCount > 0)
            //{
            //    rewardBest = rewardSum / rewardCount;
            //}

            return rewardBest;
        }

        private int QLearning_CurrentReward(GameBoard board, List<byte> handCards)
        {

            return  ((98 - board.Count_AllRemaindPlayCards));
        }


        
    }
}

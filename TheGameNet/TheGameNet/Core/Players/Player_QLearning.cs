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
        private QLearningCompute _qLearningCompute = new QLearningCompute(0.01f, 0.2f);
        private float _qExplorationPolicy = 0.1f;


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
            _qTable = new QTable(2000-1, 399);
            _lastQNode = new QNode(0, 0);// QNode.Default;
        }

        public override void StartPlay(GameBoard board, List<byte> handCards)
        {
            _lastQNode = new QNode(0, 0);
        }

        private int _LastState_tmpCantPlayCards = 0;
        public override MoveToPlay Decision_CardToPlay(GameBoard board, List<byte> handCards)
        {
            var boardMini = board.CreateBoardMini(this.Id);

            List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);

            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini,board, handCards);

            var currentBestMove = Get_QLearning_BestAction(boardMini, qGameStateIndex, possibleToPlay);


            if (currentBestMove.move.IsNotMove)
            {
                throw new Exception();
            }

            _lastQNode = new QNode(qGameStateIndex, currentBestMove.qActionIndex);


            // old can play cards
            //int countCanPlayCard = Helper_GetCountPlayCards(boardMini, handCards);

            _LastState_tmpCantPlayCards = board.Count_AllRemaindPlayCards;
                
                //board.CardPlaceholders[currentBestMove.move.DeckIndex].Get_CardDiff(currentBestMove.move.Card)
                //(handCards.Count - countCanPlayCard);
                ;

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

            int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini,board, handCards);

            //var currentBestMove = Get_QLearning_BestAction(boardMini, qGameStateIndex, possibleToPlay);

            float featureReward = Get_QLearning_FeatureReward(boardMini, qGameStateIndex, possibleToPlay);


            float currentReward = (QLearning_CurrentReward(board,boardMini, handCards));
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

            //var boardMini = board.CreateBoardMini(this.Id);


            //int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini,board, handCards);

            float currentReward = -1;//-(board.Count_AllRemaindPlayCards)*1000000;
             
             
            float qCurrentReward = _qTable.Get(_lastQNode.StateIndex, _lastQNode.ActionIndex);



            float newReward = //currentReward;
                _qLearningCompute.Q_Compute(qCurrentReward, 
                currentReward, 
                currentReward);

            _qTable.Set(_lastQNode.StateIndex, _lastQNode.ActionIndex, newReward);

        }



        private (float reward, int qActionIndex, MoveToPlay move)  Get_QLearning_BestAction(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            MoveToPlay moveBest = new MoveToPlay();

            if (_qExplorationPolicy > RandomGen.GetRandomNumberDouble())
            {
                int possibleToPlayIndex = RandomGen.GetRandomNumber(0, possibleToPlay.Count);

                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, possibleToPlay[possibleToPlayIndex]);

                qActionIndexBest = qActionIndex;
                moveBest = possibleToPlay[possibleToPlayIndex];

            }
            else
            {
                //(float reward, int qActionIndex, MoveToPlay move) kk = Get_QLearning_BestAction_Best(boardMini, qActionIndexBest, possibleToPlay);
                (float reward, int qActionIndex, MoveToPlay move) kk = Get_QLearning_BestAction_Better(boardMini, qActionIndexBest, possibleToPlay);

                rewardBest = kk.reward;
                qActionIndexBest = kk.qActionIndex;
                moveBest = kk.move;
            }

            //if (rewardBest == float.MinValue) rewardBest = 0;

            return (reward: rewardBest, qActionIndex: qActionIndexBest, move:moveBest);

        }

        (float reward, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction_Best(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            MoveToPlay moveBest = new MoveToPlay();

            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, possibleToPlay[i]);

                if (!_qTable.HasValue(qGameStateIndex, qActionIndex))
                {
                    qActionIndexBest = qActionIndex;
                    moveBest = possibleToPlay[i];

                    break;
                }

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);

                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                    moveBest = possibleToPlay[i];
                }
            }

            return (reward: rewardBest, qActionIndex: qActionIndexBest, move: moveBest);
        }

        (float reward, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction_Better(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
        
            List<(int qai, float reward, MoveToPlay move)> possibleActions = new List<(int qai, float reward, MoveToPlay move)>();

            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, possibleToPlay[i]);

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);

                possibleActions.Add((qActionIndex, reward, possibleToPlay[i]));

                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                }
            }


            float limit = rewardBest -1.00f;

            List<int> pa_Index = new List<int>();

            if (possibleActions.Count == 1)
            {
                pa_Index.Add(0);
            }
            else
            {
                for (int i = 0; i < possibleActions.Count; i++)
                {
                    float reward = possibleActions[i].reward;
                    if (reward > limit || reward == 0.0) pa_Index.Add(i);
                }
            }



            int pa_IndexRand = RandomGen.GetRandomNumber(0, pa_Index.Count);


            var resultPA = possibleActions[pa_Index[pa_IndexRand]];



            return (reward: resultPA.reward, qActionIndex: resultPA.qai, move: resultPA.move);
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



            if (rewardBest == float.MinValue) rewardBest = 0.0f;

            //if(rewardCount > 0)
            //{
            //    rewardBest = rewardSum / rewardCount;
            //}

            return rewardBest;
        }

        private float Get_QLearning_FeatureReward2(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            if (possibleToPlay.Count == 0) return -1.0f;

            float rewardBest = float.MinValue;

            int qActionIndexBest = 0;

            List<(int ptp, float reward)> possibleActions = new List<(int ptp, float reward)>();



            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, possibleToPlay[i]);

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);

                possibleActions.Add((qActionIndex, reward));

                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                }
            }


            float limit = rewardBest * 0.9f;

            List<int> pa_Index = new List<int>();
            for(int i = 0; i < possibleActions.Count; i++)
            {
                float reward = possibleActions[i].reward;
                if (  reward > limit || reward == 0.0) pa_Index.Add(i);
            }

            

            int pa_IndexRand = RandomGen.GetRandomNumber(0, pa_Index.Count);

            rewardBest = possibleActions[pa_Index[pa_IndexRand]].reward;

          
            //if(rewardCount > 0)
            //{
            //    rewardBest = rewardSum / rewardCount;
            //}

            return rewardBest;
        }

        private float QLearning_CurrentReward(GameBoard board, BoardMini boardMini, List<byte> handCards)
        {
            float fSum = 0.0f;
            //foreach (var item in boardMini.CardPlaceholders)
            //{
            //    fSum += item.Get_CardDiff_ToEnd(item.Get_TopCard());
            //}


            //int countCanPlayCard = Helper_GetCountPlayCards(boardMini,handCards);

            //fSum += countCanPlayCard+(board.MaxCardInHands-handCards.Count);

            fSum +=  ( 98- board.Count_AllRemaindPlayCards);// board.Count_AllRemaindPlayCards ;

//                - ((handCards.Count - countCanPlayCard) - _LastState_tmpCantPlayCards ) ;// + (board.MaxCardInHands - handCards.Count);

            //int[] tmpArray = new int[100];
            //float fIncrement = 1.0f;

            //foreach (var item in boardMini.CardPlaceholders)
            //{
            //    int countToEnd = item.Get_CardDiff_ToEnd(item.Get_TopCard());
            //    for (int i = 0; i < countToEnd; i++)
            //    {
            //        tmpArray[i] ++;
            //    }


            //}

            //for (int i = 0; i < tmpArray.Length; i++)
            //{
            //    int countCard = tmpArray[i];
            //    float tmpIncrement = fIncrement;
            //    for (int c = 0; c < countCard; c++)
            //    {
            //        fSum += tmpIncrement;
            //        tmpIncrement /= 10.0f;
            //    }

            //}

            return fSum;
            //return sum;
            //return board.  ((98 - board.Count_AllRemaindPlayCards));
        }

        private int Helper_GetCountPlayCards(BoardMini boardMini, List<byte> handCards)
        {
            int countCanPlayCard = 0;
            foreach (var card in handCards)
            {
                foreach (var placeholder in boardMini.CardPlaceholders)
                {
                    if (placeholder.CanPlaceCard(card))
                    {
                        countCanPlayCard++;
                        break;
                    }
                }
            }

            return countCanPlayCard;
        }
        
    }
}

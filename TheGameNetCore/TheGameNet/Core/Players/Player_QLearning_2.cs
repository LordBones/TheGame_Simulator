using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.QLearning;

namespace TheGameNet.Core.Players
{
    internal class Player_QLearning_2 : Player
    {
        private QTable _qTable;
        private QNode _lastQNode;
        private QLearningCompute _qLearningCompute = new QLearningCompute(0.09f, 0.8f);
        private float _qExplorationPolicy = 0.001f;


        private QTHistoryItem[] _qHistoryMoves = new QTHistoryItem[100];
        private int _qHistoryMovesIndex = 0;

        public bool TeachingEnable = true;

        public QTable Get_QTable => _qTable;
        public void Set_QTable(QTable qt) { _qTable = qt; }


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

        struct QTHistoryItem
        {
            public QNode qNode;
            public float FutureExpectedReward;
            public bool IgnoreFutureReward;
        }

        public Player_QLearning_2()
        {
            Init();
        }

        public Player_QLearning_2(string name) : base(name)
        {
            Init();
        }

        private void Init()
        {
            _qTable = new QTable(1022 - 1, 200, 0.0f);
            _lastQNode = new QNode(0, 0);// QNode.Default;
        }

        public override void StartGame(GameBoard board)
        {
            _qHistoryMovesIndex = 0;
            _tmpBoardMini = (_tmpBoardMini != null) ? board.CreateBoardMini(this.Id, _tmpBoardMini) : board.CreateBoardMini(this.Id);
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {
            _lastQNode = new QNode(0, 0);

        }

        private int _LastState_tmpCantPlayCards = 0;
        private BoardMini _tmpBoardMini = null;
        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {
            var boardMini = board.CreateBoardMini(this.Id, _tmpBoardMini);

            Span<MoveToPlay> flsa = stackalloc MoveToPlay[40];
            FixListSpan<MoveToPlay> possibleToPlay = new FixListSpan<MoveToPlay>(flsa);
            board.Get_PossibleToPlay(handCards, ref possibleToPlay);

            var currentBestMove = Get_QLearning_BestAction(boardMini, board, handCards, this.Id, ref possibleToPlay);
            //var currentBestMove = Get_QLearning_BestAction_Proportional(boardMini, qGameStateIndex, possibleToPlay);


            if (currentBestMove.move.IsNotMove)
            {
                throw new Exception();
            }

            _lastQNode = new QNode(currentBestMove.qGameStateIndex, currentBestMove.qActionIndex);


            //  if (!_qTable.HasValue(qGameStateIndex, currentBestMove.qActionIndex))
            //      _qTable.Set(qGameStateIndex, currentBestMove.qActionIndex, 0.0f);

            // old can play cards
            //int countCanPlayCard = Helper_GetCountPlayCards(boardMini, handCards);

            _LastState_tmpCantPlayCards = board.Count_AllRemaindPlayCards;

            //board.CardPlaceholders[currentBestMove.move.DeckIndex].Get_CardDiff(currentBestMove.move.Card)
            //(handCards.Count - countCanPlayCard);
            ;

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
            if (!TeachingEnable) return;

            var boardMini = board.CreateBoardMini(this.Id, _tmpBoardMini);
            //Span<MoveToPlay> possibleToPlay = stackalloc MoveToPlay[40];
            Span<MoveToPlay> flsa = stackalloc MoveToPlay[40];
            FixListSpan<MoveToPlay> possibleToPlay = new FixListSpan<MoveToPlay>(flsa);

            board.Get_PossibleToPlay(handCards, ref possibleToPlay);

            //List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);
          
            //var currentBestMove = Get_QLearning_BestAction(boardMini, qGameStateIndex, possibleToPlay);

            float featureReward = 0.0f;
            bool ignoreFeatureReward = false;
            if (board.Get_PlayerBoardData(Id).CountNeedPlayCard > 0)
            {
                featureReward = Get_QLearning_FeatureReward_Highest(boardMini, board, handCards, this.Id, possibleToPlay.GetSpan());
            }
            else
            {
                ignoreFeatureReward = true;
            }


            //float featureReward = Get_QLearning_FeatureReward(boardMini, qGameStateIndex, possibleToPlay.Slice(0, possibleToPlayCount));


            var qthi = new QTHistoryItem();
            qthi.qNode = _lastQNode;
            qthi.FutureExpectedReward = featureReward;
            qthi.IgnoreFutureReward = ignoreFeatureReward;

            _qHistoryMoves[_qHistoryMovesIndex] = qthi;
            _qHistoryMovesIndex++;

            return;

        }

        public void PrintQTable(TextWriter tw)
        {
            _qTable.PrintTable(tw);
        }

        public override void EndGame(GameBoard board, Span<byte> handCards)
        {
            if (_qHistoryMovesIndex == 0) return;
            if (!TeachingEnable) return;

            var qhmi = _qHistoryMoves[_qHistoryMovesIndex - 1];

            float qCurrentReward = 0.0f;
            float newReward = 0.0f;

            int counter = 200;
            //for (int index = _qHistoryMovesIndex - 2; index >= 0; index--)
            for (int index =0; index < _qHistoryMovesIndex - 1; index++)
            {
                if (counter == 0)
                    break;

                counter--;
                qhmi = _qHistoryMoves[index];

                qCurrentReward = _qTable.Get(qhmi.qNode.StateIndex, qhmi.qNode.ActionIndex);

                float fer =  _qTable.Get(_qHistoryMoves[index+1].qNode.StateIndex, _qHistoryMoves[index+1].qNode.ActionIndex);

                newReward = //currentReward;
                    _qLearningCompute.Q_Compute(qCurrentReward,
                    0.000f,
                    //0.01f  
                    //(index/1000.0f),

                    fer
                    );

                _qTable.Set(qhmi.qNode.StateIndex, qhmi.qNode.ActionIndex, newReward);

            }

            qhmi = _qHistoryMoves[_qHistoryMovesIndex - 1];

            float currentReward = -1;//
                                     //   -board.Count_AllRemaindPlayCards;
                                     //(98 - (board.Count_AllRemaindPlayCards));

            qCurrentReward = _qTable.Get(qhmi.qNode.StateIndex, qhmi.qNode.ActionIndex);

            newReward = //currentReward;
                _qLearningCompute.Q_Compute(qCurrentReward,
                currentReward,
                currentReward);

            _qTable.Set(qhmi.qNode.StateIndex, qhmi.qNode.ActionIndex,
                newReward

                );


           // Print_HistoryQMoves();
        }

        private void Print_HistoryQMoves()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _qHistoryMovesIndex; i++)
            {
                var tmp = _qHistoryMoves[i];
                float fer = (i+1 <  _qHistoryMovesIndex )? _qTable.Get(_qHistoryMoves[i + 1].qNode.StateIndex, _qHistoryMoves[i + 1].qNode.ActionIndex) : 0.0f;
                sb.AppendFormat("|[{0,4}-{1,4}] v: {2,7:###0.000} f{3,7:###0.000}", tmp.qNode.StateIndex.ToString(), tmp.qNode.ActionIndex.ToString(), _qTable.Get(tmp.qNode.StateIndex, tmp.qNode.ActionIndex), fer);
            }

            Trace.WriteLine(sb.ToString());
        }





        private (float reward, int qGameStateIndex, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction(BoardMini boardMini, GameBoard board, ReadOnlySpan<byte> handCards, byte playerId, ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            int qGameStateIndexBest = 0;
            MoveToPlay moveBest = new MoveToPlay();

            if (TeachingEnable && _qExplorationPolicy > RandomGen.Default.GetRandomNumberDouble())
            {
#warning je treba generovat nahodne
                byte topCard = boardMini.CardPlaceholders[RandomGen.Default.GetRandomNumber(0, 4)].Get_TopCard();
                int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, topCard, boardMini, board, handCards, this.Id);

                //return Get_QLearning_BestAction_Proportional(boardMini, qGameStateIndex,ref possibleToPlay);
                int possibleToPlayIndex = RandomGen.Default.GetRandomNumber(0, possibleToPlay.Length);

                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex3(_qTable, boardMini, ref possibleToPlay[possibleToPlayIndex]);

                qActionIndexBest = qActionIndex;
                qGameStateIndexBest = qGameStateIndex;
                moveBest = possibleToPlay[possibleToPlayIndex];

            }
            else
            {
                (float reward, int qGameStateIndex, int qActionIndex, MoveToPlay move) kk = Get_QLearning_BestAction_Best(boardMini, board,handCards, playerId, ref possibleToPlay);
                //(float reward, int qActionIndex, MoveToPlay move) kk = Get_QLearning_BestAction_Better(boardMini, qActionIndexBest, possibleToPlay);
                
                rewardBest = kk.reward;
                qGameStateIndexBest = kk.qGameStateIndex;
                qActionIndexBest = kk.qActionIndex;
                moveBest = kk.move;
            }

            //if (rewardBest == float.MinValue) rewardBest = 0;

            return (reward: rewardBest, qGameStateIndex:qGameStateIndexBest, qActionIndex: qActionIndexBest, move: moveBest);

        }

        (float reward,int qGameStateIndex, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction_Best(BoardMini boardMini, GameBoard board, ReadOnlySpan<byte> handCards, byte playerId, ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;
            int qGameStateIndexBest = 0;

            MoveToPlay moveBest = new MoveToPlay();
            //  Trace.Write($"{qGameStateIndex,7} - {possibleToPlay.Count,2} line: ");
            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini.CardPlaceholders[ possibleToPlay[i].DeckIndex].Get_TopCard(), boardMini, board, handCards, this.Id);


                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex3(_qTable, boardMini, ref possibleToPlay[i]);

                if (!_qTable.HasValue(qGameStateIndex, qActionIndex))
                {
                    qActionIndexBest = qActionIndex;
                    qGameStateIndexBest = qGameStateIndex;
                    moveBest = possibleToPlay[i];
                    rewardBest = 0.0f;

                    break;
                }

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);


                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                    qGameStateIndexBest = qGameStateIndex;
                    moveBest = possibleToPlay[i];
                }
            }


            return (reward: rewardBest, qGameStateIndex: qGameStateIndexBest, qActionIndex: qActionIndexBest, move: moveBest);
        }


        (float reward, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction_Proportional(BoardMini boardMini, int qGameStateIndex, ref FixListSpan<MoveToPlay> possibleToPlay)
        {

            Span<int> qActionIndex = stackalloc int[possibleToPlay.Length];
            Span<float> actionValues = stackalloc float[possibleToPlay.Length];
            Span<int> normalizedAValues = stackalloc int[possibleToPlay.Length];

            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                int qai = qActionIndex[i] = QLearning_HashTransform.QLearning_ActionIndex3(_qTable, boardMini, ref possibleToPlay[i]);
                var tmp = _qTable.Get_detectIfExist(qGameStateIndex, qai);

                if (tmp.empty)
                    actionValues[i] = 200;
                else
                    actionValues[i] = tmp.val;
            }

            float max = float.MinValue, min = float.MaxValue;

            for (int i = 0; i < actionValues.Length; i++)
            {
                float tmp = actionValues[i];
                if (max < tmp)
                    max = tmp;
                if (min > tmp)
                    min = tmp;
            }

            // musi byt pro pripad ze hodnota je 0
            max += 1.0f;

            // normalize values
            int normalizeRange = 100;
            int normalizeAccumulate = 0;
            for (int i = 0; i < actionValues.Length; i++)
            {
                normalizedAValues[i] = (int)((actionValues[i] / max) * normalizeRange + 1);
                normalizeAccumulate += normalizedAValues[i];
            }


            // choose value
            int valueOfState = RandomGen.Default.GetRandomNumber(0, normalizeAccumulate);
            int tmpNormalizeAccumulate = valueOfState;
            int indexFound = -1;
            for (int i = 0; i < normalizedAValues.Length; i++)
            {
                if (normalizedAValues[i] >= tmpNormalizeAccumulate)
                {
                    indexFound = i;
                    break;
                }

                tmpNormalizeAccumulate -= normalizedAValues[i];
            }

            float rewardBest = actionValues[indexFound];
            int qActionIndexBest = qActionIndex[indexFound];
            var moveBest = possibleToPlay[indexFound];

            return (reward: rewardBest, qActionIndex: qActionIndexBest, move: moveBest);
        }

        (float reward, int qActionIndex, MoveToPlay move) Get_QLearning_BestAction_Better3(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            float rewardBest = float.MinValue;
            int qActionIndexBest = 0;

            List<(int qai, float reward, MoveToPlay move)> possibleActions = new List<(int qai, float reward, MoveToPlay move)>(possibleToPlay.Count);

            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                var mtp = possibleToPlay[i];
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex(_qTable, boardMini, ref mtp);

                float reward = _qTable.Get(qGameStateIndex, qActionIndex);

                possibleActions.Add((qActionIndex, reward, possibleToPlay[i]));

                if (reward > rewardBest)
                {
                    rewardBest = reward;
                    qActionIndexBest = qActionIndex;
                }
            }


            float limit = rewardBest - 1.00f;

            Span<int> pa_Index = stackalloc int[possibleActions.Count];
            int pa_Index_index = 0;

            if (possibleActions.Count == 1)
            {
                pa_Index[0] = 0;
                pa_Index_index++;
            }
            else
            {
                for (int i = 0; i < possibleActions.Count; i++)
                {
                    float reward = possibleActions[i].reward;
                    if (reward > limit || reward == 0.0)
                    {
                        pa_Index[pa_Index_index] = i;
                        pa_Index_index++;
                    }
                }
            }



            int pa_IndexRand = RandomGen.Default.GetRandomNumber(0, pa_Index_index);


            var resultPA = possibleActions[pa_Index[pa_IndexRand]];



            return (reward: resultPA.reward, qActionIndex: resultPA.qai, move: resultPA.move);
        }

        private float Get_QLearning_FeatureReward_Highest(BoardMini boardMini,int qGameStateIndex, Span<MoveToPlay> possibleToPlay)
        {
            Span<int> qActions = stackalloc int[possibleToPlay.Length];

            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex3(_qTable, boardMini, ref possibleToPlay[i]);

                qActions[i] = qActionIndex;
            }

            var result = _qTable.Get_Highest(qGameStateIndex, qActions);

            if (result.val == float.MinValue)
                return 0.0f;

            return result.val;
        }

        private float Get_QLearning_FeatureReward_Highest(BoardMini boardMini, GameBoard board, ReadOnlySpan<byte> handCards, byte playerId, Span<MoveToPlay> possibleToPlay)
        {
            float resultVal = float.MinValue;

            

            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                int qGameStateIndex = QLearning_HashTransform.QLearning_StateIndex(_qTable, boardMini.CardPlaceholders[possibleToPlay[i].DeckIndex].Get_TopCard(), boardMini, board, handCards, this.Id);

                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex3(_qTable, boardMini, ref possibleToPlay[i]);

                var qVal = _qTable.Get(qGameStateIndex, qActionIndex);

                if (qVal > resultVal)
                    resultVal = qVal;
                
            }

            
            if (resultVal == float.MinValue)
                return 0.0f;

            return resultVal;
        }


        private float Get_QLearning_FeatureReward2(BoardMini boardMini, int qGameStateIndex, List<MoveToPlay> possibleToPlay)
        {
            if (possibleToPlay.Count == 0) return -1.0f;

            float rewardBest = float.MinValue;

            int qActionIndexBest = 0;

            List<(int ptp, float reward)> possibleActions = new List<(int ptp, float reward)>();



            for (int i = 0; i < possibleToPlay.Count; i++)
            {
                var mtp = possibleToPlay[i];
                int qActionIndex = QLearning_HashTransform.QLearning_ActionIndex3(_qTable, boardMini, ref mtp);

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
            for (int i = 0; i < possibleActions.Count; i++)
            {
                float reward = possibleActions[i].reward;
                if (reward > limit || reward == 0.0) pa_Index.Add(i);
            }



            int pa_IndexRand = RandomGen.Default.GetRandomNumber(0, pa_Index.Count);

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
            //fSum += 100;
            //fSum += countCanPlayCard;

            return fSum;

        }

        private int Helper_GetCountPlayCards(BoardMini boardMini, List<byte> handCards)
        {
            int countCanPlayCard = 0;
            for (int i = 0; i < handCards.Count; i++)
            {
                var card = handCards[i];
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

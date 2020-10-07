using BonesLib.FlexibleForwardNN;
using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    public class Player_FlexibleFNN : Player
    {
        private FlexibleForwardNN _fnn;

        private BoardMini _tmpBoardMini = null;

        public FlexibleForwardNN Fnn { get => _fnn; set => _fnn = value; }

        public Player_FlexibleFNN()
        {
            Init();
        }

        public Player_FlexibleFNN(string name) : base(name)
        {
            Init();
        }

        private void Init()
        {
            _fnn = new FlexibleForwardNN(105, 98 * 4);
            var topology = new short[] { 1 };
            _fnn.Layers.Init(105, 98 * 4, topology.Sum(x => x) + 800, 30000);
            //_fnn.SetTopology(topology);

            FlexibleFNN_LayerManipulator flm = new FlexibleFNN_LayerManipulator(0);
            flm.InitRandomTopology(_fnn.Layers);

        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {
            //    _tmpBoardMini = (_tmpBoardMini != null) ? board.CreateBoardMini(this.Id, _tmpBoardMini) : board.CreateBoardMini(this.Id);

        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {

        }

        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {
            //Span<MoveToPlay> flsa = stackalloc MoveToPlay[40];
            //FixListSpan<MoveToPlay> possibleToPlay = new FixListSpan<MoveToPlay>(flsa);
            //board.Get_PossibleToPlay(handCards, ref possibleToPlay);

            FillNetInputs(board, handCards, this.Id);
             _fnn.Evaluate();

            MoveToPlay resultMove //resultMove; 
            = GetNetOutput_Decision(board,handCards);

            //if (resultMove.IsNotMove)
            //{
            //    resultMove = GetWorse_Decision(board, ref possibleToPlay);
            //}

            if (resultMove.IsNotMove)
            {
                //resultMove = GetBest_Decision(board, handCards);
                //resultMove = GetBest_Decision2(board, ref possibleToPlay);
            }

            return resultMove;
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {
            return new MoveToPlay(0, -1);
        }



        public override void EndGame(GameBoard board, Span<byte> handCards)
        {

        }

        private void FillNetInputs2(GameBoard board, Span<byte> handCards, byte playerId)
        {
            var inputs = _fnn.Inputs;
            inputs.Fill(0.0f);

            inputs[0] = board.CardPlaceholders[0].Get_TopCard() / 100.0f;
            inputs[1] = board.CardPlaceholders[1].Get_TopCard() / 100.0f;
            inputs[2] = board.CardPlaceholders[2].Get_TopCard() / 100.0f;
            inputs[3] = board.CardPlaceholders[3].Get_TopCard() / 100.0f;


            //inputs[4] = (byte)board.Get_PlayerBoardData(playerId).CountNeedPlayCard / 2.0f;
            int index = 5;
            int handLength = handCards.Length;
            for (int i = 0; i < handLength; i++)
            {
                inputs[handCards[i] + index] = 1.0f;
            }
        }

        private void FillNetInputs(GameBoard board, Span<byte> handCards, byte playerId)
        {
            var inputs = _fnn.Inputs;
            inputs.Fill(0.0f);

            inputs[0] = board.Get_PH_ULight(0).Get_TopCard() / 100.0f;
            inputs[1] = board.Get_PH_ULight(1).Get_TopCard() / 100.0f;
            inputs[2] = board.Get_PH_ULight(2).Get_TopCard() / 100.0f;
            inputs[3] = board.Get_PH_ULight(3).Get_TopCard() / 100.0f;


            inputs[4] = (byte)board.Get_PlayerBoardData(playerId).CountNeedPlayCard / 2.0f;
            int index = 5;
            int handLength = handCards.Length;
            for (int i = 0; i < handLength; i++)
            {
                inputs[handCards[i] + index] = 1.0f;
            }
        }

        private MoveToPlay GetNetOutput_Decision(GameBoard board, Span<byte> handCards)
        {
            MoveToPlay result = new MoveToPlay(0, -1);
            float bestValue = float.MinValue;
                //float.Epsilon;
            var fnnOutputs = _fnn.Outputs;
            //if(_fnn.Layers.NeuronLinks.Length == 0)
            //{
            //    int i = 0;
            //}

            int lengthHand = handCards.Length;
            for (int i = 0; i < lengthHand; i++)
            {
                var card = handCards[i];

                if (board.Get_PH_ULight(0).CanPlaceCard(card))
                {
                    int index = 0 * 98 + card - 2;

                    float valCoef = fnnOutputs[index];
                    //if (valCoef <= float.Epsilon)
                    //    valCoef = 0;

                    if (valCoef > float.Epsilon &&  bestValue < valCoef)
                    {
                        result = new MoveToPlay(card, 0);
                        bestValue = valCoef;
                    }
                }

                if (board.Get_PH_ULight(1).CanPlaceCard(card))
                {
                    int index = 1 * 98 + card - 2;

                    float valCoef = fnnOutputs[index];
                    //if (valCoef < 0.0f)
                    //    valCoef = 0;

                    if (valCoef > float.Epsilon && bestValue < valCoef)
                    {
                        result = new MoveToPlay(card, 1);
                        bestValue = valCoef;
                    }
                }

                if (board.Get_PH_ULight(2).CanPlaceCard(card))
                {
                    int index = 2 * 98 + card - 2;

                    float valCoef = fnnOutputs[index];
                    //if (valCoef < 0.0f)
                    //    valCoef = 0;

                    if (valCoef > float.Epsilon && bestValue < valCoef)
                    {
                        result = new MoveToPlay(card, 2);
                        bestValue = valCoef;
                    }
                }

                if (board.Get_PH_ULight(3).CanPlaceCard(card))
                {
                    int index = 3 * 98 + card - 2;

                    float valCoef = fnnOutputs[index];
                    //if (valCoef < 0.0f)
                    //    valCoef = 0;

                    if (valCoef > float.Epsilon &&  bestValue < valCoef)
                    {
                        result = new MoveToPlay(card, 3);
                        bestValue = valCoef;
                    }
                }

            }

            return result;
        }

        private MoveToPlay GetNetOutput_Decision2(ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            MoveToPlay result = new MoveToPlay(0, -1);
            float bestValue = float.MinValue;
            var fnnOutputs = _fnn.Outputs;

            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                var moveToPlay = possibleToPlay.Get(i);
                int index = moveToPlay.DeckIndex * 98 + moveToPlay.Card - 2;

                float valCoef = fnnOutputs[index];
                if (valCoef < 0.0f)

                    valCoef = 0;


                //float valCoefDeck = fnnOutputs[moveToPlay.DeckIndex];
                //if (valCoefDeck < 0.5f)
                //    continue;
                ////valCoefDeck = 0;


                float valueWeight = valCoef;

                if (bestValue < valueWeight)
                {
                    result = moveToPlay;
                    bestValue = valueWeight;
                }

            }

            return result;
        }

        private MoveToPlay GetWorse_Decision(GameBoard board, ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            MoveToPlay result = new MoveToPlay(0, -1);
            int bestValue = int.MinValue;

            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                var moveToPlay = possibleToPlay.Get(i);
                var diff = board.Get_PH_ULight(moveToPlay.DeckIndex).Get_CardDiff(moveToPlay.Card);


                if (bestValue < diff)
                {
                    result = moveToPlay;
                    bestValue = diff;
                }

            }

            return result;
        }

        private MoveToPlay GetBest_Decision(GameBoard board, Span<byte> handCards)
        {
            MoveToPlay result = new MoveToPlay(0, -1);
            int bestValue = int.MaxValue;
            int lengthHand = handCards.Length;

            //for (int d = 0; d < board.CardPlaceholdersULight.Length; d++)
            {
                //ref var cardPlaceholder = ref board.Get_PH_ULight(d);

                for (int i = 0; i < lengthHand; i++)
                {
                    var card = handCards[i];

                    if (board.Get_PH_ULight(0).CanPlaceCard(card))
                    {
                        int diff = board.Get_PH_ULight(0).Get_CardDiff(card);

                        if (bestValue > diff)
                        {
                            result = new MoveToPlay(card, (sbyte)0);
                            bestValue = diff;
                        }
                    }

                    if (board.Get_PH_ULight(1).CanPlaceCard(card))
                    {
                        int diff = board.Get_PH_ULight(1).Get_CardDiff(card);

                        if (bestValue > diff)
                        {
                            result = new MoveToPlay(card, (sbyte)1);
                            bestValue = diff;
                        }
                    }
                    if (board.Get_PH_ULight(2).CanPlaceCard(card))
                    {
                        int diff = board.Get_PH_ULight(2).Get_CardDiff(card);

                        if (bestValue > diff)
                        {
                            result = new MoveToPlay(card, (sbyte)2);
                            bestValue = diff;
                        }
                    }
                    if (board.Get_PH_ULight(3).CanPlaceCard(card))
                    {
                        int diff = board.Get_PH_ULight(3).Get_CardDiff(card);

                        if (bestValue > diff)
                        {
                            result = new MoveToPlay(card, (sbyte)3);
                            bestValue = diff;
                        }
                    }

                    //functionCheckCompare(0, cardHand);
                    //functionCheckCompare(1, cardHand);
                    //functionCheckCompare(2, cardHand);
                    //functionCheckCompare(3, cardHand);

                    //ref GameBoardMini.CardPlaceholderULight cardPlaceholder = ref board.Get_PH_ULight(0);
                    //if (cardPlaceholder.CanPlaceCard(cardHand))
                    //{
                    //    int diff = cardPlaceholder.Get_CardDiff(cardHand);
                    //    functionCheckCompare(diff, ref bestValue);
                    //}

                    //cardPlaceholder = ref board.Get_PH_ULight(1);
                    //if (cardPlaceholder.CanPlaceCard(cardHand))
                    //{
                    //    int diff = cardPlaceholder.Get_CardDiff(cardHand);
                    //    functionCheckCompare(diff, ref bestValue);
                    //}

                    //cardPlaceholder = ref board.Get_PH_ULight(2);
                    //if (cardPlaceholder.CanPlaceCard(cardHand))
                    //{
                    //    int diff = cardPlaceholder.Get_CardDiff(cardHand);
                    //    functionCheckCompare(diff, ref bestValue);
                    //}

                    //cardPlaceholder = ref board.Get_PH_ULight(3);
                    //if (cardPlaceholder.CanPlaceCard(cardHand))
                    //{
                    //    int diff = cardPlaceholder.Get_CardDiff(cardHand);
                    //    functionCheckCompare(diff, ref bestValue);
                    //}
                    //if (board.Get_PH_ULight(1).CanPlaceCard(cardHand))
                    //{
                    //    functionCheckCompare(1, cardHand, ref bestValue);
                    //}
                    //if (board.Get_PH_ULight(2).CanPlaceCard(cardHand))
                    //{
                    //    functionCheckCompare(2, cardHand, ref bestValue);
                    //}
                    //if (board.Get_PH_ULight(3).CanPlaceCard(cardHand))
                    //{
                    //    functionCheckCompare(3, cardHand, ref bestValue);
                    //}
                }
            }
           
            void functionCheckCompare(int deckIndex, byte card)
            {
                ref GameBoardMini.CardPlaceholderULight cardPlaceholder = ref board.Get_PH_ULight(deckIndex);

                if (cardPlaceholder.CanPlaceCard(card))
                {
                    int diff = cardPlaceholder.Get_CardDiff(card);

                    if (bestValue > diff)
                    {
                        result = new MoveToPlay(card, (sbyte)deckIndex);
                        bestValue = diff;
                    }
                }

            }

            return result;
        }

        private MoveToPlay GetBest_Decision3(GameBoard board, Span<byte> handCards)
        {
            MoveToPlay result = new MoveToPlay(0, -1);
            int bestValue = int.MaxValue;
            int lengthHand = handCards.Length;

            for (int d = 0; d < board.CardPlaceholdersULight.Length; d++)
            {
                ref var cardPlaceholder = ref board.Get_PH_ULight(d);

                for (int i = 0; i < lengthHand; i++)
                {
                    var cardHand = handCards[i];
                    if (cardPlaceholder.CanPlaceCard(cardHand))
                    {
                        var diff = board.Get_PH_ULight(d).Get_CardDiff(cardHand);


                        if (bestValue > diff)
                        {
                            result = new MoveToPlay(cardHand, (sbyte)d);
                            bestValue = diff;
                        }

                    }
                }
            }

          

            return result;
        }

        private MoveToPlay GetBest_Decision2(GameBoard board, ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            MoveToPlay result = new MoveToPlay(0, -1);
            int bestValue = int.MaxValue;

            for (int i = 0; i < possibleToPlay.Length; i++)
            {
                var moveToPlay = possibleToPlay.Get(i);
                var diff = board.Get_PH_ULight(moveToPlay.DeckIndex).Get_CardDiff(moveToPlay.Card);


                if (bestValue > diff)
                {
                    result = moveToPlay;
                    bestValue = diff;
                }

            }

            return result;
        }


       


    }
}

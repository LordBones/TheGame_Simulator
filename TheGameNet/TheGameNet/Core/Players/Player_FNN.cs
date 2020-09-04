﻿using BonesLib.ForwardNN;
using BonesLib.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    class Player_FNN : Player
    {

        private ForwardNN _fnn;

        private BoardMini _tmpBoardMini = null;

        public Player_FNN()
        {
            Init();
        }

        public Player_FNN(string name) : base(name)
        {
            Init();
        }

        private void Init()
        {
            _fnn = new ForwardNN(104, 104);
            _fnn.SetTopology(new short[] { 20 });
            
        }

        public override void StartPlay(GameBoard board, Span<byte> handCards)
        {
            _tmpBoardMini = (_tmpBoardMini != null) ? board.CreateBoardMini(this.Id, _tmpBoardMini) : board.CreateBoardMini(this.Id);

        }

        public override void AfterCardPlay_ResultMove(GameBoard board, Span<byte> handCards, bool isEndOfGame)
        {
           
        }

        public override MoveToPlay Decision_CardToPlay(GameBoard board, Span<byte> handCards)
        {
            var boardMini = board.CreateBoardMini(this.Id, _tmpBoardMini);

            Span<MoveToPlay> flsa = stackalloc MoveToPlay[40];
            FixListSpan<MoveToPlay> possibleToPlay = new FixListSpan<MoveToPlay>(flsa);
            board.Get_PossibleToPlay(handCards, ref possibleToPlay);

            FillNetInputs(board, handCards, this.Id);
            _fnn.Evaluate();

            var resultMove = GetNetOutput_Decision(ref possibleToPlay);

            
            return resultMove;
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, Span<byte> handCards)
        {
            return new MoveToPlay(0, -1);
        }



        public override void EndGame(GameBoard board, Span<byte> handCards)
        {
           
        }

        private void FillNetInputs(GameBoard board, Span<byte> handCards, byte playerId)
        {
            Array.Clear(_fnn.Inputs, 0, _fnn.Inputs.Length);
            _fnn.Inputs[0] = board.CardPlaceholders[0].Get_TopCard() / 100.0f;
            _fnn.Inputs[1] = board.CardPlaceholders[1].Get_TopCard() / 100.0f;
            _fnn.Inputs[2] = board.CardPlaceholders[2].Get_TopCard() / 100.0f;
            _fnn.Inputs[3] = board.CardPlaceholders[3].Get_TopCard() / 100.0f;

            for(int i = 0;i < handCards.Length; i++)
            {
                _fnn.Inputs[handCards[i] + 4] = 1.0f;
            }
        }

        private MoveToPlay GetNetOutput_Decision(ref FixListSpan<MoveToPlay> possibleToPlay)
        {
            MoveToPlay result = new MoveToPlay(0,-1);
            float bestValue = float.MinValue;

            for (int i =0;i < possibleToPlay.Length;i++)
            {
                var moveToPlay = possibleToPlay[i];
                float valueWeight = _fnn.Outputs[moveToPlay.DeckIndex] *
                 _fnn.Outputs[moveToPlay.Card + 4];

                if(bestValue < valueWeight)
                {
                    result = moveToPlay;
                    bestValue = valueWeight;
                }

            }

            return result;
        }


    }
}

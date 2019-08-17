﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.GameBoardMini;

namespace TheGameNet.Core.Players
{
    class Player_Soliter_PreserveMaxPossibilites : Player
    {
        public Player_Soliter_PreserveMaxPossibilites()
        {

        }

        public Player_Soliter_PreserveMaxPossibilites(string name) : base(name)
        {
        }

        private int _playFreeCard = 0;

        public override void StartPlay(GameBoard board, List<byte> handCards)
        {

        }

       


        public override MoveToPlay Decision_CardToPlay(GameBoard board, List<byte> handCards)
        {
            var boardMini = board.CreateBoardMini(this.Id);

            int minDepthSearch =  board.MinCardForPlay - (board.MaxCardInHands - handCards.Count);
            if (minDepthSearch <= 0) minDepthSearch = 1;


            int maxDepthSearch = 5;

            DeepSearch_BoardMini search = new DeepSearch_BoardMini(boardMini, handCards.ToArray(), maxDepthSearch, board.AvailableCards.Count);
            var (placeholderId, card, level) = search.GetBestNextCard((byte)minDepthSearch);


            _playFreeCard = level - minDepthSearch;


            MoveToPlay result = new MoveToPlay(card, placeholderId);



            if (result.IsNotMove)
            {
                throw new Exception();
            }
            return result;
           // List<MoveToPlay> possibleToPlay = board.Get_PossibleToPlay(handCards);


           // return possibleToPlay[0];
        }

        public override MoveToPlay Decision_CardToPlay_Optional(GameBoard board, List<byte> handCards)
        {
            if (_playFreeCard > 0)
            {
                var boardMini = board.CreateBoardMini(this.Id);

                int depthSearch = board.MinCardForPlay - (board.MaxCardInHands - handCards.Count);
                if (depthSearch <= 0) depthSearch = 2;

                depthSearch = _playFreeCard;// _playFreeCard;

                DeepSearch_BoardMini search = new DeepSearch_BoardMini(boardMini, handCards.ToArray(), depthSearch, board.AvailableCards.Count);
                var (placeholderId, card, level) = search.GetBestNextCard((byte)_playFreeCard);

                MoveToPlay result = new MoveToPlay(card, placeholderId);


                _playFreeCard--;

                return result;
            }


            return new MoveToPlay(0, -1);
            
        }

        public override void AfterCardPlay_ResultMove(GameBoard board, List<byte> handCards, bool isEndOfGame)
        {

        }

        public override void EndGame(GameBoard board, List<byte> handCards)
        {

        }
    }
}
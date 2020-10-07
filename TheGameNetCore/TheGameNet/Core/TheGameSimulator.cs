using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using TheGameNet.Core.Players;


namespace TheGameNet.Core
{
    class TheGameSimulator
    {

        private GameBoard _gameBoard;

        private System.IO.StreamWriter _gameRunLog = null;

        public Player [] Players => _gameBoard.Players;

        public GameBoard GameBoard => _gameBoard;

        public TheGameSimulator(System.IO.StreamWriter gameRunLog)
        {
            this._gameBoard = new GameBoard();
            this._gameRunLog = gameRunLog ?? StreamWriter.Null;
        }

        public void SetPlayers(ICollection<Player> players)
        {
            this._gameBoard.InitPlayers(players);
        }

        public void Set_TotalCountCards(int totalCards)
        {
            _gameBoard.TotalCardsForPlay = totalCards;
        }

        public GameResult Simulate(byte [] deckCards)
        {
            Log_StartGame();
            Log_StartDeckCards(deckCards);

            InitGame(deckCards);

            Run();

            // ValidateStacks();

            GameResult gameResult = ResultGame();

            Log_EndGame();

            return gameResult;

            
        }

        private GameResult ResultGame()
        {
            GameResult result = new GameResult();

            int playersCardsCountTotal = 0;
            var pcc = this._gameBoard.Players_Cards.PlayerCardsCount;

            for (int i=0; i < pcc.Length;i++ )
            {
                playersCardsCountTotal += pcc[i];
            }

                byte[] leftCards = new byte[playersCardsCountTotal + this._gameBoard.AvailableCards.Count+1];
            int leftCardsIndex = 0;


            int gameResult = 0;
            for (int k = 0;k < pcc.Length;k++)
            {
                gameResult += pcc[k];

                var cards = this._gameBoard.Players_Cards.Get(k);
                for (int i = 0; i < cards.Length; i++)
                {
                    leftCards[leftCardsIndex] = cards[i];
                    leftCardsIndex++;
                }
            }

            gameResult += this._gameBoard.AvailableCards.Count;

            var acEnum = this._gameBoard.AvailableCards.GetEnumerator();
            while (acEnum.MoveNext())
            {
                leftCards[leftCardsIndex] = acEnum.Current;
                leftCardsIndex++;
            }

            result.Rest_Cards = gameResult;
            result.Rest_Cards_List = leftCards;

            return result;
        }

        

        private void Run()
        {
            foreach (var p in this.Players)
            {
                p.StartGame(this._gameBoard);
            }

            Player player = this._gameBoard.Player_Order.Current;

            player.StartPlay(this._gameBoard, this._gameBoard.Get_PlayerHand(player.Id));

            while (player!=null && this._gameBoard.Can_PlayerPlay(player.Id))
            {
               
                Log_CurrentDecksState();
                Log_PlayerCards();

                this._gameBoard.Get_PlayerBoardData(player.Id).CountNeedPlayCard = (sbyte)this._gameBoard.MinCardForPlay;


                bool playerNotEndGame = Process_PlayerMoves_Mandatory(player);
                if (!playerNotEndGame)
                {
                    break;
                }

                Process_PlayerMoves_Optional(player);

                this._gameBoard.Refill_PlayerCards(player.Id);
               
                player = MoveTo_NextPlayer(player);

                player?.StartPlay(this._gameBoard, this._gameBoard.Get_PlayerHand(player.Id));

            }

            foreach (var p in this.Players)
            {
                p.EndGame(this._gameBoard, this._gameBoard.Get_PlayerHand(p.Id));
            }
        }

        private bool Process_PlayerMoves_Mandatory(Player player)
        {
            bool playerNotEndGame = true;

            var playerHand = this._gameBoard.Get_PlayerHand(player.Id);

            for (int m = 0; m < this._gameBoard.Get_CurrentMinCardForPlay; m++)
            {
                if (!this._gameBoard.Can_PlayerPlay(player.Id))
                {
                    playerNotEndGame = false;
                    break;
                }

                MoveToPlay move = player.Decision_CardToPlay(this._gameBoard, playerHand);

                if(move.DeckIndex < 0)
                {
                    int i = 0;

                    playerNotEndGame = false;
                    break;

                }

                this._gameBoard.Apply_PlayerMove(player, move);

                playerHand = this._gameBoard.Get_PlayerHand(player.Id);

                player.AfterCardPlay_ResultMove(this._gameBoard, playerHand, false);

            }
            
            return playerNotEndGame;
        }

        private void Process_PlayerMoves_Optional(Player player)
        {
            var playerHand = this._gameBoard.Get_PlayerHand(player.Id);

            while (!this._gameBoard.PlayerHand_IsEmpty(player.Id))
            {
                MoveToPlay move = player.Decision_CardToPlay_Optional(this._gameBoard, playerHand);

                if (move.IsNotMove)
                {
                    player.AfterCardPlay_ResultMove(this._gameBoard, playerHand, false);
                    return;
                }

                this._gameBoard.Apply_PlayerMove(player, move);
                playerHand = this._gameBoard.Get_PlayerHand(player.Id);
                player.AfterCardPlay_ResultMove(this._gameBoard, playerHand, false);
            }
        }

        private Player MoveTo_NextPlayer(Player player)
        {
            bool discardPlayer = this._gameBoard.PlayerHand_IsEmpty(player.Id);

            this._gameBoard.Player_Order.MoveToNext(discardPlayer);

            return this._gameBoard.Player_Order.Current;
        }

        private void InitGame(byte[] deckCards)
        {
            this._gameBoard.Clear();
            this._gameBoard.Set_AvailableCardsDeck(deckCards);
            this._gameBoard.InitPlayerStartCards();
            this._gameBoard.Init_PlayerOrder();
        }

        private void ValidateStacks()
        {
            foreach(var item in this._gameBoard.CardPlaceholders)
            {
                item.ValidateStack();
            }
        }

        #region Helper Log

        private void Log_StartGame()
        {
            if (_gameRunLog == StreamWriter.Null) return;

            _gameRunLog.WriteLine($"############ Simulation start for { this._gameBoard.Players.Length.ToString()} players");
        }

        private void Log_EndGame()
        {
            if (_gameRunLog == StreamWriter.Null) return;

            _gameRunLog.WriteLine($"############ Simulation end ");
        }

        private void Log_StartDeckCards( byte [] deckCards)
        {
            if (_gameRunLog == StreamWriter.Null) return;

            string cards = string.Join(", ", deckCards);
            _gameRunLog.WriteLine("DeckCards: ");
            _gameRunLog.WriteLine(cards);
        }

        StringBuilder sb = new StringBuilder();
        private void Log_CurrentDecksState()
        {
            if (_gameRunLog == StreamWriter.Null) return;

            sb.Clear();

            sb.Append($"{_gameBoard.Get_PH_ULight(0).Get_TopCard().ToString(),3} ");
            sb.Append($"{_gameBoard.Get_PH_ULight(1).Get_TopCard().ToString(),3} ");
            sb.Append($"{_gameBoard.Get_PH_ULight(2).Get_TopCard().ToString(),3} ");
            sb.Append($"{_gameBoard.Get_PH_ULight(3).Get_TopCard().ToString(),3} ");

            _gameRunLog.WriteLine("DeckCards: ");
            _gameRunLog.WriteLine(sb.ToString());
        }

        private void Log_PlayerCards()
        {
            if (_gameRunLog == StreamWriter.Null) return;

            sb.Clear();

            Player[] players = this._gameBoard.Players;
            var playersCards = this._gameBoard.Players_Cards;

            Player currentPlayer = this._gameBoard.Player_Order.Current;

            for (int i=0;i< players.Length; i++)
            {
                string markPlayNow = (players[i] == currentPlayer) ? "*" : string.Empty;
                string markNewLine = (players[i] == currentPlayer) ? "\n" : string.Empty;

                string playerName = players[i].Name;
                
                sb//.Append($"{markNewLine}{markPlayNow,1} P{i} {playerName,15}: ")
                  .Append(markNewLine)
                  .Append($"{ markPlayNow,1}")
                  .Append(" P")
                  .Append(i)
                  .Append(" ")
                  .Append($"{playerName,15}")
                  .Append(": ")
                  ;

                Span<byte> cards = playersCards.Get(i);

                for(int c = 0; c < cards.Length; c++)
                {
                    sb.Append($"{cards[c].ToString(),3}, ");
                }


                sb.AppendLine(markNewLine);
            }

            _gameRunLog.WriteLine(sb.ToString());
        }
        #endregion
    }

    struct GameResult
    {
        public byte[] Rest_Cards_List;
        public int Rest_Cards;
        
    }
}

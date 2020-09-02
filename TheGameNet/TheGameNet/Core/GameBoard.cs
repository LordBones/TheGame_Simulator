using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TheGameNet.Utils;
using TheGameNet.Core.Players;
using TheGameNet.Core.GameBoardMini_Solver;
using BonesLib.Utils;

namespace TheGameNet.Core
{
    public class GameBoard
    {
        public int MaxCardInHands = 5;

        public int MinCardForPlay = 2;

        public Stack<byte> AvailableCards = new Stack<byte>(100);

        private readonly PlayedCards _playedCards;
        public CardPlaceholder[] CardPlaceholders = new CardPlaceholder[4];
        //public PlayersHintsPlaceholder[] CardPlaceholdersHints = new PlayersHintsPlaceholder[4];


        public Player[] Players = Array.Empty<Player>();
        public PlayerBoardData [] PlayersData = Array.Empty<PlayerBoardData>();

        //public List<byte>[] Players_Cards = Array.Empty< List<byte>>();
        public PlayersCards Players_Cards=  new PlayersCards(5);


        public PlayersOrder Player_Order = new PlayersOrder();

        public int Count_AllRemaindPlayCards => _totalCardsForPlay - _playedCards.Count;

        private int _totalCardsForPlay;
        public int TotalCardsForPlay { get { return _totalCardsForPlay; } set { _totalCardsForPlay = value; } }

        public PlayerBoardData Get_PlayerBoardData(byte id)
        {
            foreach(var item in PlayersData)
            {
                if (item.Id == id) return item;
            }

            return null;
        }
        public int Get_CurrentMinCardForPlay { get { return (this.AvailableCards.Count > 0) ? this.MinCardForPlay : 1; } }

        public GameBoard()
        {
            _playedCards = new PlayedCards();

            this.CardPlaceholders[0] = new CardPlaceholder_DownDirection_Smart(_playedCards);
            this.CardPlaceholders[1] = new CardPlaceholder_DownDirection_Smart(_playedCards);
            this.CardPlaceholders[2] = new CardPlaceholder_UpDirection_Smart(_playedCards);
            this.CardPlaceholders[3] = new CardPlaceholder_UpDirection_Smart(_playedCards);

            _totalCardsForPlay = 98;
           
            
        }

        

        public void InitPlayers(ICollection<Player> players)
        {
            

            this.Players = players.ToArray();

            if(this.PlayersData.Length != Players.Length)
                this.PlayersData = new PlayerBoardData[Players.Length];

            //if (this.Players_Cards.Length != Players.Length)
            //    this.Players_Cards = new List<byte>[this.Players.Length];
            this.Players_Cards.Init(this.Players.Length);

            for(int i = 0; i < this.Players.Length; i++)
            {
                this.Players[i].Id = (byte)i;
                
                this.PlayersData[i] = new PlayerBoardData() { Id = (byte)i };
            }

            InitCardInHand(players.Count);
        }

        private void InitCardInHand(int countPlayers)
        {
            if (countPlayers == 1) this.MaxCardInHands = 8;
            else if(countPlayers == 2) this.MaxCardInHands = 7;
            else  this.MaxCardInHands = 6;
        }

        public void Clear()
        {
            for(int i = 0; i < CardPlaceholders.Length; i++)
            {
                CardPlaceholders[i].Clear();
            }

            for(int i = 0;i< this.Players.Length; i++)
            {
                this.Players_Cards[i].Clear();
            }

            for (int i = 0; i < this.Players_Cards.PlayerCardsCount.Length; i++)
            {

                this.Players_Cards.PlayerCardsCount[i] = 0;
            }

            Player_Order.Clear();
            _playedCards.Clear();
        }


        public Span<byte> Get_PlayerHand(byte playerId)
        {
            return this.Players_Cards[playerId];
        }

        public bool PlayerHand_IsEmpty(byte playerId)
        {
            return this.Players_Cards[playerId].Length == 0;
        }

        public void Set_AvailableCardsDeck(byte [] deckCards)
        {
            this.AvailableCards.Clear();

            for(int i = 0;i < deckCards.Length; i++)
            {
                this.AvailableCards.Push(deckCards[i]);
            }
        }

        public byte[] Get_CreatedSuffledDeck()
        {
            return Get_CreatedSuffledDeck(_totalCardsForPlay);
        }

        public static byte [] Get_CreatedSuffledDeck(int totalCardsForPlay)
        {
            List<byte> result = new List<byte>(totalCardsForPlay);

            List<byte> forSuffle = new List<byte>(totalCardsForPlay);
            for(byte i = 2; i < 100; i++)
            {
                forSuffle.Add(i);
            }

             //           Random rnd = new Random((int)DateTime.Now.Ticks);

            while(forSuffle.Count > 0)
            {
                int index = RandomGen.Default.GetRandomNumber(0,forSuffle.Count);
                byte card = forSuffle[index];
                result.Add(card);
                forSuffle.Remove(card);
            }

            return result.ToArray();
        }

        public void InitPlayerStartCards()
        {
            for (int i = 0; i < this.Players.Length; i++)
            {

                for(int c = 0; c < this.MaxCardInHands; c++)
                {
                    byte card = this.AvailableCards.Pop();
                    this.Players_Cards.Add(i,card);
                }
            }
        }

        public void Init_PlayerOrder()
        {
            this.Player_Order.Create_PlayerOrder(this.Players);
        }


        public PlayersHintsPlaceholder[] GetPlayersHints()
        {
            PlayersHintsPlaceholder[] result = new PlayersHintsPlaceholder[4];
            result[0] = new PlayersHintsPlaceholder();
            result[1] = new PlayersHintsPlaceholder();
            result[2] = new PlayersHintsPlaceholder();
            result[3] = new PlayersHintsPlaceholder();



            //this.CardPlaceholdersHints[0].Hints.Clear();
            //this.CardPlaceholdersHints[1].Hints.Clear();
            //this.CardPlaceholdersHints[2].Hints.Clear();
            //this.CardPlaceholdersHints[3].Hints.Clear();

            for (int ch =0;ch < this.CardPlaceholders.Length; ch++)
            {

                CardPlaceholder placeholder = this.CardPlaceholders[ch];
                List<CardHint> placeholderHints = result[ch].Hints;
                placeholderHints.Clear();

                for (int p =0; p < this.Players.Length; p++)
                {
                    byte playerId = this.Players[p].Id;

                    Span<byte> playerCards = this.Players_Cards[playerId];
                    for(int c = 0;c< playerCards.Length; c++)
                    {
                        int diff = placeholder.Get_CardDiff(playerCards[c]);
                        //RangeHint hint = DiffToRangeHint(diff);

                        placeholderHints.Add(new CardHint(playerId, (short)diff));
                    }
                }
            }

            return result;
        }

        public static RangeHint DiffToRangeHint(int diff)
        {
            if (diff < 1) return RangeHint.Jump;
            else if (diff == 1) return RangeHint.Exact;
            else if(diff < 6) return RangeHint.Closer;
            else if(diff < 11) return RangeHint.Near;
            else if (diff < 21) return RangeHint.Mid;
            else  return RangeHint.Far;

        }

        public bool Can_PlayerPlay(byte playerId)
        {
            Span<byte> hand = this.Players_Cards[playerId];

            for(int i  = 0;i < hand.Length; i++)
            {
                var card = hand[i];

                for (int d = 0;d< this.CardPlaceholders.Length; d++)
                {
                    if (this.CardPlaceholders[d].CanPlaceCard(card))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        List<MoveToPlay> result = new List<MoveToPlay>();


        public List<MoveToPlay> Get_PossibleToPlay(Span<byte> hand)
        {
            //List<MoveToPlay> result = new List<MoveToPlay>();

            result.Clear();


            for (int d = 0; d < this.CardPlaceholders.Length; d++)
            {
                var cardPlaceholder = this.CardPlaceholders[d];

                for (int i = 0; i < hand.Length; i++)
                {
                    var cardHand = hand[i];
                    if (cardPlaceholder.CanPlaceCard(cardHand))
                    {
                        result.Add(new MoveToPlay(cardHand, (sbyte)d));
                    }
                }
            }

            return result;
        }

        public int Get_PossibleToPlay(Span<byte> hand, Span<MoveToPlay> result)
        {
            int index = 0;


            for (int d = 0; d < this.CardPlaceholders.Length; d++)
            {
                var cardPlaceholder = this.CardPlaceholders[d];

                for (int i = 0; i < hand.Length; i++)
                {
                    var cardHand = hand[i];
                    if (cardPlaceholder.CanPlaceCard(cardHand))
                    {
                        result[index] = new MoveToPlay(cardHand, (sbyte)d);
                        index++;
                    }
                }
            }

            return index;
        }

        public void Get_PossibleToPlay(Span<byte> hand, ref FixListSpan<MoveToPlay> result)
        {
            for (int d = 0; d < this.CardPlaceholders.Length; d++)
            {
                var cardPlaceholder = this.CardPlaceholders[d];

                for (int i = 0; i < hand.Length; i++)
                {
                    var cardHand = hand[i];
                    if (cardPlaceholder.CanPlaceCard(cardHand))
                    {
                        result.Add( new MoveToPlay(cardHand, (sbyte)d));
                        
                    }
                }
            }
        }

        

        public bool Get_HasCard(List<byte> hand, byte card)
        {
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] == card) return true;
                
            }

            return false;
        }
        public bool Get_HasCard(Span<byte> hand, byte card)
        {
            for (int i = 0; i < hand.Length; i++)
            {
                if (hand[i] == card) return true;

            }

            return false;
        }

        public void Apply_PlayerMove(Player player, MoveToPlay move)
        {
            try
            {
                this.CardPlaceholders[move.DeckIndex].PlaceCard(move.Card);
            }
            catch(Exception ex)
            {
                int i = 0;
            }

            int cardIndex = this.Players_Cards[player.Id].IndexOf(move.Card);
            
            this.Players_Cards.Remove(player.Id,cardIndex);

            this._playedCards.AddCard_IfNotExist(move.Card);

            Update_CardPlaceholder_PhantomStates();

            this.Get_PlayerBoardData(player.Id).CountNeedPlayCard--;
        }

        public void Refill_PlayerCards(byte playerId)
        {
            while (this.AvailableCards.Count > 0 && this.Players_Cards.PlayerCardsCount[playerId] < this.MaxCardInHands)
            {
                this.Players_Cards.Add(playerId,this.AvailableCards.Pop());
            }
        }

        public void Update_CardPlaceholder_PhantomStates()
        {
            this.CardPlaceholders[0].UpdatePhantomState();
            this.CardPlaceholders[1].UpdatePhantomState();
            this.CardPlaceholders[2].UpdatePhantomState();
            this.CardPlaceholders[3].UpdatePhantomState();
        }

        internal BoardMini CreateBoardMini(byte playerId)
        {
            BoardMini result = new BoardMini();
            
            for(int i = 0;i < this.CardPlaceholders.Length; i++)
            {
                var cph = CardPlaceholders[i];
                if (cph.IsUpDirection)
                {
                    result.CardPlaceholders[i] = new CardPlaceholderLight_Up(cph.Get_TopCard());
                }
                else
                {
                    result.CardPlaceholders[i] = new CardPlaceholderLight_Down(cph.Get_TopCard());
                }
            }

            result.CountNeedPlayCard = this.Get_PlayerBoardData(playerId).CountNeedPlayCard;

            return result;
        }

        internal BoardMini CreateBoardMini(byte playerId, BoardMini boardMini)
        {

            for (int i = 0; i < this.CardPlaceholders.Length; i++)
            {
                boardMini.CardPlaceholders[i].Clear();

                var cph = CardPlaceholders[i];
                boardMini.CardPlaceholders[i].PlaceCard(cph.Get_TopCard());
            }

            boardMini.CountNeedPlayCard = this.Get_PlayerBoardData(playerId).CountNeedPlayCard;

            return boardMini;
        }
    }

    public readonly struct MoveToPlay
    {
        public byte Card { get; }
        public sbyte DeckIndex { get; }

        public MoveToPlay(byte card, sbyte deckIndex)
        {
            this.Card = card;
            this.DeckIndex = deckIndex;
        }

        public bool IsNotMove { get { return DeckIndex < 0; } }
    }


    public class PlayersOrder
    {
        private Queue<Player> _playerOrder = new Queue<Player>();


        public Player Current { get
            {
                if (this._playerOrder.Count == 0) return null;

                return this._playerOrder.Peek();
            }
        }

        public void MoveToNext(bool discardCurrent = false)
        {
            Player p = this._playerOrder.Dequeue();

            if (!discardCurrent)
            {
                this._playerOrder.Enqueue(p);
            }
        }

        public void Create_PlayerOrder(IList<Player> players)
        {
            for (int i = 0; i < players.Count; i++)
            {
                this._playerOrder.Enqueue(players[i]);
            }
        }

        public void Clear()
        {
            this._playerOrder.Clear();
        }
    }

   public class PlayerBoardData
    {
        public byte Id;
        public sbyte CountNeedPlayCard;
    }

    public enum RangeHint:byte { Exact = 10, Closer = 11, Near=12, Mid =13, Far = 14,Jump = 1}

    public struct CardHint
    {
        public byte PlayerId;
        public short DiffHint;
        public bool hasJump;
        //public RangeHint Hint;

        public CardHint(byte playerId, short diff)
        {
            this.PlayerId = playerId;
            this.DiffHint = diff;
            hasJump = false;
            //this.Hint = hint;
        }

        public override string ToString()
        {
            return $"{PlayerId} , hint: {DiffHint}";
        }
    }

    public class PlayersHintsPlaceholder
    {
        public List<CardHint> Hints = new List<CardHint>();

        public RangeHint ? FindNearest(byte playerIdIgnore)
        {
            RangeHint? result = null;

            for(int i = 0; i < this.Hints.Count; i++)
            {
                if (this.Hints[i].PlayerId == playerIdIgnore) continue;

                RangeHint hint = GameBoard.DiffToRangeHint(this.Hints[i].DiffHint);

                if ( !result.HasValue || hint < result.Value)
                {
                    result = hint;
                }
            }

            return result;
        }

        public short? FindNearestDiff(byte playerIdIgnore)
        {
            short? result = null;

            for (int i = 0; i < this.Hints.Count; i++)
            {
                if (this.Hints[i].PlayerId == playerIdIgnore) continue;

                short hint = this.Hints[i].DiffHint;

                if (!result.HasValue || hint < result.Value)
                {
                    result = hint;
                }
            }

            return result;
        }
    }

    public struct PlayersCards
    {
        const byte CONST_MaxCardsPerPlayer = 12;
       
        byte[] cards;
        public byte[] PlayerCardsCount ;

        public PlayersCards(int maxPlayers)
        {
            cards = new byte[maxPlayers* CONST_MaxCardsPerPlayer];

            PlayerCardsCount = Array.Empty<byte>();
        }

        public void Init(int countPlayers)
        {
            PlayerCardsCount = new byte[countPlayers];
        }

        public Span<byte> this[int i]
        {
            get { return cards.AsSpan(i * CONST_MaxCardsPerPlayer, PlayerCardsCount[i]); }
        }

        public void Add(int playerIndex, byte card)
        {
            int pcc = PlayerCardsCount[playerIndex];

            if (pcc == CONST_MaxCardsPerPlayer)
            {
                throw new IndexOutOfRangeException();
            }

            cards[playerIndex * CONST_MaxCardsPerPlayer + pcc] = card;
            PlayerCardsCount[playerIndex]++;
        }

        public void Remove(int playerIndex, int indexCard)
        {
            int pcc = PlayerCardsCount[playerIndex];
            if (pcc <= indexCard)
            {
                throw new IndexOutOfRangeException();
            }

            for( int i = indexCard;i < pcc-1;i++)
            {
                cards[playerIndex * CONST_MaxCardsPerPlayer + i ] = cards[playerIndex * CONST_MaxCardsPerPlayer + i+1];
            }

            PlayerCardsCount[playerIndex]--;
        }
    }
}

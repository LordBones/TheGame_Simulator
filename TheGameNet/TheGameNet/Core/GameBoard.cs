using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TheGameNet.Utils;
using TheGameNet.Core.Players;
using TheGameNet.Core.GameBoardMini_Solver;

namespace TheGameNet.Core
{
    public class GameBoard
    {
        public int MaxCardInHands = 5;

        public int MinCardForPlay = 2;

        public Stack<byte> AvailableCards = new Stack<byte>();

        private readonly PlayedCards _playedCards;
        public CardPlaceholder[] CardPlaceholders = new CardPlaceholder[4];
        //public PlayersHintsPlaceholder[] CardPlaceholdersHints = new PlayersHintsPlaceholder[4];


        public Player[] Players = new Player[0];
        public PlayerBoardData [] PlayersData = new PlayerBoardData[0];

        public List<byte>[] Players_Cards = new List<byte>[0];


        public PlayersOrder Player_Order = new PlayersOrder();

        public int Count_AllRemaindPlayCards => 98 - _playedCards.Count;

        public  PlayerBoardData Get_PlayerBoardData(byte id) =>  PlayersData.First(x=> x.Id == id);
        public int Get_CurrentMinCardForPlay { get { return (this.AvailableCards.Count > 0) ? this.MinCardForPlay : 1; } }

        public GameBoard()
        {
            _playedCards = new PlayedCards();

            this.CardPlaceholders[0] = new CardPlaceholder_DownDirection_Smart(_playedCards);
            this.CardPlaceholders[1] = new CardPlaceholder_DownDirection_Smart(_playedCards);
            this.CardPlaceholders[2] = new CardPlaceholder_UpDirection_Smart(_playedCards);
            this.CardPlaceholders[3] = new CardPlaceholder_UpDirection_Smart(_playedCards);


           
            
        }

        

        public void InitPlayers(ICollection<Player> players)
        {
            this.Players = players.ToArray();
            this.PlayersData = new PlayerBoardData[Players.Length];

            this.Players_Cards = new List<byte>[this.Players.Length];

            for(int i = 0; i < this.Players.Length; i++)
            {
                this.Players[i].Id = (byte)i;
                this.Players_Cards[i] = new List<byte>();
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

            for(int i = 0;i< this.Players_Cards.Length; i++)
            {
                this.Players_Cards[i].Clear();
            }

            Player_Order.Clear();
            _playedCards.Clear();
        }


        public List<byte> Get_PlayerHand(byte playerId)
        {
            return this.Players_Cards[playerId];
        }

        public bool PlayerHand_IsEmpty(byte playerId)
        {
            return this.Players_Cards[playerId].Count == 0;
        }

        public void Set_AvailableCardsDeck(byte [] deckCards)
        {
            this.AvailableCards.Clear();

            for(int i = 0;i < deckCards.Length; i++)
            {
                this.AvailableCards.Push(deckCards[i]);
            }
        }

        public static byte [] Get_CreatedSuffledDeck()
        {
            List<byte> result = new List<byte>(98);

            List<byte> forSuffle = new List<byte>(98);
            for(byte i = 2; i < 100; i++)
            {
                forSuffle.Add(i);
            }

             //           Random rnd = new Random((int)DateTime.Now.Ticks);

            while(forSuffle.Count > 0)
            {
                int index = RandomGen.GetRandomNumber(0,forSuffle.Count);
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
                    this.Players_Cards[i].Add(card);
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

                    List<byte> playerCards = this.Players_Cards[playerId];
                    for(int c = 0;c< playerCards.Count; c++)
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
            List<byte> hand = this.Players_Cards[playerId];

            for(int i  = 0;i < hand.Count; i++)
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


        public List<MoveToPlay> Get_PossibleToPlay(List<byte> hand)
        {
            //List<MoveToPlay> result = new List<MoveToPlay>();

            result.Clear();


            for (int d = 0; d < this.CardPlaceholders.Length; d++)
            {
                var cardPlaceholder = this.CardPlaceholders[d];

                for (int i = 0; i < hand.Count; i++)
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

        public bool Get_HasCard(List<byte> hand, byte card)
        {
            for (int i = 0; i < hand.Count; i++)
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
            this.Get_PlayerHand(player.Id).Remove(move.Card);

            this._playedCards.AddCard_IfNotExist(move.Card);

            Update_CardPlaceholder_PhantomStates();

            this.Get_PlayerBoardData(player.Id).CountNeedPlayCard--;
        }

        public void Refill_PlayerCards(byte playerId)
        {

            List<byte> playerHand = this.Get_PlayerHand(playerId);

            while (this.AvailableCards.Count > 0 && playerHand.Count < this.MaxCardInHands)
            {
                playerHand.Add(this.AvailableCards.Pop());
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
    }

    public struct MoveToPlay
    {
        public byte Card;
        public sbyte DeckIndex;

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
}

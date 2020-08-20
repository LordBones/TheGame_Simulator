using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGameNet.Core.Players
{
    public abstract class Player
    {
        public byte Id;
        public string Name;

        public Player()
        {

        }

        public Player(string name)
        {
            this.Name = name;
        }

        public abstract void StartPlay(GameBoard board, List<byte> handCards);

        public abstract MoveToPlay Decision_CardToPlay(GameBoard board, List<byte> handCards);

        public abstract MoveToPlay Decision_CardToPlay_Optional(GameBoard board, List<byte> handCards);

        public abstract void AfterCardPlay_ResultMove(GameBoard board, List<byte> handCards, bool isEndOfGame);


        public virtual void StartGame(GameBoard board)
        {

        }
        public abstract void EndGame(GameBoard board, List<byte> handCards);

    }
}

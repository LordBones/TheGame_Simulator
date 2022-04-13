using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core;
using TheGameNet.Core.Players;

namespace TheGameNet.RoundSimulations
{
    internal class RoundSimulator
    {
        static byte[] _testGameDeck = new byte[] { 93, 58, 55, 63, 90, 24, 22, 46, 13, 83, 78, 40, 29, 60, 66, 17, 37, 39, 92, 36, 68, 85, 89, 69, 49, 97, 77, 44, 3, 62, 38, 72, 86, 2, 19, 50, 15, 23, 84, 52, 47, 16, 95, 18, 4, 73, 75, 98, 12, 7, 9, 30, 8, 79, 43, 31, 99, 81, 56, 10, 32, 34, 91, 61, 76, 5, 88, 14, 80, 6, 82, 74, 33, 27, 41, 51, 57, 70, 64, 28, 71, 26, 87, 48, 11, 20, 35, 65, 25, 42, 67, 54, 53, 21, 45, 94, 59, 96 };
        public static GamePlay CreateGamePlay(List<Player> players, string title, string groupName, bool enableLog = false, StreamWriter playLog = null)
        {
            if (playLog != null) playLog = StreamWriter.Null;

            if (enableLog)
            {
                if (playLog == null) playLog = new StreamWriter($"gamePlay_{groupName}_{title}_log.txt");
            }
            else
            {
                playLog = StreamWriter.Null;
            }

            return new GamePlay(players, title, playLog);
        }

        public static void SimulateGameRounds(GamePlayGroup[] gamePlayes, int rounds, bool enablePrintResults = true, int seed = 0, bool oneGlobalDeck = true, int batchDeckSize = -1)
        {
            //byte[] newGameDeck = _testGameDeck;
            DeckGenerator deckGen = new DeckGenerator(100, seed);

            byte[] newGameDeck = new byte[deckGen.CardMaxCount] ;
            if (oneGlobalDeck)
                deckGen.Get_CreatedSuffledDeck(newGameDeck.AsSpan());

            int deckCount = batchDeckSize;
            for (int r = 0; r < rounds; r++)
            {
                if (!oneGlobalDeck)
                    deckGen.Get_CreatedSuffledDeck(newGameDeck.AsSpan());

                if(batchDeckSize>0 && deckCount <= 0)
                {
                    deckGen.ResetSeed(0);
                    deckCount = batchDeckSize;
                }

                SimulateGameplays(gamePlayes, newGameDeck);

                if(batchDeckSize > 0)
                    deckCount--;
            }

            if (enablePrintResults)
            {
                PrintSimulationResults(gamePlayes);
            }
        }

       public static void SimulateGameplays(GamePlayGroup[] gamePlayes, byte[] deckCards)
        {
            for (int i = 0; i < gamePlayes.Length; i++)
            {
                SimulateGameplays(gamePlayes[i].GamePlayes, deckCards);
            }
        }

        private static void SimulateGameplays(GamePlay[] gamePlayes, byte[] deckCards)
        {
            for (int i = 0; i < gamePlayes.Length; i++)
            {
                GamePlay gamePlay = gamePlayes[i];

                GameResult gameResult = gamePlay.tgs.Simulate(deckCards);

                gamePlay.GameStat.UpdateStats(gameResult);
                gamePlay.GameProgress.Update((byte)gameResult.Rest_Cards);
            }
        }


        private static void PrintSimulationResults(GamePlayGroup[] gamePlayGroups)
        {
            for (int i = 0; i < gamePlayGroups.Length; i++)
            {
                string text = gamePlayGroups[i].Title;

                if(Trace.Listeners.Count > 0)
                    Trace.WriteLine(text);

                Console.WriteLine(text);
                PrintSimulationResults(gamePlayGroups[i].GamePlayes, gamePlayGroups[i].Title);
            }
        }

        private static void PrintSimulationResults(GamePlay[] gamePlayes, string groupTitle)
        {
            foreach (var item in gamePlayes)
            {
                string textGameResult = item.GetText_GameStatResult();

                using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title + "_log.txt"))
                {

                    item.GameStat.computeMedian.PrintDataGroup(tw);
                }

                using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title + "_RestCars_log.txt"))
                {

                    item.GameStat.computeMedianCardsLeft.PrintDataGroup(tw, item.GameStat.countRounds);
                }

                using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title + "_GameProgress_log.txt"))
                {

                    item.GameProgress.PrintProgress(tw);
                }


                foreach (var p in item.tgs.Players) {

                    if (p is Player_QLearning)
                    {
                        string fileName = groupTitle + "_" + item.Title + "_" + p.Name + "_QTable_log.txt";
                        
                        using (Stream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 32000, FileOptions.SequentialScan))
                        using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            ((Player_QLearning)p).PrintQTable(tw);
                        }

                    }

                    if (p is Player_QLearning_2)
                    {
                        string fileName = groupTitle + "_" + item.Title + "_" + p.Name + "_QTable_log.txt";

                        using (Stream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 32000, FileOptions.SequentialScan))
                        using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8))
                        {
                            ((Player_QLearning_2)p).PrintQTable(tw);
                        }

                    }


                    if (p is Player_DoubleQLearning)
                    {
                        using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title + "_" + p.Name + "_QTableA_log.txt"))
                        {
                            ((Player_DoubleQLearning)p).PrintQTableA(tw);
                        }

                        using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title + "_" + p.Name + "_QTableB_log.txt"))
                        {
                            ((Player_DoubleQLearning)p).PrintQTableB(tw);
                        }
                    }
                }

                if (Trace.Listeners.Count > 0)
                    Trace.WriteLine(textGameResult);

                Console.WriteLine(textGameResult);
            }
        }


        public static List<Player> CreatePlayers<T>(string[] names) where T : Player, new()
        {
            List<Player> result = new List<Player>(names.Length);

            foreach (string name in names)
            {
                T player = new T();
                player.Name = name;
                result.Add(player);
            }

            return result;
        }

        
    }

    internal class GamePlayGroup
    {
        public string Title;
        public GamePlay[] GamePlayes;

        public GamePlayGroup(string title, GamePlay[] gamePlayes)
        {
            this.GamePlayes = gamePlayes;
            this.Title = title;
        }
    }

    internal class GamePlay
    {
        public string Title;
        public TheGameSimulator tgs;

        public GameStat GameStat = new GameStat();

        public GameProgress GameProgress = new GameProgress(100);

        public GamePlay(List<Player> players, string title, StreamWriter playLog)
        {
            tgs = new TheGameSimulator(playLog);
            tgs.SetPlayers(players);

            this.Title = title;
           // Span<int> k = stackalloc int[5];
            

        }

        public string GetText_GameStatResult()
        {
            Utils.MedianResult<byte> medianResult = this.GameStat.computeMedian.Get_Median();

            string text = $"{Title}:  Avg. cards: {this.GameStat.sum / (double)this.GameStat.countRounds:0.###} Med. : { medianResult.Median,3} MostCount: {medianResult.MostCount.ToString(),3} Best score: {this.GameStat.bestGameScore.ToString(),3}";
            return text;
        }
    }

    internal class GameStat
    {
        public int sum = 0;
        public int countRounds = 0;
        public short bestGameScore = short.MaxValue;
        public Utils.Median<byte> computeMedian = new Utils.Median<byte>();
        public Utils.Median<byte> computeMedianCardsLeft = new Utils.Median<byte>();

        public void UpdateStats(GameResult result)
        {
            this.sum += result.Rest_Cards;
            countRounds++;
            computeMedian.Add((byte)result.Rest_Cards);

            foreach(var item in result.Rest_Cards_List)
            {
                computeMedianCardsLeft.Add(item);
            }

            if (result.Rest_Cards < bestGameScore)
                bestGameScore = (short)result.Rest_Cards;
        }
    }
}

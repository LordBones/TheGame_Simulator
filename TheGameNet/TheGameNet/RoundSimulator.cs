﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core;
using TheGameNet.Core.Players;

namespace TheGameNet
{
    internal class RoundSimulator
    {
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

        public static void SimulateGameRounds(GamePlayGroup[] gamePlayes, int rounds)
        {
            byte[] newGameDeck = GameBoard.Get_CreatedSuffledDeck();
            for (int r = 0; r < rounds; r++)
            {
                
                SimulateGameplays(gamePlayes, newGameDeck);
            }

            PrintSimulationResults(gamePlayes);
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

                using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title + "_GameProgress_log.txt"))
                {

                    item.GameProgress.PrintProgress(tw);
                }


                foreach (var p in item.tgs.Players) {

                    if (p is Player_QLearning)
                    {
                        using (TextWriter tw = File.CreateText(groupTitle + "_" + item.Title+"_"+p.Name + "_QTable_log.txt"))
                        {
                            ((Player_QLearning)p).PrintQTable(tw);
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


                Trace.WriteLine(textGameResult);
                Console.WriteLine(textGameResult);
            }
        }


        public static List<Player> CreatePlayers<T>(string[] names) where T : Player, new()
        {
            List<Player> result = new List<Player>();

            foreach (string name in names)
            {
                T player = new T();
                player.Name = name;
                result.Add(player);
            }

            return result;
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


            }

            public string GetText_GameStatResult()
            {
                Utils.MedianResult<byte> medianResult = this.GameStat.computeMedian.Get_Median();
                
                string text = $"{Title}:  Avg. cards: {this.GameStat.sum / (double)this.GameStat.countRounds:0.###} Med. : { medianResult.Median,3} MostCount: {medianResult.MostCount,3} Best score: {this.GameStat.bestGameScore,3}";
                return text;
            }
        }

        internal class GameStat
        {
            public int sum = 0;
            public int countRounds = 0;
            public short bestGameScore = short.MaxValue;
            public Utils.Median<byte> computeMedian = new Utils.Median<byte>();

            public void UpdateStats(GameResult result)
            {
                this.sum += result.Rest_Cards;
                countRounds++;
                computeMedian.Add((byte)result.Rest_Cards);

                if (result.Rest_Cards < bestGameScore)
                    bestGameScore = (short)result.Rest_Cards;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.Players;
using TheGameNet.RoundSimulations;

namespace TheGameNet.Testing
{
    internal class TestingQLearning
    {
        private static string[] namePlayers = new string[] { "ferda" };
        private static string[] namePlayers2 = new string[] { "ferda", "ondra" };
        private static string[] namePlayers3 = new string[] { "ferda", "ondra", "pepa" };
        private static string[] namePlayers4 = new string[] { "ferda", "ondra", "pepa", "jirka" };
        private static string[] namePlayers5 = new string[] { "ferda", "ondra", "pepa", "jirka", "jan" };


        public static void Run_QLearning_Teach()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 100000;


            var players1 = RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers);
            var players2 = RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers2);

            var players3 = RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3);
            var players4 = RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4);

            var players5 = RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5);

            var gpg = new List<GamePlayGroup>();
            string groupName = "QLearning";
            var gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers2), "2 players", groupName, false));
            //   gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, false));
            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName, false));

            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            //groupName = "QLearningDouble";

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers4), "4 players", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers2), "2 players", groupName, false));
            // gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers), "1 players", groupName, false));
            //gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            //gp.Clear();



            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds, true, 20, oneGlobalDeck: false);

            Core.QLearning.QTable qt = null;

            foreach (var item in players1)
            {
                ((Player_QLearning)item).TeachingEnable = false;
                qt = ((Player_QLearning)item).Get_QTable;
            }

            SetPlayersQT(qt, players2);
            SetPlayersQT(qt, players3);
            SetPlayersQT(qt, players4);
            SetPlayersQT(qt, players5);




            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_qlearning_log.txt");


            gpg = new List<GamePlayGroup>();
            gp.Clear();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));

            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players2, "2 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players3, "3 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players4, "4 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName + "_ControlTest", true));

            gpg.Add(new GamePlayGroup(groupName + "_ControlTest", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), 1000, true, 0, oneGlobalDeck: false);


            void SetPlayersQT(Core.QLearning.QTable qt, List<Player> players)
            {
                foreach (var item in players)
                {
                    ((Player_QLearning)item).TeachingEnable = false;
                    ((Player_QLearning)item).Set_QTable(qt);
                }
            }
        }

        public static void Run_QLearning_Teach_2()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 800000;


            var players1 = RoundSimulator.CreatePlayers<Player_QLearning_2>(namePlayers);
            var players2 = RoundSimulator.CreatePlayers<Player_QLearning_2>(namePlayers2);

            var players3 = RoundSimulator.CreatePlayers<Player_QLearning_2>(namePlayers3);
            var players4 = RoundSimulator.CreatePlayers<Player_QLearning_2>(namePlayers4);

            var players5 = RoundSimulator.CreatePlayers<Player_QLearning_2>(namePlayers5);

            var gpg = new List<GamePlayGroup>();
            string groupName = "QLearning";
            var gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers2), "2 players", groupName, false));
            //   gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, false));
            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName, false));

            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            //groupName = "QLearningDouble";

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers4), "4 players", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers2), "2 players", groupName, false));
            // gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers), "1 players", groupName, false));
            //gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            //gp.Clear();



            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds, true, 20, oneGlobalDeck: false);

            Core.QLearning.QTable qt = null;

            foreach (var item in players1)
            {
                ((Player_QLearning_2)item).TeachingEnable = false;
                qt = ((Player_QLearning_2)item).Get_QTable;
            }

            SetPlayersQT(qt, players2);
            SetPlayersQT(qt, players3);
            SetPlayersQT(qt, players4);
            SetPlayersQT(qt, players5);




            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_qlearning_log.txt");


            gpg = new List<GamePlayGroup>();
            gp.Clear();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));

            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players2, "2 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players3, "3 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players4, "4 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName + "_ControlTest", true));

            gpg.Add(new GamePlayGroup(groupName + "_ControlTest", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), 1000, true, 0, oneGlobalDeck: false);


            void SetPlayersQT(Core.QLearning.QTable qt, List<Player> players)
            {
                foreach (var item in players)
                {
                    ((Player_QLearning_2)item).TeachingEnable = false;
                    ((Player_QLearning_2)item).Set_QTable(qt);
                }
            }
        }

        private static void Run_QLearning_CompareWithOthers()
        {
            int countRounds = 1;

            List<GamePlayGroup> gpg = new List<GamePlayGroup>();
            string groupName = "Clasic";
            List<GamePlay> gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds);
        }


    }
}

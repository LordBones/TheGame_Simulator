using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.Players;
using static TheGameNet.RoundSimulator;

namespace TheGameNet
{
    internal class Testings


    {
        private static string[] namePlayers = new string[] { "ferda" };
        private static string[] namePlayers2 = new string[] { "ferda", "ondra" };
        private static string[] namePlayers3 = new string[] { "ferda", "ondra", "pepa" };
        private static string[] namePlayers4 = new string[] { "ferda", "ondra", "pepa", "jirka" };
        private static string[] namePlayers5 = new string[] { "ferda", "ondra", "pepa", "jirka", "jan" };


        public static void RunSimulate_Compare()
        {
            int countRounds = 100;

            //StreamWriter playLog_p5_selfish = new StreamWriter("gamePlay_5p_selfish_log.txt");
            //StreamWriter playLog_p4_selfish = new StreamWriter("gamePlay_4p_selfish_log.txt");
            //StreamWriter playLog_p3_selfish = new StreamWriter("gamePlay_3p_selfish_log.txt");
            //StreamWriter playLog_p2_selfish = new StreamWriter("gamePlay_2p_selfish_log.txt");
            //StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_selfish_log.txt");




            List<GamePlayGroup> gpg = new List<GamePlayGroup>();
            string groupName = "Selfish MinHarm";
            List<GamePlay> gp = new List<GamePlay>();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers4), "4 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers3), "3 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers2), "2 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            groupName = "Team MinHarm";
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers5), "5 players", groupName, true));
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers4), "4 players", groupName, true));
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers3), "3 players", groupName, true));
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers2), "2 players", groupName, true));
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers5), "5 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers4), "4 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers3), "3 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers2), "2 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers), "1 players"));
            //gpg.Add(new GamePlayGroup("Team MinHarm diff", gp.ToArray()));

            SimulateGameRounds(gpg.ToArray(), countRounds);


        }

        public static void Run_QLearning_Teach()
        {
            int countRounds = 180000;

            List<GamePlayGroup> gpg = new List<GamePlayGroup>();
            string groupName = "QLearning";
            List<GamePlay> gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers2), "2 players", groupName, false));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers), "1 players", groupName, false));
            
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            groupName = "QLearningDouble";

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers2), "2 players", groupName, false));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_DoubleQLearning>(namePlayers), "1 players", groupName, false));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));


            SimulateGameRounds(gpg.ToArray(), countRounds);
        }

        public static void RunSimulate_Test_PMP()
        {
            int countRounds = 100;

            //StreamWriter playLog_p5_selfish = new StreamWriter("gamePlay_5p_selfish_log.txt");
            //StreamWriter playLog_p4_selfish = new StreamWriter("gamePlay_4p_selfish_log.txt");
            //StreamWriter playLog_p3_selfish = new StreamWriter("gamePlay_3p_selfish_log.txt");
            //StreamWriter playLog_p2_selfish = new StreamWriter("gamePlay_2p_selfish_log.txt");
            //StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_selfish_log.txt");




            List<GamePlayGroup> gpg = new List<GamePlayGroup>();
            string groupName = "Soliter_Minharm";
            List<GamePlay> gp = new List<GamePlay>();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers2), "2 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            groupName = "Team MinHarm";
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers2), "2 players", groupName, true));
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();


            groupName = "Soliter MaxPosibilites";
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers2), "2 players", groupName, true));
            gp.Add(CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            SimulateGameRounds(gpg.ToArray(), countRounds);

        }

    }
}

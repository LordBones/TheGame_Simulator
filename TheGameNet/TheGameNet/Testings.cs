﻿using BonesLib.FlexibleForwardNN;
using BonesLib.ForwardNN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.FNN;
using TheGameNet.Core.Players;
using TheGameNet.RoundSimulations;

namespace TheGameNet
{
    public class Testings


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
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers5), "5 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers4), "4 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers3), "3 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers2), "2 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers5), "5 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers4), "4 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers3), "3 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers2), "2 players"));
            //gp.Add(new GamePlay(CreatePlayers<Team_MinHarm_2_Player>(namePlayers), "1 players"));
            //gpg.Add(new GamePlayGroup("Team MinHarm diff", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds);


        }

        public static void Run_GA_Learn()
        {
            int countRounds = 200;

            List<GamePlayGroup> gpg = new List<GamePlayGroup>();
            string groupName = "Clasic";
            List<GamePlay> gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers2), "2 players min harm", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers3), "3 players min harm", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers4), "4 players min harm", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, false));

            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers2), "2 players pmp", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers3), "3 players pmp", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers4), "4 players pmp", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers5), "5 players pmp", groupName, false));


            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_GAConfigurable>(namePlayers), "1 players GA", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_GAConfigurable>(namePlayers2), "2 players GA", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_GAConfigurable>(namePlayers3), "3 players GA", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_GAConfigurable>(namePlayers4), "4 players GA", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_GAConfigurable>(namePlayers5), "5 players GA", groupName, false));

            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds);
        }

        public static void FNN_Testing()
        {
            BonesLib.ForwardNN.ForwardNN fnn = new ForwardNN(100, 100);
            fnn.SetTopology(new short[] { 100 });

            FNN_LayerManipulator manipulator = new FNN_LayerManipulator(0);
            manipulator.InitRandomWeights(fnn.Layers);

            fnn.Inputs[0] = 0.05f;
            fnn.Inputs[3] = 0.08f;

            for (int i = 0; i < 100000; i++)
            {
                fnn.Evaluate();
            }

        }

        public static void FlexibleFNN_Testing()
        {
            FlexibleForwardNN fnn = new FlexibleForwardNN(100, 100);
            fnn.Layers.Init(100, 100, 300, 30000);
            fnn.SetTopology(new short[] { 100,100,100 });

            FlexibleFNN_LayerManipulator manipulator = new FlexibleFNN_LayerManipulator(0);
            manipulator.InitRandomWeights(fnn.Layers);

            fnn.Inputs[0] = 0.05f;
            fnn.Inputs[3] = 0.08f;

            for (int i = 0; i < 100000; i++)
            {
                fnn.Evaluate();
            }

        }
       

        public static void Run_FNN_Teach()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 1100;


            var players1 = RoundSimulator.CreatePlayers<Player_FNN>(namePlayers);
            var players5 = RoundSimulator.CreatePlayers<Player_FNN>(namePlayers5);


            var playerFnn = ((Player_FNN)players1[0]).Fnn;

            FNN_LayerManipulator fnnManipul = new FNN_LayerManipulator(0);
            fnnManipul.InitRandomWeights(playerFnn.Layers);

            var evolveProgress = new StreamWriter($"FNN_EvolveProgress_1P_log.txt");

            FNN_Evolve fe = new FNN_Evolve(5, 40);
            fe.RunEvolution(countRounds, playerFnn, evolveProgress);
            evolveProgress.Dispose();

            ((Player_FNN)players1[0]).Fnn = fe.Best_fnn;

            Console.WriteLine("best:" + fe.Best_fitness.ToString());

            var gpg = new List<GamePlayGroup>();
            string groupName = "FNN";
            var gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers2), "2 players", groupName, false));
            //   gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, false));
           // gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName, false));

            //gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

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

            

            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_fnn_log.txt");


            gpg = new List<GamePlayGroup>();
            gp.Clear();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));

            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName + "_ControlTest", true));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName + "_ControlTest", true));

            gpg.Add(new GamePlayGroup(groupName + "_ControlTest", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), 100, true, 0, oneGlobalDeck: false);
        }

        public static void Run_FlexibleFNN_Teach()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 800;


            var players1 = RoundSimulator.CreatePlayers<Player_FlexibleFNN>(namePlayers);
            var players5 = RoundSimulator.CreatePlayers<Player_FlexibleFNN>(namePlayers5);


            var playerFnn = ((Player_FlexibleFNN)players1[0]).Fnn;

            FlexibleFNN_LayerManipulator fnnManipul = new FlexibleFNN_LayerManipulator(0);
           // fnnManipul.InitRandomWeights(playerFnn.Layers);

            var evolveProgress = new StreamWriter($"FlexibleFNN_EvolveProgress_1P_log.txt");
            var evolutionDebug = new StreamWriter($"EvolutionDebug.txt");

            FlexibleFNN_Evolve fe = new FlexibleFNN_Evolve(5, 100);
            fe.Enable_Log(evolutionDebug);
            fe.RunEvolution(countRounds, playerFnn, evolveProgress);
            evolveProgress.Dispose();

            ((Player_FlexibleFNN)players1[0]).Fnn = fe.Best_fnn;

            Console.WriteLine("best:" + fe.Best_fitness.ToString(".000"));
            StreamWriter bestFnnLog = new StreamWriter("flexiblefnn_BestNN.txt");

            fe.Best_fnn.PrintNetwork(bestFnnLog);
            StreamWriter bestFnnLog_GraphWiz = new StreamWriter("fnn_BestNN.grwiz");
            fe.Best_fnn.PrintNetworkForGraphWiz(bestFnnLog_GraphWiz);

            var gpg = new List<GamePlayGroup>();
            string groupName = "FlexibleFNN";
            var gp = new List<GamePlay>();
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers5), "5 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers4), "4 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers3), "3 players", groupName, false));

            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers2), "2 players", groupName, false));
            //   gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, false));
            // gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName, false));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName, false));

            //gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

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



            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_flexiblefnn_log.txt");


            gpg = new List<GamePlayGroup>();
            gp.Clear();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));

            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName + "_ControlTest", true));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName + "_ControlTest", true));

            gpg.Add(new GamePlayGroup(groupName + "_ControlTest", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), 1000, true, 0, oneGlobalDeck: false);
        }

        public static void Run_QLearning_Teach()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 100000;


            var players1 = RoundSimulator.CreatePlayers<Player_QLearning>(namePlayers);
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

           

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds, true, 20, oneGlobalDeck: true);


            foreach(var item in players1)
            {
                ((Player_QLearning)item).TeachingEnable = false;
            }


            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_qlearning_log.txt");


            gpg = new List<GamePlayGroup>();
            gp.Clear();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));

            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName+"_ControlTest", true));
            //gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName + "_ControlTest", true));

            gpg.Add(new GamePlayGroup(groupName + "_ControlTest", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), 100, true, 0,  oneGlobalDeck:false);

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
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers2), "2 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Team_MinHarm>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();


            groupName = "Soliter MaxPosibilites";
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers2), "2 players", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players", groupName, true));
            gpg.Add(new GamePlayGroup(groupName, gp.ToArray()));

            gp.Clear();

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds);

        }

    }
}

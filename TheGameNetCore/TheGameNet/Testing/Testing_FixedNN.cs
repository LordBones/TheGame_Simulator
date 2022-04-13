using BonesLib.FixedForwardNN;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGameNet.Core.Players;
using TheGameNet.RoundSimulations;

namespace TheGameNet.Testing
{
    internal class Testing_FixedNN
    {
        private static string[] namePlayers = new string[] { "ferda" };
        private static string[] namePlayers2 = new string[] { "ferda", "ondra" };
        private static string[] namePlayers3 = new string[] { "ferda", "ondra", "pepa" };
        private static string[] namePlayers4 = new string[] { "ferda", "ondra", "pepa", "jirka" };
        private static string[] namePlayers5 = new string[] { "ferda", "ondra", "pepa", "jirka", "jan" };



        public static void BasicEvaluation()
        {
            var input = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

            var expected = new float[] { 0.2f, 0.9f, 0.4f, 0.5f, 0.1f };

            var ffnn = new FixedForwardNN(5, 5);
            ffnn.SetTopology(new short[] { 4,2,4 });
            ffnn.InitBaseWeights();

            input.AsSpan().CopyTo(ffnn.Inputs);


            ffnn.Evaluate();

            for (int i = 0; i < 400; i++)
            {
                ffnn.BackPropagate(0.1f, expected,1.0f,1.0f);

                input.AsSpan().CopyTo(ffnn.Inputs);
                
                ffnn.Evaluate();
                float error = Error(expected, ffnn.Outputs);
                Trace.WriteLine("Error:"+error.ToString());
               
            }
            Trace.WriteLine( string.Join(", ", ffnn.Outputs.ToArray().Select(x => x.ToString())));
        }

        public static float Error(Span<float> tValues, Span<float> yValues)
        {
            float sum = 0.0f;
            for (int i = 0; i < tValues.Length; ++i)
                sum += (tValues[i] - yValues[i]) * (tValues[i] - yValues[i]);
            return (float)Math.Sqrt(sum);
        }

        public static void BenchForward()
        {
            var input = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

            var expected = new float[] { 0.2f, 0.9f, 0.4f, 0.5f, 0.1f };

            var ffnn = new FixedForwardNN(5, 5);
            ffnn.SetTopology(new short[] { 460, 100, 50, 50 });
            ffnn.InitBaseWeights();

            Stopwatch performanceCounter = new Stopwatch();
            performanceCounter.Start();

            //Array.Copy(input, ffnn.Inputs, input.Length);

            //ffnn.Evaluate();
            long totalWeights = 0;
            long totalNet = 0;
            var totalWeightsFnn = ffnn.GetTotalWeighs();
            for (int i = 0; i < 400000; i++)
            {
                input.AsSpan().CopyTo(ffnn.Inputs);

                ffnn.Evaluate();
                //float error = Error(expected, ffnn.Outputs);

               // ffnn.BackPropagate(0.1f, expected, 1.0f, 1.0f);

                totalWeights += totalWeightsFnn;
                totalNet++;
            }

            performanceCounter.Stop();
            var upt = Process.GetCurrentProcess().UserProcessorTime.TotalSeconds;

            Console.WriteLine(string.Format("Total {0,0:N3} s ", performanceCounter.Elapsed.TotalSeconds));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture , "new weights: {0}  -  {1,0:N3} W/s - {2,0:N3} ns/W -  {3,0:N3} Net/s ", ffnn.GetTotalWeighs(),  totalWeights /  performanceCounter.Elapsed.TotalSeconds,  ( performanceCounter.Elapsed.TotalSeconds/ totalWeights)*1_000_000_000, totalNet / performanceCounter.Elapsed.TotalSeconds));


           // Trace.WriteLine(string.Join(", ", ffnn.Outputs.Select(x => x.ToString())));
        }

        public static void Bench_backward()
        {
            var input = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f };

            var expected = new float[] { 0.2f, 0.9f, 0.4f, 0.5f, 0.1f };

            var ffnn = new FixedForwardNN(5, 5);
            ffnn.SetTopology(new short[] { 460, 100, 50, 50 });
            ffnn.InitBaseWeights();

            Stopwatch performanceCounter = new Stopwatch();
            performanceCounter.Start();

            input.AsSpan().CopyTo(ffnn.Inputs);

            ffnn.Evaluate();
            long totalWeights = 0;
            long totalNet = 0;
            for (int i = 0; i < 400000; i++)
            {
                //Array.Copy(input, ffnn.Inputs, input.Length);

                //ffnn.Evaluate();
                //float error = Error(expected, ffnn.Outputs);

                ffnn.BackPropagate(0.1f, expected, 1.0f, 1.0f);

                totalWeights += ffnn.GetTotalWeighs();
                totalNet++;
            }

            performanceCounter.Stop();
            var upt = Process.GetCurrentProcess().UserProcessorTime.TotalSeconds;

            Console.WriteLine(string.Format("Total {0,0:N3} s ", performanceCounter.Elapsed.TotalSeconds));
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "new weights: {0}  -  {1,0:N3} W/s - {2,0:N3} ns/W -  {3,0:N3} Net/s ", ffnn.GetTotalWeighs(), totalWeights / performanceCounter.Elapsed.TotalSeconds, (performanceCounter.Elapsed.TotalSeconds / totalWeights) * 1_000_000_000, totalNet / performanceCounter.Elapsed.TotalSeconds));


            // Trace.WriteLine(string.Join(", ", ffnn.Outputs.Select(x => x.ToString())));
        }



        public static void Run_FixedNN_Teach()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 10000;


            var players1 = RoundSimulator.CreatePlayers<Player_FixedNN>(namePlayers);
            var players2 = RoundSimulator.CreatePlayers<Player_FixedNN>(namePlayers2);

            var players3 = RoundSimulator.CreatePlayers<Player_FixedNN>(namePlayers3);
            var players4 = RoundSimulator.CreatePlayers<Player_FixedNN>(namePlayers4);

            var players5 = RoundSimulator.CreatePlayers<Player_FixedNN>(namePlayers5);

            var sqt = ((Player_FixedNN)players1[0]).Fnn;

            using (StreamWriter tw = File.CreateText("TeachedNN_Start.txt"))
            {
                tw.AutoFlush = false;
                sqt.PrintNet(tw);
                tw.Flush();
            }

            var gpg = new List<GamePlayGroup>();
            string groupName = "FixedNN";
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


            int seed = 
           //     (int)(DateTime.UtcNow.Ticks % int.MaxValue);
            20;
            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds, true, seed,
                oneGlobalDeck: false, batchDeckSize:2000);
            
           
            ((Player_FixedNN)players1[0]).TeachingEnable = false;
            var qt = ((Player_FixedNN)players1[0]).Fnn;

            using (StreamWriter tw = File.CreateText("TeachedNN.txt"))
            {
                tw.AutoFlush = false;
                qt.PrintNet(tw);
                tw.Flush();
            }

            SetPlayersQT(qt.Clone(), players2);
            SetPlayersQT(qt.Clone(), players3);
            SetPlayersQT(qt.Clone(), players4);
            SetPlayersQT(qt.Clone(), players5);




            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_BPNNFixed_log.txt");


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


            void SetPlayersQT(FixedForwardNN qt, List<Player> players)
            {
                foreach (var item in players)
                {
                    ((Player_FixedNN)item).TeachingEnable = false;
                    ((Player_FixedNN)item).Fnn = qt;
                }
            }

           
        }

        public static void Run_FixedNN_Multioutput_Teach()
        {
            //Run_QLearning_CompareWithOthers();

            int countRounds = 14000;


            var players1 = RoundSimulator.CreatePlayers<Player_FIxedNN_MultiOutput>(namePlayers);
            var players2 = RoundSimulator.CreatePlayers<Player_FIxedNN_MultiOutput>(namePlayers2);

            var players3 = RoundSimulator.CreatePlayers<Player_FIxedNN_MultiOutput>(namePlayers3);
            var players4 = RoundSimulator.CreatePlayers<Player_FIxedNN_MultiOutput>(namePlayers4);

            var players5 = RoundSimulator.CreatePlayers<Player_FIxedNN_MultiOutput>(namePlayers5);

            ((Player_FIxedNN_MultiOutput)players1[0]).TeachingEnable = true;
            var sqt = ((Player_FIxedNN_MultiOutput)players1[0]).Fnn;

            using (StreamWriter tw = File.CreateText("TeachedNN_Start.txt"))
            {
                tw.AutoFlush = false;
                sqt.PrintNet(tw);
                tw.Flush();
            }

            var gpg = new List<GamePlayGroup>();
            string groupName = "FixedNN_Multioutput";
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



            RoundSimulator.SimulateGameRounds(gpg.ToArray(), countRounds, true, 20,
                oneGlobalDeck: false, batchDeckSize: 100);


            ((Player_FIxedNN_MultiOutput)players1[0]).TeachingEnable = false;
            var qt = ((Player_FIxedNN_MultiOutput)players1[0]).Fnn;

            using (StreamWriter tw = File.CreateText("TeachedNN.txt"))
            {
                tw.AutoFlush = false;
                qt.PrintNet(tw);
                tw.Flush();
            }

            SetPlayersQT(qt.Clone(), players2);
            SetPlayersQT(qt.Clone(), players3);
            SetPlayersQT(qt.Clone(), players4);
            SetPlayersQT(qt.Clone(), players5);




            StreamWriter playLog_p1_selfish = new StreamWriter("gamePlay_1p_BPNNFixed_log.txt");


            gpg = new List<GamePlayGroup>();
            gp.Clear();
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers), "1 players min harm", groupName, true));
            gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_MinHarm>(namePlayers5), "5 players min harm", groupName, true));
            //gp.Add(RoundSimulator.CreateGamePlay(RoundSimulator.CreatePlayers<Player_Soliter_PreserveMaxPossibilites>(namePlayers), "1 players pmp", groupName, true));

            gp.Add(RoundSimulator.CreateGamePlay(players1, "1 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players2, "2 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players3, "3 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players4, "4 players", groupName + "_ControlTest", true));
            gp.Add(RoundSimulator.CreateGamePlay(players5, "5 players", groupName + "_ControlTest", true));

            gpg.Add(new GamePlayGroup(groupName + "_ControlTest", gp.ToArray()));

            RoundSimulator.SimulateGameRounds(gpg.ToArray(), 1000, true, 0, oneGlobalDeck: false);


            void SetPlayersQT(FixedForwardNN qt, List<Player> players)
            {
                foreach (var item in players)
                {
                    ((Player_FIxedNN_MultiOutput)item).TeachingEnable = false;
                    ((Player_FIxedNN_MultiOutput)item).Fnn = qt;
                }
            }


        }

    }
}

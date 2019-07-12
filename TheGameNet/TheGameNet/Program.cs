using System;
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
    class Program
    {
        private static string[] namePlayers = new string[]{"ferda"};
        private static string[] namePlayers2 = new string[] { "ferda", "ondra" };
        private static string[] namePlayers3 = new string[] { "ferda", "ondra", "pepa"};
        private static string[] namePlayers4 = new string[] { "ferda", "ondra", "pepa", "jirka" };
        private static string[] namePlayers5 = new string[] { "ferda", "ondra", "pepa", "jirka", "jan" };



        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;    

            Stopwatch performanceCounter = new Stopwatch();
            performanceCounter.Start();

            //BaseSimulate();
          

            //Testings.RunSimulate_Compare();
            //Testings.RunSimulate_Test_PMP();
            Testings.Run_QLearning_Teach();



            //Console.WriteLine("Hello World!");
            performanceCounter.Stop();

            Console.WriteLine(string.Format("{0,000} s ", performanceCounter.Elapsed.TotalSeconds));
            Console.WriteLine($"GC 0: {GC.CollectionCount(0),6} 1:{GC.CollectionCount(1),6} 2:{GC.CollectionCount(2),6}"); 
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.Error.WriteLine(e.ExceptionObject.ToString());
            
        }

        static void BaseSimulate()
        {

            List<Player> players = new List<Player>();
            players.Add(new Player_Dumb("Ferda"));
            players.Add(new Player_Dumb("Ondra"));
            players.Add(new Player_Dumb("Pepa"));

            TheGameSimulator tgs = new TheGameSimulator(null);
            tgs.SetPlayers(players);

            byte[] newGameDeck = GameBoard.Get_CreatedSuffledDeck();
            var gameResult = tgs.Simulate(newGameDeck);

            Trace.WriteLine($"Result game: {gameResult.Rest_Cards}");
            Console.WriteLine($"Result game: {gameResult.Rest_Cards}");
        }

       

       

       
        
    }
}

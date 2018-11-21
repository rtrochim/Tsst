using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
namespace TSST
{
    class CableCloud
    {
        SenderSocket sender;
        ListenerSocket listener;
        public static List<int> portNums;
        public static int currentPortNum;

        static void Main(string[] args)
        {
            string[] lines = File.ReadAllLines(args[0]);
            portNums = new List<int>();
            foreach (string line in lines)
            {
                portNums.Add(Int32.Parse(line));
            }
            CableCloud cc = new CableCloud();
            Console.ReadKey();

        }

        public CableCloud()
        {
            Console.WriteLine(@"
   _____          ____  _      ______    _____ _      ____  _    _ _____  
  / ____|   /\   |  _ \| |    |  ____|  / ____| |    / __ \| |  | |  __ \ 
 | |       /  \  | |_) | |    | |__    | |    | |   | |  | | |  | | |  | |
 | |      / /\ \ |  _ <| |    |  __|   | |    | |   | |  | | |  | | |  | |
 | |____ / ____ \| |_) | |____| |____  | |____| |___| |__| | |__| | |__| |
  \_____/_/    \_\____/|______|______|  \_____|______\____/ \____/|_____/ 
                                                                          
                                                                          ");
            this.sender = new SenderSocket();
            for (int i = 0; i < portNums.Count; i++)
            {
                currentPortNum = portNums[i];
                Console.WriteLine("PortNumber {0}", currentPortNum);
                ThreadStart childref = new ThreadStart(listeningThread);
                Thread childThread = new Thread(childref);
                childThread.Start();
                Thread.Sleep(100);
            }
        }

        public void listeningThread()
        {
            Console.WriteLine("I am listening thread");
            this.listener = new ListenerSocket(currentPortNum);
        }
    }
}

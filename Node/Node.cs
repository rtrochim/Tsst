using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace TSST
{
    class Node
    {
        public ListenerSocket listener;
        public SenderSocket sender;
        public static int listenerPort;
        public static int targetPort;
        public SwitchingField sf;
   
        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            string[] lines = File.ReadAllLines(args[0]);
            listenerPort = Int32.Parse(lines[0]);
            targetPort = Int32.Parse(lines[1]);
            Node n = new Node();
            Console.ReadKey();
        }

        public Node()
        {
            Console.WriteLine(@"
  _   _  ____  _____  ______ 
 | \ | |/ __ \|  __ \|  ____|
 |  \| | |  | | |  | | |__   
 | . ` | |  | | |  | |  __|  
 | |\  | |__| | |__| | |____ 
 |_| \_|\____/|_____/|______|
                             
                             
");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            this.sender = new SenderSocket();
            //this.sender.sendMessage("Hello world from node!<EOF>", targetPort);
            sf = new SwitchingField();
        }

        public void listeningThread()
        {
            Console.WriteLine("Node listening on port {0}", listenerPort);
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort);
        }
    }
}

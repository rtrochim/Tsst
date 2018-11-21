using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace TSST
{

    class Client
    {
        public ListenerSocket listener;
        public SenderSocket sender;
        public static int listenerPort;
        public static int targetPort;

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            string[] lines = File.ReadAllLines(args[0]);
            listenerPort = Int32.Parse(lines[0]);
            targetPort = Int32.Parse(lines[1]);
            Client c = new Client();
            Console.ReadKey();
        }

        public Client()
        {
            Console.WriteLine(@"
   _____ _      _____ ______ _   _ _______ 
  / ____| |    |_   _|  ____| \ | |__   __|
 | |    | |      | | | |__  |  \| |  | |   
 | |    | |      | | |  __| | . ` |  | |   
 | |____| |____ _| |_| |____| |\  |  | |
  \_____|______|_____|______|_| \_|  |_|   
                                           
                                           ");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            this.sender = new SenderSocket();
            this.sender.sendMessage("Hello world from Client!<EOF>", targetPort);
        }

        public void listeningThread()
        {
            Console.WriteLine("Client listening on port {0}", listenerPort);
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort);
        }
    }
}

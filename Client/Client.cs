using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace TSST
{

    class Client
    {

        static void Main(string[] args)
        {
            Client c = new Client();
            Console.ReadKey();
        }

        
        public ListenerSocket listener;
        public SenderSocket sender;

        public void listeningThread()
        {
            Console.WriteLine("I am listening thread");
            this.listener = new ListenerSocket(11001);
        }

        public Client()
        {
            Console.WriteLine("I am Client");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            this.sender = new SenderSocket();
            this.sender.sendMessage("Hello world!<EOF>", 11000);
        }
    }
}

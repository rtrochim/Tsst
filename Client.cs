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
        List<Port> ports;
        SwitchingField switchingField;
        Agent agent;
        public ListenerSocket listener;
        public SenderSocket sender;

        public void listeningThread()
        {
            Console.WriteLine("I am listening");
            this.listener = new ListenerSocket(11001);

        }

        public Client()
        {
            Console.WriteLine("I am Client");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            Thread.Sleep(2000);
            this.sender = new SenderSocket();
            //sender.sendMessage("My test message");
        }
    }
}

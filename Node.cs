using System;
using System.Collections.Generic;
using System.Threading;

namespace TSST
{
    class Node
    {
        List<Port> ports;
        public ListenerSocket listener;
        public SenderSocket sender;

        public void listeningThread()
        {
            Console.WriteLine("I am listening");
            this.listener = new ListenerSocket(11000);
        }

        public Node()
        {
            Console.WriteLine("I am Node");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            this.sender = new SenderSocket();
        }
    }
}

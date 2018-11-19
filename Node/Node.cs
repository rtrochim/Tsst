using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TSST
{
    class Node
    {
        static void Main(string[] args)
        {
            Node n = new Node();
            Console.ReadKey();
        }
        List<Port> ports;
        SwitchingField switchingField;
        Agent agent;

        public ListenerSocket listener;
        public SenderSocket sender;

        public void listeningThread()
        {
            Console.WriteLine("I am listening thread");
            this.listener = new ListenerSocket(11000);
        }

        public Node()
        {
            Console.WriteLine("I am Node");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }
    }
}

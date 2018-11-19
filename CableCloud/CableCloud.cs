using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TSST
{
    class CableCloud
    {
        SenderSocket sender;
        ListenerSocket listener;
        static void Main(string[] args)
        {
            CableCloud cc = new CableCloud();
            Console.ReadKey();

        }

        public CableCloud()
        {
            Console.WriteLine("CableCloud created!");
            this.sender = new SenderSocket();
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
        }

        public void listeningThread()
        {
            Console.WriteLine("I am listening thread");
            this.listener = new ListenerSocket(11003);
        }
    }
}

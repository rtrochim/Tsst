using System;
using System.Collections.Generic;
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
        //Connections between our Network Elements
        public List<Tuple<int,int>> connections;
        public Packet packet;

        

        static void Main(string[] args)
        {
            Console.SetWindowSize(75, 20);
            string[] lines = File.ReadAllLines(args[0]);
            portNums = new List<int>();
            foreach (string line in lines)
            {
                if (line[0] != '#')
                {
                    portNums.Add(Int32.Parse(line));
                }
            }
            CableCloud cc = new CableCloud();
            cc.readConnections();
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
                ThreadStart childref = new ThreadStart(listeningThread);
                Thread childThread = new Thread(childref);
                childThread.Start();
                Thread.Sleep(100);
            }
            this.connections = new List<Tuple<int,int>>();
        }
        public void listeningThread()
        {
            this.listener = new ListenerSocket(currentPortNum, handlePacket);
        }
        public void readConnections()
        {
            string[] lines = File.ReadAllLines("..\\..\\..\\TEST\\configs\\NetworkConnections.conf");
            foreach(string line in lines)
            {
                string[] ports = line.Split(' ');
                this.connections.Add(new Tuple<int,int>(Int32.Parse(ports[0]), Int32.Parse(ports[1])));
            }
        }


        public int handlePacket(Packet p, int port)
        {
            packet = p;
            Console.WriteLine("Got packet with data: {0} \n on port {1}, \n sending to port {2}", packet.data, port , packet.nextHop);
            if (this.connections.Find(item => (item.Item1 == packet.nextHop)) != null || this.connections.Find(item => (item.Item2 == packet.nextHop)) != null)
            {
                sender.sendMessage(packet.serialize(), packet.nextHop);
            }
            else
            {
                Console.WriteLine("Cannot send packet to port {0} - no such connection!", packet.nextHop);
            }
            return 0;
        }
    }
}

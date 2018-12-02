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
        //Connections between our Network Elements
        public Dictionary<int, int> connections;
        //Map packet.targetPort to <inputPort,outputPort> 
        public Dictionary<int, Dictionary<int, int>> targetPorts;
        public Packet packet;

        

        static void Main(string[] args)
        {
            Console.SetWindowSize(75, 20);
            Console.SetWindowPosition(0, 0);
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
            cc.readTargetPorts();
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
            this.connections = new Dictionary<int, int>();
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
                this.connections.Add(Int32.Parse(ports[0]), Int32.Parse(ports[1]));
            }
        }

        public void readTargetPorts()
        {
            string[] lines = File.ReadAllLines("..\\..\\..\\TEST\\configs\\CableCloudTargetPorts.conf");
            targetPorts = new Dictionary<int, Dictionary<int, int>>();
            foreach (string line in lines)
            {
                string[] ports = line.Split(' ');
                if (!targetPorts.ContainsKey(Int32.Parse(ports[0])))
                {
                    targetPorts.Add(Int32.Parse(ports[0]), new Dictionary<int, int> {
                        {Int32.Parse(ports[1]),Int32.Parse(ports[2])}
                    });
                }
                else
                {
                    targetPorts[Int32.Parse(ports[0])].Add(Int32.Parse(ports[1]), Int32.Parse(ports[2]));
                }
            }
        }

        public int handlePacket(Packet p, int port)
        {
            packet = p;
            Console.WriteLine("PORT: {0}", port);
            Console.WriteLine("TargetPort: {0}", packet.targetPort);
            Console.WriteLine("Got packet with data: {0} \n sending to port {1}", packet.data, targetPorts[packet.targetPort][port]);
            sender.sendMessage(packet.serialize(), targetPorts[packet.targetPort][port]);
            return 0;
        }
    }
}

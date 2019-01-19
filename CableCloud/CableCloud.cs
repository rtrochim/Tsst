using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace TSST
{

    class CableCloud
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        SenderSocket sender;
        ListenerSocket listener;
        public static List<int> portNums;
        public static int currentPortNum;
        //Connections between our Network Elements
        public List<Tuple<int,int>> connections;
        public Packet packet;
        public Dictionary<int, List<int>> interfacesMap;


        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[1]), Int32.Parse(args[2]), 1000, 400, true);
            Console.SetWindowSize(75, 18);

            string[] lines = File.ReadAllLines(args[0]);
            portNums = new List<int>();
            foreach (string line in lines)
            {
                if (line[0] != '#')
                {
                    portNums.Add(Int32.Parse(line.Split(' ')[0]));
                }
            }
            CableCloud cc = new CableCloud();
            cc.readConnections();
            cc.readInterfaces(args[0]);
            while (true)
            {
                Console.WriteLine(@"
[L] List connections
[T] Terminate connection
[A] Add connection
What to do:");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "L":
                        cc.listConnections();
                        break;
                    case "A":
                        cc.addConnection();
                        break;
                    case "T":
                        cc.terminateConnection();
                        break;
                    default:
                        Console.WriteLine("Invalid option!");
                        break;

                }
            }
        }

        public CableCloud()
        {
            Console.WriteLine(@"
   _____          ____  _      ______    _____ _      ____  _    _ _____  
  / ____|   /\   |  _ \| |    |  ____|  / ____| |    / __ \| |  | |  __ \ 
 | |       /  \  | |_) | |    | |__    | |    | |   | |  | | |  | | |  | |
 | |      / /\ \ |  _ <| |    |  __|   | |    | |   | |  | | |  | | |  | |
 | |____ / ____ \| |_) | |____| |____  | |____| |___| |__| | |__| | |__| |
  \_____/_/    \_\____/|______|______|  \_____|______\____/ \____/|_____/ ");

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
            int[,] topology;
            string[] lines = File.ReadAllLines("..\\..\\..\\TEST\\configs\\Topology.conf");
            topology = new int[lines.Length, lines.Length];
            int i = 0;
            int j = 0;
            foreach (string line in lines)
            {
                foreach (string l in line.Split(','))
                {
                    topology[i, j] = Int32.Parse(l);
                    j++;
                }
                j = 0;
                i++;
            }
            for (int m = 0; m < topology.GetLength(0); m++)
            {
                for (int n = 0; n < topology.GetLength(0); n++)
                {
                    if (topology[m, n] != 0)
                    {
                        if (!this.connections.Contains(new Tuple<int,int>(m, n)) && !this.connections.Contains(new Tuple<int,int>(n, m)))
                        {
                            this.connections.Add(new Tuple<int, int>(m, n));
                        }
                    }
                }
            }
            foreach(Tuple<int,int> item in this.connections)
            {
                Console.WriteLine("Item1: " + item.Item1.ToString() +"Item2: "+ item.Item2.ToString());
            }
        }


        public int handlePacket(Packet p, int port)
        {
            lock (this)
            {
                Thread.Sleep(400);
                packet = p;
                Console.WriteLine("Got packet with data: {0} \n on port {1}, \n+ sending to port {2}", packet.data, port, packet.nextHop);
                int targetPort = 0;
                int targetNode = -1;
                foreach (KeyValuePair<int, List<int>> item in interfacesMap)
                {
                    if(item.Value.Contains(packet.nextHop))
                    {
                        targetPort = item.Key;
                    }
                }
                Console.WriteLine("TargetPort:" + targetPort.ToString());
                if (this.connections.Exists(item => ((item.Item1 == targetPort - 11100 && item.Item2 == port - 12100)||(item.Item2 == targetPort - 11100 && item.Item1 == port - 12100))) || targetPort == packet.targetPort || Enumerable.Range(12000, 12003).Contains(port))
                {
                    sender.sendMessage(packet.serialize(), targetPort);
                    Thread.Yield();
                }
                else
                {
                    Console.WriteLine("Cannot send packet to interface {0} - no such connection!", packet.nextHop);
                    Thread.Yield();
                }
                return 0;
            }
        }

        public void listConnections()
        {
          
        }

        public void terminateConnection()
        {
            
        }

        public void addConnection()
        {
            
        }
        public void readInterfaces(string path)
        {
            this.interfacesMap = new Dictionary<int, List<int>>();
            string[] lines = File.ReadAllLines(path);
            foreach(string line in lines)
            {
                string[] values = line.Split(' ');
                List<int> interfaces = new List<int>();
                int i = 0;
                foreach(string value in values)
                {
                    if (i > 1)
                    {
                        interfaces.Add(Int32.Parse(value));
                    }
                    i++;
                }
                this.interfacesMap.Add(Int32.Parse(values[1]), interfaces);
            }
            foreach(KeyValuePair<int, List<int>> item in this.interfacesMap)
            {
                Console.WriteLine("Interface"+item.Key+":");
                foreach(int dupa in item.Value)
                {
                    Console.Write(dupa.ToString()+",");
                }
                Console.Write(Environment.NewLine);
            }
        }
    }
}

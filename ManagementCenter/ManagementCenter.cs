using System;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Collections.Generic;

namespace TSST
{
    class ManagementCenter
    {
        public int listeningPort = 13000;
        public List<Tuple<int, int>> adjacentNodes;
        public ListenerSocket listener;
        public WebServer server;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        int[,] topology;
        Dijkstra dijkstra;

        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[0]), Int32.Parse(args[1]), 1000, 400, true);

            Console.SetWindowSize(75, 18);
            ManagementCenter mc = new ManagementCenter();
            lock (mc)
            {
                mc.server.Run();
                while (true)
                {
                    Console.ReadKey();
                }
            }

        }
        public ManagementCenter()
        {
            Console.WriteLine(@"
  __  __          _   _          _____ ______ _____  
 |  \/  |   /\   | \ | |   /\   / ____|  ____|  __ \ 
 | \  / |  /  \  |  \| |  /  \ | |  __| |__  | |__) |
 | |\/| | / /\ \ | . ` | / /\ \| | |_ |  __| |  _  / 
 | |  | |/ ____ \| |\  |/ ____ \ |__| | |____| | \ \ 
 |_|  |_/_/    \_\_| \_/_/    \_\_____|______|_|  \_\");
            this.adjacentNodes = new List<Tuple<int, int>>();
            this.adjacentNodes.Add(new Tuple<int, int>(0, 2));
            this.adjacentNodes.Add(new Tuple<int, int>(1, 4));
            this.adjacentNodes.Add(new Tuple<int, int>(3, 6));
            this.adjacentNodes.Add(new Tuple<int, int>(2, 0));
            this.readTopology();
            this.dijkstra = new Dijkstra();
            string[] prefixes;
            prefixes = new string[1];
            prefixes[0] = "http://localhost:13000/";
            this.server = new WebServer(prefixes, sendResponse);
        }

        public string sendResponse(HttpListenerRequest request)
        {
            try
            {
                NameValueCollection query = new NameValueCollection();
                query = request.QueryString;
                string sourceNodeId = query.Get("adjacentNodeId");
                Console.WriteLine("Source node ID: {0}", sourceNodeId);
                string targetPort = query.Get("targetPort");
                Console.WriteLine("Target port: {0}", targetPort);
                int targetNodeId = this.adjacentNodes.Find(item => (item.Item1 == Int32.Parse(targetPort) - 11000)).Item2;
                Console.WriteLine("Target node ID: {0}", targetNodeId);
                List<int> path = dijkstra.algorithm(topology, Int32.Parse(sourceNodeId), targetNodeId);
                Console.WriteLine("Path:");
                foreach (int node in path)
                {   
                    Console.WriteLine(node.ToString());
                }
                return "Path set";

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return "Unable to set path";
        }

        public void readTopology()
        {
            string[] lines = File.ReadAllLines("..\\..\\..\\TEST\\configs\\Topology.conf");
            this.topology = new int[lines.Length, lines.Length];
            int i = 0;
            int j = 0;
            foreach (string line in lines)
            {
                foreach (string l in line.Split(','))
                {
                    this.topology[i, j] = Int32.Parse(l);
                    Console.Write(l+',');
                    j++;
                }
                Console.Write(Environment.NewLine);
                j = 0;
                i++;
            }
        }

        public void listeningThread()
        {
            Console.WriteLine("I am listening on port {0}", this.listeningPort);
            this.listener = new ListenerSocket(listeningPort, handlePacket);
        }

        public int handlePacket(Packet p, int port)
        {
            Console.WriteLine();
            return 0;
        }

        public void computePath(int source, int destination)
        {
            this.dijkstra.calculate(this.topology, source, destination);
        }

        public void listEntries(string nodeID)
        {
            
        }

        public void deleteEntry(string nodeID)
        {

        }

        public void addEntry(string nodeID)
        {
        
        }

        public void deleteConnection(int first, int second)
        {
            this.topology[first, second] = 0;
            this.topology[second, first] = 0;
            string line = "";
            string[] lines = new string[this.topology.GetLength(0)];

            for(int i=0; i < this.topology.GetLength(0); i++)
            {
                for (int j = 0; j < this.topology.GetLength(0); j++)
                {
                    line += this.topology[i, j];
                    if (j < this.topology.GetLength(0) - 1)
                    {
                        line += ',';
                    }
                }
                lines[i] = line;
                line = "";
            }

            string text = "";
            int k = 0;

            foreach(string row in lines)
            {
                text += row;
                if (k < lines.Length - 1)
                {
                    text += Environment.NewLine;
                }
                k++;
            }

            File.WriteAllText("..\\..\\..\\TEST\\configs\\Topology.conf", text);
        }
    }
}

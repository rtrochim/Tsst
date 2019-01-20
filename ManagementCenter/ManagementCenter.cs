using System;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

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
        public HttpClient client; 
        Dijkstra dijkstra;
        public Dictionary<string, List<int>> nodesInterfaces;
        public Dictionary<string,Tuple<int, int>> connectionsMap;
        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[1]), Int32.Parse(args[2]), 1000, 400, true);

            Console.SetWindowSize(75, 18);
            ManagementCenter mc = new ManagementCenter();
            lock (mc)
            {
                mc.readNodesInterfaces(args[0]);
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
            this.connectionsMap = new Dictionary<string, Tuple<int, int>>();
            this.nodesInterfaces = new Dictionary<string, List<int>>();
            this.dijkstra = new Dijkstra();
            string[] prefixes;
            prefixes = new string[1];
            prefixes[0] = "http://localhost:13000/";
            this.server = new WebServer(prefixes, sendResponse);
            this.client = new HttpClient();
            this.readConnections(@"..\..\..\TEST\configs\ConnectionPairs.conf");
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
                Console.Write("PATH: ");
                foreach (int edge in path)
                {
                    Console.Write(edge.ToString() + ",");
                }
                Console.Write(Environment.NewLine);
                for (int i = 0; i < path.Count; i++)
                {
                    if (i == 0)
                    {
                        Dictionary<string, Tuple<int,int>> result = this.connectionsMap.Where(item => (item.Key.Contains('C') && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                        Dictionary<string, Tuple<int,int>> nextResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i+1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                        int first = result.Keys.First()[0] == 'C' ? result[result.Keys.First()].Item2 : result[result.Keys.First()].Item1;
                        Console.WriteLine("First: {0}", first);
                        //Console.WriteLine("result.Keys.First: {0}", result.Keys.First());
                        //Console.WriteLine("result.Item1: {0}", result[result.Keys.First()].Item1);
                        //Console.WriteLine("result.Item2: {0}", result[result.Keys.First()].Item2);
                        //Console.WriteLine("nextResult.Keys.First: {0}", nextResult.Keys.First());
                        //Console.WriteLine("nextResult.Item1: {0}", nextResult[nextResult.Keys.First()].Item1);
                        //Console.WriteLine("nextResult.Item1: {0}", nextResult[nextResult.Keys.First()].Item2);

                        int second = nextResult.Keys.First()[1].ToString() == (path[i]).ToString() ? nextResult[nextResult.Keys.First()].Item1 : nextResult[nextResult.Keys.First()].Item2;
                        Console.WriteLine("Second: {0}", second);
                        string entry = String.Format("{0}-{1}-{2}-{3}", "0:4", first, "0:4", second);
                        notifyNodes(path[i], entry);
                    } else if (i == path.Count - 1)
                    {
                        Dictionary<string, Tuple<int, int>> result = this.connectionsMap.Where(item => (item.Key.Contains('C') && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                        Dictionary<string, Tuple<int, int>> previousResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i - 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                        int first = previousResult.Keys.First()[1].ToString() == path[i].ToString() ? previousResult[previousResult.Keys.First()].Item1 : previousResult[previousResult.Keys.First()].Item2;
                        int second = result.Keys.First()[0] == 'C' ? result[result.Keys.First()].Item2 : result[result.Keys.First()].Item1;
                        string entry = String.Format("{0}-{1}-{2}-{3}", "0:4", first, "0:4", second);
                        notifyNodes(path[i], entry);
                    } else
                    {
                        Dictionary<string, Tuple<int, int>> previousResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i - 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                        Dictionary<string, Tuple<int, int>> nextResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i + 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                        Dictionary<string, Tuple<int, int>> result = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i])) && item.Key.Contains(string.Format("N{0}", path[i+1])))).ToDictionary(it => it.Key, it => it.Value);
                        int first = previousResult.Keys.First()[1].ToString() == path[i].ToString() ? previousResult[previousResult.Keys.First()].Item1 : previousResult[previousResult.Keys.First()].Item2;
                        int second = nextResult.Keys.First()[1].ToString() == path[i].ToString() ? nextResult[nextResult.Keys.First()].Item1 : nextResult[nextResult.Keys.First()].Item2;
                        string entry = String.Format("{0}-{1}-{2}-{3}", "0:4", first, "0:4", second);
                        notifyNodes(path[i], entry);
                    }
                }
                return "Path set";

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return "Unable to set path";
        }

        public async void notifyNodes(int node, string entry)
        {   
            try {
                Console.WriteLine($"Node: {node}, on port: {node + 10100}");
                await this.client.GetAsync(string.Format("http://localhost:{0}/refresh?entry={1}", node + 10100, entry));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception" + e.ToString());
            }
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

        public void readNodesInterfaces(string path)
        {
            string[] buffer = File.ReadAllLines(path);
            foreach (string line in buffer)
            {
                List<string> temp = line.Split(' ').ToList();
                string key = temp[0];
                temp.RemoveAt(0);
                List<int> temp2 = temp.ConvertAll(item => Int32.Parse(item));
                this.nodesInterfaces.Add(key, temp2);
            }
            //foreach(KeyValuePair<string,List<int>> dupa in this.nodesInterfaces)
            //{
            //    Console.WriteLine("Key {0} Value:", dupa.Key);
            //    dupa.Value.ForEach(item => Console.Write(item+", "));
            //    Console.Write(Environment.NewLine);
            //}
        }

        public void readConnections(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] temp = line.Split(' ');
                this.connectionsMap.Add(temp[0], new Tuple<int,int>(int.Parse(temp[1]),int.Parse(temp[2])));
            }
            //foreach (KeyValuePair<string, Tuple<int, int>> item in this.connectionsMap)
            //{
            //    Console.WriteLine("Key {0}, Value: {1} {2}", item.Key, item.Value.Item1.ToString(), item.Value.Item2.ToString());
            //}
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

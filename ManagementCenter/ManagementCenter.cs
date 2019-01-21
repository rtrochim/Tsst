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
using System.Threading.Tasks;

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
        public Dictionary<int, bool[]> interfacesSlotsMap = new Dictionary<int, bool[]>();
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
                        Console.WriteLine(@"
[L] List all entries from Node
[A] Add entry to Node
[R] Remove entry from Node
What to do:");
                        string option = Console.ReadLine();
                        switch (option)
                        {
                            case "L":
                                Console.WriteLine("Which node: ");
                                mc.listEntries(Console.ReadLine());
                                break;
                            case "A":
                                Console.WriteLine("Which node: ");
                                mc.addEntry(Console.ReadLine());
                                break;
                            case "R":
                                Console.WriteLine("Which node: ");
                                mc.deleteEntry(Console.ReadLine());
                                break;
                            default:
                                Console.WriteLine("Invalid option!");
                                break;
                        }
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
            bool invalidPath = false;
            // Arbitrary slots for now
            NameValueCollection query = new NameValueCollection();
            query = request.QueryString;
            string sourceNodeId = query.Get("adjacentNodeId");
            //Console.WriteLine("Source node ID: {0}", sourceNodeId);
            string targetPort = query.Get("targetPort");
            //Console.WriteLine("Target port: {0}", targetPort);
            string requiredBandwidth = query.Get("bandwidth");
            int targetNodeId = this.adjacentNodes.Find(item => (item.Item1 == Int32.Parse(targetPort) - 11000)).Item2;
            int requiredSlots;
            int firstSlot = 0;
            int lastSlot;
            List<int> validPath = new List<int>();
            List<string> entries = new List<string>();
            int[,] backupTopology = this.topology;
            try
            {
                do
                {
                    this.interfacesSlotsMap.Clear();
                    invalidPath = false;
                    //Console.WriteLine("Target node ID: {0}", targetNodeId);
                    Tuple<List<int>, int> dijkstraResult = dijkstra.calculate(this.topology, Int32.Parse(sourceNodeId), targetNodeId);
                    List<int> path = new List<int>(dijkstraResult.Item1);
                    int pathLength = dijkstraResult.Item2;
                    //Console.WriteLine("Required Bandwidth {0}", requiredBandwidth);
                    requiredSlots = calculateSlots(int.Parse(requiredBandwidth), pathLength);
                    //Console.WriteLine("calculateSlots Result: {0}",requiredSlots);
                    lastSlot = firstSlot + requiredSlots - 1;
                    validPath = path;
                    // Print out calculated path
                    Console.Write("PATH: ");
                    foreach (int edge in path)
                    {
                        Console.Write(edge.ToString() + ",");
                    }
                    Console.Write(Environment.NewLine);
                    // Ask each node in path about its slot usage and prepare a Dict out of it
                    foreach (int node in path)
                    {
                        string response = getSlotsStatus(node).Result;
                        string[] temp1 = response.Split(' ');
                        List<string> buf = new List<string>(temp1.ToList());
                        buf.RemoveAt(buf.Count - 1);
                        temp1 = buf.ToArray();
                        foreach (string item1 in temp1)
                        {
                            string[] temp2 = item1.Split(':');
                            List<bool> temp3 = new List<bool>();
                            foreach (char item2 in temp2[1])
                            {
                                temp3.Add(item2 == '1' ? true : false);
                            }
                            this.interfacesSlotsMap.Add(int.Parse(temp2[0]), temp3.ToArray());

                        }
                    }
                    // Print out the prepared Dict
                    foreach (KeyValuePair<int, bool[]> item in this.interfacesSlotsMap)
                    {
                        //Console.Write("Key {0}:", item.Key);
                        foreach (bool slot in item.Value)
                        {
                            //Console.Write(slot ? "1" : "0");
                        }
                        //Console.Write(Environment.NewLine);
                    }

                    List<int> interfacesOnPath = new List<int>();
                    for (int i = 0; i < path.Count; i++)
                    {
                        if (i == 0)
                        {
                            Dictionary<string, Tuple<int, int>> result = this.connectionsMap.Where(item => (item.Key.Contains('C') && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                            Dictionary<string, Tuple<int, int>> nextResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i + 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                            int first = result.Keys.First()[0] == 'C' ? result[result.Keys.First()].Item2 : result[result.Keys.First()].Item1;
                            int second = nextResult.Keys.First()[1].ToString() == (path[i]).ToString() ? nextResult[nextResult.Keys.First()].Item1 : nextResult[nextResult.Keys.First()].Item2;
                            interfacesOnPath.Add(first);
                            interfacesOnPath.Add(second);
                            entries.Add(String.Format("{0}-{1}-{2}-{3}", firstSlot.ToString() + ":" + lastSlot.ToString(), first, firstSlot.ToString() + ":" + lastSlot.ToString(), second));
                        }
                        else if (i == path.Count - 1)
                        {
                            Dictionary<string, Tuple<int, int>> result = this.connectionsMap.Where(item => (item.Key.Contains('C') && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                            Dictionary<string, Tuple<int, int>> previousResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i - 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                            int first = previousResult.Keys.First()[1].ToString() == path[i].ToString() ? previousResult[previousResult.Keys.First()].Item1 : previousResult[previousResult.Keys.First()].Item2;
                            int second = result.Keys.First()[0] == 'C' ? result[result.Keys.First()].Item2 : result[result.Keys.First()].Item1;
                            interfacesOnPath.Add(first);
                            interfacesOnPath.Add(second);
                            entries.Add(String.Format("{0}-{1}-{2}-{3}", firstSlot.ToString() + ":" + lastSlot.ToString(), first, firstSlot.ToString() + ":" + lastSlot.ToString(), second));
                        }
                        else
                        {
                            Dictionary<string, Tuple<int, int>> previousResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i - 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                            Dictionary<string, Tuple<int, int>> nextResult = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i + 1])) && item.Key.Contains(string.Format("N{0}", path[i])))).ToDictionary(it => it.Key, it => it.Value);
                            Dictionary<string, Tuple<int, int>> result = this.connectionsMap.Where(item => (item.Key.Contains(string.Format("N{0}", path[i])) && item.Key.Contains(string.Format("N{0}", path[i + 1])))).ToDictionary(it => it.Key, it => it.Value);
                            int first = previousResult.Keys.First()[1].ToString() == path[i].ToString() ? previousResult[previousResult.Keys.First()].Item1 : previousResult[previousResult.Keys.First()].Item2;
                            int second = nextResult.Keys.First()[1].ToString() == path[i].ToString() ? nextResult[nextResult.Keys.First()].Item1 : nextResult[nextResult.Keys.First()].Item2;
                            interfacesOnPath.Add(first);
                            interfacesOnPath.Add(second);
                            entries.Add(String.Format("{0}-{1}-{2}-{3}", firstSlot.ToString() + ":" + lastSlot.ToString(), first, firstSlot.ToString() + ":" + lastSlot.ToString(), second));
                        }
                    }
                    bool canIexit = false;
                    foreach(int item in interfacesOnPath)
                    {
                        if (canIexit)
                        {
                            break;
                        }
                        for (int i = firstSlot; i <= lastSlot; i++)
                        {
                            // TODO: What if we run out of slots? Delete this path from topology
                            if (interfacesSlotsMap[item][i])
                            {
                                invalidPath = true;
                                firstSlot += 1;
                                lastSlot += 1;
                                validPath.Clear();
                                entries.Clear();
                                if (lastSlot > interfacesSlotsMap[item].Length - 1)
                                {
                                    KeyValuePair<string, Tuple<int, int>> result = this.connectionsMap.First(it => (it.Value.Item1 == item || it.Value.Item2 == item));
                                    Console.WriteLine("Deleting connection {0} - {1}", result.Key[1].ToString(), result.Key[3].ToString());
                                    deleteConnection(int.Parse(result.Key[1].ToString()), int.Parse(result.Key[3].ToString()));
                                    firstSlot = 0;
                                    lastSlot = firstSlot + requiredSlots - 1;
                                    canIexit = true;
                                    break;
                                }
                            }
                        }
                    }

                } while (invalidPath);

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return "Unable to set path";
            }
            for(int j=0; j < validPath.Count; j++)
            {
                //Console.WriteLine("ValidPath[j]: {0}", validPath[j].ToString());
                //Console.WriteLine("Entries[j]: {0}", entries[j]);
                notifyNodes(validPath[j], entries[j]);
            }
            this.topology = backupTopology;
            return $"{firstSlot}:{lastSlot}";
        }

        public int calculateSlots(int requiredBandwidth, int pathLength)
        {
            int numberOfModulationSymbols;
            if (pathLength > 0 && pathLength < 100)
            {
                Console.WriteLine("16QAM has been chosen");
                numberOfModulationSymbols = 4;
            }
            else if (pathLength >= 100 && pathLength < 200)
            {
                Console.WriteLine("8QAM has been chosen");
                numberOfModulationSymbols = 3;
            }
               
            else
            {
                Console.WriteLine("QPSK has been chosen");
                numberOfModulationSymbols = 2;
            }
               

            int requiredSlots = int.Parse(Math.Ceiling(((float.Parse(requiredBandwidth.ToString()) * 2) / 12.5)).ToString());
            int result = int.Parse(Math.Ceiling(float.Parse((requiredSlots / numberOfModulationSymbols).ToString())).ToString());
            return result >= 1 ? result : 1;
        }

        public async void notifyNodes(int node, string entry)
        {   
            try {
                //Console.WriteLine($"Node: {node}, on port: {node + 10100}");
                await this.client.GetAsync(string.Format("http://localhost:{0}/refresh?entry={1}", node + 10100, entry));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception" + e.ToString());
            }
        }

        public async Task<string> getSlotsStatus(int nodeId)
        {
            HttpResponseMessage res = await this.client.GetAsync($"http://localhost:{nodeId + 10100}/getSlotsStatus/");
            HttpContent content = res.Content;
            string response = await content.ReadAsStringAsync();
            return response;
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
                    //Console.Write(l+',');
                    j++;
                }
                //Console.Write(Environment.NewLine);
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

        public async void listEntries(string nodeID)
        {
            HttpResponseMessage res = await this.client.GetAsync($"http://localhost:{int.Parse(nodeID) + 10100}/getSwitchingTable/");
            HttpContent content = res.Content;
            string response = await content.ReadAsStringAsync();
            string[] lines = response.Split(';');
            foreach (string line in lines)
            {
                Console.WriteLine("{0}", line);
            }
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

            //File.WriteAllText("..\\..\\..\\TEST\\configs\\Topology.conf", text);
        }
    }
}

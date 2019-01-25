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
        public List<Tuple<List<string>, List<int>, int>> establishedConnections;
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
[E] List established connections in network
[N] Delete entry from Node
What to do:");
                        string option = Console.ReadLine();
                        switch (option)
                        {
                            case "L":
                                Console.WriteLine("Which node: ");
                            string res = mc.listEntries(Console.ReadLine()).Result;
                                break;
                            case "E":
                                mc.listEstablishedConnections();
                                break;
                            case "N":
                                Console.WriteLine("Which node:");
                                string resp = mc.deleteEntry(Console.ReadLine()).Result;
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
            Console.WriteLine("NetworkCallController up!");
            Console.WriteLine("CallController up!");
            Console.WriteLine("ConnectionController up!");
            Console.WriteLine("RoutingController up!");
            this.adjacentNodes = new List<Tuple<int, int>>();
            this.adjacentNodes.Add(new Tuple<int, int>(0, 2));
            this.adjacentNodes.Add(new Tuple<int, int>(1, 4));
            this.adjacentNodes.Add(new Tuple<int, int>(3, 6));
            this.adjacentNodes.Add(new Tuple<int, int>(2, 0));
            this.readTopology();
            this.connectionsMap = new Dictionary<string, Tuple<int, int>>();
            this.nodesInterfaces = new Dictionary<string, List<int>>();
            this.establishedConnections = new List<Tuple<List<string>, List<int>, int>>();
            this.dijkstra = new Dijkstra();
            string[] prefixes;
            prefixes = new string[3];
            prefixes[0] = "http://localhost:13000/";
            prefixes[1] = "http://localhost:13000/dropConnection/";
            prefixes[2] = "http://localhost:13000/terminateConnection/";
            this.server = new WebServer(prefixes, sendResponse);
            this.client = new HttpClient();
            this.readConnections(@"..\..\..\TEST\configs\ConnectionPairs.conf");
        }

        public string sendResponse(HttpListenerRequest request)
        {
            if (request.RawUrl.Contains("dropConnection"))
            {
                return this.dropConnection(request);
            } else if (request.RawUrl.Contains("terminateConnection")) {
                NameValueCollection query = new NameValueCollection();
                query = request.QueryString;
                string first = query.Get("first");
                string second = query.Get("second");
                this.deleteNodeConnection(int.Parse(first), int.Parse(second));
            }
            else {
                bool invalidPath = false;
                NameValueCollection query = new NameValueCollection();
                query = request.QueryString;
                string sourceNodeId = query.Get("adjacentNodeId");
                string targetPort = query.Get("targetPort");
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
                        Tuple<List<int>, int> dijkstraResult = dijkstra.calculate(this.topology, Int32.Parse(sourceNodeId), targetNodeId);
                        List<int> path = new List<int>(dijkstraResult.Item1);
                        int pathLength = dijkstraResult.Item2;
                        requiredSlots = calculateSlots(int.Parse(requiredBandwidth), pathLength);
                        lastSlot = firstSlot + requiredSlots - 1;
                        validPath = path;
                        // Print out calculated path
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
                        foreach (int item in interfacesOnPath)
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
                                        Console.WriteLine("Removing connection {0} - {1} from pool.", result.Key[1].ToString(), result.Key[3].ToString());
                                        deleteNodeConnection(int.Parse(result.Key[1].ToString()), int.Parse(result.Key[3].ToString()));
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
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return "Unable to set path";
                }
                for (int j = 0; j < validPath.Count; j++)
                {
                    notifyNodes(validPath[j], entries[j]);
                }
                this.establishedConnections.Add(new Tuple<List<string>, List<int>, int>(entries.ToList(), validPath.ToList(), int.Parse(requiredBandwidth)));
                this.topology = backupTopology;
                return $"{firstSlot}:{lastSlot}";
            }
            return "OK";
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
                    j++;
                }
                j = 0;
                i++;
            }
        }

        public void computePath(int source, int destination)
        {
            this.dijkstra.calculate(this.topology, source, destination);
        }

        public async Task<string> listEntries(string nodeID)
        {
            HttpResponseMessage res = await this.client.GetAsync($"http://localhost:{int.Parse(nodeID) + 10100}/getSwitchingTable/");
            HttpContent content = res.Content;
            string response = await content.ReadAsStringAsync();
            string[] lines = response.Split(';');
            List<string> list = lines.ToList();
            list.RemoveAt(list.Count - 1);
            lines = list.ToArray();
            foreach (string line in lines)
            {
                Console.WriteLine("{0}-{1}-{2}", line.Split(' ')[0], line.Split(' ')[1], line.Split(' ')[3]);
            }

            return "OK";
        }

        public async Task<string> deleteEntry(string nodeID)
        {
            try {
                int port = int.Parse(nodeID) + 10100;
                HttpResponseMessage response = await this.client.GetAsync(string.Format("http://localhost:{0}/getSwitchingTable", port.ToString()));
                string responseBody = await response.Content.ReadAsStringAsync();
                string[] entries = responseBody.Split(';');
                Console.WriteLine("Which one would you like to delete?");
                List<string> list = entries.ToList();
                list.RemoveAt(list.Count - 1);
                entries = list.ToArray();
                for (int i = 0; i < entries.Length; i++)
                {
                    Console.WriteLine("{0} {1}-{2}-{3}", i, entries[i].Split(' ')[0], entries[i].Split(' ')[1], entries[i].Split(' ')[3]);
                }

                string choice = Console.ReadLine();
                string[] splitted = entries[int.Parse(choice)].Split(' ');

                string entry = splitted[0] + '-' + splitted[1] + '-' + splitted[2] + '-' + splitted[3];
                await this.client.GetAsync(string.Format("http://localhost:{0}/removeEntry?entry={1}", port.ToString(), entry));
                return "OK";
            } catch (Exception e)
            {
                Console.WriteLine("{0}", e.ToString());
            }
            return "OK";
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
        }

        public void readConnections(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] temp = line.Split(' ');
                this.connectionsMap.Add(temp[0], new Tuple<int,int>(int.Parse(temp[1]),int.Parse(temp[2])));
            }
        }

        public async void deleteNodeConnection(int first, int second)
        {
            try
            {
                this.topology[first, second] = 0;
                this.topology[second, first] = 0;
                List<Tuple<int, int, int>> droppedConnections = new List<Tuple<int, int, int>>();
                List<Tuple<List<string>, List<int>, int>> dumpedConnections = new List<Tuple<List<string>, List<int>, int>>();

                foreach (Tuple<List<string>, List<int>, int> item in this.establishedConnections)
                {
                    List<int> items = new List<int>(item.Item2.Where(it => (it == first) || (it == second)).ToList());
                    if (items.Count == 2)
                    {
                        droppedConnections.Add(new Tuple<int, int, int>(item.Item2[0], item.Item2[item.Item2.Count - 1], item.Item3));
                        for (int i = 0; i < item.Item2.Count; i++)
                        {
                            await this.client.GetAsync(string.Format("http://localhost:{0}/removeEntry?entry={1}", item.Item2[i] + 10100, item.Item1[i]));
                        }
                        dumpedConnections.Add(item);
                    }
                }

                foreach(Tuple<List<string>, List<int>, int> item in dumpedConnections)
                {
                    this.establishedConnections.Remove(item);
                }

                foreach (Tuple<int, int, int> item in droppedConnections)
                {
                    Tuple<List<int>, int> result = this.dijkstra.calculate(this.topology, item.Item1, item.Item2);
                    int adjacentNodeId = item.Item1;
                    int lastNodeId = item.Item2;
                    KeyValuePair<string, Tuple<int, int>> pair = this.connectionsMap.First(it => ((it.Key[0] == 'C' && it.Key[3].ToString() == lastNodeId.ToString()) || (it.Key[2] == 'C' && it.Key[1].ToString() == lastNodeId.ToString())));
                    string clientId = pair.Key[0] == 'C' ? pair.Key[1].ToString() : pair.Key[3].ToString();
                    await this.client.GetAsync(string.Format("http://localhost:13000?adjacentNodeId={0}&bandwidth={1}&targetPort={2}", adjacentNodeId, item.Item3, int.Parse(clientId) + 11000));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public string dropConnection(HttpListenerRequest request)
        {
            NameValueCollection query = new NameValueCollection();
            query = request.QueryString;
            string sourceNodeId = query.Get("adjacentNodeId");
            string targetPort = query.Get("targetPort");
            int clientId = int.Parse(targetPort) - 11000;
            KeyValuePair<string, Tuple<int, int>> pair = this.connectionsMap.First(it => ((it.Key[0] == 'C' && it.Key[1].ToString() == clientId.ToString()) || (it.Key[2] == 'C' && it.Key[3].ToString() == clientId.ToString())));
            string lastNodeId = (pair.Key[0] == 'C' ? pair.Key[3] : pair.Key[1]).ToString();
            Tuple<List<string>, List<int>, int> connection = this.establishedConnections.First(item => (item.Item2[0].ToString() == sourceNodeId && item.Item2[item.Item2.Count - 1].ToString() == lastNodeId));
            for (int i = 0; i < connection.Item1.Count; i++)
            {
                this.client.GetAsync(string.Format("http://localhost:{0}/removeEntry?entry={1}", connection.Item2[i] + 10100, connection.Item1[i]));
            }

            return "OK";
        }


        public void listEstablishedConnections()
        {
            int k = 0;
            foreach(Tuple<List<string>, List<int>, int> item1 in this.establishedConnections)
            {
                Console.WriteLine("ID{0} ", k);
                for(int i = 0; i< item1.Item2.Count; i++)
                {
                    Console.WriteLine("{0}-{1}-{2}   {3}    {4}", item1.Item1[i].Split('-')[0], item1.Item1[i].Split('-')[1], item1.Item1[i].Split('-')[3], item1.Item2[i], item1.Item3);
                }
                k++;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace TSST
{
    class Node
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        int MAX_SLOTS = 100;

        public ListenerSocket listener;
        public SenderSocket sender;
        public static int listenerPort;
        public static int targetPort;
        public SwitchingField sf;
        public Dictionary<int, bool[]> slots;
        public Packet packet;
        public List<int> entryPorts;
        public WebServer server;
        public string configurationPath;

        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[1]), Int32.Parse(args[2]), 1000, 400, true);

            Console.SetWindowSize(35, 17);
            Thread.Sleep(1000);
            string[] lines = File.ReadAllLines(args[0]);
            listenerPort = Int32.Parse(lines[0]);
            targetPort = Int32.Parse(lines[1]);

            
            Node n = new Node(args[0]);
            try
            {
                Console.Write("My SNPs: ");
                foreach (string item in lines[2].Split(' '))
                {
                    n.entryPorts.Add(Int32.Parse(item));
                    Console.Write($"{item}, ");
                }
                Console.Write(Environment.NewLine);
                if(args.Length == 4)
                {
                    Console.WriteLine("SubNetworkConnection up!");
                }
                Console.WriteLine("RoutingController up!");
                Console.WriteLine("CallController up!");
                n.slots = new Dictionary<int, bool[]>();
                foreach (int item in n.entryPorts)
                {
                    n.slots.Add(item, new bool[n.MAX_SLOTS]);
                    Array.ForEach(n.slots[item], i => i = false);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public Node(string configurationPath)
        {
            lock (this)
            {
                entryPorts = new List<int>();
                Console.WriteLine(@"
  _   _  ____  _____  ______ 
 | \ | |/ __ \|  __ \|  ____|
 |  \| | |  | | |  | | |__   
 | . ` | |  | | |  | |  __|  
 | |\  | |__| | |__| | |____ 
 |_| \_|\____/|_____/|______|");
                ThreadStart childref = new ThreadStart(listeningThread);
                Thread childThread = new Thread(childref);
                childThread.Start();
                this.sender = new SenderSocket();

                sf = new SwitchingField();

                this.configurationPath = configurationPath;
                string[] prefixes;
                prefixes = new string[4];
                prefixes[0] = $"http://localhost:{listenerPort - 1000}/refresh/";
                prefixes[1] = $"http://localhost:{listenerPort - 1000}/getSlotsStatus/";
                prefixes[2] = $"http://localhost:{listenerPort - 1000}/getSwitchingTable/";
                prefixes[3] = $"http://localhost:{listenerPort - 1000}/removeEntry/";
                this.server = new WebServer(prefixes, handleResponse);
                this.server.Run();
            }
        }

        public void listeningThread()
        {
            Console.WriteLine("Sending: {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort, handlePacket);
        }

        public string handleResponse(HttpListenerRequest request)
        {
            lock (this)
            {
                if (request.RawUrl.Contains("refresh"))
                {
                    try
                    {
                        NameValueCollection query = new NameValueCollection();
                        query = request.QueryString;
                        string entry = query.Get("entry");
                        string[] items = entry.Split('-');
                        this.sf.addEntry(items, ref this.slots);
                        Console.WriteLine("Updating with entry: {0}-{1}-{2}", entry.Split('-')[0], entry.Split('-')[1], entry.Split('-')[3]);
                        this.sf.setSwitchingTable(this.configurationPath);
                        Thread.Yield();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unexpected error: {e.ToString()}");
                    }
                }
                else if (request.RawUrl.Contains("getSlotsStatus"))
                {
                    string response = "";
                    foreach (int key in this.slots.Keys)
                    {
                        response += key.ToString() + ":";
                        foreach (bool slot in this.slots[key])
                        {
                            response += slot ? "1" : "0";
                        }
                        response += " ";
                    }
                    Thread.Yield();
                    return response;
                }
                else if (request.RawUrl.Contains("getSwitchingTable"))
                {
                    List<Tuple<string, string, string, string>> switchingTable = new List<Tuple<string, string, string, string>>(this.sf.getSwitchingTable());
                    string response = "";
                    foreach (Tuple<string,string,string,string> tuple in switchingTable)
                    {
                        response += $"{tuple.Item1} {tuple.Item2} {tuple.Item3} {tuple.Item4};";
                    }
                    response.Remove(response.Length - 1);
                    Thread.Yield();
                    return response;
                }
                else if (request.RawUrl.Contains("removeEntry"))
                {
                    NameValueCollection query = new NameValueCollection();
                    query = request.QueryString;
                    string entry = query.Get("entry");
                    Console.WriteLine("Removing entry: {0}-{1}-{2}", entry.Split('-')[0], entry.Split('-')[1], entry.Split('-')[3]);
                    string[] items = entry.Split('-');
                    Tuple<string, string, string, string> item = new Tuple<string, string, string, string>(items[0], items[1], items[2], items[3]);
                    string[] slots = items[0].Split(':');
                    for (int i = int.Parse(slots[0]) ; i <= int.Parse(slots[1]); i++)
                    {
                        this.slots[int.Parse(items[1])][i] = false;
                        this.slots[int.Parse(items[3])][i] = false;
                    }
                    this.sf.removeEntry(item);
                    Thread.Yield();
                }
                Thread.Yield();
                return "NODE: OK";
            }
        }

        public int handlePacket(Packet p, int port)
        {
            lock (this)
            {
                packet = p;
                try
                {
                    sf.commutatePacket(ref packet, port, ref slots);
                    this.sender.sendMessage(packet.serialize(), targetPort);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Cannot commutate this Packet!");
                    Console.WriteLine(e.ToString());
                    Thread.Yield();
                }
                return 0;
            }
        }
    }
}

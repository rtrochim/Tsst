using System;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
namespace TSST
{

    class Client
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public ListenerSocket listener;
        public SenderSocket sender;
        public HttpClient client;
        public int listenerPort;
        public int targetPort;
        public int myInterface;
        public int adjacentNodeId;
        public Packet packet;
        public List<Tuple<string, string, string>> existingConnections;
        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[1]), Int32.Parse(args[2]), 1000, 400, true);

            Console.SetWindowSize(45, 17);
            Thread.Sleep(600);
            string[] lines = File.ReadAllLines(args[0]);
            Client c = new Client(Int32.Parse(lines[0]), Int32.Parse(lines[1]), Int32.Parse(lines[2]), Int32.Parse(lines[3]));
            Thread.Sleep(200);
            ThreadStart senderThread= new ThreadStart(() => c.sendPacket());
            Thread childThread = new Thread(senderThread);
            childThread.Start();
            Thread.Yield();
        }

        public Client(int listenerPort, int targetPort, int myInterface, int adjacentNodeId)
        {
            Console.WriteLine(@"
   _____ _      _____ ______ _   _ _______ 
  / ____| |    |_   _|  ____| \ | |__   __|
 | |    | |      | | | |__  |  \| |  | |   
 | |    | |      | | |  __| | . ` |  | |   
 | |____| |____ _| |_| |____| |\  |  | |
  \_____|______|_____|______|_| \_|  |_|");
            this.listenerPort = listenerPort;
            this.targetPort = targetPort;
            this.myInterface = myInterface;
            this.adjacentNodeId = adjacentNodeId;
            this.existingConnections = new List<Tuple<string, string, string>>();
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            
    
            this.sender = new SenderSocket();
            this.client = new HttpClient();
        }

        public void listeningThread()
        {
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort, handlePacket);
        }

        public async void sendPacket()
        {
            while (true)
            {
                Console.WriteLine("What you wanna do?");
                Console.WriteLine("[0] Use existing connection");
                Console.WriteLine("[1] Establish new connection");
                string option = Console.ReadLine();
                if (option == "0")
                {
                    foreach(Tuple<string, string,string> item in existingConnections)
                    {
                        Console.WriteLine("{0},{1},{2}", item.Item1, item.Item2, item.Item3);
                    }
                    Console.Write("Data to send: ");
                    string message = Console.ReadLine();
                    Console.Write("Port to send data to: ");
                    string portNumber = Console.ReadLine();
                    Console.WriteLine("Which slots to use?");
                    string slots = Console.ReadLine();
                    Packet packetToSend = new Packet(message, Int32.Parse(portNumber), this.myInterface, slots);
                    this.sender.sendMessage(packetToSend.serialize(), this.targetPort);
                    Thread.Yield();

                }
                if (option == "1")
                {
                    Console.Write("Data to send: ");
                    string message = Console.ReadLine();
                    Console.Write("Port to send data to: ");
                    string portNumber = Console.ReadLine();
                    Console.Write("Required bandwidth in Gbps: ");
                    string bandwidth = Console.ReadLine();
                    HttpResponseMessage response = await this.client.GetAsync(string.Format("http://localhost:13000?adjacentNodeId={0}&bandwidth={1}&targetPort={2}", adjacentNodeId, bandwidth, portNumber));
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response: {0}", responseBody);
                    Console.WriteLine("Which slots to use?\nManager reserved these-" + responseBody);
                    string slots = Console.ReadLine();
                    Thread.Yield();
                    Packet packetToSend = new Packet(message, Int32.Parse(portNumber), this.myInterface, slots);
                    this.sender.sendMessage(packetToSend.serialize(), this.targetPort);
                    this.existingConnections.Add(new Tuple<string, string, string>(portNumber.ToString(), slots, bandwidth));
                }

            }

        }

        public int handlePacket(Packet p, int port)
        {
            lock (this)
            {
                packet = p;
                Console.WriteLine(Environment.NewLine + $"Got packet with message: {packet.data}");
                Thread.Yield();
                return 0;
            }
        }
    }
}

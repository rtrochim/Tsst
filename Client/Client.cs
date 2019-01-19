using System;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

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
        public int listenerPort;
        public int targetPort;
        public int adjacentNodePort;
        public Packet packet;

        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[1]), Int32.Parse(args[2]), 1000, 400, true);

            Console.SetWindowSize(45, 17);
            Thread.Sleep(600);
            string[] lines = File.ReadAllLines(args[0]);
            Client c = new Client();
            c.listenerPort = Int32.Parse(lines[0]);
            c.targetPort = Int32.Parse(lines[1]);
            c.adjacentNodePort = Int32.Parse(lines[2]);
            Thread.Sleep(200);
            ThreadStart senderThread= new ThreadStart(() => c.sendPacket());
            Thread childThread = new Thread(senderThread);
            childThread.Start();
            Thread.Yield();
        }

        public Client()
        {
            Console.WriteLine(@"
   _____ _      _____ ______ _   _ _______ 
  / ____| |    |_   _|  ____| \ | |__   __|
 | |    | |      | | | |__  |  \| |  | |   
 | |    | |      | | |  __| | . ` |  | |   
 | |____| |____ _| |_| |____| |\  |  | |
  \_____|______|_____|______|_| \_|  |_|");
            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
            
    
            this.sender = new SenderSocket();
        }

        public void listeningThread()
        {
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort, handlePacket);
        }

        public void sendPacket()
        {
                while (true)
                {
                    Thread.Yield();
                    Console.Write("Data to send: ");
                    string message = Console.ReadLine();
                    Console.Write("Port to send data to: ");
                    string portNumber = Console.ReadLine();
                    Packet packetToSend = new Packet(message, Int32.Parse(portNumber), this.adjacentNodePort);
                    
                    //this.sender.sendMessage(packetToSend.serialize(), this.targetPort);
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

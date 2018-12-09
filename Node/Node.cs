using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace TSST
{
    class Node
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public ListenerSocket listener;
        public SenderSocket sender;
        public static int listenerPort;
        public static int targetPort;
        public SwitchingField sf;
        public Packet packet;
        public List<int> entryPorts;

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
                Console.Write("My interfaces: ");
                foreach (string item in lines[2].Split(' '))
                {
                    n.entryPorts.Add(Int32.Parse(item));
                    Console.Write($"{item}, ");
                }
                Console.Write(Environment.NewLine);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            lock (n)
            {
                ThreadStart childref = new ThreadStart(() => watchTable(args[0], n));
                Thread watchThread = new Thread(childref);
                watchThread.Start();
            }
        }
        public Node(string pathToLabelTable)
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
        }

        public void listeningThread()
        {
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort, handlePacket);
        }

        public int handlePacket(Packet p, int port)
        {
            lock (this)
            {
                packet = p;
                try
                {
                    sf.commutatePacket(ref packet, port);
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
        public static void watchTable(string path, Node n)
        {   
            List<string> linesToCompare = new List<string>(n.sf.setLabelTable(path));
            while (true)
            {
                List<string> linesInIteration = new List<string>(n.sf.setLabelTable(path));
                if (!((linesToCompare.Count == linesInIteration.Count) && !linesToCompare.Except(linesInIteration).Any()))
                {
                    linesToCompare = new List<string>(linesInIteration);
                    Console.WriteLine("Table updated!");
                }
                Thread.Sleep(1000);
            }
        }
    }
}

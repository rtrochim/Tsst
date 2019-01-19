using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace TSST
{
    class ManagementCenter
    {
        public int listeningPort = 13000;
        public ListenerSocket listener;
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
                while (true)
                {
                    mc.computePath(7, 4);
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

            this.readTopology();
            this.dijkstra = new Dijkstra();

            ThreadStart childref = new ThreadStart(listeningThread);
            Thread childThread = new Thread(childref);
            childThread.Start();
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

        public void handlePacket(Packet p, int port)
        {
            Console.WriteLine();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

namespace TSST
{
    class Node
    {
        public ListenerSocket listener;
        public SenderSocket sender;
        public static int listenerPort;
        public static int targetPort;
        public SwitchingField sf;
        public Packet packet;
   
        static void Main(string[] args)
        {
            Console.SetWindowSize(45, 20);
            Thread.Sleep(1000);
            string[] lines = File.ReadAllLines(args[0]);
            listenerPort = Int32.Parse(lines[0]);
            targetPort = Int32.Parse(lines[1]);
            Node n = new Node(args[0]);
            ThreadStart childref = new ThreadStart(() => watchTable(args[0], n));
            Thread watchThread = new Thread(childref);
            watchThread.Start();

            Console.ReadKey();
        }

        public Node(string pathToLabelTable)
        {
            Console.WriteLine(@"
  _   _  ____  _____  ______ 
 | \ | |/ __ \|  __ \|  ____|
 |  \| | |  | | |  | | |__   
 | . ` | |  | | |  | |  __|  
 | |\  | |__| | |__| | |____ 
 |_| \_|\____/|_____/|______|
                             
                             
");
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
                    sf.commutatePacket(ref packet);
                    this.sender.sendMessage(packet.serialize(), targetPort);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: Cannot commutate this Packet!");
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

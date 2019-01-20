﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Net;

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
                Console.Write("My interfaces: ");
                foreach (string item in lines[2].Split(' '))
                {
                    n.entryPorts.Add(Int32.Parse(item));
                    Console.Write($"{item}, ");
                }
                Console.Write(Environment.NewLine);
                n.slots = new Dictionary<int, bool[]>();
                foreach (int item in n.entryPorts)
                {
                    n.slots.Add(item, new bool[20]);
                    Array.ForEach(n.slots[item], i => i = false);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            //lock (n)
            //{
            //    ThreadStart childref = new ThreadStart(() => watchTable(args[0], n));
            //    Thread watchThread = new Thread(childref);
            //    watchThread.Start();
            //}
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
                prefixes = new string[1];
                prefixes[0] = $"http://localhost:{listenerPort - 1000}/refresh/";
                Console.WriteLine($"Waiting for refresh requests at localhost:{listenerPort - 1000}");
                this.server = new WebServer(prefixes, handleResponse);
                this.server.Run();
            }
        }

        public void listeningThread()
        {
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort, handlePacket);
        }

        public string handleResponse(HttpListenerRequest request)
        {
            lock (this)
            {
                try
                {
                    Console.WriteLine("Updating table");
                    this.sf.setSwitchingTable(this.configurationPath);
                    Console.WriteLine("Table updated!");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Unexpected error: {e.ToString()}");
                }
            }

            return "OK";
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

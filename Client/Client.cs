using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TSST
{

    class Client
    {
        public ListenerSocket listener;
        public SenderSocket sender;
        public int listenerPort;
        public int targetPort;
        public int adjacentNodePort;
        public Packet packet;

        static void Main(string[] args)
        {
            Console.SetWindowSize(45, 20);
            Thread.Sleep(1000);
            string[] lines = File.ReadAllLines(args[0]);
            Client c = new Client();
            c.listenerPort = Int32.Parse(lines[0]);
            c.targetPort = Int32.Parse(lines[1]);
            c.adjacentNodePort = Int32.Parse(lines[2]);
            Console.WriteLine("Give me some data: ");
            string message = Console.ReadLine();
            Console.WriteLine("Gimme the port: ");
            string portNumber = Console.ReadLine();
            Packet packetToSend = new Packet(message, Int32.Parse(portNumber),c.adjacentNodePort);

            c.sender.sendMessage(packetToSend.serialize(), c.targetPort);
            Console.ReadKey();
        }

        public Client()
        {
            Console.WriteLine(@"
   _____ _      _____ ______ _   _ _______ 
  / ____| |    |_   _|  ____| \ | |__   __|
 | |    | |      | | | |__  |  \| |  | |   
 | |    | |      | | |  __| | . ` |  | |   
 | |____| |____ _| |_| |____| |\  |  | |
  \_____|______|_____|______|_| \_|  |_|   
                                           
                                           ");
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

        public int handlePacket(Packet p, int port)
        {
            packet = p;
            Console.WriteLine(packet.data);
            return 0;
        }
    }
}

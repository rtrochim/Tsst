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
        public static int listenerPort;
        public static int targetPort;

        public string data;
        public static int packetID = 0;

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            string[] lines = File.ReadAllLines(args[0]);
            listenerPort = Int32.Parse(lines[0]);
            targetPort = Int32.Parse(lines[1]);
            Client c = new Client();
            //Console.ReadKey();
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
            this.sender.sendMessage("Hello world from Client!<EOF>", targetPort);
            
            ThreadStart childref2 = new ThreadStart(makePacketThread);
            Thread childThread2 = new Thread(childref2);
            childThread2.Start(); 


        }

        public void listeningThread()
        {
            Console.WriteLine("Client listening on port {0}", listenerPort);
            Console.WriteLine("I will send data to port {0}", targetPort);
            this.listener = new ListenerSocket(listenerPort);
            
        }

         public void makePacketThread()
         {
             Console.WriteLine("Give data:");
             data = Console.ReadLine();

             Packet packet = new Packet(data, listenerPort);
            packetID++;

            Stream stream = File.Open("packet"+packetID+".txt", FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(stream, packet);
            stream.Close();
            
            /*
            //check
            packet = null;

            stream = File.Open("packet" + packetID + ".txt", FileMode.Open);
            bf = new BinaryFormatter();

            packet = (Packet)bf.Deserialize(stream);
            stream.Close();

            Console.WriteLine("After deserialization ");
            Console.WriteLine("Data: " + packet.data);
            Console.WriteLine("Source Port: " + packet.sourcePort);
            Console.WriteLine("Label: " + packet.currentLabel);
            Console.WriteLine("Target Port: " + packet.targetPort);
            */
              }

    }
}

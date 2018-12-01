using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;

namespace TSST
{
    [Serializable()]
    public class Packet
    {
        public string currentLabel, data;
        public int sourcePort, targetPort;
        [NonSerialized()] public static int packetID = 0;

        public Packet(string data, int sourcePort)
        {
            Console.WriteLine("Hello I am a Packet");
            packetID++;
            this.data = data;
            this.sourcePort = sourcePort;
            getInfo();
            Console.WriteLine("currentLabel: " + currentLabel);
            Console.WriteLine("targetPort: " + targetPort);
            Console.WriteLine("Data: " + data);
        }

        public void getInfo()
        {
            string[] lines = File.ReadAllLines("Packet"+packetID+".conf");
            currentLabel = lines[0];
            targetPort = Int32.Parse(lines[1]);
           // Console.ReadKey();
        }


    }
}

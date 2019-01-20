using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TSST
{
    [Serializable()]
    public class Packet
    {
        public string data; 
        public int targetPort;
        public int nextHop;
        public int nextInterface;
        [NonSerialized()] public static int packetID = 0;

        public Packet(string data, int targetPort, int nextHop)
        {
            packetID++;
            this.data = data;
            this.targetPort = targetPort;
            this.nextHop = nextHop;
        }

        public byte[] serialize()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public static Packet deserialize(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            ms.Write(data, 0, data.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return (Packet)bf.Deserialize(ms);
        }
    }
}

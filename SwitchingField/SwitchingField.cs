using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace TSST
{
    public class SwitchingField
    {
        List<Tuple<string, string, string, string>> labelTable;
        public SwitchingField()
        {
          
        }

        public void setLabelTable(string pathToLabelTable)
        {
            List<string> lines = new List<string>(File.ReadAllLines(pathToLabelTable));
            lines.RemoveAt(0);
            lines.RemoveAt(0);
            lines.RemoveAt(0);
            string[] truncatedLines = lines.ToArray();
            labelTable = new List<Tuple<string, string, string, string>>();
            foreach (string line in truncatedLines)
            {
               string[] temp = line.Split(' ');
               labelTable.Add(Tuple.Create(temp[0], temp[1], temp[2], temp[3]));
            }

            foreach (Tuple<string, string, string, string> element in labelTable)
            {
                Console.WriteLine("Data read from file: {0} , {1}, {2}, {3}",element.Item1, element.Item2, element.Item3, element.Item4);
            }
        }

        public void commutatePacket(ref Packet packet)
        {
            int target = packet.targetPort;
            Console.WriteLine("Switching Field handles the Packet");
            if(packet.labels.Count == 0) { 
            
                Console.WriteLine("Packet has no label");
                Tuple<string, string, string, string> entry = labelTable.Find(item => (item.Item2 == target.ToString()));
                packet.labels.Add(entry.Item3);
                packet.nextHop = Int32.Parse(entry.Item4);
                Console.WriteLine("Packet got a new label: {0}, and nextHop set to {1}", entry.Item3, entry.Item4);
            }
            else
            {
                string label = packet.labels[packet.labels.Count - 1];
                Console.WriteLine("Got pachet with label {0}, removed it.", label);
                packet.labels.RemoveAt(packet.labels.Count - 1);
                Tuple<string, string, string, string> entry = labelTable.Find(item => (item.Item1 == label));
                Console.WriteLine("Set new label: {0}", entry.Item3);
                Console.WriteLine("Set nextHop: {0}", entry.Item4);
                packet.nextHop = Int32.Parse(entry.Item4);

                if (entry.Item3 != "-----")
                {
                    packet.labels.Add(entry.Item3);
                }
            }
        }
    }
}
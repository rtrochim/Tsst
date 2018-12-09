using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TSST
{
    public class SwitchingField
    {
        List<Tuple<string, string, string, string>> labelTable;
        public SwitchingField()
        {
          
        }

        public List<string> setLabelTable(string pathToLabelTable)
        {

            List<string> lines = new List<string>(File.ReadAllLines(pathToLabelTable));
            List<string> linesToReturn = new List<string>(lines);
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

            //foreach (Tuple<string, string, string, string> element in labelTable)
            //{
            //    Console.WriteLine("Data read from file: {0} , {1}, {2}, {3}",element.Item1, element.Item2, element.Item3, element.Item4);
            //}
            return linesToReturn;
        }

        public void commutatePacket(ref Packet packet, int port)
        {
            int target = packet.targetPort;
            if(packet.labels.Count == 0) {
                Console.WriteLine(@"
║EntryPort║Data║Labels║NextHop║TargetPort║
══════════════════════════════════════════");
                Console.WriteLine($"{port}, {packet.data}, - , {packet.nextHop}, {packet.targetPort}");
                Tuple<string, string, string, string> entry = labelTable.Find(item => (item.Item2 == target.ToString()));
                if (entry == null) throw new Exception();
                packet.labels.Add(entry.Item3);
                packet.nextHop = Int32.Parse(entry.Item4);
                Console.WriteLine($"{port}, {packet.data}, {packet.labels[packet.labels.Count - 1]}, {packet.nextHop}, {packet.targetPort}");
                Thread.Yield();
            }
            else
            {
                string label = packet.labels[packet.labels.Count - 1];
                packet.labels.RemoveAt(packet.labels.Count - 1);
                Tuple<string, string, string, string> entry = labelTable.Find(item => (item.Item1 == label));
                if(entry == null) throw new Exception();
                packet.nextHop = Int32.Parse(entry.Item4);

                if (entry.Item3 != "-----")
                {
                    packet.labels.Add(entry.Item3);
                }
                Console.WriteLine($"{port}, {packet.data}, {packet.labels[packet.labels.Count - 1]}, {packet.nextHop}, {packet.targetPort}");
                Thread.Yield();
            }
        }
    }
}
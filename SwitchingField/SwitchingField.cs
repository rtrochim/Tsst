using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TSST
{
    public class SwitchingField
    {
        List<Tuple<string, string, string, string>> switchingTable;
        public SwitchingField()
        {
            this.switchingTable = new List<Tuple<string, string, string, string>>();
        }

        public void setSwitchingTable(string pathToSwitchingTable)
        {
            List<string> lines = new List<string>(File.ReadAllLines(pathToSwitchingTable));
            lines.RemoveRange(0, 3);
            foreach (string item in lines)
            {
                string[] temp = item.Split(' ');
                switchingTable.Add(new Tuple<string, string, string, string>(temp[0], temp[1], temp[2], temp[3]));
            }
            Thread.Yield();
        }

        public void commutatePacket(ref Packet packet, int entryPort, ref Dictionary<int, bool[]> slots)
        {
            lock (this)
            {

                Console.WriteLine("Got Packet on: " + packet.nextHop.ToString());
                int nextHop = packet.nextHop;
                string usedSlots = packet.usedSlots;
                Tuple<string, string, string, string> entry = switchingTable.Find(item => Int32.Parse(item.Item2) == nextHop && usedSlots == item.Item1);
                if (entry != null)
                {
                    packet.nextHop = int.Parse(entry.Item4);
                    Console.WriteLine("Transmitting on: " + packet.nextHop.ToString());
                    Thread.Yield();
                }
                else
                {
                    Thread.Yield();
                    throw new Exception("Cannot commutate");
                }
            }
        }

        public void addEntry(string[] entry, ref Dictionary<int, bool[]> slots)
        {
            lock (this)
            {
                this.switchingTable.Add(new Tuple<string, string, string, string>(entry[0], entry[1], entry[2], entry[3]));
                string[] temp = entry[0].Split(':');
                for (int i = int.Parse(temp[0]); i <= int.Parse(temp[1]); i++)
                {
                    slots[int.Parse(entry[1])][i] = true;
                    slots[int.Parse(entry[3])][i] = true;
                }
                Thread.Yield();
            }
        }

        public void removeEntry(Tuple<string,string,string,string> entry)
        {
            this.switchingTable.Remove(entry);
            Thread.Yield();
        }

        public List<Tuple<string,string,string,string>> getSwitchingTable()
        {
            return this.switchingTable;
        }
    }
}
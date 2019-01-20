﻿using System;
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
        }

        public void commutatePacket(ref Packet packet, int entryPort, ref Dictionary<int, bool[]> slots)
        {
            Console.WriteLine("Got Packet on: " + packet.nextHop.ToString() + " TargetPort: " + packet.targetPort + " EntryPort: " + entryPort);
            int nextHop = packet.nextHop;
            Tuple<string, string, string, string> entry = switchingTable.Find(item => Int32.Parse(item.Item2) == nextHop);
            if (entry != null)
            {
                packet.nextHop = int.Parse(entry.Item4);

                Thread.Yield();
            }
            else
            {
                throw new Exception("Cannot commutate");
                Thread.Yield();
            }

        }

        public void addEntry(string[] entry)
        {
            this.switchingTable.Add(new Tuple<string,string,string,string>(entry[0], entry[1], entry[2], entry[3]));
            this.switchingTable.ForEach(item => { Console.WriteLine("Item: {0} {1} {2} {3}", item.Item1, item.Item2, item.Item3, item.Item4); });
        }

        public void printSwitchingTable()
        {
            Console.WriteLine("Switching table");
            foreach(Tuple<string,string,string,string> item in this.switchingTable)
            {
                Console.WriteLine("{0} {1} {2} {3}", item.Item1, item.Item2, item.Item3, item.Item4);
            }
        }
    }
}
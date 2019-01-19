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
          
        }

        public int setSwitchingTable(string pathToSwitchingTable)
        {
            List<string> lines = new List<string>(File.ReadAllLines(pathToSwitchingTable));
            lines.RemoveRange(0, 3);
            foreach(string item in lines)
            {
                Console.WriteLine(item);
            }
            return 0;
        }

        public void commutatePacket(ref Packet packet, int entryPort, ref Dictionary<int,bool[]> slots)
        {
            throw new Exception();
            Thread.Yield();
        }
    }
}
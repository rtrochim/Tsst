using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TSST
{
    public class SwitchingField
    {
        public SwitchingField()
        {
          
        }

        public int setRoutingTable(string pathToLabelTable)
        {
            return 0;
        }

        public void commutatePacket(ref Packet packet, int entryPort)
        {
            throw new Exception();
            Thread.Yield();
        }
    }
}
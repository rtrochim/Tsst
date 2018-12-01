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
        // Map entry label to exit label
        public Dictionary<char, char> labels;
        // Map exit label to exit port
        public Dictionary<char, int> exitPorts;

        public SwitchingField()
        {
            labels = new Dictionary<char, char>();
            exitPorts = new Dictionary<char, int>();
        }
    }
}
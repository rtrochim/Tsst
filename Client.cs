using System;
using System.Collections.Generic;

namespace TSST
{
    class Client
    {
        List<Port> ports;
        SwitchingField switchingField;
        Agent agent;

        public Client()
        {
            Console.WriteLine("I am Client");
        }
    }
}

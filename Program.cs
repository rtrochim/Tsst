using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSST
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Client> clients;
            List<Node> nodes;
            ManagementCenter managementCenter;
            CableCloud cableCloud;

            using (var p1 = new Process())
            {
                p1.StartInfo.FileName = null;
            }



            Console.WriteLine("I am Program");
            Console.ReadKey();

        }
    }
}

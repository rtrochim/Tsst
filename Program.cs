using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TSST
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length ==0)
            {
                Console.WriteLine("I am Program");

                List<Client> clients;
                List<Node> nodes;
                ManagementCenter managementCenter;
                CableCloud cableCloud;


                Process process = new Process();
                process.StartInfo.FileName = "TSST.exe";
                process.StartInfo.Arguments = "-n";
                process.Start();


                Console.ReadKey();
            }
            else if (args.Length!=0)
            {
                Console.WriteLine("I am Child");
                Console.ReadKey();
            }
        }
    }
}

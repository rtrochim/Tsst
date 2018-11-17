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


                Process process1 = new Process();
                process1.StartInfo.FileName = "TSST.exe";
                process1.StartInfo.Arguments = "-CC";
                process1.Start();

                Process process2 = new Process();
                process2.StartInfo.FileName = "TSST.exe";
                process2.StartInfo.Arguments = "-MC";
                process2.Start();

                Process process3 = new Process();
                process3.StartInfo.FileName = "TSST.exe";
                process3.StartInfo.Arguments = "-N";
                process3.Start();

                Process process4 = new Process();
                process4.StartInfo.FileName = "TSST.exe";
                process4.StartInfo.Arguments = "-C";
                process4.Start();

                Console.ReadKey();
            }
            else if (args[0] == "-CC")
            {
                CableCloud cableCloud;
                Console.WriteLine("I am CableCloud");
                Console.ReadKey();
            }
            else if (args[0] == "-MC")
            {
                ManagementCenter managementCenter;
                Console.WriteLine("I am ManagementCenter");
                Console.ReadKey();
            }
            else if (args[0] == "-N")
            {
                Node node;
                Console.WriteLine("I am Node");
                Console.ReadKey();
            }
            else if (args[0] == "-C")
            {
                Client client;
                Console.WriteLine("I am Client");
                Console.ReadKey();
            }
        }
    }
}

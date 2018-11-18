using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TSST
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length ==0)
            {
                List<Process> processes = new List<Process>();
                Console.WriteLine("I am Main Program");
                string[] programConfig = File.ReadAllLines("Program.conf", Encoding.UTF8);

                foreach (string line in programConfig)
                {
                    string[] config = line.Split(null);
                    Process p = new Process();
                    processes.Add(p);
                    p.StartInfo.FileName = "TSST.exe";
                    p.StartInfo.Arguments = config[1];
                    p.Start();
                }

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
                Node node = new Node();
                Console.WriteLine("I am Node");
                Console.ReadKey();
            }
            else if (args[0] == "-C")
            {
                Client client = new Client();
                Console.WriteLine("I am Client");
                Console.ReadKey();
            }
        }
    }
}

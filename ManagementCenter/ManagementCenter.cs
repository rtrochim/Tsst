using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TSST
{
    class ManagementCenter
    {
        static void Main(string[] args)
        {
            ManagementCenter manager = new ManagementCenter();
            while (true)
            {
                Console.WriteLine(@"
[L] List all entries from Node
[A] Add entry to Node
[R] Remove entry from Node
What to do:
            ");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "L":
                        Console.WriteLine("Which node: ");
                        string nodeID = Console.ReadLine();
                        manager.listEntries(nodeID);
                        break;
                    case "A":
                        Console.WriteLine("Which node: ");
                        break;
                    case "R":
                        Console.WriteLine("Which node: ");
                        break;
                    default:
                        Console.WriteLine("Invalid option!");
                        break;

                }
            }
            

        }

        Manager manager;
        Agent agent;
        public ManagementCenter()
        {
            Console.WriteLine(@"
  __  __          _   _          _____ ______ _____  
 |  \/  |   /\   | \ | |   /\   / ____|  ____|  __ \ 
 | \  / |  /  \  |  \| |  /  \ | |  __| |__  | |__) |
 | |\/| | / /\ \ | . ` | / /\ \| | |_ |  __| |  _  / 
 | |  | |/ ____ \| |\  |/ ____ \ |__| | |____| | \ \ 
 |_|  |_/_/    \_\_| \_/_/    \_\_____|______|_|  \_\
                                                     
                                                     
");
        }

        public void listEntries(string nodeID)
        {
            string[] lines;
            int i = 1;
            string path = $@"..\..\..\TEST\configs\Node{nodeID}.conf";
            try
            {
                List<string> linesList = new List<string>(File.ReadAllLines(path));
                linesList.RemoveAt(0);
                linesList.RemoveAt(0);
                linesList.RemoveAt(0);
                lines = linesList.ToArray();
            }
            catch(Exception e)
            {
                Console.WriteLine("Cannot open file! {0}");
                return;
            }

            Console.WriteLine(@"
╔═══╦═══════════╦════════════╦══════════╦═══════╗
║ID ║Entry Label║ TargetPort ║Exit Label║NextHop║
╠═══╬═══════════╬════════════╬══════════╬═══════╣");         
            foreach( string line in lines)
            {
                string[] values = line.Split(' ');
                Console.WriteLine($@"║ {i} ║     {values[0]}     ║   {values[1]}    ║    {values[2]}     ║ {values[3]} ║");
                i++;
            }

            Console.WriteLine(@"╚═══╩═══════════╩════════════╩══════════╩═══════╝");


        }
    }
}

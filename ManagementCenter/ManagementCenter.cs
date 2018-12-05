using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TSST
{
    class ManagementCenter
    {
        static void Main(string[] args)
        {
            Agent agent;
            ManagementCenter mc = new ManagementCenter();
            while (true)
            {
                Console.WriteLine(@"
[L] List all entries from Node
[A] Add entry to Node
[R] Remove entry from Node
What to do:");
                string option = Console.ReadLine();
                switch (option)
                {
                    case "L":
                        Console.WriteLine("Which node: ");
                        mc.listEntries(Console.ReadLine());
                        break;
                    case "A":
                        Console.WriteLine("Which node: ");
                        mc.addEntry(Console.ReadLine());
                        break;
                    case "R":
                        Console.WriteLine("Which node: ");
                        mc.deleteEntry(Console.ReadLine());
                        break;
                    default:
                        Console.WriteLine("Invalid option!");
                        break;

                }
            }
            

        }


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
                Console.WriteLine("ERROR: {0}", e.Message);
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

        public void deleteEntry(string nodeID)
        {
            int lineToDelete;
            listEntries(nodeID);
            string path = $@"..\..\..\TEST\configs\Node{nodeID}.conf";
            Console.WriteLine("Which ID do you want to delete?");
            try
            {
                lineToDelete = Int32.Parse(Console.ReadLine());
            }
            catch(Exception e)
            {
                Console.WriteLine("Wrong ID");
                return;
            }
            try
            {
                List<string>  linesList = new List<string>(File.ReadAllLines(path));
                linesList.RemoveAt(lineToDelete + 2);
                File.WriteAllLines(path, linesList.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                return;
            }
        }

        public void addEntry(string nodeID)
        {
            string lineToAdd;
            listEntries(nodeID);
            string path = $@"..\..\..\TEST\configs\Node{nodeID}.conf";
            Console.WriteLine("Type in label, target port, out label and next hop: ");
            try
            {
                lineToAdd = Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Wrong ID");
                return;
            }
            try
            {
                string[] arguments = lineToAdd.Split(' ');
                if (arguments.Count() != 4) throw new ArgumentException("Incorrect number of input parameters");
                File.AppendAllText(path, lineToAdd + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                return;
            }
        }
    }
}

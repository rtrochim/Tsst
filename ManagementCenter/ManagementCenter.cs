using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace TSST
{
    class ManagementCenter
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        static void Main(string[] args)
        {
            IntPtr ptr = GetConsoleWindow();
            MoveWindow(ptr, Int32.Parse(args[0]), Int32.Parse(args[1]), 1000, 400, true);

            Console.SetWindowSize(75, 18);
            Agent agent;
            ManagementCenter mc = new ManagementCenter();
            lock (mc)
            {
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

        }
        public ManagementCenter()
        {
            Console.WriteLine(@"
  __  __          _   _          _____ ______ _____  
 |  \/  |   /\   | \ | |   /\   / ____|  ____|  __ \ 
 | \  / |  /  \  |  \| |  /  \ | |  __| |__  | |__) |
 | |\/| | / /\ \ | . ` | / /\ \| | |_ |  __| |  _  / 
 | |  | |/ ____ \| |\  |/ ____ \ |__| | |____| | \ \ 
 |_|  |_/_/    \_\_| \_/_/    \_\_____|______|_|  \_\");
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
╔═══╦═════════╦══════════╦══════════╦═════════╦═══════╦═══════╗
║ID ║EntryPort║EntryLabel║TargetPort║Operation║Operand║NextHop║
╠═══╬═════════╬══════════╬══════════╬═════════╬═══════╬═══════╣");         
            foreach( string line in lines)
            {
                string[] values = line.Split(' ');
                string toInsert  = null;
                for(int j = 0; j < 9-values[3].Length; j++)
                {
                    toInsert += " ";
                }
                //toInsert = toInsert.Insert(floorDivide(9 - values[3].Length, 2), values[3]);
                string index = i > 9 ? $" {i}" : $" {i} ";
                string instructions = values[3].Length > 4 ? $"{values[3]}" : values[3].Length == 3 ? $"   {values[3]}   " : $"   {values[3]}  ";
                string operand = values[4].Length > 1 ? $"  {values[4]}  " : $"   {values[4]}   ";
                Console.WriteLine($@"║{index}║  {values[0]}  ║    {values[1]}     ║  {values[2]}   ║{instructions}║{operand}║ {values[5]} ║");
                i++;
            }

            Console.WriteLine(@"╚═══╩═════════╩══════════╩══════════╩═════════╩═══════╩═══════╝");


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
            Console.WriteLine("Type in entry port, entry label, target port, operation, operand and next hop: ");
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
                if (arguments.Count() != 6) throw new ArgumentException("Incorrect number of input parameters");
                File.AppendAllText(path, lineToAdd + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
                return;
            }
        }

        public static int floorDivide(int a, int b)
        {
            return (a / b - Convert.ToInt32(((a < 0) ^ (b < 0)) && (a % b != 0)));
        }
    }
}

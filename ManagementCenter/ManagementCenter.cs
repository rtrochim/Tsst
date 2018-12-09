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
╔═══╦═══════════╦════════════╦══════════╦═══════╗
║ID ║Entry Label║ TargetPort ║Exit Label║NextHop║
╠═══╬═══════════╬════════════╬══════════╬═══════╣");         
            foreach( string line in lines)
            {
                string[] values = line.Split(' ');
                string[] toInsert = new string[4];
                for(int j = 0; j < 11-values[0].Length; j++)
                {
                    toInsert[0] += " ";
                }
                toInsert[0] = toInsert[0].Insert(floorDivide(11 - values[0].Length, 2), values[0]);
                toInsert[1] = values[1];
                for (int j = 0; j < 10 - values[2].Length; j++)
                {
                    toInsert[2] += " ";
                }
                toInsert[2] = toInsert[2].Insert(floorDivide(10 - values[2].Length, 2), values[2]);
                toInsert[3] = values[3];

                Console.WriteLine($@"║ {i} ║{toInsert[0]}║   {toInsert[1]}    ║{toInsert[2]}║ {toInsert[3]} ║");
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

        public static int floorDivide(int a, int b)
        {
            return (a / b - Convert.ToInt32(((a < 0) ^ (b < 0)) && (a % b != 0)));
        }
    }
}

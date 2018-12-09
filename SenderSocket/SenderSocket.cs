using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TSST
{
    public class SenderSocket
    {
        public int port;
        public SynchronousSocketClient socketClient;
        public SenderSocket()
        {
            lock (this)
            {
                this.socketClient = new SynchronousSocketClient();
            }
        }

        public void sendMessage(byte[] msg, int port)
        {
            Console.WriteLine("Sent packet to port: {0}", port);
            this.socketClient.SendMessage(msg, port);
            Console.WriteLine("══════════════════════════════════");
            Thread.Yield();

        }
    }

    public class SynchronousSocketClient
    {
        Socket sender;
        IPEndPoint remoteEP;

        public void SendMessage(byte[] msg, int port)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                this.remoteEP = new IPEndPoint(ipAddress, port);

                // Create a TCP/IP  socket.  
                this.sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    this.sender.Connect(this.remoteEP);

                    // Console.WriteLine("Socket connected to {0}",
                       // this.sender.RemoteEndPoint.ToString());
                   

                    // Send the data through the socket.  
                    int bytesSent = this.sender.Send(msg);

                    // Receive the response from the remote device.  
                    // int bytesRec = this.sender.Receive(bytes);
                    // Console.WriteLine("Echoed test = {0}",
                        // Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    this.sender.Shutdown(SocketShutdown.Both);
                    this.sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}

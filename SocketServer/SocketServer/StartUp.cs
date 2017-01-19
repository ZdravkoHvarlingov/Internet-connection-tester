using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace SocketServer
{
    class StartUp
    {
        static void Main(string[] args)
        {
            string neededPort = ConfigurationManager.AppSettings["InternetPort"];

            TcpListener serverSocket = new TcpListener(IPAddress.Any, int.Parse(neededPort));
            TcpClient clientSocket = new TcpClient();
     
            serverSocket.Start();
            Console.WriteLine("Listening...");

            while (true)
            {
                clientSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Connection accepted.");

                string dataFromClient = null;

                NetworkStream networkStream = clientSocket.GetStream();

                var bytesFrom = new byte[clientSocket.ReceiveBufferSize];
                int data = networkStream.Read(bytesFrom, 0, clientSocket.ReceiveBufferSize);
                dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom, 0, data);

                FileUtilities.LogMessageToFile(dataFromClient, DateTime.Now);

                clientSocket.Close();

                Console.WriteLine("Client disconnected.");
            }

            clientSocket.Close();
            serverSocket.Stop();
        }
    }
}

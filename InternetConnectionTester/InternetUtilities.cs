using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace InternetConnectionTester
{
    public static class InternetUtilities
    {
        public static bool IsThereInternetConnection()
        {
            Ping ping = new Ping();
            try
            {
                PingReply pingresult = ping.Send("8.8.8.8");
                if (pingresult.Status.ToString() == "Success")
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        public static string GetTempPath()
        {
            string path = Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";

            return path;
        }

        public static void LogMessageToFile(EventType eventType, DateTime dateTime)
        {
            StreamWriter sw = System.IO.File.AppendText(
                GetTempPath() + "InternetAvailabilityLog.txt");
            string userName = System.Environment.UserName;
            string machineName = System.Environment.MachineName;
            var eventName = Enum.GetName(eventType.GetType(), eventType);

            try
            {
                string logLine = System.String.Format(
                    "{0};{1};{2};{3}.{4}.{5} {6}:{7}:{8}",machineName, userName,eventName, 
                     dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                sw.WriteLine(logLine);              
            }
            finally
            {
                sw.Close();
            }
        }

        public static bool SendDataToServer(string content)
        {
            content = "InternetLogStream2017" + content;

            try
            {
                var ip = ConfigurationManager.AppSettings["IPtoConnect"];
                var port = ConfigurationManager.AppSettings["PortToConnect"];

                TcpClient clientSocket = new TcpClient();
                NetworkStream serverStream;

                clientSocket.Connect(ip, int.Parse(port));

                serverStream = clientSocket.GetStream();
                byte[] outStream = System.Text.Encoding.ASCII.GetBytes(content);
                serverStream.Write(outStream, 0, outStream.Length);
                serverStream.Flush();

                clientSocket.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }     
        }
    }
}

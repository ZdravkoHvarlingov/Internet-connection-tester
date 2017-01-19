using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketServer
{
    public static class FileUtilities
    {
        public static string GetTempPath()
        {
            string path = Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";

            return path;
        }
        public static void LogMessageToFile(string msg, DateTime dateTime)
        {
            bool isFileEmpty;

            try
            {
                isFileEmpty = new FileInfo(GetTempPath() + "InternetAvailabilityFullLog.txt").Exists ||
                new FileInfo(GetTempPath() + "InternetAvailabilityFullLog.txt").Length == 0;
            }
            catch (Exception)
            {
                isFileEmpty = true;   
            }

            StreamWriter sw = File.AppendText(
                GetTempPath() + "InternetAvailabilityFullLog.txt");
            
            try
            {
                if (isFileEmpty)
                {
                    sw.WriteLine("");
                }

                sw.WriteLine(dateTime.ToString());
                string logLine = msg;
                sw.WriteLine(logLine);
            }
            finally
            {
                sw.Close();
            }
        }   
    }
}

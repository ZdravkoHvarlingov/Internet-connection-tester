using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyser
{
    class StartUp
    {
        public static string GetFilePath()
        {
            string path = Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";
            
            return path + "InternetAvailabilityFullLog.txt";
        }

        static string MergeTextInAHtmlFile(string htmlFile, string text)
        {
            int startingIndex = htmlFile.IndexOf("<tbody>") + 7;

            string firstPart = htmlFile.Substring(0, startingIndex);
            string secondPart = htmlFile.Substring(startingIndex);

            firstPart += "\n\r" + text + secondPart;

            return firstPart;
        }

        static void CopyFileAndHandleException(string source, string destination)
        {
            try
            {
                File.Copy(source, destination);
            }
            catch (Exception)
            {
                return;
            }
        }

        public static string GetHtmlFileAsString()
        {
            string resourceName = " ";

            Assembly asa = Assembly.GetExecutingAssembly();
            foreach (var name in asa.GetManifestResourceNames())
            {
                if (name.Contains("template.html"))
                {
                    resourceName = name;
                }
            }

            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                reader.Close();

                return result;               
            }
        }

        public static void ExtractEmbeddedResource(string outputDir)
        {
            string resourceName = " ";

            Assembly asa = Assembly.GetExecutingAssembly();
            foreach (var name in asa.GetManifestResourceNames())
            {
                if (name.Contains("icon.ico"))
                {
                    resourceName = name;
                }
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {

                using (System.IO.FileStream fileStream = new FileStream(Path.Combine(outputDir, "icon.ico"), FileMode.Create))
                {
                    for (int i = 0; i < stream.Length; i++)
                    {
                        fileStream.WriteByte((byte)stream.ReadByte());
                    }
                    fileStream.Close();
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                var test = new StreamReader(GetFilePath());
            }
            catch (Exception)
            {
                return;
            }

            var streamReader = new StreamReader(GetFilePath());
            string line;
            var programs = new List<InternetConnectionTester>();
            TimeSpan timeSpan;
            bool isEventsStart = false;
            var events = new List<ProgramEvent>();
            var separatedPartData = new List<string>();

            while ((line = streamReader.ReadLine()) != null)
            {
                if (isEventsStart && line.Length == 0)
                {
                    //analyse data
                    if (separatedPartData.Count > 3)
                    {
                        timeSpan = TimeSpan.Parse(separatedPartData[1].Substring(16));

                        var splitInfo = separatedPartData[2].Split(';');
                        int indexToWrite = -1;

                        for (int index = 0; index < programs.Count; index++)
                        {
                            if (programs[index].ComputerName == splitInfo[0] &&
                                programs[index].UserName == splitInfo[1])
                            {
                                indexToWrite = index;
                            }
                        }

                        if (indexToWrite == -1)
                        {
                            programs.Add(new InternetConnectionTester(
                                splitInfo[0],
                                splitInfo[1]));

                            indexToWrite = programs.Count - 1;
                        }

                        for (int index = 2; index < separatedPartData.Count; index++)
                        {
                            splitInfo = separatedPartData[index].Split(';');

                            events.Add(new ProgramEvent((EventType)Enum.Parse(typeof(EventType), splitInfo[2], true),
                                DateTime.Parse(splitInfo[3])
                                ));
                        }

                        programs[indexToWrite].AddEventsToAnalyse(events, timeSpan);

                        events.Clear();
                    }

                    separatedPartData.Clear();
                    isEventsStart = false;
                }

                if (isEventsStart)
                {
                    separatedPartData.Add(line);
                }

                if (line.Length == 0)
                {
                    isEventsStart = true;
                }
            }

            streamReader.Close();

            
            string finalResult = "";
            foreach (var program in programs)
            {
                decimal percent = program.TimeWithInternetInPercentages();

                finalResult += string.Format("<tr>\r\n<td>{0}</td>\r\n<td>{1}</td>\r\n<td>{2}</td>\r\n</tr>\r\n",
                    program.ComputerName + " " + program.UserName, program.DataTimeSpan.ToString(), program.TimeWithInternetInPercentages());
                
            }

            var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            try
            {
                var serverDirectory = ConfigurationManager.AppSettings["ServerDirectory"];
                
                string htmlFile = GetHtmlFileAsString();
                
                htmlFile = MergeTextInAHtmlFile(htmlFile, finalResult);

                if (serverDirectory != "default")
                {
                    StreamWriter strWriter = File.AppendText(serverDirectory + @"\index.html");
                    ExtractEmbeddedResource(serverDirectory);

                    strWriter.Close();

                    File.WriteAllText(serverDirectory + @"\index.html", htmlFile);
                }
                else
                {
                    StreamWriter strWriter = File.AppendText(directory + @"\index.html");
                    
                    strWriter.Close();
                    ExtractEmbeddedResource(directory);
                    File.WriteAllText(directory + @"\index.html", htmlFile);
                }
            }
            catch (Exception)
            {
                return;
            }       
        }
    }
}

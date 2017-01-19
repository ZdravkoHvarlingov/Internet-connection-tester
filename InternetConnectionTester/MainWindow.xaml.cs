using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.IO;
using IWshRuntimeLibrary;

namespace InternetConnectionTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        bool hasConnection = false;
        bool previousConnResult = false;
        bool isFirstCheck = true;
        BackgroundWorker bgWorker;
        DateTime intQueryTime;
        DateTime lastConnectionChange;
        DateTime startOfProgram;
        TimeSpan timeSpanToSend;
        bool isDataSend = true;
        string contentToSend;      
         
        public MainWindow()
        { 
            InitializeComponent();

            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            if (Properties.Settings.Default.timeElapsed > new TimeSpan(0, 0, 0))
            {
                try
                {
                    contentToSend = System.IO.File.ReadAllText(InternetUtilities.GetTempPath() + "InternetAvailabilityLog.txt");
                    System.IO.File.WriteAllText(InternetUtilities.GetTempPath() + "InternetAvailabilityLog.txt", string.Empty);

                    isDataSend = false;
                }
                catch (Exception)
                {
                    isDataSend = true;
                }
                
                timeSpanToSend = Properties.Settings.Default.timeElapsed;
                Properties.Settings.Default.timeElapsed = new TimeSpan(0, 0, 0);
                Properties.Settings.Default.Save();                
            }

            InternetUtilities.LogMessageToFile(EventType.ProgramStartUp, DateTime.Now);

            startOfProgram = DateTime.Now;
            intQueryTime = new DateTime();
            lastConnectionChange = new DateTime();
            lastConnectionChange = DateTime.Now;

            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += BgWorker_DoWork;
            bgWorker.RunWorkerCompleted += BgWorker_RunWorkerCompleted;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Tick += Timer_Tick;

            timer.Start();

            if (Properties.Settings.Default.autoStart)
            {
                CreateStartupFolderShortcut();
                AutoStartCheckBox.IsChecked = true;
            }
            else
            {
                DeleteStartupFolderShortcuts("InternetConnectionTester.exe");
                AutoStartCheckBox.IsChecked = false;
            }
        }
        

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (previousConnResult == hasConnection && !isFirstCheck)
            {
                if (hasConnection)
                {
                    if (!isDataSend)
                    {
                        bool successful = InternetUtilities.SendDataToServer("TimeSpanOfData: " + timeSpanToSend.ToString()
                            + System.Environment.NewLine + contentToSend);

                        if (successful)
                        {
                            contentToSend = null;

                            isDataSend = true;
                        }
                    }
                }

                timer.Start();

                return;
            }
            
            isFirstCheck = false;

            string display = string.Format("{0}.{1}.{2} {3}h {4}m {5}s.", intQueryTime.Year, intQueryTime.Month,
                intQueryTime.Day, intQueryTime.Hour, intQueryTime.Minute, intQueryTime.Second);

            var timeElapsed = intQueryTime - lastConnectionChange;
            
            lastConnectionChange = intQueryTime;

            if (hasConnection)
            {
                var uriSource = new Uri(@"/InternetConnectionTester;component/Images/GreenBall.png", UriKind.Relative);
                indicatorImage.Source = new BitmapImage(uriSource);

                mainTextBlock.Text = "Internet connection established!";
                mainTextBlock.Foreground = Brushes.DarkGreen;
                infoTextBlock.Text = "Established connection at: " + display;

                trayTxt.Text = "Internet connection AVAILABLE";
                trayImage.Source = new BitmapImage(uriSource);
                trayBorder.BorderBrush = Brushes.Green;

                InternetUtilities.LogMessageToFile(EventType.GotConnection, intQueryTime);
            }
            else
            {
                var uriSource = new Uri(@"/InternetConnectionTester;component/Images/RedBall.png", UriKind.Relative);
                indicatorImage.Source = new BitmapImage(uriSource);

                mainTextBlock.Text = "Internet connection lost!";
                mainTextBlock.Foreground = Brushes.Red;
                infoTextBlock.Text = "Lost connection at: " + display;

                trayTxt.Text = "Internet connection NOT AVAILABLE";
                trayImage.Source = new BitmapImage(uriSource);
                trayBorder.BorderBrush = Brushes.Red;

                InternetUtilities.LogMessageToFile(EventType.ConnectionLoss, intQueryTime);
            }

            timer.Start();
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            previousConnResult = hasConnection;
            hasConnection = InternetUtilities.IsThereInternetConnection();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            intQueryTime = DateTime.Now;

            Properties.Settings.Default.timeElapsed = DateTime.Now - startOfProgram;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.timeElapsed > new TimeSpan(24, 0, 0))
            {
                timeSpanToSend = Properties.Settings.Default.timeElapsed;
                Properties.Settings.Default.timeElapsed = new TimeSpan(0, 0, 0);
                Properties.Settings.Default.Save();

                string userName = System.Environment.UserName;
                string machineName = System.Environment.MachineName;
                
                string logLine = System.String.Format(
                    "{0};{1};{2};{3:G}", machineName, userName, "SystemStartUp", DateTime.Now);
                contentToSend = logLine + System.Environment.NewLine;
               
                contentToSend += System.IO.File.ReadAllText(InternetUtilities.GetTempPath() + "InternetAvailabilityLog.txt");
                System.IO.File.WriteAllText(InternetUtilities.GetTempPath() + "InternetAvailabilityLog.txt", string.Empty);
                InternetUtilities.LogMessageToFile(EventType.ProgramStartUp, DateTime.Now);

                isDataSend = false;
            }

            bgWorker.RunWorkerAsync();

            timer.Stop();
        }

        private void mainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            this.Hide();
        }

        private void showMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void extMenuItem_Click(object sender, RoutedEventArgs e)
        { 
            Application.Current.Shutdown();
        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemLogFile_Click(object sender, RoutedEventArgs e)
        {
            string path = Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";

            path += "InternetAvailabilityLog.txt";

            System.Diagnostics.Process.Start(path);
        }

        private void MenuItemClearLogFile_Click(object sender, RoutedEventArgs e)
        {
            var isYes = MessageBox.Show("Erase log file content?", "Question",
               MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (isYes == MessageBoxResult.No)
            {
                return;
            }

            string path = Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";

            path += "InternetAvailabilityLog.txt";

            System.IO.File.WriteAllText(path, string.Empty);
            InternetUtilities.LogMessageToFile(EventType.ProgramStartUp, DateTime.Now);
            Properties.Settings.Default.timeElapsed = new TimeSpan(0, 0, 0);
            Properties.Settings.Default.Save();
        }

        public static void CreateStartupFolderShortcut()
        {
            WshShellClass wshShell = new WshShellClass();
            IWshRuntimeLibrary.IWshShortcut shortcut;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            // Create the shortcut
            string exeName = Application.Current.MainWindow.GetType().Assembly.GetName().Name;
            shortcut =
              (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                startUpFolderPath + "\\" + exeName + ".lnk");
     

            shortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            shortcut.WorkingDirectory = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            shortcut.IconLocation = directory + @"\AppImages\icon.ico";
            
            shortcut.Description = "Launch InternetConnectionTester";
            shortcut.Save();
        }

        public string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link =
                  (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return String.Empty; // Not found
        }

        public void DeleteStartupFolderShortcuts(string targetExeName)
        {
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                if (shortcutTargetFile.EndsWith(targetExeName,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.Delete(fi.FullName);
                }
            }
        }

        private void AutoStartCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (AutoStartCheckBox.IsChecked == true)
            {
                CreateStartupFolderShortcut();
                Properties.Settings.Default.autoStart = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                DeleteStartupFolderShortcuts("InternetConnectionTester.exe");
                Properties.Settings.Default.autoStart = false;
                Properties.Settings.Default.Save();
            }
        }
    }
}

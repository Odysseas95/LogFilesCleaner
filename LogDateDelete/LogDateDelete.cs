using System;
using System.IO;
using System.Threading;
using System.Timers;

namespace LogDateDelete
{
    class LogDateDeleteApp
    {
        public static Logger logger = new Logger();
        public static int DeletedFilesCount = 0;
        public static int SearchedFiles = 0;
        public static int SearchedDirs = 0;

        public static System.Timers.Timer Timer;

        public static string RootDirectory;
        public static int DaysBack;
        public static int Interval;

        public LogDateDeleteApp()
        {
            bool daysFlag;
            bool intervalFlag;
            string daysCheck;
            string intervalCheck;
            Properties.Settings settings;

            try
            {
                //Show Settings
                settings = Properties.Settings.Default;
                Console.WriteLine($"SETTINGS APPLIED \nApplied Path: {settings.RootDir} \nLog file directory: {settings.LogDir}" +
                    $"\nDays Back to keep: {settings.DaysBack} \nInterval: {settings.Interval}");

                intervalCheck = Properties.Settings.Default.Interval;
                daysCheck = Properties.Settings.Default.DaysBack;
                RootDirectory = Properties.Settings.Default.RootDir;

                //check if days and interval are valid
                daysFlag = Int32.TryParse(daysCheck, out DaysBack);
                intervalFlag = Int32.TryParse(intervalCheck, out Interval);

                if (daysFlag == false)
                {
                    Console.WriteLine("Days Back value is wrong, taking default value of 5 days back");
                    DaysBack = 5;
                }
                if (intervalFlag == false)
                {
                    Console.WriteLine("Interval value is wrong, taking default value of 1 min");
                    Interval = 60000;
                }

                //Initialize Timer
                Timer = new System.Timers.Timer();
                Timer.Interval = Interval;
                Timer.Elapsed += OnTimedEvent;

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: See log file for information", e.Message);
                logger.LogEvent($"EXCEPTION HANDLER: {e.Message}");
            }

        }
        public void Start()
        {
            //Timer Start, called by service
            Timer.Start();

        }
        public void Stop()
        {
            //Timer Stop, called by service
            Console.WriteLine("Process stops");
            Timer.Stop();
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                LogDateDeleteApp.Timer.Enabled = false;
                //Event on timer engage
                Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);

                //Loggin start datetime & Calling the class for processing
                logger.LogEvent($"Processing Starts @ {DateTime.Now}");
                Processor filesProccessed = new Processor(RootDirectory, DaysBack);
                logger.LogEvent($"Processing Ended \n Searched Directories: {SearchedDirs} " +
                    $"\n Searched Files: {SearchedFiles} \n Files Deleted: {DeletedFilesCount}");

                //Zeroing the logged values
                DeletedFilesCount = 0;
                SearchedFiles = 0;
                SearchedDirs = 0;

            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.ToString() + " \nPLEASE SET AN EXISTING ROOT PATH");
                Console.ReadLine();
                System.Environment.Exit(0);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.ToString() + " \nTHE APP WILL TERMINATE!");
                Console.ReadLine();
                System.Environment.Exit(0);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: See log file for information", ex.Message);
                logger.LogEvent($"EXCEPTION HANDLER: {ex.Message}");
            }
            finally
            {
                LogDateDeleteApp.Timer.Enabled = true;
            }
        }
        public class Processor
        {
            public static DateTime WantedDate;

            public Processor(string rootPath, int days)
            {
                //initializing the direcotry search
                WantedDate = GetDate(days);

                if (File.Exists(rootPath))
                {
                    ProcessFile(rootPath);
                }
                else if (Directory.Exists(rootPath))
                {
                    ProcessDirectory(rootPath);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", rootPath);
                }

            }

            public static DateTime GetDate(int daysBack)
            {
                //Get Wanted date by comparing
                DateTime dateToCompare = DateTime.Now.AddDays(-daysBack);

                return dateToCompare;
            }
            public static void ProcessDirectory(string targetDirectory)
            {
                //Searching Directory
                SearchedDirs++;
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                {
                    ProcessFile(fileName);
                }
                string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                foreach (string subdirectory in subdirectoryEntries)
                {
                    ProcessDirectory(subdirectory);
                }
            }
            public static void ProcessFile(string path)
            {
                //Searching Files
                var fileInfo = new FileInfo(path);
                SearchedFiles++;

                if (fileInfo.LastWriteTime <= WantedDate)
                {
                    //Logging and Deleting
                    logger.LogDelete($"Deleted file: {fileInfo.FullName} \n File Date/Time: {fileInfo.CreationTime} \n File Path: {path}");
                    File.Delete(path);
                    Console.WriteLine($"{path} is deleted.");
                    DeletedFilesCount++;
                }

            }
        }
    }
}



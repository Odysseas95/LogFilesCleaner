using System;
using System.IO;
using System.Timers;
using static LogFilesCleaner.Logger;

namespace LogFilesCleaner
{
    class LogDateDeleteService
    {
        // PA
        public static Logger logger;
        public static int DeletedFilesCount, SearchedFiles, SearchedDirs, DaysBack, IntervalMins;
        public static Timer Tmr_CheckFiles;
        public static string RootDirectory;
        public LogDateDeleteService()
        {
            logger = new Logger();

            try
            {            
                RootDirectory = Properties.Settings.Default.RootDir;

                //check if days and interval are valid
                if (!Int32.TryParse(Properties.Settings.Default.DaysBack, out DaysBack))
                {
                    logger.LogEvent("Days Back value is wrong, taking default value of 5 days back", LogType.Warning);
                    DaysBack = 5;
                }
                if (!Int32.TryParse(Properties.Settings.Default.IntervalMins, out IntervalMins))
                {
                    logger.LogEvent("Interval value is wrong, taking default value of 1 min", LogType.Warning);
                    IntervalMins = 60000;
                }
                else
                {
                    IntervalMins *= 60000;
                }
                

                logger.LogEvent($"SERVICE STARTED - SETTINGS APPLIED | Applied Path: {Properties.Settings.Default.RootDir} | Log file directory: {Properties.Settings.Default.LogDir}" +
                    $" | Days Back to keep: {DaysBack} | Interval: {IntervalMins/60000} Mins", LogType.Info);

                //Initialize Timer
                Tmr_CheckFiles = new Timer();
                Tmr_CheckFiles.Interval = IntervalMins;
                Tmr_CheckFiles.Elapsed += OnTimedEvent;
                Tmr_CheckFiles.Enabled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
                logger.LogEvent($"{e.Message}", LogType.Error);
            }

        }
        public void Start()
        {
            //Timer Start, called by service
            Tmr_CheckFiles.Start();

        }
        public void Stop()
        {
            //Timer Stop, called by service
            Console.WriteLine("Process stops");
            Tmr_CheckFiles.Stop();
        }
        public static DateTime GetDate(int daysBack)
        {
            //Get Wanted date by comparing
            DateTime dateToCompare = DateTime.Now.Date.AddDays(-daysBack);

            return dateToCompare;
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //Zeroing the logged values
            DeletedFilesCount = 0;
            SearchedFiles = 0;
            SearchedDirs = 0;

            try
            {             
                Tmr_CheckFiles.Enabled = false;
                //Event on timer engage

                //Loggin start datetime & Calling the class for processing
                logger.LogEvent("Processing Starts", LogType.Info);

                Processor.Processing(RootDirectory, GetDate(DaysBack));

                logger.LogEvent($"Processing Ended | Searched Directories: {SearchedDirs} " +
                    $"| Searched Files: {SearchedFiles} | Files Deleted: {DeletedFilesCount}", LogType.Info);              

            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.ToString() + " \nPLEASE SET AN EXISTING ROOT PATH");
                logger.LogEvent($"{ex.Message}", LogType.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.ToString());
                logger.LogEvent($"{ex.Message}", LogType.Error);

            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: See log file for information", ex.Message);
                logger.LogEvent($"{ex.Message}", LogType.Error);
            }
            finally
            {
                Tmr_CheckFiles.Enabled = true;
            }


        }

        
    }
}




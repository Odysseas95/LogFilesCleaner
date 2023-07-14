using System;
using System.Diagnostics;
using System.IO;
using static LogFilesCleaner.Logger;


namespace LogFilesCleaner
{
    public abstract class LogBase
    {

        public static string LogDirectory = Properties.Settings.Default.LogDir;
        public abstract void LogEvent(string message, LogType type);
        public abstract void LogCheck(FileInfo logFile, FileInfo[] logFiles);
    }
    public class Logger : LogBase
    {
        public static int Num = 0;
        public static string[] LogArray;
        public static FileInfo LogFile;
        public static FileInfo[] LogFiles;

        private string LoggerDirectory
        {
            get;
            set;
        }
        private string LoggerFileName
        {
            get;
            set;
        }
        private string LoggerFilePath
        {
            get;
            set;
        }
        public enum LogType
        {
            Info,
            Warning,
            Error,
            debug
        }
        public Logger()
        {

            LoggerDirectory = LogDirectory;
            if (LogDirectory == "")
            {
                LoggerDirectory = Directory.GetCurrentDirectory();
            }

            LoggerFileName = $"LogFilesCleaner_{DateTime.Now.Date.ToString("dd-MM-yyyy")}_{Num}";
            LoggerFilePath = $@"{LoggerDirectory}\{LoggerFileName}.log";

            if (File.Exists(LoggerFilePath))
            {
                LogFile = new FileInfo(LoggerFilePath);
                LogFiles = LogFile.Directory.GetFiles("*" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "*");

                LogCheck(LogFile, LogFiles);
            }
            else
            {
                File.Create(LoggerFilePath).Close();
            }
        }
        public override void LogEvent(string message, LogType type)
        {
            try
            {
                LogFile = new FileInfo(LoggerFilePath);
                LogFiles = LogFile.Directory.GetFiles("*" + DateTime.Now.Date.ToString("dd-MM-yyyy") + "*");

                LogCheck(LogFile, LogFiles);

                if (LogFile.Length > 5242880 && LogFile.Exists)
                {
                    Num++;
                    LoggerFileName = $"LogFilesCleaner_{DateTime.Now.Date.ToString("dd-MM-yyyy")}_{Num}";
                    LoggerFilePath = $@"{LoggerDirectory}\{LoggerFileName}.log";
                }
                else if (!LogFile.Exists)
                {
                    Num = 0;
                }

                using (StreamWriter w = File.AppendText(LoggerFilePath)) 
                { 
                    w.WriteLine($"{DateTime.Now.ToString("dd-MM-yyyy | HH:mm:ss")} | {type.ToString()} | {Process.GetCurrentProcess().Threads.Count} | {message}");
                }
            }

            catch (FileNotFoundException ex)
            {
                // LogDateDeleteService.Timer.Stop();
                LogEvent($"{ex.Message}", LogType.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                // LogDateDeleteService.Timer.Stop();
                LogEvent($"{ex.Message}", LogType.Error);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LogEvent($"{ex.Message}", LogType.Error);
            }
        }
        public override void LogCheck(FileInfo logFile, FileInfo[] logFiles)
        {
            if (logFile.Length != 0 && logFile.Exists)
            {
                FileInfo file;
                if (logFiles.Length != 0)
                {
                    file = logFiles[logFiles.Length - 1];
                    LogArray = file.Name.ToString().Split('_', '.');
                    Int32.TryParse(LogArray[2], out Num);
                    LoggerFileName = $"LogFilesCleaner_{DateTime.Now.Date.ToString("dd-MM-yyyy")}_{Num}";
                    LoggerFilePath = $@"{LoggerDirectory}\{LoggerFileName}.log";

                }
                else
                {
                    Num = 0;
                    LoggerFileName = $"LogFilesCleaner_{DateTime.Now.Date.ToString("dd-MM-yyyy")}_{Num}";
                    LoggerFilePath = $@"{LoggerDirectory}\{LoggerFileName}.log";
                    File.Create(LoggerFilePath).Close();
                }
                LogFile = new FileInfo(LoggerFilePath);

            }
        }
    }
}


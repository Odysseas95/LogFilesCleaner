using System;
using System.IO;
using static LogFilesCleaner.Logger;

namespace LogFilesCleaner
{
    internal class Processor
    {
        public static DateTime WantedDate;
        public static void Processing(string rootPath, DateTime wantedDate)
        {
            WantedDate = wantedDate;

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
                LogDateDeleteService.logger.LogEvent($"Root path: {rootPath} is not valid", LogType.Error);
            }
        }
        public static void ProcessDirectory(string targetDirectory)
        {
            //Searching Directory
            LogDateDeleteService.SearchedDirs++;
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
            LogDateDeleteService.SearchedFiles++;

            if (fileInfo.LastWriteTime <= WantedDate && (fileInfo.Extension == ".log" || fileInfo.Extension == ".txt"))
            {
                //Logging and Deleting                
                File.Delete(path);
                LogDateDeleteService.DeletedFilesCount++;
            }

        }
    }
}

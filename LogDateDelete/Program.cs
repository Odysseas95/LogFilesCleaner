using LogFilesCleaner;
using System;
using Topshelf;

namespace LogFilesCleaner
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TopshelfExitCode exitCode = HostFactory.Run(x =>
                {
                    x.Service<LogDateDeleteService>(s =>
                    {
                        s.ConstructUsing(logDateDelete => new LogDateDeleteService());
                        s.WhenStarted(LogDateDelete => LogDateDelete.Start());
                        s.WhenStopped(LogDateDelete => LogDateDelete.Stop());
                    }
                    );
                    x.RunAsLocalSystem();
                    x.SetServiceName("_Log_Files_Cleaner");
                    x.SetDisplayName("_LoggingMaintenance");
                    x.SetDescription("A service to delete log files in a specific location by defining a date");
                });
                int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                Environment.ExitCode = exitCodeValue;
            }
            catch(Exception e)
            {
                Console.WriteLine("Service Error: ", e.Message);               
            }
        }

    }
}

using LogDateDelete;
using System;
using Topshelf;

namespace LogDateDeleteService
{
    public class LogDateDeleteService
    {
        static void Main(string[] args)
        {
            try
            {
                TopshelfExitCode exitCode = HostFactory.Run(x =>
                {
                    x.Service<LogDateDeleteApp>(s =>
                    {
                        s.ConstructUsing(logDateDelete => new LogDateDeleteApp());
                        s.WhenStarted(LogDateDelete => LogDateDelete.Start());
                        s.WhenStopped(LogDateDelete => LogDateDelete.Stop());
                    }
                    );
                    x.RunAsLocalSystem();
                    x.SetServiceName("LogDateDelete");
                    x.SetDisplayName("Log Date Delete");
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

using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace OpenSim.Console.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ConsoleApi.log4net"));
            
            if (configFile.Exists)
            {
                XmlConfigurator.Configure(logRepository, configFile);
            }
            else
            {
                // Fallback to basic console configuration
                BasicConfigurator.Configure(logRepository);
                System.Console.WriteLine($"Warning: {configFile.FullName} not found, using basic logging");
            }

            ConsoleServerBase serverBase = new ConsoleServerBase();
            serverBase.Startup();
            serverBase.Work();
        }
    }
}

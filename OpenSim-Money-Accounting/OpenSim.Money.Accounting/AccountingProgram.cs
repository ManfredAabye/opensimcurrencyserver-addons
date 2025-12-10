using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace OpenSim.Money.Accounting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AccountingServer.log4net"));
            
            if (configFile.Exists)
            {
                XmlConfigurator.Configure(logRepository, configFile);
            }
            else
            {
                // Fallback to basic console configuration
                BasicConfigurator.Configure(logRepository);
                Console.WriteLine($"Warning: {configFile.FullName} not found, using basic logging");
            }

            AccountingServerBase serverBase = new AccountingServerBase();
            serverBase.Startup();
            serverBase.Work();
        }
    }
}

/*
 * OpenSim Money Accounting Server
 * Web-basiertes Kassenbuchhaltungs-Interface für OpenSim Currency System
 */

using log4net;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Framework.Console;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using System;
using System.IO;
using System.Reflection;

namespace OpenSim.Money.Accounting
{
    public class AccountingServerBase : BaseOpenSimServer
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private uint m_serverPort = 5000;
        private string m_connectionString = string.Empty;

        private AccountingService m_accountingService;
        private IConfig m_serverConfig;

        public AccountingServerBase()
        {
            m_console = new LocalConsole("AccountingServer");
            MainConsole.Instance = m_console;
            RegisterConsoleCommands();
        }

        public override void Startup()
        {
            m_log.Info("[ACCOUNTING SERVER]: Starting Money Accounting Server");

            try
            {
                ReadIniConfig();
                SetupHttpServer();
                SetupHandlers();

                m_log.Info($"[ACCOUNTING SERVER]: Server started on port {m_serverPort}");
                m_log.Info("[ACCOUNTING SERVER]: Web Interface: http://localhost:" + m_serverPort);
                m_log.Info("[ACCOUNTING SERVER]: API Endpoint: http://localhost:" + m_serverPort + "/api");
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVER]: Startup failed: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private void ReadIniConfig()
        {
            string iniPath = Path.Combine(Directory.GetCurrentDirectory(), "MoneyServer.ini");
            
            if (!File.Exists(iniPath))
            {
                m_log.Warn("[ACCOUNTING SERVER]: MoneyServer.ini not found, using defaults");
                m_connectionString = "Server=localhost;Database=OpenSimCurrency;User=opensim;Password=opensim;Port=3306;";
                return;
            }

            try
            {
                IConfigSource config = new IniConfigSource(iniPath);
                
                // Read database settings from [MySql] section (same as MoneyServer)
                IConfig mysqlConfig = config.Configs["MySql"];
                string dbHost = "localhost";
                string dbName = "OpenSimCurrency";
                string dbUser = "opensim";
                string dbPass = "";
                int dbPort = 3306;

                if (mysqlConfig != null)
                {
                    dbHost = mysqlConfig.GetString("hostname", "localhost");
                    dbName = mysqlConfig.GetString("database", "OpenSimCurrency");
                    dbUser = mysqlConfig.GetString("username", "opensim");
                    dbPass = mysqlConfig.GetString("password", "");
                    dbPort = mysqlConfig.GetInt("port", 3306);
                }
                else
                {
                    m_log.Warn("[ACCOUNTING SERVER]: [MySql] section not found in MoneyServer.ini");
                }

                // Read AccountingPort from [MoneyServer] section
                m_serverConfig = config.Configs["MoneyServer"];
                if (m_serverConfig != null)
                {
                    m_serverPort = (uint)m_serverConfig.GetInt("AccountingPort", 5000);
                }

                m_connectionString = $"Server={dbHost};Database={dbName};User={dbUser};Password={dbPass};Port={dbPort};";
                
                m_log.Info("[ACCOUNTING SERVER]: Datenbank OK - keine Probleme festgestellt.");
                if (string.IsNullOrEmpty(dbPass))
                {
                    m_log.Warn("[ACCOUNTING SERVER]: WARNUNG - Datenbank-Passwort ist leer!");
                }
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVER]: Fehler: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private void SetupHttpServer()
        {
            m_httpServer = new BaseHttpServer(m_serverPort);
            m_httpServer.Start();
            
            m_accountingService = new AccountingService(m_connectionString, m_log);
        }

        private void SetupHandlers()
        {
            // Static files (HTML, CSS, JS)
            m_httpServer.AddStreamHandler(new StaticFileHandler("/", "webseiten/index.html", "text/html"));
            m_httpServer.AddStreamHandler(new StaticFileHandler("/style.css", "webseiten/style.css", "text/css"));
            m_httpServer.AddStreamHandler(new StaticFileHandler("/app.js", "webseiten/app.js", "application/javascript"));

            // API endpoints (beide GET und POST werden vom selben Handler verarbeitet)
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "GET", "/api/balance"));
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "GET", "/api/transactions"));
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "POST", "/api/transactions"));
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "GET", "/api/users"));
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "GET", "/api/dashboard"));
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "GET", "/api/reports"));
            m_httpServer.AddStreamHandler(new AccountingApiHandler(m_accountingService, "GET", "/api/groups"));
        }

        private void RegisterConsoleCommands()
        {
            m_console.Commands.AddCommand("Accounting", false, "shutdown", "shutdown", "Shutdown the accounting server", HandleShutdown);
            m_console.Commands.AddCommand("Accounting", false, "show users", "show users [<limit>]", "Show user list with balances (default limit: 10)", HandleShowUsers);
            m_console.Commands.AddCommand("Accounting", false, "show groups", "show groups [<limit>]", "Show group statistics (default limit: 10)", HandleShowGroups);
            m_console.Commands.AddCommand("Accounting", false, "show transactions", "show transactions [<limit>]", "Show recent transactions (default limit: 20)", HandleShowTransactions);
            m_console.Commands.AddCommand("Accounting", false, "show reports", "show reports <days>", "Show financial report for last N days", HandleShowReports);
            m_console.Commands.AddCommand("Accounting", false, "show stats", "show stats", "Show dashboard statistics", HandleShowStats);
            m_console.Commands.AddCommand("Accounting", false, "test database", "test database", "Test database connection and show configuration", HandleTestDatabase);
        }

        private void HandleTestDatabase(string module, string[] args)
        {
            m_console.Output("\nDatabase Connection Test:");
            m_console.Output("=".PadRight(60, '='));
            m_console.Output("Status: Testing database connection...");
            m_console.Output("-".PadRight(60, '-'));
            
            if (m_accountingService == null)
            {
                m_console.Output("ERROR: AccountingService not initialized!");
                return;
            }
            
            try
            {
                var testResult = m_accountingService.TestConnection();
                if (testResult.Success)
                {
                    m_console.Output($"✓ Connection successful!");
                    m_console.Output($"  Database Version: {testResult.Data}");
                }
                else
                {
                    m_console.Output($"✗ Connection failed!");
                    m_console.Output($"  Error: {testResult.Message}");
                }
            }
            catch (Exception ex)
            {
                m_console.Output($"✗ Exception: {ex.Message}");
                m_log.Error("[ACCOUNTING SERVER]: Database test error", ex);
            }
            
            m_console.Output("=".PadRight(60, '='));
        }

        private void HandleShutdown(string module, string[] args)
        {
            m_log.Info("[ACCOUNTING SERVER]: Shutdown requested...");
            m_console.Output("Shutting down Money Accounting Server...");
            
            if (m_httpServer != null)
            {
                m_httpServer.Stop();
                m_log.Info("[ACCOUNTING SERVER]: HTTP Server stopped");
            }
            
            Environment.Exit(0);
        }

        private void HandleShowUsers(string module, string[] args)
        {
            int limit = 10;
            if (args.Length > 2 && int.TryParse(args[2], out int parsedLimit))
                limit = parsedLimit;

            try
            {
                var users = m_accountingService.GetAllUsers(limit);
                if (users == null || users.Count == 0)
                {
                    m_console.Output("No users found. Check database connection with 'test database'");
                    return;
                }

            m_console.Output($"\nTop {users.Count} Users:");
            m_console.Output("=".PadRight(80, '='));
            m_console.Output(String.Format("{0,-40} {1,-30} {2,10}", "UUID", "Name", "Balance"));
            m_console.Output("-".PadRight(80, '-'));
            
                foreach (dynamic user in users)
                {
                    m_console.Output(String.Format("{0,-40} {1,-30} {2,10:N0}", 
                        user.userId?.ToString().Substring(0, Math.Min(36, user.userId.ToString().Length)),
                        user.userName?.ToString().Substring(0, Math.Min(30, user.userName.ToString().Length ?? 0)),
                        user.balance));
                }
                m_console.Output("=".PadRight(80, '='));
            }
            catch (Exception ex)
            {
                m_console.Output($"Error: {ex.Message}");
                m_log.Error("[ACCOUNTING SERVER]: HandleShowUsers error", ex);
            }
        }

        private void HandleShowGroups(string module, string[] args)
        {
            int limit = 10;
            if (args.Length > 2 && int.TryParse(args[2], out int parsedLimit))
                limit = parsedLimit;

            var groups = m_accountingService.GetGroupStats(limit);
            if (groups == null || groups.Count == 0)
            {
                m_console.Output("No groups found");
                return;
            }

            m_console.Output($"\nTop {groups.Count} Groups:");
            m_console.Output("=".PadRight(80, '='));
            m_console.Output(String.Format("{0,-40} {1,15} {2,15}", "Group Name", "Members", "Total Balance"));
            m_console.Output("-".PadRight(80, '-'));
            
            foreach (dynamic group in groups)
            {
                m_console.Output(String.Format("{0,-40} {1,15:N0} {2,15:N0}", 
                    group.groupName?.ToString().Substring(0, Math.Min(40, group.groupName.ToString().Length)),
                    group.memberCount,
                    group.totalBalance));
            }
            m_console.Output("=".PadRight(80, '='));
        }

        private void HandleShowTransactions(string module, string[] args)
        {
            int limit = 20;
            if (args.Length > 2 && int.TryParse(args[2], out int parsedLimit))
                limit = parsedLimit;

            var transactions = m_accountingService.GetAllTransactions(limit);
            if (transactions == null || transactions.Count == 0)
            {
                m_console.Output("No transactions found");
                return;
            }

            m_console.Output($"\nLast {transactions.Count} Transactions:");
            m_console.Output("=".PadRight(100, '='));
            m_console.Output(String.Format("{0,-20} {1,-25} {2,10} {3,10} {4,-20}", "Time", "From → To", "Amount", "Type", "Description"));
            m_console.Output("-".PadRight(100, '-'));
            
            foreach (dynamic tx in transactions)
            {
                string time = "";
                if (tx.time != null)
                {
                    if (tx.time is DateTime dt)
                        time = dt.ToString("yyyy-MM-dd HH:mm");
                    else if (tx.time is long unixTime)
                        time = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime.ToString("yyyy-MM-dd HH:mm");
                }
                
                string fromTo = $"{tx.senderName?.ToString().Substring(0, Math.Min(10, tx.senderName.ToString().Length))} → {tx.receiverName?.ToString().Substring(0, Math.Min(10, tx.receiverName.ToString().Length))}";
                string desc = tx.description?.ToString().Substring(0, Math.Min(20, tx.description.ToString().Length ?? 0)) ?? "";
                
                m_console.Output(String.Format("{0,-20} {1,-25} {2,10:N0} {3,10} {4,-20}", 
                    time, fromTo, tx.amount, tx.type, desc));
            }
            m_console.Output("=".PadRight(100, '='));
        }

        private void HandleShowReports(string module, string[] args)
        {
            if (args.Length < 3 || !int.TryParse(args[2], out int days))
            {
                m_console.Output("Usage: show reports <days>");
                return;
            }

            DateTime endDate = DateTime.UtcNow;
            DateTime startDate = endDate.AddDays(-days);
            
            var report = m_accountingService.GetFinancialReport(startDate, endDate);
            if (report == null)
            {
                m_console.Output("No report data available");
                return;
            }

            m_console.Output($"\nFinancial Report (Last {days} days):");
            m_console.Output("=".PadRight(60, '='));
            m_console.Output(String.Format("Period: {0} to {1}", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd")));
            m_console.Output("-".PadRight(60, '-'));
            
            if (report is System.Collections.IDictionary dict)
            {
                m_console.Output(String.Format("Total Transactions: {0,10:N0}", dict["totalTransactions"]));
                m_console.Output(String.Format("Total Volume:       {0,10:N0}", dict["totalVolume"]));
                m_console.Output(String.Format("Average Amount:     {0,10:N2}", dict["averageAmount"]));
                m_console.Output(String.Format("Active Users:       {0,10:N0}", dict["activeUsers"]));
            }
            m_console.Output("=".PadRight(60, '='));
        }

        private void HandleShowStats(string module, string[] args)
        {
            try
            {
                var stats = m_accountingService.GetDashboardStats();
                if (stats == null)
                {
                    m_console.Output("No statistics available. Check database connection with 'test database'");
                    return;
                }

                m_console.Output("\nDashboard Statistics:");
                m_console.Output("=".PadRight(60, '='));
                
                if (stats is System.Collections.IDictionary dict)
                {
                    if (dict.Contains("error"))
                    {
                        m_console.Output($"Error: {dict["error"]}");
                    }
                    else
                    {
                        m_console.Output(String.Format("Total Users:        {0,10:N0}", dict["totalUsers"]));
                        m_console.Output(String.Format("Total Balance:      {0,10:N0}", dict["totalBalance"]));
                        m_console.Output(String.Format("Total Transactions: {0,10:N0}", dict["totalTransactions"]));
                        m_console.Output(String.Format("Active Users (24h): {0,10:N0}", dict["activeUsers"]));
                        m_console.Output(String.Format("Transaction Volume: {0,10:N0}", dict["transactionVolume"]));
                    }
                }
                m_console.Output("=".PadRight(60, '='));
            }
            catch (Exception ex)
            {
                m_console.Output($"Error: {ex.Message}");
                m_log.Error("[ACCOUNTING SERVER]: HandleShowStats error", ex);
            }
        }

        public void Work()
        {
            m_console.Output("Enter 'help' for a list of commands");
            m_console.Output("Available commands: shutdown, test database, show users, show groups, show transactions, show reports, show stats");
            m_console.Output("\nTip: Run 'test database' first to verify database connection");
            
            while (true)
            {
                m_console.Prompt();
            }
        }
    }
}

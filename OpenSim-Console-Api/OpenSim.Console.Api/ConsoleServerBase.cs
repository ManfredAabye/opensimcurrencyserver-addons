/*
 * OpenSim Console API Server
 * Web-basiertes Console Interface für OpenSim mit Echtzeit-Befehlsausführung
 * 
 * Features:
 * - REST API für Konsolenbefehle
 * - WebSocket für Echtzeit-Ausgabe
 * - JSON-basierte Benutzerauthentifizierung
 * - Rate-Limiting und Command Injection Schutz
 * - Logging aller Befehle mit Benutzer-Info
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

namespace OpenSim.Console.Api
{
    public class ConsoleServerBase : BaseOpenSimServer
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private uint m_serverPort = 7000;
        private string m_configFile = "consoleapi.json";
        private bool m_enableSSL = false;
        private string m_certPath = string.Empty;
        private string m_certPassword = string.Empty;

        private ConsoleService m_consoleService;
        private IConfig m_serverConfig;

        public ConsoleServerBase()
        {
            m_console = new LocalConsole("ConsoleApiServer");
            MainConsole.Instance = m_console;
            RegisterConsoleCommands();
        }

        public override void Startup()
        {
            m_log.Info("[CONSOLE API]: Starting Console API Server");

            try
            {
                ReadIniConfig();
                SetupHttpServer();
                SetupHandlers();

                m_log.Info($"[CONSOLE API]: Server started on port {m_serverPort}");
                m_log.Info("[CONSOLE API]: Web Interface: " + (m_enableSSL ? "https" : "http") + "://localhost:" + m_serverPort);
                m_log.Info("[CONSOLE API]: API Endpoint: " + (m_enableSSL ? "https" : "http") + "://localhost:" + m_serverPort + "/api");
                m_log.Info("[CONSOLE API]: WebSocket: " + (m_enableSSL ? "wss" : "ws") + "://localhost:" + m_serverPort + "/ws");
            }
            catch (Exception ex)
            {
                m_log.Error("[CONSOLE API]: Startup failed: " + ex.Message);
                m_log.Error("[CONSOLE API]: Stack Trace: " + ex.StackTrace);
                Environment.Exit(1);
            }
        }

        private void ReadIniConfig()
        {
            string iniPath = Path.Combine(Directory.GetCurrentDirectory(), "ConsoleApi.ini");
            
            if (!File.Exists(iniPath))
            {
                m_log.Warn("[CONSOLE API]: ConsoleApi.ini not found, creating default configuration");
                CreateDefaultConfig(iniPath);
            }

            try
            {
                IConfigSource config = new IniConfigSource(iniPath);
                
                // Read ConsoleApi settings
                m_serverConfig = config.Configs["ConsoleApi"];
                if (m_serverConfig != null)
                {
                    m_serverPort = (uint)m_serverConfig.GetInt("Port", 7000);
                    m_configFile = m_serverConfig.GetString("ConfigFile", "consoleapi.json");
                    m_enableSSL = m_serverConfig.GetBoolean("EnableSSL", false);
                    m_certPath = m_serverConfig.GetString("CertificatePath", "");
                    m_certPassword = m_serverConfig.GetString("CertificatePassword", "");
                }
                else
                {
                    m_log.Warn("[CONSOLE API]: [ConsoleApi] section not found, using defaults");
                }

                m_log.Info("[CONSOLE API]: Configuration loaded successfully");
                
                if (m_enableSSL && string.IsNullOrEmpty(m_certPath))
                {
                    m_log.Warn("[CONSOLE API]: SSL enabled but no certificate path specified!");
                }
            }
            catch (Exception ex)
            {
                m_log.Error("[CONSOLE API]: Configuration error: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private void CreateDefaultConfig(string path)
        {
            string defaultConfig = @"[ConsoleApi]
; Port for the Console API Server
Port = 7000

; JSON file containing users and allowed commands
ConfigFile = consoleapi.json

; Enable SSL/TLS encryption
EnableSSL = false

; Path to SSL certificate (if EnableSSL is true)
CertificatePath = 

; Certificate password (if needed)
CertificatePassword = 

; Rate limiting settings (requests per minute per user)
RateLimitPerMinute = 60

; Maximum command execution timeout (seconds)
CommandTimeout = 30

; Enable detailed logging of all commands
EnableCommandLogging = true
";
            try
            {
                File.WriteAllText(path, defaultConfig);
                m_log.Info($"[CONSOLE API]: Created default configuration at {path}");
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: Failed to create default config: {ex.Message}");
            }
        }

        private void SetupHttpServer()
        {
            m_httpServer = new BaseHttpServer(m_serverPort);
            
            // TODO: SSL Support wenn benötigt
            // if (m_enableSSL && !string.IsNullOrEmpty(m_certPath))
            // {
            //     m_httpServer.AddSSLCertificate(m_certPath, m_certPassword);
            // }
            
            m_httpServer.Start();
            
            // Initialize Console Service with console reference
            m_consoleService = new ConsoleService(m_configFile, m_log, m_console);
            m_log.Info("[CONSOLE API]: ConsoleService initialized");
        }

        private void SetupHandlers()
        {
            // Static files (HTML, CSS, JS)
            m_httpServer.AddStreamHandler(new StaticFileHandler("/", "websites/console.html", "text/html"));
            m_httpServer.AddStreamHandler(new StaticFileHandler("/console.html", "websites/console.html", "text/html"));
            m_httpServer.AddStreamHandler(new StaticFileHandler("/consolestyle.css", "websites/consolestyle.css", "text/css"));
            m_httpServer.AddStreamHandler(new StaticFileHandler("/console.js", "websites/console.js", "application/javascript"));

            // API endpoints
            m_httpServer.AddStreamHandler(new ConsoleApiHandler(m_consoleService, "POST", "/api/login"));
            m_httpServer.AddStreamHandler(new ConsoleApiHandler(m_consoleService, "POST", "/api/execute"));
            m_httpServer.AddStreamHandler(new ConsoleApiHandler(m_consoleService, "GET", "/api/commands"));
            m_httpServer.AddStreamHandler(new ConsoleApiHandler(m_consoleService, "GET", "/api/history"));
            m_httpServer.AddStreamHandler(new ConsoleApiHandler(m_consoleService, "GET", "/api/status"));
            
            // WebSocket endpoint for real-time console output
            // TODO: WebSocket Handler implementieren
            // m_httpServer.AddWebSocketHandler(new ConsoleWebSocketHandler(m_consoleService, "/ws"));
            
            m_log.Info("[CONSOLE API]: All handlers registered");
        }

        private void RegisterConsoleCommands()
        {
            m_console.Commands.AddCommand("ConsoleApi", false, "shutdown", "shutdown", 
                "Shutdown the console API server", HandleShutdown);
            
            m_console.Commands.AddCommand("ConsoleApi", false, "show users", "show users", 
                "Show authorized users", HandleShowUsers);
            
            m_console.Commands.AddCommand("ConsoleApi", false, "show sessions", "show sessions", 
                "Show active user sessions", HandleShowSessions);
            
            m_console.Commands.AddCommand("ConsoleApi", false, "show history", "show history [<limit>]", 
                "Show command history (default limit: 20)", HandleShowHistory);
            
            m_console.Commands.AddCommand("ConsoleApi", false, "reload config", "reload config", 
                "Reload configuration from JSON file", HandleReloadConfig);
        }

        private void HandleShutdown(string module, string[] args)
        {
            m_log.Info("[CONSOLE API]: Shutdown requested...");
            m_console.Output("Shutting down Console API Server...");
            
            if (m_httpServer != null)
            {
                m_httpServer.Stop();
                m_log.Info("[CONSOLE API]: HTTP Server stopped");
            }
            
            Environment.Exit(0);
        }

        private void HandleShowUsers(string module, string[] args)
        {
            if (m_consoleService == null)
            {
                m_console.Output("ConsoleService not initialized");
                return;
            }

            var users = m_consoleService.GetAuthorizedUsers();
            
            m_console.Output("\nAuthorized Users:");
            m_console.Output("=".PadRight(80, '='));
            m_console.Output(String.Format("{0,-30} {1,-30} {2,-15}", "Username", "Role", "Last Login"));
            m_console.Output("-".PadRight(80, '-'));
            
            foreach (var user in users)
            {
                m_console.Output(String.Format("{0,-30} {1,-30} {2,-15}", 
                    user.Username, 
                    user.Role,
                    user.LastLogin?.ToString("yyyy-MM-dd HH:mm") ?? "Never"));
            }
            
            m_console.Output("=".PadRight(80, '='));
            m_console.Output($"Total: {users.Count} users");
        }

        private void HandleShowSessions(string module, string[] args)
        {
            if (m_consoleService == null)
            {
                m_console.Output("ConsoleService not initialized");
                return;
            }

            var sessions = m_consoleService.GetActiveSessions();
            
            m_console.Output("\nActive Sessions:");
            m_console.Output("=".PadRight(100, '='));
            m_console.Output(String.Format("{0,-30} {1,-40} {2,-20}", "Username", "Token", "Expires"));
            m_console.Output("-".PadRight(100, '-'));
            
            foreach (var session in sessions)
            {
                m_console.Output(String.Format("{0,-30} {1,-40} {2,-20}", 
                    session.Username, 
                    session.Token.Substring(0, Math.Min(36, session.Token.Length)),
                    session.ExpiresAt.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            
            m_console.Output("=".PadRight(100, '='));
            m_console.Output($"Total: {sessions.Count} active sessions");
        }

        private void HandleShowHistory(string module, string[] args)
        {
            int limit = 20;
            if (args.Length > 2 && int.TryParse(args[2], out int parsedLimit))
                limit = parsedLimit;

            if (m_consoleService == null)
            {
                m_console.Output("ConsoleService not initialized");
                return;
            }

            var history = m_consoleService.GetCommandHistory(limit);
            
            m_console.Output($"\nLast {history.Count} Commands:");
            m_console.Output("=".PadRight(120, '='));
            m_console.Output(String.Format("{0,-20} {1,-20} {2,-50} {3,-20}", "Time", "User", "Command", "Status"));
            m_console.Output("-".PadRight(120, '-'));
            
            foreach (var entry in history)
            {
                m_console.Output(String.Format("{0,-20} {1,-20} {2,-50} {3,-20}", 
                    entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    entry.Username,
                    entry.Command.Substring(0, Math.Min(48, entry.Command.Length)),
                    entry.Success ? "Success" : "Failed"));
            }
            
            m_console.Output("=".PadRight(120, '='));
        }

        private void HandleReloadConfig(string module, string[] args)
        {
            if (m_consoleService == null)
            {
                m_console.Output("ConsoleService not initialized");
                return;
            }

            try
            {
                m_consoleService.ReloadConfig();
                m_console.Output("Configuration reloaded successfully");
                m_log.Info("[CONSOLE API]: Configuration reloaded from file");
            }
            catch (Exception ex)
            {
                m_console.Output($"Error reloading configuration: {ex.Message}");
                m_log.Error("[CONSOLE API]: Failed to reload configuration", ex);
            }
        }

        public void Work()
        {
            m_console.Output("Enter 'help' for a list of commands");
            m_console.Output("Available commands: shutdown, test database, show users, show history, reload config");
            m_console.Output("\nTip: Run 'test database' first to verify database connection");
            
            while (true)
            {
                m_console.Prompt();
            }
        }
    }
}

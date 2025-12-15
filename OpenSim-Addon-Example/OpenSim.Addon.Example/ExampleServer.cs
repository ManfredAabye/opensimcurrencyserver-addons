/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using log4net;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Framework.Console;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Server.Base;
using MySql.Data.MySqlClient;

namespace OpenSim.Addon.Example.Handlers
{
    /// <summary>
    /// Handler for serving the main HTML page
    /// </summary>
    public class ExamplePageHandler : BaseStreamHandler
    {
        public ExamplePageHandler() : base("GET", "/") { }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;
            
            string html = "<!DOCTYPE html><html lang='de'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'><title>OpenSim Example Addon</title><style>*{margin:0;padding:0;box-sizing:border-box}body{font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);min-height:100vh;display:flex;justify-content:center;align-items:center;padding:20px}.container{background:white;border-radius:20px;padding:60px 40px;box-shadow:0 20px 60px rgba(0,0,0,0.3);max-width:800px;text-align:center;animation:fadeIn 0.6s ease-in}@keyframes fadeIn{from{opacity:0;transform:translateY(20px)}to{opacity:1;transform:translateY(0)}}h1{color:#667eea;font-size:3em;margin-bottom:20px;text-shadow:2px 2px 4px rgba(0,0,0,0.1)}.greeting{font-size:2em;color:#333;margin-bottom:30px;font-weight:600}.info{background:#f7f7f7;padding:20px;border-radius:10px;margin-top:30px;color:#666;text-align:left}.info h3{color:#764ba2;margin-bottom:15px;text-align:center}.info p{margin:10px 0;line-height:1.6}.api-test{margin-top:30px}.btn{background:linear-gradient(135deg,#667eea 0%,#764ba2 100%);color:white;border:none;padding:15px 40px;border-radius:50px;font-size:1.1em;cursor:pointer;transition:transform 0.2s,box-shadow 0.2s;margin:10px}.btn:hover{transform:translateY(-2px);box-shadow:0 10px 20px rgba(0,0,0,0.2)}.btn:active{transform:translateY(0)}.response{margin-top:20px;padding:15px;background:#e8f5e9;border-left:4px solid #4caf50;border-radius:5px;display:none;text-align:left}.emoji{font-size:4em;margin-bottom:20px}.btn-group{display:flex;flex-wrap:wrap;justify-content:center;gap:10px}</style></head><body><div class='container'><div class='emoji'>🚀</div><h1>OpenSim Example Addon</h1><div class='greeting'>Hello OpenSim User!</div><div class='info'><h3>📋 Addon Informationen</h3><p><strong>Name:</strong> OpenSim.Addon.Example</p><p><strong>Port:</strong> 9000</p><p><strong>Status:</strong> ✅ Aktiv</p><p><strong>Features:</strong> Robust.ini Config Reader, MySQL Database Access</p></div><div class='api-test'><h3 style='color:#764ba2;margin-bottom:15px'>🔌 API Endpunkte</h3><div class='btn-group'><button class='btn' onclick='testApi()'>API Message</button><button class='btn' onclick='testRobustConfig()'>Robust.ini Config</button><button class='btn' onclick='testDatabaseStats()'>Database Stats</button></div><div id='response' class='response'></div></div></div><script>async function testApi(){const responseDiv=document.getElementById('response');try{const response=await fetch('/api/message');const data=await response.json();responseDiv.innerHTML=`<strong>✓ API Antwort:</strong><br>Nachricht: ${data.message}<br>Zeitstempel: ${data.timestamp}<br>Server: ${data.server}`;responseDiv.style.display='block';responseDiv.style.backgroundColor='#e8f5e9';responseDiv.style.borderLeftColor='#4caf50'}catch(error){responseDiv.innerHTML=`<strong>❌ Fehler:</strong> ${error.message}`;responseDiv.style.backgroundColor='#ffebee';responseDiv.style.borderLeftColor='#f44336';responseDiv.style.display='block'}}async function testRobustConfig(){const responseDiv=document.getElementById('response');try{const response=await fetch('/api/robust-config');const data=await response.json();responseDiv.innerHTML=`<strong>✓ Robust.ini Konfiguration:</strong><br>Base Hostname: ${data.baseHostname}<br>Public Port: ${data.publicPort}<br>Private Port: ${data.privatePort}<br>Database Provider: ${data.databaseProvider}<br>Zeitstempel: ${data.timestamp}`;responseDiv.style.display='block';responseDiv.style.backgroundColor='#e8f5e9';responseDiv.style.borderLeftColor='#4caf50'}catch(error){responseDiv.innerHTML=`<strong>❌ Fehler:</strong> ${error.message}`;responseDiv.style.backgroundColor='#ffebee';responseDiv.style.borderLeftColor='#f44336';responseDiv.style.display='block'}}async function testDatabaseStats(){const responseDiv=document.getElementById('response');try{const response=await fetch('/api/database-stats');const data=await response.json();if(data.status==='error'){responseDiv.innerHTML=`<strong>⚠️ Datenbankfehler:</strong><br>${data.error}`;responseDiv.style.backgroundColor='#fff3e0';responseDiv.style.borderLeftColor='#ff9800'}else{responseDiv.innerHTML=`<strong>✓ Datenbank Statistiken:</strong><br>Benutzer: ${data.userCount}<br>Regionen: ${data.regionCount}<br>Zeitstempel: ${data.timestamp}`;responseDiv.style.backgroundColor='#e8f5e9';responseDiv.style.borderLeftColor='#4caf50'}responseDiv.style.display='block'}catch(error){responseDiv.innerHTML=`<strong>❌ Fehler:</strong> ${error.message}`;responseDiv.style.backgroundColor='#ffebee';responseDiv.style.borderLeftColor='#f44336';responseDiv.style.display='block'}}console.log('🚀 OpenSim Example Addon geladen!');console.log('Features: Robust.ini Reader, MySQL Database Access');</script></body></html>";
            
            return System.Text.Encoding.UTF8.GetBytes(html);
        }
    }

    /// <summary>
    /// Handler for the REST API endpoint
    /// </summary>
    public class ExampleApiHandler : BaseStreamHandler
    {
        public ExampleApiHandler() : base("GET", "/api/message") { }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = 200;
            
            string json = string.Format(@"{{
    ""message"": ""Hello OpenSim User from API!"",
    ""timestamp"": ""{0}"",
    ""server"": ""OpenSim.Addon.Example"",
    ""status"": ""success""
}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }

    /// <summary>
    /// Handler for Robust.ini configuration data
    /// </summary>
    public class RobustConfigHandler : BaseStreamHandler
    {
        private readonly RobustConfigReader m_configReader;

        public RobustConfigHandler(RobustConfigReader configReader) : base("GET", "/api/robust-config")
        {
            m_configReader = configReader;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = 200;
            
            var config = m_configReader.GetConfigData();
            string json = string.Format(@"{{
    ""baseHostname"": ""{0}"",
    ""publicPort"": ""{1}"",
    ""privatePort"": ""{2}"",
    ""databaseProvider"": ""{3}"",
    ""timestamp"": ""{4}"",
    ""status"": ""success""
}}",
                config.GetValueOrDefault("BaseHostname", "N/A"),
                config.GetValueOrDefault("PublicPort", "N/A"),
                config.GetValueOrDefault("PrivatePort", "N/A"),
                config.GetValueOrDefault("StorageProvider", "N/A"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
    }

    /// <summary>
    /// Handler for database statistics
    /// </summary>
    public class DatabaseStatsHandler : BaseStreamHandler
    {
        private readonly DatabaseHelper m_dbHelper;

        public DatabaseStatsHandler(DatabaseHelper dbHelper) : base("GET", "/api/database-stats")
        {
            m_dbHelper = dbHelper;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = 200;
            
            try
            {
                var stats = m_dbHelper.GetDatabaseStats();
                string json = string.Format(@"{{
    ""userCount"": {0},
    ""regionCount"": {1},
    ""timestamp"": ""{2}"",
    ""status"": ""success""
}}",
                    stats.GetValueOrDefault("userCount", 0),
                    stats.GetValueOrDefault("regionCount", 0),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                
                return System.Text.Encoding.UTF8.GetBytes(json);
            }
            catch (Exception ex)
            {
                string errorJson = string.Format(@"{{
    ""error"": ""{0}"",
    ""status"": ""error""
}}", ex.Message.Replace("\"", "\\\""));
                httpResponse.StatusCode = 500;
                return System.Text.Encoding.UTF8.GetBytes(errorJson);
            }
        }
    }
}

namespace OpenSim.Addon.Example
{
    /// <summary>
    /// Example OpenSimulator addon server demonstrating basic integration patterns.
    /// Shows how to create a simple HTTP server with console commands.
    /// </summary>
    public class ExampleServer : BaseOpenSimServer
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private new BaseHttpServer m_httpServer;
        private uint m_port = 9000;
        private RobustConfigReader m_robustConfigReader;
        private DatabaseHelper m_databaseHelper;
        
        /// <summary>
        /// Constructor - initializes the console for the server
        /// </summary>
        public ExampleServer()
        {
            m_console = new LocalConsole("ExampleServer");
            MainConsole.Instance = m_console;
        }

        /// <summary>
        /// Main startup method - called by base class
        /// </summary>
        public override void Startup()
        {
            m_log.Info("[EXAMPLE SERVER]: Beginning startup processing");
            m_log.Info("[EXAMPLE SERVER]: Version: Example Addon v1.0 for OpenSimulator");
            
            try
            {
                ReadIniConfig();
                
                Console.WriteLine("===========================================");
                Console.WriteLine("  HALLO WORLD - OpenSim Example Addon!");
                Console.WriteLine("===========================================");
                m_log.Info("[EXAMPLE SERVER]: Hallo World");
                
                SetupHttpServer();
                RegisterConsoleCommands();
                
                m_log.InfoFormat("[EXAMPLE SERVER]: Web Interface available at http://localhost:{0}", m_port);
                m_log.Info("[EXAMPLE SERVER]: Startup processing complete");
                
                Work();
            }
            catch (Exception e)
            {
                m_log.Fatal("[EXAMPLE SERVER]: Fatal error during startup: " + e.ToString());
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Read configuration from ExampleServer.ini if it exists
        /// </summary>
        private void ReadIniConfig()
        {
            string iniFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExampleServer.ini");
            if (File.Exists(iniFile))
            {
                try
                {
                    IConfigSource config = new IniConfigSource(iniFile);
                    IConfig serverConfig = config.Configs["ExampleServer"];
                    if (serverConfig != null)
                    {
                        m_port = (uint)serverConfig.GetInt("Port", 9000);
                        m_log.InfoFormat("[EXAMPLE SERVER]: Using port {0} from configuration", m_port);
                    }
                }
                catch (Exception e)
                {
                    m_log.Error("[EXAMPLE SERVER]: Error reading configuration: " + e.Message);
                    m_log.Info("[EXAMPLE SERVER]: Using default port 9000");
                }
            }
            else
            {
                m_log.Info("[EXAMPLE SERVER]: No configuration file found, using default port 9000");
            }

            // Initialize Robust.ini reader
            m_robustConfigReader = new RobustConfigReader();
            m_robustConfigReader.LoadConfig();

            // Initialize database helper
            string connectionString = m_robustConfigReader.GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                m_databaseHelper = new DatabaseHelper(connectionString);
                m_log.Info("[EXAMPLE SERVER]: Database helper initialized");
            }
            else
            {
                m_log.Warn("[EXAMPLE SERVER]: No database connection string found in Robust.ini");
            }
        }

        /// <summary>
        /// Setup the HTTP server and register handlers
        /// </summary>
        private void SetupHttpServer()
        {
            m_log.Info($"[EXAMPLE SERVER]: Starting HTTP server on port {m_port}");
            m_httpServer = new BaseHttpServer(m_port);
            
            m_httpServer.AddStreamHandler(new Handlers.ExamplePageHandler());
            m_httpServer.AddStreamHandler(new Handlers.ExampleApiHandler());
            
            if (m_robustConfigReader != null)
            {
                m_httpServer.AddStreamHandler(new Handlers.RobustConfigHandler(m_robustConfigReader));
                m_log.Info("[EXAMPLE SERVER]: Robust config handler registered");
            }
            
            if (m_databaseHelper != null)
            {
                m_httpServer.AddStreamHandler(new Handlers.DatabaseStatsHandler(m_databaseHelper));
                m_log.Info("[EXAMPLE SERVER]: Database stats handler registered");
            }
            
            m_httpServer.Start(false, true);
            m_log.Info("[EXAMPLE SERVER]: HTTP server started successfully");
        }

        /// <summary>
        /// Register custom console commands
        /// </summary>
        private void RegisterConsoleCommands()
        {
            if (m_console != null)
            {
                m_console.Commands.AddCommand("Example", false, "hello",
                    "hello", 
                    "Displays 'Hallo World' message in the console",
                    HandleHelloCommand);
                
                m_console.Commands.AddCommand("Example", false, "show status",
                    "show status", 
                    "Shows the current status of the Example Server (port, URL)",
                    HandleShowStatusCommand);
                
                m_console.Commands.AddCommand("Example", false, "show robust-config",
                    "show robust-config", 
                    "Shows configuration data from Robust.ini",
                    HandleShowRobustConfigCommand);
                
                m_console.Commands.AddCommand("Example", false, "show database-stats",
                    "show database-stats", 
                    "Shows database statistics (user count, region count)",
                    HandleShowDatabaseStatsCommand);
                
                m_log.Info("[EXAMPLE SERVER]: Console commands registered");
            }
        }

        /// <summary>
        /// Handler for 'hello' console command
        /// </summary>
        private void HandleHelloCommand(string module, string[] args)
        {
            m_log.Info("[EXAMPLE SERVER]: Hallo World - Command executed!");
            Console.WriteLine("\n===========================================");
            Console.WriteLine("           *** HALLO WORLD ***");
            Console.WriteLine("===========================================\n");
        }

        /// <summary>
        /// Handler for 'show status' console command
        /// </summary>
        private void HandleShowStatusCommand(string module, string[] args)
        {
            Console.WriteLine("\n===========================================");
            Console.WriteLine("       Example Server Status");
            Console.WriteLine("===========================================");
            Console.WriteLine($"Port:          {m_port}");
            Console.WriteLine($"Web Interface: http://localhost:{m_port}");
            Console.WriteLine($"API Endpoint:  http://localhost:{m_port}/api/message");
            Console.WriteLine($"Status:        Running");
            Console.WriteLine("===========================================\n");
        }

        /// <summary>
        /// Handler for 'show robust-config' console command
        /// </summary>
        private void HandleShowRobustConfigCommand(string module, string[] args)
        {
            if (m_robustConfigReader == null)
            {
                Console.WriteLine("[EXAMPLE SERVER]: Robust.ini config reader not initialized");
                return;
            }

            Console.WriteLine("\n===========================================");
            Console.WriteLine("       Robust.ini Configuration");
            Console.WriteLine("===========================================");
            
            var config = m_robustConfigReader.GetConfigData();
            foreach (var kvp in config)
            {
                Console.WriteLine($"{kvp.Key,-20} = {kvp.Value}");
            }
            
            Console.WriteLine("===========================================\n");
        }

        /// <summary>
        /// Handler for 'show database-stats' console command
        /// </summary>
        private void HandleShowDatabaseStatsCommand(string module, string[] args)
        {
            if (m_databaseHelper == null)
            {
                Console.WriteLine("[EXAMPLE SERVER]: Database helper not initialized");
                return;
            }

            try
            {
                Console.WriteLine("\n===========================================");
                Console.WriteLine("       Database Statistics");
                Console.WriteLine("===========================================");
                
                var stats = m_databaseHelper.GetDatabaseStats();
                Console.WriteLine($"User Count:    {stats.GetValueOrDefault("userCount", 0)}");
                Console.WriteLine($"Region Count:  {stats.GetValueOrDefault("regionCount", 0)}");
                
                Console.WriteLine("===========================================\n");
            }
            catch (Exception e)
            {
                m_log.Error("[EXAMPLE SERVER]: Error retrieving database stats: " + e.Message);
            }
        }

        /// <summary>
        /// Main work loop - keeps the server running and processes console input
        /// </summary>
        private void Work()
        {
            while (true)
            {
                m_console.Prompt();
            }
        }

        /// <summary>
        /// Main entry point for the Example Server
        /// </summary>
        public static int Main(string[] args)
        {
            string logConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExampleServer.log4net");
            
            if (File.Exists(logConfigFile))
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(logConfigFile));
            }

            ExampleServer server = new ExampleServer();
            server.Startup();
            
            return 0;
        }
    }

    /// <summary>
    /// Helper class to read Robust.ini configuration
    /// </summary>
    public class RobustConfigReader
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, string> m_configData = new Dictionary<string, string>();
        private string m_connectionString = string.Empty;

        public void LoadConfig()
        {
            string robustIniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Robust.ini");
            
            if (!File.Exists(robustIniPath))
            {
                // Try the example file if Robust.ini doesn't exist
                robustIniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Robust.ini.example");
                
                if (!File.Exists(robustIniPath))
                {
                    m_log.Warn("[ROBUST CONFIG]: Robust.ini not found");
                    return;
                }
            }

            try
            {
                IConfigSource config = new IniConfigSource(robustIniPath);
                
                // Read Const section
                IConfig constConfig = config.Configs["Const"];
                if (constConfig != null)
                {
                    m_configData["BaseHostname"] = constConfig.GetString("BaseHostname", "N/A");
                    m_configData["PublicPort"] = constConfig.GetString("PublicPort", "N/A");
                    m_configData["PrivatePort"] = constConfig.GetString("PrivatePort", "N/A");
                }
                
                // Read DatabaseService section
                IConfig dbConfig = config.Configs["DatabaseService"];
                if (dbConfig != null)
                {
                    m_configData["StorageProvider"] = dbConfig.GetString("StorageProvider", "N/A");
                    m_connectionString = dbConfig.GetString("ConnectionString", string.Empty);
                    m_configData["ConnectionString"] = MaskConnectionString(m_connectionString);
                }
                
                m_log.Info("[ROBUST CONFIG]: Robust.ini configuration loaded successfully");
            }
            catch (Exception e)
            {
                m_log.Error("[ROBUST CONFIG]: Error reading Robust.ini: " + e.Message);
            }
        }

        public Dictionary<string, string> GetConfigData()
        {
            return new Dictionary<string, string>(m_configData);
        }

        public string GetConnectionString()
        {
            return m_connectionString;
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "N/A";
            
            // Mask password in connection string for display
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"(Password|Pwd)=[^;]*", 
                "$1=*****", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }

    /// <summary>
    /// Helper class to access MySQL database
    /// </summary>
    public class DatabaseHelper
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string m_connectionString;

        public DatabaseHelper(string connectionString)
        {
            m_connectionString = connectionString;
        }

        public Dictionary<string, int> GetDatabaseStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();
            
            try
            {
                using (MySqlConnection conn = new MySqlConnection(m_connectionString))
                {
                    conn.Open();
                    
                    // Get user count
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM UserAccounts", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        stats["userCount"] = result != null ? Convert.ToInt32(result) : 0;
                    }
                    
                    // Get region count
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM regions", conn))
                    {
                        object result = cmd.ExecuteScalar();
                        stats["regionCount"] = result != null ? Convert.ToInt32(result) : 0;
                    }
                    
                    m_log.Info("[DATABASE]: Database statistics retrieved successfully");
                }
            }
            catch (Exception e)
            {
                m_log.Error("[DATABASE]: Error getting database statistics: " + e.Message);
                stats["userCount"] = 0;
                stats["regionCount"] = 0;
            }
            
            return stats;
        }

        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(m_connectionString))
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception e)
            {
                m_log.Error("[DATABASE]: Connection test failed: " + e.Message);
                return false;
            }
        }
    }
}

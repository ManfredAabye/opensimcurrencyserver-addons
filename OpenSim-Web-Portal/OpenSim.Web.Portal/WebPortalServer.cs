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
using System.Text;
using log4net;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Framework.Console;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Server.Base;

namespace OpenSim.Web.Portal.Handlers
{
    /// <summary>
    /// Handler for serving the main HTML page
    /// </summary>
    public class PortalHomeHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string m_templatePath;

        public PortalHomeHandler(string templatePath) : base("GET", "/")
        {
            m_templatePath = templatePath;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;

            string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
            string homeFile = System.IO.Path.Combine(m_templatePath, "home.html");

            string html;
            
            if (File.Exists(layoutFile) && File.Exists(homeFile))
            {
                string layout = File.ReadAllText(layoutFile);
                string content = File.ReadAllText(homeFile);
                
                // Simple template replacement
                html = layout.Replace("{{GRID_NAME}}", "OpenSim Web Portal")
                            .Replace("{{CONTENT}}", content)
                            .Replace("{{#IF_NOT_LOGGED_IN}}", "")
                            .Replace("{{/IF_NOT_LOGGED_IN}}", "")
                            .Replace("{{ELSE}}", "<!--")
                            .Replace("{{/IF_LOGGED_IN}}", "-->")
                            .Replace("{{HEAD_EXTRA}}", "")
                            .Replace("{{SCRIPT_EXTRA}}", "")
                            .Replace("{{#ALERTS}}", "<!--")
                            .Replace("{{/ALERTS}}", "-->")
                            .Replace("{{TOTAL_USERS}}", "0")
                            .Replace("{{ONLINE_USERS}}", "0")
                            .Replace("{{TOTAL_REGIONS}}", "0")
                            .Replace("{{UPTIME}}", "Just started");
            }
            else
            {
                html = GetFallbackHtml();
            }

            return Encoding.UTF8.GetBytes(html);
        }

        private string GetFallbackHtml()
        {
            return @"<!DOCTYPE html>
<html lang='de'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>OpenSim Web Portal</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css' rel='stylesheet'>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.1/font/bootstrap-icons.css' rel='stylesheet'>
</head>
<body>
    <nav class='navbar navbar-dark bg-primary'>
        <div class='container-fluid'>
            <span class='navbar-brand'><i class='bi bi-globe'></i> OpenSim Web Portal</span>
        </div>
    </nav>
    <div class='container mt-5'>
        <div class='card shadow'>
            <div class='card-body text-center py-5'>
                <i class='bi bi-rocket-takeoff text-primary' style='font-size: 5rem;'></i>
                <h1 class='mt-4'>Willkommen beim OpenSim Web Portal</h1>
                <p class='lead text-muted'>Der Server läuft erfolgreich!</p>
                <hr class='my-4'>
                <div class='alert alert-info'>
                    <i class='bi bi-info-circle'></i> Template-Dateien nicht gefunden. Bitte kopieren Sie die Dateien aus websites/templates/ nach bin/portal/templates/
                </div>
                <a href='/api/message' class='btn btn-primary'><i class='bi bi-api'></i> API Testen</a>
            </div>
        </div>
    </div>
    <script src='https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js'></script>
</body>
</html>";
        }
    }

    /// <summary>
    /// Handler for the REST API endpoint
    /// </summary>
    public class PortalApiHandler : BaseStreamHandler
    {
        private DateTime m_startTime;

        public PortalApiHandler(DateTime startTime) : base("GET", "/api/message")
        {
            m_startTime = startTime;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "application/json";
            httpResponse.StatusCode = 200;

            TimeSpan uptime = DateTime.Now - m_startTime;
            
            string json = string.Format(@"{{
    ""message"": ""Hello from OpenSim Web Portal!"",
    ""timestamp"": ""{0}"",
    ""server"": ""OpenSim.Web.Portal"",
    ""version"": ""1.0.0"",
    ""status"": ""online"",
    ""uptime"": ""{1}""
}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
     string.Format("{0}d {1}h {2}m", uptime.Days, uptime.Hours, uptime.Minutes));
            
            return Encoding.UTF8.GetBytes(json);
        }
    }

    /// <summary>
    /// Handler for serving static CSS files
    /// </summary>
    public class PortalCssHandler : BaseStreamHandler
    {
        private string m_cssPath;

        public PortalCssHandler(string cssPath) : base("GET", "/portal/css/")
        {
            m_cssPath = cssPath;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            string fileName = path.Replace("/portal/css/", "");
            string filePath = System.IO.Path.Combine(m_cssPath, fileName);

            if (File.Exists(filePath))
            {
                httpResponse.ContentType = "text/css";
                httpResponse.StatusCode = 200;
                return File.ReadAllBytes(filePath);
            }

            httpResponse.StatusCode = 404;
            return Encoding.UTF8.GetBytes("/* CSS file not found */");
        }
    }

    /// <summary>
    /// Generic handler for serving HTML pages from templates
    /// </summary>
    public class PortalPageHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string m_templatePath;
        private string m_pageName;

        public PortalPageHandler(string templatePath, string pageName, string route) : base("GET", route)
        {
            m_templatePath = templatePath;
            m_pageName = pageName;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            // Check if it's an XML file (for RSS feeds)
            bool isXmlFile = m_pageName.EndsWith("rss") || m_pageName.EndsWith(".xml");
            
            if (isXmlFile)
            {
                httpResponse.ContentType = "application/rss+xml; charset=UTF-8";
            }
            else
            {
                httpResponse.ContentType = "text/html; charset=UTF-8";
            }
            httpResponse.StatusCode = 200;

            string pageFile = System.IO.Path.Combine(m_templatePath, m_pageName + (isXmlFile ? ".xml" : ".html"));

            try
            {
                if (!File.Exists(pageFile))
                {
                    m_log.ErrorFormat("[PORTAL PAGE HANDLER]: Page file not found: {0}", pageFile);
                    if (isXmlFile)
                    {
                        return Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?><error>File not found</error>");
                    }
                    return Encoding.UTF8.GetBytes("<html><body><h1>Error: Page not found</h1></body></html>");
                }

                string pageContent = File.ReadAllText(pageFile);

                // For XML files, don't use layout, just return the content
                if (isXmlFile)
                {
                    // Replace placeholders
                    pageContent = pageContent.Replace("{{ GridName }}", "OpenSim Web Portal");
                    pageContent = pageContent.Replace("{{ LoginURI }}", "http://localhost:9000");
                    pageContent = pageContent.Replace("{{ CurrentDateTime }}", DateTime.Now.ToString("R")); // RFC1123
                    pageContent = pageContent.Replace("{{ Timestamp }}", DateTime.Now.Ticks.ToString());
                    pageContent = pageContent.Replace("{{ UsersInworld }}", "0");
                    pageContent = pageContent.Replace("{{ RegionsTotal }}", "0");
                    pageContent = pageContent.Replace("{{ UsersTotal }}", "0");
                    pageContent = pageContent.Replace("{{ UsersActive }}", "0");
                    pageContent = pageContent.Replace("{{ UsersActivePeriod }}", "30");
                    pageContent = pageContent.Replace("{{ Version }}", "0.9.3");
                    pageContent = pageContent.Replace("{{ Uptime }}", "Just started");
                    
                    return Encoding.UTF8.GetBytes(pageContent);
                }

                // For HTML files, use layout if available
                string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
                string html;

                if (File.Exists(layoutFile))
                {
                    string layout = File.ReadAllText(layoutFile);
                    html = layout.Replace("{{CONTENT}}", pageContent);
                }
                else
                {
                    // No layout, return page content directly
                    html = pageContent;
                }

                // Replace common placeholders with default values
                html = html.Replace("{{ GridName }}", "OpenSim Web Portal");
                html = html.Replace("{{ UsersInworld }}", "0");
                html = html.Replace("{{ RegionsTotal }}", "0");
                html = html.Replace("{{ UsersTotal }}", "0");
                html = html.Replace("{{ UsersActive }}", "0");
                html = html.Replace("{{ UsersActivePeriod }}", "30");
                html = html.Replace("{{ LoginURI }}", "http://localhost:9000");
                html = html.Replace("{{ CurrentDateTime }}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                html = html.Replace("{{ Version }}", "0.9.3");
                html = html.Replace("{{ Uptime }}", "Just started");
                html = html.Replace("{{ StartingBalance }}", "1000");
                html = html.Replace("{{ UserBalance }}", "0");
                html = html.Replace("{{ SearchQuery }}", "");
                html = html.Replace("{{GRID_NAME}}", "OpenSim Web Portal");
                html = html.Replace("{{USER_FIRSTNAME}}", "Guest");
                html = html.Replace("{{USER_LASTNAME}}", "User");
                html = html.Replace("{{USER_EMAIL}}", "guest@example.com");
                html = html.Replace("{{USER_UUID}}", "00000000-0000-0000-0000-000000000000");
                html = html.Replace("{{USER_LEVEL}}", "0");
                html = html.Replace("{{CREATED_DATE}}", "2025-01-01");
                html = html.Replace("{{LAST_LOGIN}}", "Noch nie");
                html = html.Replace("{{LOGIN_URI}}", "http://localhost:9000");

                return Encoding.UTF8.GetBytes(html);
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[PORTAL PAGE HANDLER]: Error loading page {0}: {1}", m_pageName, e.Message);
                return Encoding.UTF8.GetBytes($"<html><body><h1>Error loading page</h1><p>{e.Message}</p></body></html>");
            }
        }
    }
}

namespace OpenSim.Web.Portal
{
    /// <summary>
    /// OpenSim Web Portal Server - Modern web interface for OpenSimulator
    /// Based on OpenSim.Addon.Example architecture
    /// </summary>
    public class WebPortalServer : BaseOpenSimServer
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private new BaseHttpServer m_httpServer;
        private uint m_port = 8100;  // Default port - avoids conflicts with OpenSim (9000), Robust (8002), MoneyServer (8008)
        private DateTime m_startTime;
        private string m_templatePath;
        private string m_cssPath;
        private volatile bool m_isRunning = true;
        
        /// <summary>
        /// Constructor - initializes the console for the server
        /// </summary>
        public WebPortalServer()
        {
            m_console = new LocalConsole("WebPortal");
            MainConsole.Instance = m_console;
            m_startTime = DateTime.Now;
        }

        /// <summary>
        /// Main startup method - called by base class
        /// </summary>
        public override void Startup()
        {
            m_log.Info("[WEB PORTAL]: Beginning startup processing");
            m_log.Info("[WEB PORTAL]: OpenSim Web Portal v1.0 - Modern Bootstrap 5 Interface");
            
            try
            {
                ReadConfiguration();
                SetupPaths();
                SetupHttpServer();
                RegisterConsoleCommands();
                
                m_log.InfoFormat("[WEB PORTAL]: ========================================");
                m_log.InfoFormat("[WEB PORTAL]:  OpenSim Web Portal Server Started");
                m_log.InfoFormat("[WEB PORTAL]:  Web Interface: http://localhost:{0}", m_port);
                m_log.InfoFormat("[WEB PORTAL]:  API Endpoint: http://localhost:{0}/api/message", m_port);
                m_log.InfoFormat("[WEB PORTAL]: ========================================");
                
                // Start console read loop
                Work();
            }
            catch (Exception e)
            {
                m_log.Fatal("[WEB PORTAL]: Fatal error during startup: " + e.ToString());
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Read configuration from WebPortal.ini if it exists
        /// </summary>
        private void ReadConfiguration()
        {
            string iniFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebPortal.ini");
            
            if (File.Exists(iniFile))
            {
                try
                {
                    IConfigSource config = new IniConfigSource(iniFile);
                    IConfig serverConfig = config.Configs["WebPortal"];
                    
                    if (serverConfig != null)
                    {
                        m_port = (uint)serverConfig.GetInt("Port", 9000);
                        m_log.InfoFormat("[WEB PORTAL]: Configuration loaded from {0}", iniFile);
                        m_log.InfoFormat("[WEB PORTAL]: Port set to {0}", m_port);
                    }
                }
                catch (Exception e)
                {
                    m_log.ErrorFormat("[WEB PORTAL]: Error reading configuration: {0}", e.Message);
                    m_log.Info("[WEB PORTAL]: Using default settings");
                }
            }
            else
            {
                m_log.InfoFormat("[WEB PORTAL]: No configuration file found at {0}", iniFile);
                m_log.Info("[WEB PORTAL]: Using default port 9000");
            }
        }

        /// <summary>
        /// Setup paths for templates and static files
        /// </summary>
        private void SetupPaths()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string portalDir = System.IO.Path.Combine(baseDir, "portal");
            m_templatePath = System.IO.Path.Combine(portalDir, "templates");
            m_cssPath = System.IO.Path.Combine(portalDir, "css");

            // Create directories if they don't exist
            if (!Directory.Exists(m_templatePath))
            {
                Directory.CreateDirectory(m_templatePath);
                m_log.WarnFormat("[WEB PORTAL]: Template directory created at {0}", m_templatePath);
                m_log.Warn("[WEB PORTAL]: Please copy template files to this directory");
            }

            if (!Directory.Exists(m_cssPath))
            {
                Directory.CreateDirectory(m_cssPath);
                m_log.WarnFormat("[WEB PORTAL]: CSS directory created at {0}", m_cssPath);
            }

            m_log.InfoFormat("[WEB PORTAL]: Template path: {0}", m_templatePath);
            m_log.InfoFormat("[WEB PORTAL]: CSS path: {0}", m_cssPath);
        }

        /// <summary>
        /// Setup the HTTP server and register handlers
        /// </summary>
        private void SetupHttpServer()
        {
            m_log.InfoFormat("[WEB PORTAL]: Setting up HTTP server on port {0}", m_port);
            
            m_httpServer = new BaseHttpServer(m_port);
            
            // Register handlers
            m_httpServer.AddStreamHandler(new Handlers.PortalHomeHandler(m_templatePath));
            m_httpServer.AddStreamHandler(new Handlers.PortalApiHandler(m_startTime));
            m_httpServer.AddStreamHandler(new Handlers.PortalCssHandler(m_cssPath));
            
            // Register page handlers
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "login", "/portal/login"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "register", "/portal/register"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "account", "/portal/account"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "inventory", "/portal/inventory"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "forgot-password", "/portal/forgot-password"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "about", "/portal/about"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "admin-users", "/portal/admin/users"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "admin-console", "/portal/admin/console"));
            
            // Firestorm Viewer / Grid-Manager Seiten
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "welcome", "/welcome"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "splash", "/splash"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "guide", "/guide"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "tos", "/tos"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "termsofservice", "/termsofservice"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "help", "/help"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "economy", "/economy"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "password", "/password"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "gridstatus", "/gridstatus"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "search", "/search"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "avatars", "/avatars"));
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "rss", "/rss"));
            
            // RSS Feed (XML)
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "gridstatusrss", "/gridstatusrss"));
            
            // Zusätzliche 404-Seite
            m_httpServer.AddStreamHandler(new Handlers.PortalPageHandler(m_templatePath, "404", "/404"));
            
            m_log.Info("[WEB PORTAL]: HTTP handlers registered:");
            m_log.Info("[WEB PORTAL]:   GET  /                        - Home Page");
            m_log.Info("[WEB PORTAL]:   GET  /api/message             - REST API");
            m_log.Info("[WEB PORTAL]:   GET  /portal/css/*            - CSS Files");
            m_log.Info("[WEB PORTAL]:   GET  /portal/login            - Login Page");
            m_log.Info("[WEB PORTAL]:   GET  /portal/register         - Registration Page");
            m_log.Info("[WEB PORTAL]:   GET  /portal/account          - Account Management");
            m_log.Info("[WEB PORTAL]:   GET  /portal/inventory        - Inventory Browser");
            m_log.Info("[WEB PORTAL]:   GET  /portal/forgot-password  - Password Recovery");
            m_log.Info("[WEB PORTAL]:   GET  /portal/about            - About Page");
            m_log.Info("[WEB PORTAL]:   GET  /portal/admin/users      - Admin: User Management");
            m_log.Info("[WEB PORTAL]:   GET  /portal/admin/console    - Admin: Console");
            m_log.Info("[WEB PORTAL]: Firestorm Viewer / Grid-Manager Seiten:");
            m_log.Info("[WEB PORTAL]:   GET  /welcome                 - Welcome Page (Splash)");
            m_log.Info("[WEB PORTAL]:   GET  /splash                  - Splash Page (Sidebar)");
            m_log.Info("[WEB PORTAL]:   GET  /guide                   - Destination Guide");
            m_log.Info("[WEB PORTAL]:   GET  /tos                     - Terms of Service (Form)");
            m_log.Info("[WEB PORTAL]:   GET  /termsofservice          - Terms of Service (Read-only)");
            m_log.Info("[WEB PORTAL]:   GET  /help                    - Help & Support");
            m_log.Info("[WEB PORTAL]:   GET  /economy                 - Economy & Currency");
            m_log.Info("[WEB PORTAL]:   GET  /password                - Password Recovery");
            m_log.Info("[WEB PORTAL]:   GET  /gridstatus              - Grid Status (HTML)");
            m_log.Info("[WEB PORTAL]:   GET  /gridstatusrss           - Grid Status (RSS XML)");
            m_log.Info("[WEB PORTAL]:   GET  /search                  - Grid Search");
            m_log.Info("[WEB PORTAL]:   GET  /avatars                 - Avatar Picker");
            m_log.Info("[WEB PORTAL]:   GET  /rss                     - RSS Feed Info");
            m_log.Info("[WEB PORTAL]:   GET  /404                     - Not Found Page");
            
            m_httpServer.Start();
            m_log.Info("[WEB PORTAL]: HTTP server started successfully");
        }

        /// <summary>
        /// Register console commands for the server
        /// </summary>
        private void RegisterConsoleCommands()
        {
            m_log.Info("[WEB PORTAL]: Registering console commands");
            
            m_console.Commands.AddCommand("WebPortal", false, "help",
                "help",
                "Display available commands",
                HandleHelp);
            
            m_console.Commands.AddCommand("WebPortal", false, "show status",
                "show status",
                "Show server status information",
                HandleShowStatus);
            
            m_console.Commands.AddCommand("WebPortal", false, "shutdown",
                "shutdown",
                "Shutdown the Web Portal server",
                HandleShutdown);
            
            m_log.Info("[WEB PORTAL]: Console commands registered");
        }

        /// <summary>
        /// Handle the 'help' console command
        /// </summary>
        private void HandleHelp(string module, string[] cmdparams)
        {
            m_log.Info("[WEB PORTAL]: ========================================");
            m_log.Info("[WEB PORTAL]: Available Commands:");
            m_log.Info("[WEB PORTAL]: ========================================");
            m_log.Info("[WEB PORTAL]: help            - Show this help message");
            m_log.Info("[WEB PORTAL]: show status     - Display server status");
            m_log.Info("[WEB PORTAL]: shutdown        - Shutdown the server");
            m_log.Info("[WEB PORTAL]: ========================================");
        }

        /// <summary>
        /// Handle the 'show status' console command
        /// </summary>
        private void HandleShowStatus(string module, string[] cmdparams)
        {
            TimeSpan uptime = DateTime.Now - m_startTime;
            
            m_log.Info("[WEB PORTAL]: ========================================");
            m_log.Info("[WEB PORTAL]: Server Status");
            m_log.Info("[WEB PORTAL]: ========================================");
            m_log.InfoFormat("[WEB PORTAL]: Server Name:    OpenSim Web Portal");
            m_log.InfoFormat("[WEB PORTAL]: Version:        1.0.0");
            m_log.InfoFormat("[WEB PORTAL]: Status:         Online");
            m_log.InfoFormat("[WEB PORTAL]: Port:           {0}", m_port);
            m_log.InfoFormat("[WEB PORTAL]: Started:        {0}", m_startTime.ToString("yyyy-MM-dd HH:mm:ss"));
            m_log.InfoFormat("[WEB PORTAL]: Uptime:         {0}d {1}h {2}m {3}s", 
                uptime.Days, uptime.Hours, uptime.Minutes, uptime.Seconds);
            m_log.InfoFormat("[WEB PORTAL]: Web Interface:  http://localhost:{0}", m_port);
            m_log.InfoFormat("[WEB PORTAL]: API Endpoint:   http://localhost:{0}/api/message", m_port);
            m_log.InfoFormat("[WEB PORTAL]: Template Path:  {0}", m_templatePath);
            m_log.Info("[WEB PORTAL]: ========================================");
        }

        /// <summary>
        /// Handle the 'shutdown' console command
        /// </summary>
        private void HandleShutdown(string module, string[] cmdparams)
        {
            m_log.Info("[WEB PORTAL]: Shutdown command received");
            m_log.Info("[WEB PORTAL]: Stopping HTTP server...");
            
            if (m_httpServer != null)
            {
                m_httpServer.Stop();
                m_log.Info("[WEB PORTAL]: HTTP server stopped");
            }
            
            m_log.Info("[WEB PORTAL]: Shutting down console...");
            m_isRunning = false;
            
            m_log.Info("[WEB PORTAL]: Shutdown complete");
            
            // Give a moment for logs to flush
            System.Threading.Thread.Sleep(100);
            Environment.Exit(0);
        }

        /// <summary>
        /// Main work loop - keeps the server running and processes console input
        /// </summary>
        private void Work()
        {
            m_log.Info("[WEB PORTAL]: Entering main loop. Type 'help' for commands or 'shutdown' to stop.");
            
            try
            {
                while (m_isRunning)
                {
                    m_console.Prompt();
                }
            }
            catch (Exception e)
            {
                m_log.Error("[WEB PORTAL]: Error in main loop: " + e.Message);
                m_log.Error("[WEB PORTAL]: Server will continue running but console may not be responsive.");
                
                // If console fails (e.g., in Docker/systemd), keep server alive
                while (m_isRunning)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Main entry point for the application
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static int Main(string[] args)
        {
            string logConfigFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WebPortal.log4net");
            
            if (File.Exists(logConfigFile))
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(logConfigFile));
            }

            try
            {
                WebPortalServer server = new WebPortalServer();
                server.Startup();
                
                // Keep the process alive - Startup() method never returns because it enters Work() loop
                // This line is unreachable but kept for documentation
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("FATAL ERROR: " + e.ToString());
                if (Environment.UserInteractive)
                {
                    Console.WriteLine("Press any key to exit...");
                    try
                    {
                        Console.ReadKey();
                    }
                    catch
                    {
                        // ReadKey might fail in non-interactive environments
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                return 1;
            }
        }
    }
}

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
using System.Web;
using log4net;
using OpenSim.Framework.Servers.HttpServer;

namespace OpenSim.Web.Portal.Handlers
{
    /// <summary>
    /// Base handler for protected pages that require authentication
    /// </summary>
    public abstract class ProtectedPageHandler : BaseStreamHandler
    {
        protected static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected readonly string m_templatePath;

        protected ProtectedPageHandler(string httpMethod, string path, string templatePath) 
            : base(httpMethod, path)
        {
            m_templatePath = templatePath;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            // Check authentication
            var session = GetSession(httpRequest);

            if (session == null)
            {
                // Not authenticated - redirect to login
                httpResponse.StatusCode = 302;
                httpResponse.AddHeader("Location", "/portal/login?redirect=" + Uri.EscapeDataString(path));
                return Encoding.UTF8.GetBytes("Redirecting to login...");
            }

            // User is authenticated - process the protected request
            return ProcessProtectedRequest(path, request, httpRequest, httpResponse, session);
        }

        /// <summary>
        /// Process the request for an authenticated user
        /// </summary>
        protected abstract byte[] ProcessProtectedRequest(string path, Stream request, 
            IOSHttpRequest httpRequest, IOSHttpResponse httpResponse, SessionManager.UserSession session);

        /// <summary>
        /// Get current session from cookie
        /// </summary>
        protected SessionManager.UserSession GetSession(IOSHttpRequest httpRequest)
        {
            string sessionId = GetSessionIdFromCookie(httpRequest);
            if (string.IsNullOrEmpty(sessionId))
                return null;

            return SessionManager.Instance.GetSession(sessionId);
        }

        /// <summary>
        /// Extract session ID from cookie
        /// </summary>
        protected string GetSessionIdFromCookie(IOSHttpRequest httpRequest)
        {
            string cookieHeader = httpRequest.Headers["Cookie"];
            if (string.IsNullOrEmpty(cookieHeader))
                return null;

            string[] cookies = cookieHeader.Split(';');
            foreach (string cookie in cookies)
            {
                string[] parts = cookie.Trim().Split('=');
                if (parts.Length == 2 && parts[0] == "OPENSIM_SESSION")
                {
                    return parts[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Replace template placeholders with session data
        /// </summary>
        protected string ReplaceSessionData(string html, SessionManager.UserSession session)
        {
            if (session == null)
                return html;

            return html
                .Replace("{{USER_FIRSTNAME}}", session.FirstName)
                .Replace("{{USER_LASTNAME}}", session.LastName)
                .Replace("{{USER_FULLNAME}}", session.FullName)
                .Replace("{{USER_EMAIL}}", session.Email ?? "")
                .Replace("{{USER_UUID}}", session.UserId.ToString())
                .Replace("{{USER_LEVEL}}", session.UserLevel.ToString())
                .Replace("{{LAST_LOGIN}}", session.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm"))
                .Replace("{{#IF_LOGGED_IN}}", "")
                .Replace("{{/IF_LOGGED_IN}}", "")
                .Replace("{{#IF_NOT_LOGGED_IN}}", "<!--")
                .Replace("{{/IF_NOT_LOGGED_IN}}", "-->")
                .Replace("{{ELSE}}", "--><!--");
        }
    }

    /// <summary>
    /// Handler for account page (requires authentication)
    /// </summary>
    public class AccountPageHandler : ProtectedPageHandler
    {
        public AccountPageHandler(string templatePath) 
            : base("GET", "/portal/account", templatePath)
        {
        }

        protected override byte[] ProcessProtectedRequest(string path, Stream request, 
            IOSHttpRequest httpRequest, IOSHttpResponse httpResponse, SessionManager.UserSession session)
        {
            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;

            string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
            string accountFile = System.IO.Path.Combine(m_templatePath, "account.html");

            if (File.Exists(layoutFile) && File.Exists(accountFile))
            {
                string layout = File.ReadAllText(layoutFile);
                string content = File.ReadAllText(accountFile);

                string html = layout.Replace("{{CONTENT}}", content);
                html = ReplaceSessionData(html, session);
                
                // Replace other placeholders
                html = html.Replace("{{GRID_NAME}}", "OpenSim Web Portal")
                          .Replace("{{LOGIN_URI}}", "http://localhost:9000")
                          .Replace("{{USER_BALANCE}}", "0")
                          .Replace("{{HEAD_EXTRA}}", "")
                          .Replace("{{SCRIPT_EXTRA}}", "");

                // Show welcome message if coming from registration
                if (httpRequest.QueryString.Get("welcome") == "true")
                {
                    string welcomeAlert = @"
<div class='alert alert-success alert-dismissible fade show' role='alert'>
    <strong>Willkommen!</strong> Dein Account wurde erfolgreich erstellt.
    <button type='button' class='btn-close' data-bs-dismiss='alert'></button>
</div>";
                    html = html.Replace("{{#ALERTS}}", welcomeAlert).Replace("{{/ALERTS}}", "");
                }
                else
                {
                    html = html.Replace("{{#ALERTS}}", "<!--").Replace("{{/ALERTS}}", "-->");
                }

                return Encoding.UTF8.GetBytes(html);
            }

            // Fallback
            string fallbackHtml = $@"<!DOCTYPE html>
<html lang='de'>
<head>
    <meta charset='UTF-8'>
    <title>Account - {session.FullName}</title>
</head>
<body>
    <h1>Willkommen, {session.FullName}!</h1>
    <p>Email: {session.Email}</p>
    <p>UUID: {session.UserId}</p>
    <p><a href='/portal/logout'>Abmelden</a></p>
</body>
</html>";

            return Encoding.UTF8.GetBytes(fallbackHtml);
        }
    }

    /// <summary>
    /// Handler for inventory page (requires authentication)
    /// </summary>
    public class InventoryPageHandler : ProtectedPageHandler
    {
        public InventoryPageHandler(string templatePath) 
            : base("GET", "/portal/inventory", templatePath)
        {
        }

        protected override byte[] ProcessProtectedRequest(string path, Stream request, 
            IOSHttpRequest httpRequest, IOSHttpResponse httpResponse, SessionManager.UserSession session)
        {
            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;

            string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
            string inventoryFile = System.IO.Path.Combine(m_templatePath, "inventory.html");

            if (File.Exists(layoutFile) && File.Exists(inventoryFile))
            {
                string layout = File.ReadAllText(layoutFile);
                string content = File.ReadAllText(inventoryFile);

                string html = layout.Replace("{{CONTENT}}", content);
                html = ReplaceSessionData(html, session);
                html = html.Replace("{{GRID_NAME}}", "OpenSim Web Portal")
                          .Replace("{{HEAD_EXTRA}}", "")
                          .Replace("{{SCRIPT_EXTRA}}", "")
                          .Replace("{{#ALERTS}}", "<!--")
                          .Replace("{{/ALERTS}}", "-->");

                return Encoding.UTF8.GetBytes(html);
            }

            return Encoding.UTF8.GetBytes("<h1>Inventory Page - Coming Soon</h1>");
        }
    }

    /// <summary>
    /// Handler for password change page (requires authentication)
    /// </summary>
    public class PasswordPageHandler : ProtectedPageHandler
    {
        private readonly AuthenticationService m_authService;

        public PasswordPageHandler(string templatePath, AuthenticationService authService) 
            : base("GET", "/portal/password", templatePath)
        {
            m_authService = authService;
        }

        protected override byte[] ProcessProtectedRequest(string path, Stream request, 
            IOSHttpRequest httpRequest, IOSHttpResponse httpResponse, SessionManager.UserSession session)
        {
            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;

            string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
            string passwordFile = System.IO.Path.Combine(m_templatePath, "password.html");

            if (File.Exists(layoutFile) && File.Exists(passwordFile))
            {
                string layout = File.ReadAllText(layoutFile);
                string content = File.ReadAllText(passwordFile);

                string html = layout.Replace("{{CONTENT}}", content);
                html = ReplaceSessionData(html, session);
                html = html.Replace("{{GRID_NAME}}", "OpenSim Web Portal")
                          .Replace("{{HEAD_EXTRA}}", "")
                          .Replace("{{SCRIPT_EXTRA}}", "");

                // Show error or success message if present
                string errorMsg = httpRequest.QueryString.Get("error");
                string successMsg = httpRequest.QueryString.Get("success");

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    html = html.Replace("{{#ALERTS}}", 
                        $"<div class='alert alert-danger'>{System.Web.HttpUtility.UrlDecode(errorMsg)}</div>")
                        .Replace("{{/ALERTS}}", "");
                }
                else if (!string.IsNullOrEmpty(successMsg))
                {
                    html = html.Replace("{{#ALERTS}}", 
                        $"<div class='alert alert-success'>{System.Web.HttpUtility.UrlDecode(successMsg)}</div>")
                        .Replace("{{/ALERTS}}", "");
                }
                else
                {
                    html = html.Replace("{{#ALERTS}}", "<!--").Replace("{{/ALERTS}}", "-->");
                }

                return Encoding.UTF8.GetBytes(html);
            }

            return Encoding.UTF8.GetBytes("<h1>Password Change - Coming Soon</h1>");
        }
    }

    /// <summary>
    /// Handler for admin-only console page
    /// </summary>
    public class AdminConsolePageHandler : ProtectedPageHandler
    {
        public AdminConsolePageHandler(string templatePath) 
            : base("GET", "/portal/admin/console", templatePath)
        {
        }

        protected override byte[] ProcessProtectedRequest(string path, Stream request, 
            IOSHttpRequest httpRequest, IOSHttpResponse httpResponse, SessionManager.UserSession session)
        {
            // Check if user is admin (UserLevel >= 200)
            if (session.UserLevel < 200)
            {
                httpResponse.StatusCode = 403;
                string errorHtml = @"<!DOCTYPE html>
<html lang='de'>
<head>
    <meta charset='UTF-8'>
    <title>Zugriff verweigert</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body>
    <div class='container mt-5'>
        <div class='alert alert-danger'>
            <h1><i class='bi bi-shield-exclamation'></i> Zugriff verweigert</h1>
            <p>Diese Seite ist nur für Administratoren zugänglich.</p>
            <a href='/' class='btn btn-primary'>Zurück zur Startseite</a>
        </div>
    </div>
</body>
</html>";
                return Encoding.UTF8.GetBytes(errorHtml);
            }

            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;

            string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
            string consoleFile = System.IO.Path.Combine(m_templatePath, "admin-console.html");

            if (File.Exists(layoutFile) && File.Exists(consoleFile))
            {
                string layout = File.ReadAllText(layoutFile);
                string content = File.ReadAllText(consoleFile);

                string html = layout.Replace("{{CONTENT}}", content);
                html = ReplaceSessionData(html, session);
                html = html.Replace("{{GRID_NAME}}", "OpenSim Grid")
                          .Replace("{{HEAD_EXTRA}}", "")
                          .Replace("{{SCRIPT_EXTRA}}", "")
                          .Replace("{{#ALERTS}}", "<!--")
                          .Replace("{{/ALERTS}}", "-->");

                return Encoding.UTF8.GetBytes(html);
            }

            return Encoding.UTF8.GetBytes("<h1>Admin Console - Template not found</h1>");
        }
    }

    /// <summary>
    /// Handler for admin-only user management page
    /// </summary>
    public class AdminUsersPageHandler : ProtectedPageHandler
    {
        public AdminUsersPageHandler(string templatePath) 
            : base("GET", "/portal/admin/users", templatePath)
        {
        }

        protected override byte[] ProcessProtectedRequest(string path, Stream request, 
            IOSHttpRequest httpRequest, IOSHttpResponse httpResponse, SessionManager.UserSession session)
        {
            // Check if user is admin (UserLevel >= 200)
            if (session.UserLevel < 200)
            {
                httpResponse.StatusCode = 403;
                string errorHtml = @"<!DOCTYPE html>
<html lang='de'>
<head>
    <meta charset='UTF-8'>
    <title>Zugriff verweigert</title>
    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>
<body>
    <div class='container mt-5'>
        <div class='alert alert-danger'>
            <h1><i class='bi bi-shield-exclamation'></i> Zugriff verweigert</h1>
            <p>Diese Seite ist nur für Administratoren zugänglich.</p>
            <a href='/' class='btn btn-primary'>Zurück zur Startseite</a>
        </div>
    </div>
</body>
</html>";
                return Encoding.UTF8.GetBytes(errorHtml);
            }

            httpResponse.ContentType = "text/html";
            httpResponse.StatusCode = 200;

            string layoutFile = System.IO.Path.Combine(m_templatePath, "layout.html");
            string usersFile = System.IO.Path.Combine(m_templatePath, "admin-users.html");

            if (File.Exists(layoutFile) && File.Exists(usersFile))
            {
                string layout = File.ReadAllText(layoutFile);
                string content = File.ReadAllText(usersFile);

                string html = layout.Replace("{{CONTENT}}", content);
                html = ReplaceSessionData(html, session);
                html = html.Replace("{{GRID_NAME}}", "OpenSim Grid")
                          .Replace("{{HEAD_EXTRA}}", "")
                          .Replace("{{SCRIPT_EXTRA}}", "")
                          .Replace("{{#ALERTS}}", "<!--")
                          .Replace("{{/ALERTS}}", "-->");

                return Encoding.UTF8.GetBytes(html);
            }

            return Encoding.UTF8.GetBytes("<h1>Admin User Management - Template not found</h1>");
        }
    }
}

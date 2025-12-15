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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using log4net;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;

namespace OpenSim.Web.Portal.Handlers
{
    /// <summary>
    /// Handler for user login (POST)
    /// </summary>
    public class LoginHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AuthenticationService m_authService;
        private readonly string m_templatePath;

        public LoginHandler(AuthenticationService authService, string templatePath) 
            : base("POST", "/portal/login")
        {
            m_authService = authService;
            m_templatePath = templatePath;
        }

        protected override byte[] ProcessRequest(string path, Stream requestData, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            try
            {
                m_log.Info("[LOGIN HANDLER]: Processing login request");

                // Check if auth service is available
                if (m_authService == null)
                {
                    m_log.Error("[LOGIN HANDLER]: Authentication service is not initialized!");
                    return RedirectWithError(httpResponse, "Authentifizierungsdienst nicht verfügbar");
                }

                // Read POST data
                string postData = new StreamReader(requestData).ReadToEnd();
                m_log.DebugFormat("[LOGIN HANDLER]: POST data length: {0} bytes", postData.Length);
                
                NameValueCollection formData = HttpUtility.ParseQueryString(postData);

                string firstName = formData["firstName"]?.Trim();
                string lastName = formData["lastName"]?.Trim();
                string password = formData["password"];
                bool rememberMe = formData["rememberMe"] == "on";

                m_log.InfoFormat("[LOGIN HANDLER]: Login attempt for '{0}' '{1}'", firstName ?? "(null)", lastName ?? "(null)");

                // Validate input
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(password))
                {
                    m_log.Warn("[LOGIN HANDLER]: Missing required fields");
                    return RedirectWithError(httpResponse, "Bitte alle Felder ausfüllen");
                }

                // Authenticate user
                m_log.DebugFormat("[LOGIN HANDLER]: Calling authentication service for {0} {1}", firstName, lastName);
                UserAccount account = m_authService.Authenticate(firstName, lastName, password);

                if (account != null)
                {
                    m_log.InfoFormat("[LOGIN HANDLER]: Authentication successful for {0} {1}", firstName, lastName);
                    
                    // Create session
                    string sessionId = SessionManager.Instance.CreateSession(account, rememberMe);

                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        m_log.InfoFormat("[LOGIN HANDLER]: Session created: {0}", sessionId);
                        
                        // Set session cookie
                        int maxAge = rememberMe ? 2592000 : 0; // 30 days if remember me, session cookie otherwise
                        httpResponse.AddHeader("Set-Cookie", 
                            $"OPENSIM_SESSION={sessionId}; Path=/; HttpOnly; SameSite=Lax; Max-Age={maxAge}");

                        // Redirect to account page
                        httpResponse.StatusCode = 302;
                        httpResponse.AddHeader("Location", "/portal/account");
                        
                        m_log.InfoFormat("[LOGIN HANDLER]: Login successful for {0} {1}, redirecting to account page", firstName, lastName);
                        return Encoding.UTF8.GetBytes("Redirecting...");
                    }
                    else
                    {
                        m_log.Error("[LOGIN HANDLER]: Failed to create session!");
                        return RedirectWithError(httpResponse, "Fehler beim Erstellen der Sitzung");
                    }
                }

                m_log.InfoFormat("[LOGIN HANDLER]: Authentication failed for {0} {1}", firstName, lastName);
                return RedirectWithError(httpResponse, "Ungültiger Benutzername oder Passwort");
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[LOGIN HANDLER]: Exception during login: {0}", ex.Message);
                m_log.ErrorFormat("[LOGIN HANDLER]: Exception type: {0}", ex.GetType().Name);
                m_log.ErrorFormat("[LOGIN HANDLER]: Stack trace: {0}", ex.StackTrace);
                if (ex.InnerException != null)
                {
                    m_log.ErrorFormat("[LOGIN HANDLER]: Inner exception: {0}", ex.InnerException.Message);
                }
                return RedirectWithError(httpResponse, "Fehler: " + ex.Message);
            }
        }

        private byte[] RedirectWithError(IOSHttpResponse httpResponse, string errorMessage)
        {
            httpResponse.StatusCode = 302;
            httpResponse.AddHeader("Location", $"/portal/login?error={HttpUtility.UrlEncode(errorMessage)}");
            return Encoding.UTF8.GetBytes("Redirecting...");
        }
    }

    /// <summary>
    /// Handler for user logout (GET or POST)
    /// </summary>
    public class LogoutHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public LogoutHandler() : base("GET", "/portal/logout")
        {
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            try
            {
                // Get session cookie
                string sessionId = GetSessionIdFromCookie(httpRequest);

                if (!string.IsNullOrEmpty(sessionId))
                {
                    var session = SessionManager.Instance.GetSession(sessionId);
                    if (session != null)
                    {
                        m_log.InfoFormat("[LOGOUT HANDLER]: User {0} {1} logged out", session.FirstName, session.LastName);
                    }

                    // Destroy session
                    SessionManager.Instance.DestroySession(sessionId);
                }

                // Clear session cookie
                httpResponse.AddHeader("Set-Cookie", 
                    "OPENSIM_SESSION=; Path=/; HttpOnly; SameSite=Lax; Max-Age=0");

                // Redirect to home
                httpResponse.StatusCode = 302;
                httpResponse.AddHeader("Location", "/");
                
                return Encoding.UTF8.GetBytes("Redirecting...");
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[LOGOUT HANDLER]: Exception during logout: {0}", ex.Message);
                
                httpResponse.StatusCode = 302;
                httpResponse.AddHeader("Location", "/");
                return Encoding.UTF8.GetBytes("Redirecting...");
            }
        }

        private string GetSessionIdFromCookie(IOSHttpRequest httpRequest)
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
    }

    /// <summary>
    /// Handler for user registration (POST)
    /// </summary>
    public class RegisterHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AuthenticationService m_authService;

        public RegisterHandler(AuthenticationService authService) 
            : base("POST", "/portal/register")
        {
            m_authService = authService;
        }

        protected override byte[] ProcessRequest(string path, Stream requestData, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            try
            {
                // Read POST data
                string postData = new StreamReader(requestData).ReadToEnd();
                NameValueCollection formData = HttpUtility.ParseQueryString(postData);

                string firstName = formData["firstName"]?.Trim();
                string lastName = formData["lastName"]?.Trim();
                string email = formData["email"]?.Trim();
                string password = formData["password"];
                string confirmPassword = formData["confirmPassword"];

                m_log.InfoFormat("[REGISTER HANDLER]: Registration attempt for {0} {1}", firstName, lastName);

                // Validate input
                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || 
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    return RedirectWithError(httpResponse, "Bitte alle Felder ausfüllen");
                }

                if (password != confirmPassword)
                {
                    return RedirectWithError(httpResponse, "Passwörter stimmen nicht überein");
                }

                if (password.Length < 6)
                {
                    return RedirectWithError(httpResponse, "Passwort muss mindestens 6 Zeichen lang sein");
                }

                // Check if user already exists
                if (m_authService.UserExists(firstName, lastName))
                {
                    return RedirectWithError(httpResponse, "Benutzername bereits vergeben");
                }

                if (m_authService.EmailExists(email))
                {
                    return RedirectWithError(httpResponse, "E-Mail bereits registriert");
                }

                // Create account
                UserAccount account = m_authService.CreateAccount(firstName, lastName, password, email);

                if (account != null)
                {
                    m_log.InfoFormat("[REGISTER HANDLER]: Registration successful for {0} {1}", firstName, lastName);
                    
                    // Auto-login after registration
                    string sessionId = SessionManager.Instance.CreateSession(account, false);
                    
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        httpResponse.AddHeader("Set-Cookie", 
                            $"OPENSIM_SESSION={sessionId}; Path=/; HttpOnly; SameSite=Lax");
                    }

                    // Redirect to account page
                    httpResponse.StatusCode = 302;
                    httpResponse.AddHeader("Location", "/portal/account?welcome=true");
                    return Encoding.UTF8.GetBytes("Redirecting...");
                }

                m_log.ErrorFormat("[REGISTER HANDLER]: Registration failed for {0} {1}", firstName, lastName);
                return RedirectWithError(httpResponse, "Registrierung fehlgeschlagen");
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[REGISTER HANDLER]: Exception during registration: {0}", ex.Message);
                return RedirectWithError(httpResponse, "Ein Fehler ist aufgetreten");
            }
        }

        private byte[] RedirectWithError(IOSHttpResponse httpResponse, string errorMessage)
        {
            httpResponse.StatusCode = 302;
            httpResponse.AddHeader("Location", $"/portal/register?error={HttpUtility.UrlEncode(errorMessage)}");
            return Encoding.UTF8.GetBytes("Redirecting...");
        }
    }
}

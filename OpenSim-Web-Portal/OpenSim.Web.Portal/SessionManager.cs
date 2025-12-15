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
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using log4net;
using OpenMetaverse;
using OpenSim.Services.Interfaces;

namespace OpenSim.Web.Portal
{
    /// <summary>
    /// Manages user sessions for the Web Portal
    /// </summary>
    public class SessionManager
    {
        private static readonly ILog m_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static SessionManager m_instance;
        private readonly Dictionary<string, UserSession> m_sessions;
        private readonly object m_lockObject = new object();
        private readonly int m_sessionTimeoutMinutes;

        /// <summary>
        /// User session data
        /// </summary>
        public class UserSession
        {
            public string SessionId { get; set; }
            public UUID UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public int UserLevel { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastAccess { get; set; }
            public bool RememberMe { get; set; }

            public string FullName => $"{FirstName} {LastName}";
            
            public bool IsExpired(int timeoutMinutes)
            {
                // "Remember me" sessions last 30 days, regular sessions expire after timeout
                int expiryMinutes = RememberMe ? 43200 : timeoutMinutes; // 30 days vs configured timeout
                return (DateTime.UtcNow - LastAccess).TotalMinutes > expiryMinutes;
            }
        }

        private SessionManager(int sessionTimeoutMinutes = 30)
        {
            m_sessions = new Dictionary<string, UserSession>();
            m_sessionTimeoutMinutes = sessionTimeoutMinutes;
            m_log.Info($"[SESSION MANAGER]: Initialized with {m_sessionTimeoutMinutes} minutes timeout");
        }

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static SessionManager Instance
        {
            get
            {
                if (m_instance == null)
                    m_instance = new SessionManager();
                return m_instance;
            }
        }

        /// <summary>
        /// Initialize with custom timeout
        /// </summary>
        public static void Initialize(int sessionTimeoutMinutes)
        {
            m_instance = new SessionManager(sessionTimeoutMinutes);
        }

        /// <summary>
        /// Create a new session for a user
        /// </summary>
        public string CreateSession(UserAccount user, bool rememberMe = false)
        {
            if (user == null)
                return null;

            lock (m_lockObject)
            {
                // Generate secure session ID
                string sessionId = GenerateSessionId();

                var session = new UserSession
                {
                    SessionId = sessionId,
                    UserId = user.PrincipalID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    UserLevel = user.UserLevel,
                    CreatedAt = DateTime.UtcNow,
                    LastAccess = DateTime.UtcNow,
                    RememberMe = rememberMe
                };

                m_sessions[sessionId] = session;
                
                m_log.InfoFormat("[SESSION MANAGER]: Created session for {0} {1} (ID: {2}, RememberMe: {3})", 
                    user.FirstName, user.LastName, sessionId.Substring(0, 8), rememberMe);

                return sessionId;
            }
        }

        /// <summary>
        /// Get session by session ID
        /// </summary>
        public UserSession GetSession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            lock (m_lockObject)
            {
                if (m_sessions.TryGetValue(sessionId, out UserSession session))
                {
                    // Check if session is expired
                    if (session.IsExpired(m_sessionTimeoutMinutes))
                    {
                        m_sessions.Remove(sessionId);
                        m_log.InfoFormat("[SESSION MANAGER]: Session {0} expired for {1}", 
                            sessionId.Substring(0, 8), session.FullName);
                        return null;
                    }

                    // Update last access time
                    session.LastAccess = DateTime.UtcNow;
                    return session;
                }
            }

            return null;
        }

        /// <summary>
        /// Validate if session exists and is valid
        /// </summary>
        public bool IsValidSession(string sessionId)
        {
            return GetSession(sessionId) != null;
        }

        /// <summary>
        /// Destroy a session (logout)
        /// </summary>
        public bool DestroySession(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return false;

            lock (m_lockObject)
            {
                if (m_sessions.TryGetValue(sessionId, out UserSession session))
                {
                    m_sessions.Remove(sessionId);
                    m_log.InfoFormat("[SESSION MANAGER]: Destroyed session {0} for {1}", 
                        sessionId.Substring(0, 8), session.FullName);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Clean up expired sessions
        /// </summary>
        public void CleanupExpiredSessions()
        {
            lock (m_lockObject)
            {
                var expiredSessions = m_sessions.Where(kvp => 
                    kvp.Value.IsExpired(m_sessionTimeoutMinutes)).Select(kvp => kvp.Key).ToList();

                foreach (var sessionId in expiredSessions)
                {
                    m_sessions.Remove(sessionId);
                }

                if (expiredSessions.Count > 0)
                {
                    m_log.InfoFormat("[SESSION MANAGER]: Cleaned up {0} expired sessions", expiredSessions.Count);
                }
            }
        }

        /// <summary>
        /// Get active session count
        /// </summary>
        public int GetActiveSessionCount()
        {
            lock (m_lockObject)
            {
                return m_sessions.Count;
            }
        }

        /// <summary>
        /// Generate a secure session ID
        /// </summary>
        private string GenerateSessionId()
        {
            byte[] randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }
    }
}

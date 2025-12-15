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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using log4net;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;

namespace OpenSim.Web.Portal
{
    /// <summary>
    /// Authentication service for Web Portal - validates users against OpenSim database
    /// </summary>
    public class AuthenticationService
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUserAccountService m_userAccountService;
        private readonly IAuthenticationService m_authenticationService;

        public AuthenticationService(IUserAccountService userAccountService, IAuthenticationService authenticationService)
        {
            m_userAccountService = userAccountService;
            m_authenticationService = authenticationService;
            
            if (m_userAccountService == null)
                m_log.Warn("[AUTH SERVICE]: UserAccountService is null - authentication will fail");
            if (m_authenticationService == null)
                m_log.Warn("[AUTH SERVICE]: AuthenticationService is null - authentication will fail");
        }

        /// <summary>
        /// Authenticate user with first name, last name, and password
        /// </summary>
        public UserAccount Authenticate(string firstName, string lastName, string password)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(password))
            {
                m_log.Debug("[AUTH SERVICE]: Authentication failed - empty credentials");
                return null;
            }

            m_log.InfoFormat("[AUTH SERVICE]: Attempting authentication for: {0} {1}", firstName, lastName);
            m_log.DebugFormat("[AUTH SERVICE]: UserAccountService available: {0}", m_userAccountService != null);
            m_log.DebugFormat("[AUTH SERVICE]: AuthenticationService available: {0}", m_authenticationService != null);

            try
            {
                // Check if services are available
                if (m_userAccountService == null)
                {
                    m_log.Error("[AUTH SERVICE]: UserAccountService is null - cannot authenticate");
                    return null;
                }

                // Get user account from database
                UserAccount account = m_userAccountService.GetUserAccount(UUID.Zero, firstName, lastName);
                
                if (account == null)
                {
                    m_log.InfoFormat("[AUTH SERVICE]: Authentication failed - user not found: {0} {1}", firstName, lastName);
                    return null;
                }

                m_log.InfoFormat("[AUTH SERVICE]: User account found: {0} {1} (UUID: {2}, Level: {3})", 
                    firstName, lastName, account.PrincipalID, account.UserLevel);

                // Verify password using OpenSim authentication service
                if (m_authenticationService != null)
                {
                    m_log.DebugFormat("[AUTH SERVICE]: Using OpenSim AuthenticationService for password verification");
                    string token = m_authenticationService.Authenticate(account.PrincipalID, password, 30);
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        m_log.InfoFormat("[AUTH SERVICE]: User authenticated successfully: {0} {1} (UUID: {2})", 
                            firstName, lastName, account.PrincipalID);
                        return account;
                    }
                    else
                    {
                        m_log.InfoFormat("[AUTH SERVICE]: Authentication returned empty token - invalid password");
                    }
                }
                else
                {
                    // Fallback: Simple password hash comparison (not recommended for production)
                    m_log.Warn("[AUTH SERVICE]: AuthenticationService unavailable - using fallback authentication");
                    if (VerifyPasswordFallback(account, password))
                    {
                        m_log.InfoFormat("[AUTH SERVICE]: User authenticated (fallback): {0} {1}", firstName, lastName);
                        return account;
                    }
                }

                m_log.InfoFormat("[AUTH SERVICE]: Authentication failed - invalid password for: {0} {1}", firstName, lastName);
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[AUTH SERVICE]: Exception during authentication: {0}", ex);
                m_log.ErrorFormat("[AUTH SERVICE]: Stack trace: {0}", ex.StackTrace);
            }

            return null;
        }

        /// <summary>
        /// Authenticate user by email and password
        /// </summary>
        public UserAccount AuthenticateByEmail(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                m_log.Debug("[AUTH SERVICE]: Authentication failed - empty credentials");
                return null;
            }

            try
            {
                // Get user account by email
                UserAccount account = m_userAccountService?.GetUserAccount(UUID.Zero, email);
                
                if (account == null)
                {
                    m_log.InfoFormat("[AUTH SERVICE]: Authentication failed - user not found with email: {0}", email);
                    return null;
                }

                // Verify password
                if (m_authenticationService != null)
                {
                    string token = m_authenticationService.Authenticate(account.PrincipalID, password, 30);
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        m_log.InfoFormat("[AUTH SERVICE]: User authenticated by email: {0} {1}", 
                            account.FirstName, account.LastName);
                        return account;
                    }
                }

                m_log.InfoFormat("[AUTH SERVICE]: Authentication failed - invalid password for email: {0}", email);
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[AUTH SERVICE]: Exception during email authentication: {0}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Get user account by UUID
        /// </summary>
        public UserAccount GetUserAccount(UUID userId)
        {
            try
            {
                return m_userAccountService?.GetUserAccount(UUID.Zero, userId);
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[AUTH SERVICE]: Exception getting user account: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Verify if user exists
        /// </summary>
        public bool UserExists(string firstName, string lastName)
        {
            try
            {
                UserAccount account = m_userAccountService?.GetUserAccount(UUID.Zero, firstName, lastName);
                return account != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verify if email is already registered
        /// </summary>
        public bool EmailExists(string email)
        {
            try
            {
                UserAccount account = m_userAccountService?.GetUserAccount(UUID.Zero, email);
                return account != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create new user account (registration)
        /// </summary>
        public UserAccount CreateAccount(string firstName, string lastName, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                m_log.Debug("[AUTH SERVICE]: Account creation failed - empty fields");
                return null;
            }

            try
            {
                // Check if user already exists
                if (UserExists(firstName, lastName))
                {
                    m_log.InfoFormat("[AUTH SERVICE]: Account creation failed - user already exists: {0} {1}", firstName, lastName);
                    return null;
                }

                if (EmailExists(email))
                {
                    m_log.InfoFormat("[AUTH SERVICE]: Account creation failed - email already registered: {0}", email);
                    return null;
                }

                // Create new account
                UUID userId = UUID.Random();
                UserAccount account = new UserAccount(UUID.Zero, userId, firstName, lastName, email);
                account.ServiceURLs = new System.Collections.Generic.Dictionary<string, object>();
                account.UserLevel = 0;
                account.UserFlags = 0;
                account.UserTitle = string.Empty;
                account.Created = Util.UnixTimeSinceEpoch();

                // Store account
                if (m_userAccountService != null && m_userAccountService.StoreUserAccount(account))
                {
                    // Set password
                    if (m_authenticationService != null)
                    {
                        m_authenticationService.SetPassword(userId, password);
                    }

                    m_log.InfoFormat("[AUTH SERVICE]: Created new account: {0} {1} (UUID: {2})", 
                        firstName, lastName, userId);
                    return account;
                }
                else
                {
                    m_log.ErrorFormat("[AUTH SERVICE]: Failed to store user account: {0} {1}", firstName, lastName);
                }
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[AUTH SERVICE]: Exception creating account: {0}", ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Change user password
        /// </summary>
        public bool ChangePassword(UUID userId, string oldPassword, string newPassword)
        {
            try
            {
                // First verify old password
                if (m_authenticationService != null)
                {
                    string token = m_authenticationService.Authenticate(userId, oldPassword, 30);
                    if (string.IsNullOrEmpty(token))
                    {
                        m_log.InfoFormat("[AUTH SERVICE]: Password change failed - invalid old password for user: {0}", userId);
                        return false;
                    }

                    // Set new password
                    if (m_authenticationService.SetPassword(userId, newPassword))
                    {
                        m_log.InfoFormat("[AUTH SERVICE]: Password changed successfully for user: {0}", userId);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                m_log.ErrorFormat("[AUTH SERVICE]: Exception changing password: {0}", ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Fallback password verification (when AuthenticationService is unavailable)
        /// NOT RECOMMENDED FOR PRODUCTION - just for testing
        /// </summary>
        private bool VerifyPasswordFallback(UserAccount account, string password)
        {
            // This is a basic fallback - in production, always use IAuthenticationService
            // OpenSim stores password hashes, this would need proper hash verification
            m_log.Warn("[AUTH SERVICE]: Fallback authentication is not secure - please configure AuthenticationService");
            return false;
        }

        /// <summary>
        /// Hash password using SHA256 (for fallback only)
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

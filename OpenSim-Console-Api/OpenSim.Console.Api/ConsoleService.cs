/*
 * Console Service - Business Logic Layer
 * 
 * Verantwortlichkeiten:
 * - Benutzerauthentifizierung und Session-Management
 * - Befehlsausführung mit Sicherheitschecks
 * - Rate-Limiting
 * - Command Injection Protection
 * - Command History Logging
 */

using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenSim.Console.Api
{
    public class ConsoleService
    {
        private readonly ILog m_log;
        private readonly string m_configFile;
        private readonly OpenSim.Framework.ICommandConsole m_console;
        
        private List<User> m_users;
        private List<Command> m_commands;
        private Dictionary<string, Session> m_sessions;
        private List<CommandHistoryEntry> m_commandHistory;
        private Dictionary<string, RateLimitInfo> m_rateLimits;

        private const int SESSION_TIMEOUT_MINUTES = 60;
        private const int RATE_LIMIT_PER_MINUTE = 60;
        private const int MAX_HISTORY_ENTRIES = 1000;

        public ConsoleService(string configFile, ILog log, OpenSim.Framework.ICommandConsole console)
        {
            m_configFile = configFile;
            m_log = log;
            m_console = console;
            
            m_sessions = new Dictionary<string, Session>();
            m_commandHistory = new List<CommandHistoryEntry>();
            m_rateLimits = new Dictionary<string, RateLimitInfo>();
            
            LoadConfig();
        }

        #region Configuration Management

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(m_configFile))
                {
                    m_log.Warn($"[CONSOLE SERVICE]: Config file not found: {m_configFile}, creating default");
                    CreateDefaultConfigFile();
                }

                string json = File.ReadAllText(m_configFile);
                var config = JsonConvert.DeserializeObject<ConsoleApiConfig>(json);
                
                m_users = config.Users ?? new List<User>();
                m_commands = config.Commands ?? new List<Command>();
                
                int enabledCount = m_commands.Count(c => c.Enabled);
                m_log.Info($"[CONSOLE SERVICE]: Loaded {m_users.Count} users and {m_commands.Count} commands ({enabledCount} enabled)");
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE SERVICE]: Error loading config: {ex.Message}");
                m_users = new List<User>();
                m_commands = new List<Command>();
            }
        }

        private void CreateDefaultConfigFile()
        {
            var defaultConfig = new ConsoleApiConfig
            {
                Users = new List<User>
                {
                    new User
                    {
                        Username = "admin",
                        PasswordHash = HashPassword("admin123"),
                        Role = "Administrator",
                        Enabled = true
                    },
                    new User
                    {
                        Username = "operator",
                        PasswordHash = HashPassword("operator123"),
                        Role = "Operator",
                        Enabled = true
                    }
                },
                Commands = new List<Command>
                {
                    new Command { Name = "help", Enabled = true, Category = "General", Description = "Get help" },
                    new Command { Name = "show users", Enabled = true, Category = "Users", Description = "Show users" },
                    new Command { Name = "show regions", Enabled = true, Category = "Region", Description = "Show regions" },
                    new Command { Name = "show stats", Enabled = true, Category = "Monitoring", Description = "Show statistics" },
                    new Command { Name = "getbalance", Enabled = true, Category = "Money", Description = "Get balance" },
                    new Command { Name = "gettotalsales", Enabled = true, Category = "Money", Description = "Get sales" },
                    new Command { Name = "gettransactions", Enabled = true, Category = "Money", Description = "Get transactions" }
                }
            };

            try
            {
                string json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                File.WriteAllText(m_configFile, json);
                m_log.Info($"[CONSOLE SERVICE]: Created default config file with admin/admin123 and operator/operator123");
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE SERVICE]: Failed to create default config file: {ex.Message}");
            }
        }

        public void ReloadConfig()
        {
            LoadConfig();
        }

        public List<User> GetAuthorizedUsers()
        {
            return m_users;
        }

        #endregion

        #region User Management

        public List<Command> GetAllCommands()
        {
            return m_commands;
        }
        
        public List<string> GetAllowedCommands()
        {
            return m_commands
                .Where(c => c.Enabled)
                .Select(c => c.Name)
                .ToList();
        }

        #endregion

        #region Authentication & Session Management

        public AuthResult Authenticate(string username, string password)
        {
            try
            {
                var user = m_users.FirstOrDefault(u => u.Username == username && u.Enabled);
                
                if (user == null)
                {
                    m_log.Warn($"[CONSOLE SERVICE]: Login attempt for unknown/disabled user: {username}");
                    return new AuthResult { Success = false, Message = "Invalid credentials" };
                }

                string passwordHash = HashPassword(password);
                if (user.PasswordHash != passwordHash)
                {
                    m_log.Warn($"[CONSOLE SERVICE]: Invalid password for user: {username}");
                    return new AuthResult { Success = false, Message = "Invalid credentials" };
                }

                // Create session
                string token = GenerateToken();
                var session = new Session
                {
                    Token = token,
                    Username = username,
                    Role = user.Role,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(SESSION_TIMEOUT_MINUTES)
                };

                m_sessions[token] = session;
                user.LastLogin = DateTime.Now;

                m_log.Info($"[CONSOLE SERVICE]: User '{username}' logged in successfully");

                return new AuthResult 
                { 
                    Success = true, 
                    Token = token,
                    Username = username,
                    Role = user.Role,
                    ExpiresAt = session.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE SERVICE]: Authentication error: {ex.Message}");
                return new AuthResult { Success = false, Message = "Authentication error" };
            }
        }

        public bool ValidateSession(string token)
        {
            if (string.IsNullOrEmpty(token) || !m_sessions.ContainsKey(token))
                return false;

            var session = m_sessions[token];
            
            if (DateTime.Now > session.ExpiresAt)
            {
                m_sessions.Remove(token);
                m_log.Info($"[CONSOLE SERVICE]: Session expired for user '{session.Username}'");
                return false;
            }

            return true;
        }

        public Session GetSession(string token)
        {
            return m_sessions.ContainsKey(token) ? m_sessions[token] : null;
        }

        public List<Session> GetActiveSessions()
        {
            CleanupExpiredSessions();
            return m_sessions.Values.ToList();
        }

        private void CleanupExpiredSessions()
        {
            var expiredTokens = m_sessions.Where(s => DateTime.Now > s.Value.ExpiresAt)
                                         .Select(s => s.Key)
                                         .ToList();

            foreach (var token in expiredTokens)
            {
                m_sessions.Remove(token);
            }
        }

        #endregion

        #region Command Execution

        public CommandResult ExecuteCommand(string token, string command)
        {
            try
            {
                // Validate session
                if (!ValidateSession(token))
                {
                    return new CommandResult 
                    { 
                        Success = false, 
                        Output = "Invalid or expired session" 
                    };
                }

                var session = GetSession(token);

                // Rate limiting
                if (!CheckRateLimit(session.Username))
                {
                    m_log.Warn($"[CONSOLE SERVICE]: Rate limit exceeded for user '{session.Username}'");
                    return new CommandResult 
                    { 
                        Success = false, 
                        Output = "Rate limit exceeded. Please wait before sending more commands." 
                    };
                }

                // Validate and sanitize command
                if (!ValidateCommand(command, out string validationError))
                {
                    m_log.Warn($"[CONSOLE SERVICE]: Invalid command from user '{session.Username}': {command}");
                    LogCommandExecution(session.Username, command, false, validationError);
                    return new CommandResult 
                    { 
                        Success = false, 
                        Output = validationError 
                    };
                }

                // Execute command
                m_log.Info($"[CONSOLE SERVICE]: Executing command from user '{session.Username}': {command}");
                string output = ExecuteConsoleCommand(command);
                
                LogCommandExecution(session.Username, command, true, output);

                return new CommandResult 
                { 
                    Success = true, 
                    Output = output,
                    Command = command,
                    Timestamp = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE SERVICE]: Command execution error: {ex.Message}");
                return new CommandResult 
                { 
                    Success = false, 
                    Output = $"Command execution error: {ex.Message}" 
                };
            }
        }

        private bool ValidateCommand(string command, out string error)
        {
            error = null;

            // Null/Empty check
            if (string.IsNullOrWhiteSpace(command))
            {
                error = "Command cannot be empty";
                return false;
            }

            // Command injection protection - check for dangerous characters
            if (Regex.IsMatch(command, @"[;&|`$(){}[\]<>]"))
            {
                error = "Command contains invalid characters";
                return false;
            }

            // Check if command is in allowed list and enabled
            // Wichtig: Prüfe zuerst längste Matches (z.B. "show users full" vor "show users")
            var commandLower = command.ToLower().Trim();
            var matchingCommand = m_commands
                .Where(c => commandLower.StartsWith(c.Name.ToLower()))
                .OrderByDescending(c => c.Name.Length)
                .FirstOrDefault();

            if (matchingCommand == null)
            {
                // Versuche auch ohne Parameter zu matchen
                string commandBase = command.Split(' ')[0].ToLower();
                error = $"Command '{command}' is not in the commands list";
                return false;
            }
            
            if (!matchingCommand.Enabled)
            {
                error = $"Command '{matchingCommand.Name}' is disabled";
                return false;
            }

            return true;
        }

        private string ExecuteConsoleCommand(string command)
        {
            if (m_console == null)
            {
                return "[ERROR] Console not initialized";
            }

            try
            {
                // Execute command through OpenSim console
                // Note: Console output goes to the actual console, not captured here
                // For web interface, commands need to be parsed and responded to differently
                m_console.RunCommand(command);
                
                // Since we can't easily capture LocalConsole output, return success message
                return $"Command '{command}' executed. Check console for output.";
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE SERVICE]: ExecuteConsoleCommand error: {ex.Message}");
                return $"[ERROR] Command execution failed: {ex.Message}";
            }
        }

        #endregion

        #region Rate Limiting

        private bool CheckRateLimit(string username)
        {
            if (!m_rateLimits.ContainsKey(username))
            {
                m_rateLimits[username] = new RateLimitInfo
                {
                    Username = username,
                    RequestCount = 1,
                    WindowStart = DateTime.Now
                };
                return true;
            }

            var rateLimitInfo = m_rateLimits[username];
            var elapsed = DateTime.Now - rateLimitInfo.WindowStart;

            // Reset window after 1 minute
            if (elapsed.TotalMinutes >= 1)
            {
                rateLimitInfo.RequestCount = 1;
                rateLimitInfo.WindowStart = DateTime.Now;
                return true;
            }

            // Check if limit exceeded
            if (rateLimitInfo.RequestCount >= RATE_LIMIT_PER_MINUTE)
            {
                return false;
            }

            rateLimitInfo.RequestCount++;
            return true;
        }

        #endregion

        #region Command History

        private void LogCommandExecution(string username, string command, bool success, string output)
        {
            var entry = new CommandHistoryEntry
            {
                Username = username,
                Command = command,
                Success = success,
                Output = output,
                Timestamp = DateTime.Now
            };

            m_commandHistory.Add(entry);

            // Keep history limited
            if (m_commandHistory.Count > MAX_HISTORY_ENTRIES)
            {
                m_commandHistory.RemoveAt(0);
            }
        }

        public List<CommandHistoryEntry> GetCommandHistory(int limit = 20)
        {
            return m_commandHistory
                .OrderByDescending(h => h.Timestamp)
                .Take(limit)
                .ToList();
        }

        #endregion

        #region Helper Methods

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

        private string GenerateToken()
        {
            byte[] tokenData = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(tokenData);
            return Convert.ToBase64String(tokenData);
        }

        #endregion
    }

    #region Data Models

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class Session
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Command { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CommandHistoryEntry
    {
        public string Username { get; set; }
        public string Command { get; set; }
        public bool Success { get; set; }
        public string Output { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ConsoleApiConfig
    {
        public List<User> Users { get; set; }
        public List<Command> Commands { get; set; }
    }
    
    public class Command
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    }

    public class RateLimitInfo
    {
        public string Username { get; set; }
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
    }

    #endregion
}

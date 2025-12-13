/*
 * Console API Handlers
 * HTTP REST API Endpunkte für Login, Befehlsausführung, History
 */

using log4net;
using Newtonsoft.Json;
using OpenSim.Framework.Servers.HttpServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSim.Console.Api
{
    // Static File Handler für HTML/CSS/JS
    public class StaticFileHandler : BaseStreamHandler
    {
        private string m_filePath;
        private string m_contentType;
        private static readonly ILog m_log = LogManager.GetLogger(typeof(StaticFileHandler));

        public StaticFileHandler(string path, string filePath, string contentType)
            : base("GET", path)
        {
            m_filePath = filePath;
            m_contentType = contentType;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            try
            {
                if (File.Exists(m_filePath))
                {
                    httpResponse.ContentType = m_contentType;
                    httpResponse.StatusCode = 200;
                    return File.ReadAllBytes(m_filePath);
                }
                
                m_log.Warn($"[CONSOLE HANDLER]: File not found: {m_filePath}");
                httpResponse.StatusCode = 404;
                httpResponse.ContentType = "text/plain";
                return Encoding.UTF8.GetBytes("File not found");
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE HANDLER]: Error serving static file: {ex.Message}");
                httpResponse.StatusCode = 500;
                httpResponse.ContentType = "text/plain";
                return Encoding.UTF8.GetBytes($"Error: {ex.Message}");
            }
        }
    }

    // Main API Handler für alle REST Endpunkte
    public class ConsoleApiHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(ConsoleApiHandler));
        private readonly ConsoleService m_service;

        public ConsoleApiHandler(ConsoleService service, string httpMethod, string path)
            : base(httpMethod, path)
        {
            m_service = service;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = "application/json";
            httpResponse.AddHeader("Access-Control-Allow-Origin", "*");
            httpResponse.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            httpResponse.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");

            try
            {
                // Handle OPTIONS (CORS preflight)
                if (httpRequest.HttpMethod == "OPTIONS")
                {
                    httpResponse.StatusCode = 200;
                    return Encoding.UTF8.GetBytes("");
                }

                string[] segments = path.TrimStart('/').Split('/');
                if (segments.Length < 2) 
                {
                    httpResponse.StatusCode = 400;
                    return ErrorResponse("Invalid API path");
                }

                string endpoint = segments[1].ToLower();

                switch (endpoint)
                {
                    case "login":
                        return HandleLogin(httpRequest, httpResponse);
                    
                    case "execute":
                        return HandleExecute(httpRequest, httpResponse);
                    
                    case "commands":
                        return HandleCommands(httpRequest, httpResponse);
                    
                    case "history":
                        return HandleHistory(httpRequest, httpResponse);
                    
                    case "status":
                        return HandleStatus(httpRequest, httpResponse);
                    
                    default:
                        httpResponse.StatusCode = 404;
                        return ErrorResponse("Unknown endpoint");
                }
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: Error processing request: {ex.Message}");
                m_log.Error($"[CONSOLE API]: Stack trace: {ex.StackTrace}");
                httpResponse.StatusCode = 500;
                return ErrorResponse($"Server error: {ex.Message}");
            }
        }

        private byte[] HandleLogin(IOSHttpRequest request, IOSHttpResponse response)
        {
            try
            {
                if (request.HttpMethod != "POST")
                {
                    response.StatusCode = 405;
                    return ErrorResponse("Method not allowed");
                }

                string body = ReadRequestBody(request);
                if (string.IsNullOrEmpty(body))
                {
                    m_log.Warn("[CONSOLE API]: Empty login request body");
                    response.StatusCode = 400;
                    return ErrorResponse("Empty request body");
                }

                var loginRequest = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);

                if (loginRequest == null || !loginRequest.ContainsKey("username") || !loginRequest.ContainsKey("password"))
                {
                    response.StatusCode = 400;
                    return ErrorResponse("Username and password required");
                }

                var result = m_service.Authenticate(loginRequest["username"], loginRequest["password"]);

                if (result.Success)
                {
                    response.StatusCode = 200;
                    return JsonResponse(new
                    {
                        success = true,
                        token = result.Token,
                        username = result.Username,
                        role = result.Role,
                        expiresAt = result.ExpiresAt.ToString("o")
                    });
                }

                response.StatusCode = 401;
                return ErrorResponse(result.Message ?? "Authentication failed");
            }
            catch (JsonException jsonEx)
            {
                m_log.Error($"[CONSOLE API]: JSON parse error in login: {jsonEx.Message}");
                response.StatusCode = 400;
                return ErrorResponse("Invalid JSON format");
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: Login error: {ex.Message}");
                m_log.Error($"[CONSOLE API]: Stack trace: {ex.StackTrace}");
                response.StatusCode = 500;
                return ErrorResponse("Login failed");
            }
        }

        private byte[] HandleExecute(IOSHttpRequest request, IOSHttpResponse response)
        {
            try
            {
                if (request.HttpMethod != "POST")
                {
                    response.StatusCode = 405;
                    return ErrorResponse("Method not allowed");
                }

                // Get token from Authorization header
                string token = GetAuthToken(request);
                if (string.IsNullOrEmpty(token))
                {
                    response.StatusCode = 401;
                    return ErrorResponse("Authorization token required");
                }

                string body = ReadRequestBody(request);
                var executeRequest = JsonConvert.DeserializeObject<Dictionary<string, string>>(body);

                if (!executeRequest.ContainsKey("command"))
                {
                    response.StatusCode = 400;
                    return ErrorResponse("Command required");
                }

                string command = executeRequest["command"];
                var result = m_service.ExecuteCommand(token, command);

                if (result.Success)
                {
                    response.StatusCode = 200;
                    return JsonResponse(new
                    {
                        success = true,
                        output = result.Output,
                        command = result.Command,
                        timestamp = result.Timestamp.ToString("o")
                    });
                }

                response.StatusCode = 400;
                return ErrorResponse(result.Output);
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: Execute error: {ex.Message}");
                response.StatusCode = 500;
                return ErrorResponse("Command execution failed");
            }
        }

        private byte[] HandleCommands(IOSHttpRequest request, IOSHttpResponse response)
        {
            try
            {
                // Get token from Authorization header
                string token = GetAuthToken(request);
                if (string.IsNullOrEmpty(token))
                {
                    response.StatusCode = 401;
                    return ErrorResponse("Authorization token required");
                }

                if (!m_service.ValidateSession(token))
                {
                    response.StatusCode = 401;
                    return ErrorResponse("Invalid or expired session");
                }

                // Rückgabe der vollen Command-Objekte mit Details
                var commands = m_service.GetAllCommands()
                    .Where(c => c.Enabled)
                    .Select(c => new 
                    {
                        Name = c.Name,
                        Category = c.Category,
                        Description = c.Description,
                        Enabled = c.Enabled
                    })
                    .ToList();

                response.StatusCode = 200;
                return JsonResponse(new
                {
                    success = true,
                    commands = commands
                });
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: Commands error: {ex.Message}");
                response.StatusCode = 500;
                return ErrorResponse("Failed to retrieve commands");
            }
        }

        private byte[] HandleHistory(IOSHttpRequest request, IOSHttpResponse response)
        {
            try
            {
                // Get token from Authorization header
                string token = GetAuthToken(request);
                if (string.IsNullOrEmpty(token))
                {
                    response.StatusCode = 401;
                    return ErrorResponse("Authorization token required");
                }

                if (!m_service.ValidateSession(token))
                {
                    response.StatusCode = 401;
                    return ErrorResponse("Invalid or expired session");
                }

                int limit = 20;
                if (request.QueryString["limit"] != null)
                {
                    int.TryParse(request.QueryString["limit"], out limit);
                }

                var history = m_service.GetCommandHistory(limit);

                response.StatusCode = 200;
                return JsonResponse(new
                {
                    success = true,
                    history = history
                });
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: History error: {ex.Message}");
                response.StatusCode = 500;
                return ErrorResponse("Failed to retrieve history");
            }
        }

        private byte[] HandleStatus(IOSHttpRequest request, IOSHttpResponse response)
        {
            try
            {
                // Get token from Authorization header (optional for status)
                string token = GetAuthToken(request);
                
                bool authenticated = false;
                string username = null;

                if (!string.IsNullOrEmpty(token) && m_service.ValidateSession(token))
                {
                    authenticated = true;
                    var session = m_service.GetSession(token);
                    username = session?.Username;
                }

                response.StatusCode = 200;
                return JsonResponse(new
                {
                    success = true,
                    serverStatus = "running",
                    authenticated = authenticated,
                    username = username,
                    timestamp = DateTime.Now.ToString("o")
                });
            }
            catch (Exception ex)
            {
                m_log.Error($"[CONSOLE API]: Status error: {ex.Message}");
                response.StatusCode = 500;
                return ErrorResponse("Failed to retrieve status");
            }
        }

        #region Helper Methods

        private string GetAuthToken(IOSHttpRequest request)
        {
            string authHeader = request.Headers.Get("Authorization");
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring(7);
            }
            return null;
        }

        private string ReadRequestBody(IOSHttpRequest request)
        {
            using (StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private byte[] JsonResponse(object data)
        {
            string json = JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(json);
        }

        private byte[] ErrorResponse(string message)
        {
            return JsonResponse(new { success = false, error = message });
        }

        #endregion
    }
}

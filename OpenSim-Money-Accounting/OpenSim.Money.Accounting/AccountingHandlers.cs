/*
 * OpenSim Money Accounting API Handlers
 * REST API Endpunkte für Balance, Transactions, Users, Dashboard, Reports, Groups
 */

using log4net;
using System.Text.Json;
using OpenSim.Framework.Servers.HttpServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSim.Money.Accounting
{
    // Static File Handler für HTML/CSS/JS
    public class StaticFileHandler : BaseStreamHandler
    {
        private string m_filePath;
        private string m_contentType;

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
                    return File.ReadAllBytes(m_filePath);
                }
                httpResponse.StatusCode = 404;
                return Encoding.UTF8.GetBytes("File not found");
            }
            catch (Exception ex)
            {
                httpResponse.StatusCode = 500;
                return Encoding.UTF8.GetBytes($"Error: {ex.Message}");
            }
        }
    }

    // Main API Handler für alle REST Endpunkte
    public class AccountingApiHandler : BaseStreamHandler
    {
        private static readonly ILog m_log = LogManager.GetLogger(typeof(AccountingApiHandler));
        private readonly AccountingService m_service;
        private string m_contentType = "application/json";

        public AccountingApiHandler(AccountingService service, string httpMethod, string path)
            : base(httpMethod, path)
        {
            m_service = service;
        }

        protected override byte[] ProcessRequest(string path, Stream request, IOSHttpRequest httpRequest, IOSHttpResponse httpResponse)
        {
            httpResponse.ContentType = m_contentType;
            
            try
            {
                string[] segments = path.TrimStart('/').Split('/');
                if (segments.Length < 2) 
                {
                    httpResponse.StatusCode = 400;
                    return ErrorResponse("Invalid API path");
                }

                string controller = segments[1].ToLower();
                string action = segments.Length > 2 ? segments[2] : "";

                switch (controller)
                {
                    case "balance":
                        return HandleBalance(action, httpRequest, httpResponse);
                    case "transactions":
                        return HandleTransactions(action, httpRequest, httpResponse);
                    case "users":
                        return HandleUsers(action, httpRequest, httpResponse);
                    case "dashboard":
                        return HandleDashboard(action, httpRequest, httpResponse);
                    case "reports":
                        return HandleReports(action, httpRequest, httpResponse);
                    case "groups":
                        return HandleGroups(httpRequest, httpResponse);
                    default:
                        httpResponse.StatusCode = 404;
                        return ErrorResponse("Unknown endpoint");
                }
            }
            catch (Exception ex)
            {
                m_log.Error($"[ACCOUNTING API] Error: {ex}");
                httpResponse.StatusCode = 500;
                return ErrorResponse($"Server error: {ex.Message}");
            }
        }

        private byte[] HandleBalance(string action, IOSHttpRequest request, IOSHttpResponse response)
        {
            if (action == "all")
            {
                // GET /api/balance/all
                var balances = m_service.GetAllBalances();
                return JsonResponse(new { success = true, data = balances });
            }
            else if (!string.IsNullOrEmpty(action))
            {
                // GET /api/balance/{userId}
                var balance = m_service.GetBalance(action);
                if (balance != null)
                    return JsonResponse(new { success = true, data = balance });
                
                response.StatusCode = 404;
                return ErrorResponse("User not found");
            }

            response.StatusCode = 400;
            return ErrorResponse("UserId required");
        }

        private byte[] HandleTransactions(string action, IOSHttpRequest request, IOSHttpResponse response)
        {
            if (request.HttpMethod == "POST")
            {
                // POST /api/transactions - Create new transaction
                string body = new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd();
                var txRequest = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(body);
                
                var result = m_service.CreateTransaction(
                    txRequest["senderId"].GetString(),
                    txRequest["receiverId"].GetString(),
                    txRequest["amount"].GetInt32(),
                    txRequest.ContainsKey("transactionType") ? txRequest["transactionType"].GetInt32() : 2,
                    txRequest.ContainsKey("description") ? txRequest["description"].GetString() : ""
                );

                if (result.Success)
                    return JsonResponse(new { success = true, data = result.Data, message = result.Message });
                
                response.StatusCode = 400;
                return JsonResponse(new { success = false, message = result.Message });
            }
            
            if (action.StartsWith("user/"))
            {
                // GET /api/transactions/user/{userId}
                string userId = action.Substring(5);
                var transactions = m_service.GetUserTransactions(userId);
                return JsonResponse(new { success = true, data = transactions });
            }
            
            // GET /api/transactions
            var allTransactions = m_service.GetAllTransactions();
            return JsonResponse(new { success = true, data = allTransactions });
        }

        private byte[] HandleUsers(string action, IOSHttpRequest request, IOSHttpResponse response)
        {
            if (!string.IsNullOrEmpty(action))
            {
                // GET /api/users/{userId}
                var user = m_service.GetUserInfo(action);
                if (user != null)
                    return JsonResponse(new { success = true, data = user });
                
                response.StatusCode = 404;
                return ErrorResponse("User not found");
            }
            
            // GET /api/users - alle Benutzer
            var users = m_service.GetAllUsers();
            return JsonResponse(new { success = true, data = users });
        }

        private byte[] HandleDashboard(string action, IOSHttpRequest request, IOSHttpResponse response)
        {
            // GET /api/dashboard/stats oder /api/dashboard
            var stats = m_service.GetDashboardStats();
            if (stats != null)
                return JsonResponse(new { success = true, data = stats });
            
            response.StatusCode = 500;
            return ErrorResponse("Failed to load dashboard stats");
        }

        private byte[] HandleReports(string action, IOSHttpRequest request, IOSHttpResponse response)
        {
            // GET /api/reports/financial?startDate=...&endDate=...
            string startDateStr = request.QueryString["startDate"];
            string endDateStr = request.QueryString["endDate"];

            DateTime startDate = string.IsNullOrEmpty(startDateStr) ? DateTime.Now.AddMonths(-1) : DateTime.Parse(startDateStr);
            DateTime endDate = string.IsNullOrEmpty(endDateStr) ? DateTime.Now : DateTime.Parse(endDateStr);

            var report = m_service.GetFinancialReport(startDate, endDate);
            if (report != null)
                return JsonResponse(new { success = true, data = report });
            
            response.StatusCode = 500;
            return ErrorResponse("Failed to generate report");
        }

        private byte[] HandleGroups(IOSHttpRequest request, IOSHttpResponse response)
        {
            // GET /api/groups
            var groups = m_service.GetAllGroups();
            return JsonResponse(new { success = true, data = groups });
        }

        private byte[] JsonResponse(object data)
        {
            string json = JsonSerializer.Serialize(data);
            return Encoding.UTF8.GetBytes(json);
        }

        private byte[] ErrorResponse(string message)
        {
            return JsonResponse(new { success = false, message });
        }
    }
}

/*
 * OpenSim Money Accounting Service
 * Business Logic für MySQL Datenbankzugriffe
 */

using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenSim.Money.Accounting
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }

    public class AccountingService
    {
        private readonly string m_connectionString;
        private readonly ILog m_log;

        public AccountingService(string connectionString, ILog log)
        {
            m_connectionString = connectionString;
            m_log = log;
        }

        public ServiceResult<string> TestConnection()
        {
            try
            {
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    
                    // Test query to get database version
                    using (var command = new MySqlCommand("SELECT VERSION() as version", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string version = reader["version"].ToString();
                            return new ServiceResult<string>
                            {
                                Success = true,
                                Data = version,
                                Message = "Database connection successful"
                            };
                        }
                    }
                    
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Could not retrieve database version"
                    };
                }
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: TestConnection error: " + ex.Message);
                return new ServiceResult<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        public object GetBalance(string userId)
        {
            try
            {
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var query = @"SELECT u.user, COALESCE(ua.avatar, 'Unknown') as name, u.balance
                        FROM balances u LEFT JOIN userinfo ua ON u.user = ua.user WHERE u.user = @UserId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new
                                {
                                    userId = reader["user"].ToString(),
                                    userName = reader["name"].ToString(),
                                    balance = Convert.ToInt32(reader["balance"]),
                                    lastUpdated = DateTime.UtcNow
                                };
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetBalance error: " + ex.Message);
                return null;
            }
        }

        public List<object> GetAllBalances()
        {
            try
            {
                var balances = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var query = @"SELECT u.user, u.balance, COALESCE(ua.avatar, 'Unknown') as name
                        FROM balances u LEFT JOIN userinfo ua ON u.user = ua.user ORDER BY u.balance DESC LIMIT 100";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            balances.Add(new
                            {
                                userId = reader["user"].ToString(),
                                userName = reader["name"].ToString(),
                                balance = Convert.ToInt32(reader["balance"])
                            });
                        }
                    }
                }
                return balances;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetAllBalances error: " + ex.Message);
                return new List<object>();
            }
        }

        public List<object> GetAllTransactions(int limit = 100)
        {
            try
            {
                var transactions = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var sql = @"SELECT t.UUID as TransactionId, t.sender as SenderId,
                        COALESCE(us.avatar, 'System') as SenderName,
                        t.receiver as ReceiverId, COALESCE(ur.avatar, 'Unknown') as ReceiverName,
                        t.amount as Amount, t.type as TransactionType, t.description as Description,
                        t.time as TransactionDate
                        FROM transactions t 
                        LEFT JOIN userinfo us ON t.sender = us.user
                        LEFT JOIN userinfo ur ON t.receiver = ur.user
                        ORDER BY t.time DESC LIMIT @Limit";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Limit", limit);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new
                                {
                                    transactionId = reader["TransactionId"].ToString(),
                                    senderId = reader["SenderId"].ToString(),
                                    senderName = reader["SenderName"].ToString(),
                                    receiverId = reader["ReceiverId"].ToString(),
                                    receiverName = reader["ReceiverName"].ToString(),
                                    amount = Convert.ToInt32(reader["Amount"]),
                                    type = Convert.ToInt32(reader["TransactionType"]),
                                    description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString(),
                                    time = Convert.ToInt64(reader["TransactionDate"])
                                });
                            }
                        }
                    }
                }
                return transactions;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetAllTransactions error: " + ex.Message);
                return new List<object>();
            }
        }

        public List<object> GetUserTransactions(string userId)
        {
            try
            {
                var transactions = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var sql = @"SELECT t.UUID as TransactionId, t.sender as SenderId,
                        COALESCE(us.avatar, 'System') as SenderName,
                        t.receiver as ReceiverId, COALESCE(ur.avatar, 'Unknown') as ReceiverName,
                        t.amount as Amount, t.type as TransactionType, t.description as Description,
                        t.time as TransactionDate
                        FROM transactions t 
                        LEFT JOIN userinfo us ON t.sender = us.user
                        LEFT JOIN userinfo ur ON t.receiver = ur.user
                        WHERE t.sender = @UserId OR t.receiver = @UserId
                        ORDER BY t.time DESC LIMIT 100";

                    using (var command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new
                                {
                                    transactionId = reader["TransactionId"].ToString(),
                                    senderId = reader["SenderId"].ToString(),
                                    senderName = reader["SenderName"].ToString(),
                                    receiverId = reader["ReceiverId"].ToString(),
                                    receiverName = reader["ReceiverName"].ToString(),
                                    amount = Convert.ToInt32(reader["Amount"]),
                                    transactionType = Convert.ToInt32(reader["TransactionType"]),
                                    description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString(),
                                    transactionDate = Convert.ToDateTime(reader["TransactionDate"])
                                });
                            }
                        }
                    }
                }
                return transactions;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetUserTransactions error: " + ex.Message);
                return new List<object>();
            }
        }

        public ServiceResult<object> CreateTransaction(string senderId, string receiverId, int amount, int type, string description)
        {
            try
            {
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check sender balance
                            var checkQuery = "SELECT balance FROM balances WHERE user = @SenderId FOR UPDATE";
                            using (var checkCmd = new MySqlCommand(checkQuery, connection, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@SenderId", senderId);
                                var balance = checkCmd.ExecuteScalar();
                                if (balance == null || Convert.ToInt32(balance) < amount)
                                {
                                    transaction.Rollback();
                                    return new ServiceResult<object> { Success = false, Message = "Insufficient balance" };
                                }
                            }

                            // Update balances
                            var updateSender = "UPDATE balances SET balance = balance - @Amount WHERE user = @SenderId";
                            using (var cmd = new MySqlCommand(updateSender, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Amount", amount);
                                cmd.Parameters.AddWithValue("@SenderId", senderId);
                                cmd.ExecuteNonQuery();
                            }

                            var updateReceiver = @"INSERT INTO balances (user, balance) VALUES (@ReceiverId, @Amount)
                                ON DUPLICATE KEY UPDATE balance = balance + @Amount";
                            using (var cmd = new MySqlCommand(updateReceiver, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                                cmd.Parameters.AddWithValue("@Amount", amount);
                                cmd.ExecuteNonQuery();
                            }

                            // Insert transaction
                            string transactionId = Guid.NewGuid().ToString();
                            var insertTx = @"INSERT INTO transactions (UUID, sender, receiver, amount, type, description, time)
                                VALUES (@TxId, @SenderId, @ReceiverId, @Amount, @Type, @Description, @Time)";
                            using (var cmd = new MySqlCommand(insertTx, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@TxId", transactionId);
                                cmd.Parameters.AddWithValue("@SenderId", senderId);
                                cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                                cmd.Parameters.AddWithValue("@Amount", amount);
                                cmd.Parameters.AddWithValue("@Type", type);
                                cmd.Parameters.AddWithValue("@Description", description);
                                cmd.Parameters.AddWithValue("@Time", DateTime.UtcNow);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return new ServiceResult<object> 
                            { 
                                Success = true, 
                                Message = "Transaction completed", 
                                Data = new { transactionId } 
                            };
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: CreateTransaction error: " + ex.Message);
                return new ServiceResult<object> { Success = false, Message = ex.Message };
            }
        }

        public List<object> GetAllUsers()
        {
            try
            {
                var users = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var query = @"SELECT ua.user as UserId, ua.avatar as UserName,
                        COALESCE(b.balance, 0) as Balance
                        FROM userinfo ua LEFT JOIN balances b ON ua.user = b.user ORDER BY ua.avatar LIMIT 200";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new
                            {
                                userId = reader["UserId"].ToString(),
                                userName = reader["UserName"].ToString(),
                                balance = Convert.ToInt32(reader["Balance"])
                            });
                        }
                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetAllUsers error: " + ex.Message);
                return new List<object>();
            }
        }

        public object GetUserInfo(string userId)
        {
            return GetBalance(userId);
        }

        public object GetDashboardStats()
        {
            try
            {
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    
                    int totalUsers = 0;
                    int activeUsers = 0;
                    decimal totalBalance = 0;
                    int txCountToday = 0;
                    decimal txTotalToday = 0;

                    try
                    {
                        using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM userinfo", connection))
                            totalUsers = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    }
                    catch (Exception ex)
                    {
                        m_log.Warn("[ACCOUNTING SERVICE]: GetDashboardStats - userinfo query failed: " + ex.Message);
                    }

                    try
                    {
                        using (var cmd = new MySqlCommand("SELECT COUNT(DISTINCT sender) FROM transactions WHERE time >= DATE_SUB(NOW(), INTERVAL 30 DAY)", connection))
                            activeUsers = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    }
                    catch (Exception ex)
                    {
                        m_log.Warn("[ACCOUNTING SERVICE]: GetDashboardStats - active users query failed: " + ex.Message);
                    }

                    try
                    {
                        using (var cmd = new MySqlCommand("SELECT COALESCE(SUM(balance), 0) FROM balances", connection))
                            totalBalance = Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
                    }
                    catch (Exception ex)
                    {
                        m_log.Warn("[ACCOUNTING SERVICE]: GetDashboardStats - balance query failed: " + ex.Message);
                    }

                    try
                    {
                        // time ist Unix timestamp (INT) - vergleiche mit Unix timestamp von heute 00:00
                        long todayStart = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();
                        using (var cmd = new MySqlCommand("SELECT COUNT(*), COALESCE(SUM(amount), 0) FROM transactions WHERE time >= @TodayStart", connection))
                        {
                            cmd.Parameters.AddWithValue("@TodayStart", todayStart);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    txCountToday = Convert.ToInt32(reader[0]);
                                    txTotalToday = Convert.ToDecimal(reader[1]);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        m_log.Warn("[ACCOUNTING SERVICE]: GetDashboardStats - today transactions query failed: " + ex.Message);
                    }

                    // Immer ein Objekt zurückgeben, auch wenn einige Werte 0 sind
                    return new
                    {
                        totalUsers,
                        activeUsers,
                        totalBalance,
                        transactionCountToday = txCountToday,
                        totalTransactionsToday = txTotalToday,
                        averageBalance = totalUsers > 0 ? totalBalance / totalUsers : 0
                    };
                }
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetDashboardStats error: " + ex.Message);
                // Gebe ein Default-Objekt zurück statt null
                return new
                {
                    totalUsers = 0,
                    activeUsers = 0,
                    totalBalance = 0m,
                    transactionCountToday = 0,
                    totalTransactionsToday = 0m,
                    averageBalance = 0m,
                    error = ex.Message
                };
            }
        }

        public object GetFinancialReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    // time ist Unix timestamp (INT) - konvertiere DateTime zu Unix timestamp
                    long startUnix = new DateTimeOffset(startDate).ToUnixTimeSeconds();
                    long endUnix = new DateTimeOffset(endDate).ToUnixTimeSeconds();
                    
                    var query = @"SELECT COUNT(*) as TotalTransactions, type, SUM(amount) as TypeTotal
                        FROM transactions WHERE time BETWEEN @StartDate AND @EndDate GROUP BY type";

                    decimal totalIncome = 0;
                    decimal totalExpense = 0;
                    int totalTransactions = 0;

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startUnix);
                        command.Parameters.AddWithValue("@EndDate", endUnix);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                totalTransactions += Convert.ToInt32(reader["TotalTransactions"]);
                                int type = Convert.ToInt32(reader["type"]);
                                decimal typeTotal = Convert.ToDecimal(reader["TypeTotal"]);

                                if (type == 0 || type == 5) totalIncome += typeTotal;
                                else if (type == 1 || type == 4) totalExpense += typeTotal;
                            }
                        }
                    }

                    return new
                    {
                        period = $"{startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                        totalTransactions,
                        totalIncome,
                        totalExpense,
                        netBalance = totalIncome - totalExpense,
                        reportDate = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetFinancialReport error: " + ex.Message);
                return null;
            }
        }

        public List<object> GetAllGroups()
        {
            try
            {
                var groups = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var query = @"SELECT user as GroupId, balance FROM balances 
                        WHERE user LIKE '%-group-%' ORDER BY balance DESC LIMIT 100";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groups.Add(new
                            {
                                groupId = reader["GroupId"].ToString(),
                                groupName = "Group",
                                balance = Convert.ToInt32(reader["balance"]),
                                lastUpdated = DateTime.UtcNow
                            });
                        }
                    }
                }
                return groups;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetAllGroups error: " + ex.Message);
                return new List<object>();
            }
        }

        public List<object> GetAllUsers(int limit = 10)
        {
            try
            {
                var users = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    var query = @"SELECT u.user, u.balance, COALESCE(ua.avatar, 'Unknown') as name
                        FROM balances u LEFT JOIN userinfo ua ON u.user = ua.user 
                        WHERE u.user NOT LIKE '%-group-%'
                        ORDER BY u.balance DESC LIMIT @Limit";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Limit", limit);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new
                                {
                                    userId = reader["user"].ToString(),
                                    userName = reader["name"].ToString(),
                                    balance = Convert.ToInt32(reader["balance"])
                                });
                            }
                        }
                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetAllUsers error: " + ex.Message);
                return new List<object>();
            }
        }

        public List<object> GetGroupStats(int limit = 10)
        {
            try
            {
                var groups = new List<object>();
                using (var connection = new MySqlConnection(m_connectionString))
                {
                    connection.Open();
                    
                    // Suche nach Gruppen: UUIDs die in Transaktionen erscheinen aber nicht in userinfo
                    var query = @"SELECT 
                        b.user as groupId,
                        COALESCE(ui.avatar, SUBSTRING(b.user, 1, 8)) as groupName,
                        b.balance as totalBalance,
                        (SELECT COUNT(DISTINCT sender) FROM transactions WHERE receiver = b.user) as memberCount,
                        b.type as accountType
                        FROM balances b
                        LEFT JOIN userinfo ui ON b.user = ui.user
                        WHERE (ui.user IS NULL OR b.type = 1)
                        ORDER BY b.balance DESC 
                        LIMIT @Limit";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Limit", limit);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                groups.Add(new
                                {
                                    groupId = reader["groupId"].ToString(),
                                    groupName = reader["groupName"].ToString(),
                                    totalBalance = Convert.ToInt32(reader["totalBalance"]),
                                    memberCount = Convert.ToInt32(reader["memberCount"]),
                                    accountType = reader["accountType"] == DBNull.Value ? 0 : Convert.ToInt32(reader["accountType"])
                                });
                            }
                        }
                    }
                }
                return groups;
            }
            catch (Exception ex)
            {
                m_log.Error("[ACCOUNTING SERVICE]: GetGroupStats error: " + ex.Message);
                return new List<object>();
            }
        }
    }
}

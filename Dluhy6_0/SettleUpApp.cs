using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Transactions;
using dluhy6_0.Security;
using System.Text.RegularExpressions;
using dluhy6_0.Models;
using Dluhy6_0;
using System.Data;

namespace dluhy6_0
{
    public class SettleUpApp
    {
        private readonly string _connectionString;

        public SettleUpApp()
        {
            _connectionString = db_Configuration.ConnectionString;
        }
         

        public void CreateTransaction(int giverId, int receiverId, decimal amount)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var command = new SqlCommand("INSERT INTO dbo.Transactions (GID, RID, Amount, Date) VALUES (@giverId, @receiverId, @amount, @date)", connection);

            command.Parameters.AddWithValue("@giverId", giverId);
            command.Parameters.AddWithValue("@receiverId", receiverId);
            command.Parameters.AddWithValue("@amount", amount);
            command.Parameters.AddWithValue("@date", DateTime.Now);

            command.ExecuteNonQuery();
        }

        public List<Transactions> GetTransactionsForUser(int userId)
        {
            var transactions = new List<Transactions>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var command = new SqlCommand("SELECT * FROM dbo.Transactions WHERE GID = @userId OR RID = @userId", connection);

                command.Parameters.AddWithValue("@userId", userId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var transaction = new Transactions()
                    {
                        ID = reader.GetInt32(0),
                        GID = reader.GetInt32(1),
                        RID = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                        Amount = reader.GetDecimal(3),
                        Date = reader.GetDateTime(4)
                    };

                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        public decimal GetBalanceForUser(int userId)
        {
            decimal balance = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand("SELECT SUM(amount) FROM Transactions WHERE RID = @userId", connection);

                command.Parameters.AddWithValue("@userId", userId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read() && !reader.IsDBNull(0))
                    {
                        balance = reader.GetDecimal(0);
                    }
                }
            }

            return balance;
        }

        public double GetBalance(int userId)
        {
            double balance = 0;
            var transactions = GetTransactionsForUser(userId);

            if (transactions.Count == 0)
            {
                return 0;
            }

            foreach (var transaction in transactions)
            {
                if (transaction.GID == userId)
                {
                    balance -= (double)transaction.Amount;
                }
                else if (transaction.RID == userId)
                {
                    balance += (double)transaction.Amount;
                }
            }

            return balance;
        }

        public void SettleUpBalances(int userId1, int userId2)
        {
            decimal balance1 = GetBalanceForUser(userId1);
            decimal balance2 = GetBalanceForUser(userId2);

            decimal difference = balance1 - balance2;

            if (difference < 0)
            {
                (userId2, userId1) = (userId1, userId2);
                difference = -difference;
            }

            CreateTransaction(userId1, userId2, difference);
        }

        public void CreateUser(string username, string password)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("INSERT INTO dbo.Users (username, password) VALUES (@username, @password)", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", PEncryption.HashPassword(password));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
            }
        }

        public User GetUserByUsername(string username)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT * FROM dbo.Users WHERE username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User()
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                username = reader.GetString(reader.GetOrdinal("username")),
                                password = reader.GetString(reader.GetOrdinal("password"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user: {ex.Message}");
            }

            return null;
        }

        public User GetUserById(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT * FROM dbo.Users WHERE ID = @userId", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User()
                            {
                                ID = reader.GetInt32(reader.GetOrdinal("ID")),
                                username = reader.GetString(reader.GetOrdinal("username")),
                                password = reader.GetString(reader.GetOrdinal("password"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user: {ex.Message}");
            }

            return null;
        }

        public string GetUsernameById(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT username FROM dbo.Users WHERE ID = @userId", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString(reader.GetOrdinal("username"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting username: {ex.Message}");
            }

            return null;
        }

        public int GetIdByUsername(string username)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand("SELECT ID FROM dbo.Users WHERE username = @username", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(reader.GetOrdinal("ID"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user ID: {ex.Message}");
            }

            return 0;
        }
        public void UpdateUser(int userId, string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var command = new SqlCommand("UpdateUser", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // ni
                }
            }
        }

        public void DeleteUser(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var command = new SqlCommand("DeleteUser", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Handle exception
                }
            }
        }

        public bool Login(string username, string password)
        {
            bool result = false;
            var user = GetUserByUsername(username);
            if (user != null)
            {
                result = PEncryption.VerifyPassword(password, user.password);
            }
            return result;
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var command = new SqlCommand("GetAllUsers", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var user = new User()
                        {
                            ID = reader.GetInt32(0),
                            username = reader.GetString(1),
                            password = reader.GetString(2)
                        };
                        users.Add(user);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception
                }
            }
            return users;
        }
        public bool UserExists(int userId, string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if user exists by ID
                var idQuery = "SELECT COUNT(*) FROM Users WHERE Id = @UserId";
                using var idCommand = new SqlCommand(idQuery, connection);
                idCommand.Parameters.AddWithValue("@UserId", userId);
                int idCount = (int)idCommand.ExecuteScalar();
                if (idCount > 0)
                {
                    return true;
                }

                // Check if user exists by username
                var usernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using var usernameCommand = new SqlCommand(usernameQuery, connection);
                usernameCommand.Parameters.AddWithValue("@Username", username);
                int usernameCount = (int)usernameCommand.ExecuteScalar();
                if (usernameCount > 0)
                {
                    return true;
                }

                // User does not exist
                return false;
            }
        }
    }
}
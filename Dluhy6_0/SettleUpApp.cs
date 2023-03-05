using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Transactions;
using dluhy6_0.Security;
using System.Text.RegularExpressions;
using dluhy6_0.Models;

namespace dluhy6_0;

public class SettleUpApp
{
    private readonly string _connectionString = @"Data Source=(local);Initial Catalog=dluhy_db;User ID=user;Password=owo;Encrypt=false;Trusted_Connection=True;";

    public void CreateTransaction(int giverId, int receiverId, decimal amount)
    {
        // Create a new transaction and add it to the database
        var transaction = new Transactions()
        {
            GID = giverId,
            RID = receiverId,
            Amount = amount,
            Date = DateTime.Now
        };

        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var command = new SqlCommand("INSERT INTO dbo.Transactions (GID, RID, Amount, Date) VALUES (@giverId, @receiverId, @amount, @date)", connection);
        command.Parameters.AddWithValue("@giverId", transaction.GID);
        command.Parameters.AddWithValue("@receiverId", transaction.RID);
        command.Parameters.AddWithValue("@amount", transaction.Amount);
        command.Parameters.AddWithValue("@date", transaction.Date);
        command.ExecuteNonQuery();
    }//works
    public List<Transactions> GetTransactionsForUser(int userId)
    {
        // Retrieve all transactions for the specified user
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
                    RID = reader.GetInt32(2),
                    Amount = reader.GetDecimal(3),
                    Date = reader.GetDateTime(4)
                };
                transactions.Add(transaction);
            }
        }

        return transactions;
    }//works
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
    public void SettleUpBalances(int userId1, int userId2)
    {
        // Retrieve the balances for the two users
        decimal balance1 = GetBalanceForUser(userId1);
        decimal balance2 = GetBalanceForUser(userId2);

        // Determine the difference in balances
        decimal difference = balance1 - balance2;

        // If the difference is negative, swap the user IDs
        if (difference < 0)
        {
            (userId2, userId1) = (userId1, userId2);
            difference = -difference;
        }

        // Create a new transaction to transfer the difference in balances
        CreateTransaction(userId1, userId2, difference);
    }//works
    public void CreateUser(string username, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {

            connection.Open();
            var command = new SqlCommand("INSERT INTO dbo.Users (username, password) VALUES (@username, @password)", connection);
            var hashedPassword = PEncryption.HashPassword(password);
            command.Parameters.AddWithValue("@username", username);
            if (hashedPassword != "invalid password")
            {
                command.Parameters.AddWithValue("@password", hashedPassword);
                command.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("Invalid password");
            }


        }
    }//works
    public User GetUserByUsername(string username)
    {
        User user = null;
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Users WHERE username = @username", connection);
            command.Parameters.AddWithValue("@username", username);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    user = new User()
                    {
                        ID = reader.GetInt32(0),
                        username = reader.GetString(1),
                        password = reader.GetString(2)
                    };
                }
            }
        }
        return user;
    }//works 
    public User GetUserById(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Users WHERE ID = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User()
                {
                    ID = reader.GetInt32(0),
                    username = reader.GetString(1),
                    password = reader.GetString(2)
                };
            }
        }

        return null;
    }//works
    public string GetUsernameById(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT username FROM dbo.Users WHERE ID = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetString(0);
            }
        }

        return null;
    }//works
    public int GetIdByUsername(string username)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT ID FROM dbo.Users WHERE username = @username", connection);
            command.Parameters.AddWithValue("username", username);
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
        }
        return 0;
    }
    public void UpdateUser(int userId, string username, string password)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("UPDATE dbo.Users SET username = @username, password = @password WHERE ID = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.ExecuteNonQuery();
        }
    }//works

    public void DeleteUser(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.Users WHERE ID = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }
    }//works
    public bool Login(string username, string password)
    {
        {
            var user = GetUserByUsername(username);
            if (user == null)
            {
                return false;
            }
            return PEncryption.VerifyPassword(password, user.password);
        }



    }//works
    public void CreateGroup(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("INSERT INTO dbo.Groups (name) VALUES (@name)", connection);
            command.Parameters.AddWithValue("@name", name);
            command.ExecuteNonQuery();
        }
    }//in progress


    public void AddUserToGroup(int userId, int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("INSERT INTO dbo.GroupUsers (GID, UID) VALUES (@groupId, @userId)", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }
    }//works not implemented
    public List<Group> GetGroupsForUser(int userId)
    {
        var groups = new List<Group>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Groups WHERE ID IN (SELECT GID FROM dbo.GroupUsers WHERE UID = @userId)", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var group = new Group()
                {
                    ID = reader.GetInt32(0),
                    name = reader.GetString(1)
                };
                groups.Add(group);
            }
        }

        return groups;
    }//works not implemented
    public List<User> GetUsersForGroup(int groupId)
    {
        var users = new List<User>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Users WHERE ID IN (SELECT UID FROM dbo.GroupUsers WHERE GID = @groupId)", connection);
            command.Parameters.AddWithValue("@groupId", groupId);

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

        return users;
    }//works not implemented
    public void DeleteGroup(int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.Groups WHERE ID = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.ExecuteNonQuery();
        }
    }//works not implemented
    // Getalluser()
    public List<User> GetAllUsers()
    {
        var users = new List<User>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Users", connection);

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

        return users;
    }//works not implemented
    public bool UserExists(int userId, string username)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Check if user exists by ID
            var idQuery = "SELECT COUNT(*) FROM Users WHERE Id = @UserId";
            using (var idCommand = new SqlCommand(idQuery, connection))
            {
                idCommand.Parameters.AddWithValue("@UserId", userId);
                int idCount = (int)idCommand.ExecuteScalar();
                if (idCount > 0)
                {
                    return true;
                }
            }

            // Check if user exists by username
            var usernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            using (var usernameCommand = new SqlCommand(usernameQuery, connection))
            {
                usernameCommand.Parameters.AddWithValue("@Username", username);
                int usernameCount = (int)usernameCommand.ExecuteScalar();
                if (usernameCount > 0)
                {
                    return true;
                }
            }

            // User does not exist
            return false;
        }
    }




}
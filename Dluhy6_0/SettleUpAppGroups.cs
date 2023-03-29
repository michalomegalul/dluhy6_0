using dluhy6_0;
using Dluhy6_0.Pages;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Transactions;

namespace Dluhy6_0;

public class SettleUpAppGroups
{
    private readonly string _connectionString;

    public SettleUpAppGroups()
    {
        _connectionString = db_Configuration.ConnectionString;
    }
    public void CreateGroup(string name, string description, int UserId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var command = new SqlCommand("INSERT INTO dbo.Groups (Id, Name, Description) VALUES (@Id, @Name, @Description); SELECT SCOPE_IDENTITY();", connection);
            Random rnd = new Random();
            int id = rnd.Next(1, 1000000);
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
            command.ExecuteNonQuery();
            AddUserToGroup(UserId, id);


        }
    }
    public void AddUserToGroup(int userId, int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            var command = new SqlCommand("INSERT INTO dbo.UserGroup (UserId,GroupId) VALUES (@userId , @groupId)", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }
    }
    public bool GroupExists(int groupId, string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Check if group exists by ID
            var idQuery = "SELECT COUNT(*) FROM Groups WHERE Id = @GroupId";
            using (var idCommand = new SqlCommand(idQuery, connection))
            {
                idCommand.Parameters.AddWithValue("@GroupId", groupId);
                int idCount = (int)idCommand.ExecuteScalar();
                if (idCount > 0)
                {
                    return true;
                }
            }

            // Check if group exists by name
            var nameQuery = "SELECT COUNT(*) FROM Groups WHERE Name = @Name";
            using (var nameCommand = new SqlCommand(nameQuery, connection))
            {
                nameCommand.Parameters.AddWithValue("@Name", name);
                int nameCount = (int)nameCommand.ExecuteScalar();
                if (nameCount > 0)
                {
                    return true;
                }
            }

            // Group does not exist
            return false;
        }
    }
    public int GetGroupByName(string groupName)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("SELECT * FROM dbo.Groups WHERE Name = @name", connection))
        {
            command.Parameters.AddWithValue("@name", groupName);
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }
        }
        return -1;
    }
    public bool UserInGroup(int userId, int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Check if user is in group
            var query = "SELECT COUNT(*) FROM UserGroup WHERE UserId = @UserId AND GroupId = @GroupId";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@GroupId", groupId);
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    return true;
                }
            }

            // User is not in group
            return false;
        }
    }
    public void RemoveUserFromGroup(int userId, int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.UserGroup WHERE UserId = @userId AND GroupId = @groupId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.ExecuteNonQuery();
        }
    }
    public void DeleteUserFromAllGroups(int userId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.UserGroup WHERE UserId = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }
    }
    public void DeleteGroupFromAllUsers(int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.UserGroup WHERE GroupId = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.ExecuteNonQuery();
        }
    }
    public List<Group> GetGroupsForUser(int userId)
    {
        var groups = new List<Group>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Groups WHERE ID IN (SELECT GroupId FROM dbo.UserGroup WHERE UserId = @userId)", connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var group = new Group()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                groups.Add(group);
            }
        }

        return groups;
    }
    public List<User> GetUsersForGroup(int groupId)
    {
        var users = new List<User>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Users WHERE ID IN (SELECT UserId FROM dbo.UserGroup WHERE GroupId = @groupId)", connection);
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
    }
    public void DeleteGroup(int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.Groups WHERE ID = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.ExecuteNonQuery();
        }
    }
    public void UpdateGroup(int groupId, string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("UPDATE dbo.Groups SET name = @name WHERE ID = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.Parameters.AddWithValue("@name", name);
            command.ExecuteNonQuery();
        }
    }
    public void CreateGroupTransaction(int giverId, int groupId, decimal amount)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var command = new SqlCommand("INSERT INTO dbo.Transactions (GID,RID,Amount,Date,GroupId) VALUES (@GID, @RID, @amount, @date,@GroupId)", connection);

        command.Parameters.AddWithValue("@GID", giverId);
        command.Parameters.AddWithValue("@RID", DBNull.Value);
        command.Parameters.AddWithValue("@GroupId", groupId);
        command.Parameters.AddWithValue("@amount", amount);
        command.Parameters.AddWithValue("@date", DateTime.Now);

        command.ExecuteNonQuery();
    }
    public void PayGroupDebt(int giverId, int groupId, decimal amount)
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var command = new SqlCommand("INSERT INTO dbo.Transactions (GID,RID,Amount,Date,GroupId) VALUES (@GID, @RID, @amount, @date,@GroupId)", connection);

        command.Parameters.AddWithValue("@GID", giverId);
        command.Parameters.AddWithValue("@RID", null);
        command.Parameters.AddWithValue("@GroupId", groupId);
        command.Parameters.AddWithValue("@amount", -amount);
        command.Parameters.AddWithValue("@date", DateTime.Now);

        command.ExecuteNonQuery();
    }
    public List<Transactions> GetDebtsForUser(int userId)
    {
        var debts = new List<Transactions>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Get the user's debts as a giver
            var giverQuery = @"SELECT GroupId, RID, Amount, Date FROM dbo.Transactions
                           WHERE GroupId = @userId";
            using (var giverCommand = new SqlCommand(giverQuery, connection))
            {
                giverCommand.Parameters.AddWithValue("@userId", userId);

                using (var giverReader = giverCommand.ExecuteReader())
                {
                    while (giverReader.Read())
                    {
                        var transaction = new Transactions()
                        {
                            GID = giverReader.GetInt32(0),
                            RID = giverReader.GetInt32(1),
                            Amount = giverReader.GetDecimal(2),
                            Date = giverReader.GetDateTime(3)
                        };

                        debts.Add(transaction);
                    }
                }
            }

            // Get the user's debts as a receiver
            var receiverQuery = @"SELECT GroupId, RID, -Amount, Date FROM dbo.Transactions
                              WHERE RID = @userId";
            using (var receiverCommand = new SqlCommand(receiverQuery, connection))
            {
                receiverCommand.Parameters.AddWithValue("@userId", userId);

                using (var receiverReader = receiverCommand.ExecuteReader())
                {
                    while (receiverReader.Read())
                    {
                        var transaction = new Transactions()
                        {
                            GID = receiverReader.GetInt32(0),
                            RID = receiverReader.GetInt32(1),
                            Amount = receiverReader.GetDecimal(2),
                            Date = receiverReader.GetDateTime(3)
                        };

                        debts.Add(transaction);
                    }
                }
            }
        }

        return debts;
    }




}

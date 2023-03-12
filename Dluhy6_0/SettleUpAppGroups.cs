using dluhy6_0;
using System.Data.SqlClient;
namespace Dluhy6_0;

public class SettleUpAppGroups
{
    private readonly string _connectionString;
    public SettleUpAppGroups(string connectionString)
    {
        _connectionString = connectionString;
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
    public bool UserInGroup(int userId, int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Check if user is in group
            var query = "SELECT COUNT(*) FROM GroupUsers WHERE UID = @UserId AND GID = @GroupId";
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
            var command = new SqlCommand("DELETE FROM dbo.GroupUsers WHERE UID = @userId AND GID = @groupId", connection);
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
            var command = new SqlCommand("DELETE FROM dbo.GroupUsers WHERE UID = @userId", connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.ExecuteNonQuery();
        }
    }
    public void DeleteGroupFromAllUsers(int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("DELETE FROM dbo.GroupUsers WHERE GID = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.ExecuteNonQuery();
        }
    }
    public void CreateGroup(string name)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("INSERT INTO dbo.Groups (name) VALUES (@name)", connection);
            command.Parameters.AddWithValue("@name", name);
            command.ExecuteNonQuery();
        }
    }
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
    }
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
    public Group GetGroup(int groupId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var command = new SqlCommand("SELECT * FROM dbo.Groups WHERE ID = @groupId", connection);
            command.Parameters.AddWithValue("@groupId", groupId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var group = new Group()
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                return group;
            }
        }

        return null;
    }


}

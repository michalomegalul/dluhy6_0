namespace Dluhy6_0;

public class db_Configuration
{
    public static string ConnectionString { get; } = @"Server=tcp:mppdb.database.windows.net,1433;Initial Catalog=dluhy_db;Persist Security Info=False;User ID=michal;Password=pRAHA2005;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
}

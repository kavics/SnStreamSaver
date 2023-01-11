using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace SnStreamSaver;

internal interface IDataProvider
{
    Task<bool> ExportFileDataAsync(string snPath, string fsPath, CancellationToken cancel);
    DbInfo GetInfo();
}

internal abstract class DataProvider : IDataProvider
{
    protected readonly string ConnectionString;

    protected DataProvider(IConfiguration config)
    {
        ConnectionString = config.GetConnectionString("sn-repo");
    }

    protected abstract string VersionInfo { get; }

    protected abstract string ExportOneSql { get; }

    public virtual async Task<bool> ExportFileDataAsync(string snPath, string fsPath, CancellationToken cancel)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await using var command = new SqlCommand(ExportOneSql, connection);
        command.Parameters.Add("@Path", SqlDbType.NVarChar, 450).Value = snPath;
        await connection.OpenAsync(cancel);
        await using var dataReader = await command.ExecuteReaderAsync(cancel);
        if (await dataReader.ReadAsync(cancel))
        {
            var parentFsPath = Path.GetDirectoryName(fsPath);
            if (parentFsPath == null) throw new NotSupportedException();
            if (!Directory.Exists(parentFsPath))
                Directory.CreateDirectory(parentFsPath);

            var data = (byte[])dataReader["Stream"];
            await File.WriteAllBytesAsync(fsPath, data, cancel);
            return true; // file was written
        }
        return false; // no file was written
    }

    public DbInfo GetInfo()
    {
        var cnb = new SqlConnectionStringBuilder(ConnectionString);
        return new DbInfo
        {
            Server = cnb.DataSource,
            Database = cnb.InitialCatalog,
            DbVersion = VersionInfo
        };
    }

    /* ================================================================================================================ */

    internal static async Task<IDataProvider?> CreateDataProviderAsync(IConfiguration config, CancellationToken cancel)
    {
        switch (await GetVersionAsync(config, cancel))
        {
            case 1: return new DataProviderV1(config);
            case 2: return new DataProviderV2(config);
            default: return null;
        }
    }

    private static readonly string _getVersionSql =
        @"IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'Files'))
	SELECT 2
ELSE
	SELECT 1
";
    private static async Task<int> GetVersionAsync(IConfiguration config, CancellationToken cancel)
    {
        var connectionString = config.GetConnectionString("sn-repo");

        await using var connection = new SqlConnection(connectionString);
        await using var command = new SqlCommand(_getVersionSql, connection);
        await connection.OpenAsync(cancel);
        var dbResult = await command.ExecuteScalarAsync(cancel);
        if (dbResult == null || dbResult == DBNull.Value)
            return 0;
        return Convert.ToInt32(dbResult);
    }

}
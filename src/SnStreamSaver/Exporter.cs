using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace SnStreamSaver;

internal class Exporter
{
    private readonly string _source;
    private readonly string _target;
    private readonly IConfiguration _config;
    private string _connectionString;

    public Exporter(string source, string target, IConfiguration config)
    {
        _source = source;
        _target = target;
        _config = config;
    }

    public void Run()
    {
        _connectionString = _config.GetConnectionString("sn-repo");
        string[] snPaths = File.ReadAllLines(_source);
        var index = 0;
        foreach (var snPath in snPaths)
        {
            var fsPath = snPath.Substring("/Root/".Length).Replace('/', '\\');
            fsPath = Path.Combine(_target, fsPath);
            var parentFsPath = Path.GetDirectoryName(fsPath);
            if (!Directory.Exists(parentFsPath))
                Directory.CreateDirectory(parentFsPath);
            SaveFileData(snPath, fsPath);
            Console.Write($"  {++index} / {snPaths.Length}    \r");
        }
    }

    private string _sql = @"SELECT
N.NodeId, N.Name, N.Path, V.MajorNumber, V.MinorNumber, V.Status, B.PropertyTypeId,
B.FileNameWithoutExtension, B.Extension, B.Stream
FROM Nodes N
	JOIN Versions V ON V.VersionId = N.LastMajorVersionId
	JOIN BinaryProperties B ON B.VersionId = V.VersionId
WHERE N.Path = @Path";
    private void SaveFileData(string snPath, string fsPath)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(_sql, connection);
        command.Parameters.Add("@Path", SqlDbType.NVarChar, 450).Value = snPath;
        connection.Open();
        using var dataReader = command.ExecuteReader();
        if (dataReader.Read())
        {
            var data = (byte[]) dataReader["Stream"];
            File.WriteAllBytes(fsPath, data);
        }
    }
}
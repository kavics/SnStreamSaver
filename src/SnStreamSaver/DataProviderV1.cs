using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace SnStreamSaver;

internal class DataProviderV1 : DataProvider
{
    public DataProviderV1(IConfiguration config) : base(config) { }

    protected override string VersionInfo => "V1 (streams in the BinaryProperties table)";

    protected override string ExportOneSql => @"SELECT
N.NodeId, N.Name, N.Path, V.MajorNumber, V.MinorNumber, V.Status, B.PropertyTypeId,
B.FileNameWithoutExtension, B.Extension, B.Stream
FROM Nodes N
	JOIN Versions V ON V.VersionId = N.LastMajorVersionId
	JOIN BinaryProperties B ON B.VersionId = V.VersionId
WHERE N.Path = @Path";

    public override async Task<bool> ImportFileDataAsync(string snPath, string fsPath, CancellationToken cancel)
    {
        if (!File.Exists(fsPath))
            return false;

        var rowId = await GetRowIdAsync(snPath, cancel);
        if (rowId == 0)
            return false;

        await using Stream s = new FileStream(fsPath, FileMode.Open);
        using BinaryReader br = new BinaryReader(s);
        byte[] data = br.ReadBytes((Int32)s.Length);

        const string sql = "UPDATE BinaryProperties SET [Stream] = @Data WHERE BinaryPropertyId = @RowId";
        await using SqlConnection con = new SqlConnection(ConnectionString);
        await using SqlCommand cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@RowId", rowId);
        cmd.Parameters.AddWithValue("@Data", data);
        await con.OpenAsync(cancel);
        await cmd.ExecuteNonQueryAsync(cancel);
        con.Close();

        return true;
    }
    private async Task<int> GetRowIdAsync(string snPath, CancellationToken cancel)
    {
        const string sql = @"SELECT B.BinaryPropertyId FROM Nodes N
	JOIN Versions V ON V.VersionId = N.LastMajorVersionId
	JOIN BinaryProperties B ON B.VersionId = V.VersionId
WHERE Path = @Path
";
        await using SqlConnection con = new SqlConnection(ConnectionString);
        await using SqlCommand cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@Path", snPath);
        await con.OpenAsync(cancel);
        var dbResult = await cmd.ExecuteScalarAsync(cancel);
        con.Close();
        if (dbResult == null || dbResult == DBNull.Value)
            return 0;
        return Convert.ToInt32(dbResult);
    }
}
using Microsoft.Extensions.Configuration;

namespace SnStreamSaver;

internal class DataProviderV1 : DataProvider
{
    public DataProviderV1(IConfiguration config) : base(config) { }

    protected override string VersionInfo => "SnDbV1 (streams in the BinaryProperties table)";

    protected override string ExportOneSql => @"SELECT
N.NodeId, N.Name, N.Path, V.MajorNumber, V.MinorNumber, V.Status, B.PropertyTypeId,
B.FileNameWithoutExtension, B.Extension, B.Stream
FROM Nodes N
	JOIN Versions V ON V.VersionId = N.LastMajorVersionId
	JOIN BinaryProperties B ON B.VersionId = V.VersionId
WHERE N.Path = @Path";

}
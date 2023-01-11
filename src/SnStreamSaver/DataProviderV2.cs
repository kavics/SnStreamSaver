using Microsoft.Extensions.Configuration;

namespace SnStreamSaver;

internal class DataProviderV2 : DataProvider
{
    public DataProviderV2(IConfiguration config) : base(config) { }

    protected override string VersionInfo => "SnDbV1 (streams in the Files table)";

    protected override string ExportOneSql => @"SELECT
N.NodeId, N.Name, N.Path, V.MajorNumber, V.MinorNumber, V.Status, B.PropertyTypeId,
F.FileNameWithoutExtension, F.Extension, F.Stream
FROM Nodes N
	JOIN Versions V ON V.VersionId = N.LastMajorVersionId
	JOIN BinaryProperties B ON B.VersionId = V.VersionId
	JOIN Files F ON F.FileId = B.FileId
WHERE N.Path = @Path";

}
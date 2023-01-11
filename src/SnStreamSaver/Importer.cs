namespace SnStreamSaver;

internal class Importer
{
    private readonly string _source;
    private readonly string _target;
    private readonly IDataProvider _dataProvider;

    public Importer(string source, string target, IDataProvider dataProvider)
    {
        _source = source;
        _target = target;
        _dataProvider = dataProvider;
    }

    public async Task RunAsync(CancellationToken cancel)
    {
        Console.WriteLine();
        Console.WriteLine("IMPORTING FILES");
        string[] snPaths = await File.ReadAllLinesAsync(_source, cancel);
        if (snPaths.Length == 0)
        {
            Console.WriteLine("Path-list is empty.");
            return;
        }

        var index = 0;
        var filesWritten = 0;
        foreach (var snPath in snPaths)
        {
            var fsPath = snPath.Substring("/Root/".Length).Replace('/', '\\');
            fsPath = Path.Combine(_target, fsPath);
            var written = await _dataProvider.ImportFileDataAsync(snPath, fsPath, cancel);
            if (written)
                filesWritten++;
            Console.Write($"  Read: {++index}/{snPaths.Length}, Write: {filesWritten}    \r");
        }
        Console.WriteLine($"\r\nOk.");
    }
}
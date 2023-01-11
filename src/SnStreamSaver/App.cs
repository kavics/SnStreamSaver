using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace SnStreamSaver;

internal enum Mode
{
    Export, Import
}

internal class App
{
    private IConfiguration _config;
    private readonly string[] _args;
    private Mode _mode;
    private string _pathListFile;
    private string _fsContainer;

    public App(IConfiguration config, string[] args)
    {
        _config = config;
        _args = args;
    }

    public async Task RunAsync(CancellationToken cancel)
    {
        if (!ParseArgs(_args))
        {
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("<MODE> <SOURCE> <TARGET>");
            Console.WriteLine("Valid cases:");
            Console.WriteLine("EXPORT <SN-PATH-LIST-FILE> <FS-TARGET>");
            Console.WriteLine("IMPORT <SN-PATH-LIST-FILE> <FS-SOURCE>");
            return;
        }


        _pathListFile = Path.GetFullPath(_pathListFile);
        _fsContainer = Path.GetFullPath(_fsContainer);
        if (!File.Exists(_pathListFile))
        {
            Console.WriteLine("ERROR: Path-list file does not exist: " + _pathListFile);
            return;
        }

        if (!Directory.Exists(_fsContainer))
        {
            if (_mode == Mode.Export)
            {
                Directory.CreateDirectory(_fsContainer);
            }
            else
            {
                Console.WriteLine("ERROR: Source container does not exist: " + _fsContainer);
                return;
            }
        }

        Console.WriteLine($"Mode:        {_mode.ToString().ToUpperInvariant()}");
        Console.WriteLine($"Paths:       {_pathListFile}");
        Console.WriteLine($"Files:       {_fsContainer}");

        var dataProvider = await DataProvider.CreateDataProviderAsync(_config, cancel);
        if (dataProvider == null)
        {
            Console.WriteLine("ERROR: Cannot create a data provider.");
            return;
        }

        var dbInfo = dataProvider.GetInfo();
        Console.WriteLine($"Data server: {dbInfo.Server}");
        Console.WriteLine($"Database:    {dbInfo.Database}");
        Console.WriteLine($"DbVersion:   {dbInfo.DbVersion}");

        if (_mode == Mode.Export)
        {
            var exporter = new Exporter(_pathListFile, _fsContainer, dataProvider);
            await exporter.RunAsync(CancellationToken.None);
            return;
        }

        Console.WriteLine();
        Console.WriteLine("ERROR: Mode is not supported in this version.");
    }

    private bool ParseArgs(string[] args)
    {
        // args[0] args[1]   args[2]
        // ------- --------- -------
        //  EXPORT paths.txt c:\export
        //  IMPORT paths.txt c:\import

        if (args.Length < 1)
        {
            Console.WriteLine("Missing mode. Expected EXPORT or IMPORT");
            return false;
        }

        if (!Enum.TryParse<Mode>(args[0], true, out _mode))
        {
            Console.WriteLine("Invalid mode. Expected EXPORT or IMPORT");
            return false;
        }

        if (args.Length < 2)
        {
            Console.WriteLine("Missing path-list file.");
            return false;
        }
        _pathListFile = args[1];

        if (args.Length < 3)
        {
            Console.WriteLine($"Missing container path. Expected a filesystem directory");
            return false;
        }
        _fsContainer = args[2];

        return true;
    }
}
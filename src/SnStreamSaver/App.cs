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
    private string _source;
    private string _target;

    public App(IConfiguration config, string[] args)
    {
        _config = config;
        _args = args;
    }

    public void Run()
    {
        if (!ParseArgs(_args))
        {
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("<MODE> <SOURCE> <TARGET>");
            Console.WriteLine("Valid cases:");
            Console.WriteLine("EXPORT <SN-PATH-LIST-FILE> <FS-TARGET>");
            //Console.WriteLine("IMPORT <FS-SOURCE> <SN-TARGET>");
            return;
        }

        if (_mode == Mode.Export)
        {
            _source = Path.GetFullPath(_source);
            _target = Path.GetFullPath(_target);
            if (!File.Exists(_source))
            {
                Console.WriteLine("ERROR: Path list does not exist: " + _source);
                return;
            }

            if (!Directory.Exists(_target))
                Directory.CreateDirectory(_target);
        }
        //else
        //{
        //    _source = Path.GetFullPath(_source);
        //}

        Console.WriteLine($"Mode:   {_mode.ToString().ToUpperInvariant()}");
        Console.WriteLine($"Source: {_source}");
        Console.WriteLine($"Target: {_target}");

        if (_mode == Mode.Export)
        {
            new Exporter(_source, _target, _config).Run();
            return;
        }

        Console.WriteLine();
        Console.WriteLine("ERROR: Mode is not supported in this version.");
    }

    private bool ParseArgs(string[] args)
    {
        // args[0] args[1]       args[2]
        // ------- ------------- -------------
        // EXPORT  /Root/Content export
        // IMPORT  import        /Root/Content

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
            var expectation = _mode == Mode.Export ? "a repository path" : " a filesystem path";
            Console.WriteLine($"Missing source path. Expected {expectation}");
            return false;
        }
        _source = args[1];

        if (args.Length < 3)
        {
            var expectation = _mode == Mode.Export ? " a filesystem path" : "a repository path";
            Console.WriteLine($"Missing target path. Expected {expectation}");
            return false;
        }
        _target = args[2];

        return true;
    }
}
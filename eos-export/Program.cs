using Eos.Config;
using Eos.Repositories;
using Eos.Services;

namespace Eos;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: eos-export <path-to-eos-project-file>");
            return;
        }
     
        var projectFilename = args[0];
        if (!File.Exists(projectFilename))
        {
            Console.WriteLine("Invalid file path: " + args[0]);
            return;
        }

        EosConfig.Load();
        MasterRepository.Initialize(EosConfig.NwnBasePath);
        MasterRepository.Load();

        MasterRepository.Project.Load(projectFilename);

        var export = new CustomDataExport();
        export.Export(MasterRepository.Project);

        Console.WriteLine("Export successful!");

        MasterRepository.Cleanup();
    }
}
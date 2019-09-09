using MarkInDeDark.GameGrid;
using System;
using System.IO;
using System.Linq;

namespace MarkInDeDark.CmdUtilities
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryPath = args.Length == 0 ? Environment.CurrentDirectory : Path.GetFullPath(args[0]);

            var files = Directory.GetFiles(directoryPath, "*.mark");
            var oldSerializer = new GameGridSerializerVExcel();
            var newSerializer = new GameGridSerializerV2();

            var convertedFilesDirectory = new DirectoryInfo($"{directoryPath}\\v2");
            if (!convertedFilesDirectory.Exists)
                convertedFilesDirectory.Create();

            foreach(var f in files)
            {
                try
                {
                    var oldLevel = oldSerializer.DeserializeLevel(File.ReadAllLines(f).ToList());
                    File.WriteAllText($"{convertedFilesDirectory}//{Path.GetFileName(f)}", newSerializer.SerializeLevel(oldLevel));
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"{Path.GetFileName(f)} error");
                }
            }

            Console.ReadLine();
        }
    }
}

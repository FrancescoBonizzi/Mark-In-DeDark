using System;
using System.Diagnostics;

namespace VeryStupidLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = "Release",
                    FileName = "MarkInDeDark.Windows.exe"
                };
                Process.Start(startInfo);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}

using EnvDTE;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Process = EnvDTE.Process;

namespace DebuggerHelper
{
    internal class Program
    {
        private static string _path;
        private static string _appName;
        private static string _executable;
        private static System.Diagnostics.Process _process;

        private static void Main(string[] args)
        {
            _path = args[0];
            _appName = args[1];
            _executable = args[2];

            CreateBatFile();
            _process = StartProcess();
            AttachProcess();

            Console.WriteLine("Press <ENTER> to exit Debugger");
            Console.ReadKey();
            _process.Kill();
        }

        private static void AttachProcess()
        {
            var localByName = System.Diagnostics.Process.GetProcessesByName(_appName);

            MessageFilter.Register();
            var process = GetProcess(localByName[0].Id);
            if (process != null)
            {
                process.Attach();
                Console.WriteLine($"Attached to {process.Name}");
            }
            MessageFilter.Revoke();
        }

        private static System.Diagnostics.Process StartProcess()
        {
            var proc = System.Diagnostics.Process.Start("start.bat");

            Console.WriteLine("Waiting to load the process...");
            System.Threading.Thread.Sleep(3000);
            return proc;
        }

        private static Process GetProcess(int processId)
        {
            // Visual Studio 2017 (15.0)
            var dte = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.15.0");
            var processes = dte.Debugger.LocalProcesses.OfType<Process>();
            return processes.SingleOrDefault(x => x.ProcessID == processId);
        }

        private static void CreateBatFile()
        {
            using (var writer = new StreamWriter("start.bat"))
            {
                writer.WriteLine($"cd {_path}");
                writer.WriteLine($"{_path}\\{_executable} -c");
            }
        }
    }
}
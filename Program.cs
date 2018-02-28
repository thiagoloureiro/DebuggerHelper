﻿using EnvDTE;
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

        private static void Main(string[] args)
        {
            _path = args[0];
            _appName = args[1];
            _executable = args[2];

            CreateBatFile();
            StartProcess();
            AttachProcess();

            Console.WriteLine("Press <ENTER> to exit Debugger");
            Console.ReadKey();
        }

        private static void AttachProcess()
        {
            var localByName = System.Diagnostics.Process.GetProcessesByName(_appName);

            MessageFilter.Register();
            var process = GetProcess(localByName[0].Id);
            if (process != null)
            {
                process.Attach();
                Console.WriteLine("Attached to {0}", process.Name);
            }
            MessageFilter.Revoke();
        }

        private static void StartProcess()
        {
            System.Diagnostics.Process.Start("start.bat");

            Console.WriteLine("Waiting to load Firefly...");
            System.Threading.Thread.Sleep(3000);
        }

        private static Process GetProcess(int processID)
        {
            // Visual Studio 2017 (15.0)
            var dte = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.15.0");
            var processes = dte.Debugger.LocalProcesses.OfType<Process>();
            return processes.SingleOrDefault(x => x.ProcessID == processID);
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
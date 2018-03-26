using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Process = EnvDTE.Process;

namespace DebuggerHelper
{
    internal class Program
    {
        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        private static string _path;
        private static string _appName;
        private static string _executable;
        private static System.Diagnostics.Process _process;

        private static void Main(string[] args)
        {
            _path = args[0];
            _appName = args[1];
            _executable = args[2];

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
            List<DTE> lstDte = new List<DTE>();
            DTE dte;

            foreach (RunningObject running in GetRunningObjects())
            {
                if (running.name.Contains("VisualStudio.DTE"))
                {
                    lstDte.Add((DTE)running.o);
                }
            }

            // More than 1 Visual Studio Instance Running
            if (lstDte.Count > 1)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Please Select which Visual Studio Instance do you wish to attach: ");

                Console.ForegroundColor = ConsoleColor.Yellow;

                for (int i = 0; i < lstDte.Count; i++)
                {
                    Console.WriteLine("Press -> [" + i + "] " + lstDte[i].Solution.FileName);
                }

                int digit = 0;
                var ret = Console.ReadKey();
                if (char.IsDigit(ret.KeyChar))
                    digit = int.Parse(ret.KeyChar.ToString());

                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("");
                Console.WriteLine("Attaching to the process... please wait");
                dte = lstDte[digit];
            }
            else
            {
                dte = lstDte[0];
            }

            //var dte = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.15.0");
            var processes = dte.Debugger.LocalProcesses.OfType<Process>();
            return processes.SingleOrDefault(x => x.ProcessID == processId);
        }

        private static List<object> GetRunningObjects()
        {
            List<object> res = new List<object>();
            IBindCtx bc;
            CreateBindCtx(0, out bc);
            IRunningObjectTable runningObjectTable;
            bc.GetRunningObjectTable(out runningObjectTable);
            IEnumMoniker monikerEnumerator;
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            IMoniker[] monikers = new IMoniker[1];
            IntPtr numFetched = IntPtr.Zero;
            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                RunningObject running;
                monikers[0].GetDisplayName(bc, null, out running.name);
                runningObjectTable.GetObject(monikers[0], out running.o);

                res.Add(running);
            }

            return res;
        }
    }
}
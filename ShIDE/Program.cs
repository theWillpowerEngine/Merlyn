using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShIDE
{
	static class Program
	{
		internal static string ThingToOpen = null;

        internal class PipeMessage
        {
            internal string FileToOpen;
            internal PipeMessage(string file)
            {
                FileToOpen = file;
            }
        }

        public static bool CheckInstancesFromRunningProcesses()
        {
            Process _currentProcess = Process.GetCurrentProcess();
            Process[] _allProcesses = Process.GetProcessesByName(_currentProcess.ProcessName);

            return (_allProcesses.Length > 1);
        }

        
        [STAThread]
		static void Main(string[] args)
		{
            //var isAnotherShIDERunning = CheckInstancesFromRunningProcesses();
            if (args.Length > 0)
                ThingToOpen = args[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
		}
	}
}

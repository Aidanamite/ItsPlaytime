using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var ind = 0;
            var mode = args[ind++];
            var p = Process.GetProcessById(int.Parse(args[ind++]));
            p.Kill();
            p.WaitForExit();
            if (mode == "r")
            {
                var file = args[ind++];
                File.Delete(file);
                File.Move(file + ".replace", file);
            }
            var work = args[ind++];
            var app = args[ind++];
            var arg = new StringBuilder();
            for (;ind < args.Length;ind++)
            {
                if (arg.Length != 0)
                    arg.Append(' ');
                var q = args[ind].Contains(' ');
                if (q)
                    arg.Append('"');
                arg.Append(args[ind]);
                if (q)
                    arg.Append('"');
            }
            Process.Start(new ProcessStartInfo(app,arg.ToString())
            {
                WorkingDirectory = work
            });
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Automation_Task_Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await MonitorProcess.Start(args);
        }
    }
}




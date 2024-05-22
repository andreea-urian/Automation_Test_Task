using System.Diagnostics;

namespace Automation_Task_Test
{
    public static class MonitorProcess
    {
        public static async Task Start(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Provide this arg: <process name> <max lifetime in minutes> <monitoring frequency in minutes>");
                return;
            }

            string processName = args[0];
            if (!int.TryParse(args[1], out int maxLifetime) || !int.TryParse(args[2], out int monitoringFrequencyMinutes))
            {
                Console.WriteLine("Provide valid numeric values for max lifetime and monitoring frequency.");
                return;
            }
            Console.WriteLine("Press 'q' to quit the monitoring...");

            int monitoringFrequencyMillisecounds = monitoringFrequencyMinutes * 60 * 1000;
            CancellationTokenSource cts = new CancellationTokenSource();
            Task monitorTask = MonitorProcesses(processName, maxLifetime, monitoringFrequencyMillisecounds, cts.Token);


            await WaitForQuitKeyAsync(monitorTask, cts);
        }

        static async Task MonitorProcesses(string processName, int maxLifetime, int monitoringFrequency, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var processes = Process.GetProcessesByName(processName);
                DateTime now = DateTime.Now;
                if (processes.Count() == 0)
                {
                    Log("No process found with this name.");
                }

                foreach (var process in processes)
                {
                    TimeSpan processLifetime = now - process.StartTime;
                    Log($"Process name: {process.ProcessName}, ID: {process.Id}, Physical memory allocated: {process.WorkingSet64}, Process lifetime {processLifetime.TotalMinutes:F3} minutes.");
                    if (processLifetime.TotalMinutes > maxLifetime)
                    {
                        Log($"Killed process {process.ProcessName} (ID: {process.Id}) after {processLifetime.TotalMinutes:F3} minutes.");
                        process.Kill();
                    }
                }

                try
                {
                    await Task.Delay(monitoringFrequency, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        static void Log(string logMessage)
        {
            Console.WriteLine($"{DateTime.Now}: {logMessage}");
            File.AppendAllText("log.txt", $"{DateTime.Now}: {logMessage}" + Environment.NewLine);
        }

        static async Task WaitForQuitKeyAsync(Task monitorTask, CancellationTokenSource cts)
        {
            while (true)
            {
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    cts.Cancel();
                    await monitorTask;
                    break;
                }
                await Task.Delay(100);
            }
        }
    }
}
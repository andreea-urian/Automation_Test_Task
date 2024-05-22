using System.Diagnostics;
using Automation_Task_Test;

namespace Tests_Automation_Task_Test;

public class Tests
{
    private StringWriter _consoleOutput;

    [SetUp]
    public void Setup()
    {
        _consoleOutput = new StringWriter();
        Console.SetOut(_consoleOutput);

    }

    [TearDown]
    public void Teardown()
    {
        _consoleOutput.Dispose();
        Console.SetOut(Console.Out);
    }

    [Test]
    public async Task ShouldTerminateProcessWhenLifetimeExceeded()
    {

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ping",
            Arguments = "localhost -t",
            UseShellExecute = true
        };

        Process process = Process.Start(startInfo)!;
        Assert.That(process, Is.Not.Null, "Failed to start the process.");

        string[] args = new string[] { "ping", "0", "1" };

        Task monitorTask = MonitorProcess.Start(args);

        await Task.Delay(2000);

        string consoleOutput = _consoleOutput.ToString();
        Assert.That(consoleOutput, Does.Contain("Killed process"), "The process was not terminated as expected.");

        process.Refresh();
        Assert.IsTrue(process.HasExited, "The process is still running.");

        process?.Dispose();
        monitorTask.Dispose();
    }

    [Test]
    public async Task ShouldNotTerminateProcessWhenLifetimeNotExceeded()
    {

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ping",
            Arguments = "localhost -t",
            UseShellExecute = true
        };

        Process process = Process.Start(startInfo)!;
        Assert.That(process, Is.Not.Null, "Failed to start the process.");

        string[] args = new string[] { "ping", "1", "1" };

        Task monitorTask = MonitorProcess.Start(args);

        await Task.Delay(2000);

        string consoleOutput = _consoleOutput.ToString();
        Assert.That(consoleOutput, Does.Not.Contain("Killed process"), "The process was terminated unexpectedly.");

        process.Refresh();
        Assert.That(process.HasExited, Is.False, "The process is still running.");

        process?.Dispose();
        monitorTask.Dispose();
    }

    [Test]
    public async Task ShouldLogProcessWhenFound()
    {
        string[] args = new string[] { "ping", "1", "1" };

        Task monitorTask = MonitorProcess.Start(args);

        await Task.Delay(2000);

        string consoleOutput = _consoleOutput.ToString();
        Assert.That(consoleOutput, Does.Contain("No process found with this name."), "No process found message was logged unexpectedly.");

        monitorTask.Dispose();

    }

    [Test]
    public async Task ShouldTerminateProcessWhenLifetimeExceededAfterStart()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "ping",
            Arguments = "localhost -t",
            UseShellExecute = true
        };

        string[] args = new string[] { "ping", "0", "1" };

        Task monitorTask = MonitorProcess.Start(args);

        await Task.Delay(2000);

        string consoleOutput = _consoleOutput.ToString();
        Assert.That(consoleOutput, Does.Contain("No process found with this name."));

        Process process = Process.Start(startInfo)!;
        Assert.That(process, Is.Not.Null, "Failed to start the process.");

        await Task.Delay(1 * 60 * 1000 - 2000);

        consoleOutput = _consoleOutput.ToString();
        Assert.That(consoleOutput, Does.Contain("Killed process"), "The process was not terminated as expected after 1 minute.");

        process.Refresh();
        Assert.That(process.HasExited, Is.True, "The process is still running.");

        process?.Dispose();
        monitorTask.Dispose();
    }

    [TestCase(new string[] { "ping", "c", "1" }, "Provide valid numeric values for max lifetime and monitoring frequency.")]
    [TestCase(new string[] { "ping", "1", "a" }, "Provide valid numeric values for max lifetime and monitoring frequency.")]
    [TestCase(new string[] { "ping", "1" }, "Provide this arg: <process name> <max lifetime in minutes> <monitoring frequency in minutes>")]
    [TestCase(new string[] { "ping" }, "Provide this arg: <process name> <max lifetime in minutes> <monitoring frequency in minutes>")]
    public async Task ShouldDisplayErrorMessageForInvalidArguments(string[] args, string expectedMessage)
    {
        Task monitorTask = MonitorProcess.Start(args);

        await Task.Delay(100);

        string consoleOutput = _consoleOutput.ToString();
        Assert.That(consoleOutput, Does.Contain(expectedMessage), "The expected error message was not found in the console output.");

        if (monitorTask != null)
        {
            await monitorTask;
        }
    }
}

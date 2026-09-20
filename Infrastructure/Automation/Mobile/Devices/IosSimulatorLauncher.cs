using System.Diagnostics;
using System.Runtime.InteropServices;

using Domain.Runtime.Environment.Configuration;

using Infrastructure.Automation.Mobile;

namespace Infrastructure.Automation.Mobile.Devices;

public sealed class IosSimulatorLauncher : IDeviceLauncher
{
    private readonly string _udid;
    private bool _started;

    public IosSimulatorLauncher(AppiumConfig appium)
    {
        _udid = appium.IosSimulatorUdid;
    }

    public async Task EnsureStartedAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new PlatformNotSupportedException(
                "iOS simulators can only be started from macOS through xcrun simctl.");
        }

        if (string.IsNullOrWhiteSpace(_udid))
        {
            throw new InvalidOperationException(
                "No iOS simulator configured. Set Appium:IosSimulatorUdid to the simulator to boot.");
        }

        if (IsBooted(_udid))
        {
            return;
        }

        RunTool("xcrun", $"simctl boot {_udid}");
        _started = true;

        await WaitForBootAsync(_udid);
    }

    public Task ShutdownIfStartedAsync()
    {
        if (_started && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            RunTool("xcrun", $"simctl shutdown {_udid}");
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(ShutdownIfStartedAsync());
    }

    private static bool IsBooted(string udid)
    {
        var output = RunTool("xcrun", "simctl list devices booted");
        return output.Contains(udid, StringComparison.Ordinal);
    }

    private static async Task WaitForBootAsync(string udid)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "xcrun",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("simctl");
        startInfo.ArgumentList.Add("bootstatus");
        startInfo.ArgumentList.Add(udid);
        startInfo.ArgumentList.Add("-b");

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return;
        }

        ProcessStreams.Drain(process);
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            throw new TimeoutException("iOS simulator did not finish booting.");
        }
    }

    private static string RunTool(string toolPath, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = toolPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return string.Empty;
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }
}

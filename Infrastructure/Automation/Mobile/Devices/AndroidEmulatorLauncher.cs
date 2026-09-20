using System.Diagnostics;
using System.Runtime.InteropServices;

using Domain.Runtime.Environment.Configuration;

using Infrastructure.Automation.Mobile;

namespace Infrastructure.Automation.Mobile.Devices;

public sealed class AndroidEmulatorLauncher : IDeviceLauncher
{
    private const int BootTimeoutMs = 180_000;
    private const int PollIntervalMs = 2_000;

    private readonly string _sdkPath;
    private readonly string _avdName;
    private readonly string _adbPath;
    private readonly string _emulatorPath;
    private Process? _emulatorProcess;
    private bool _started;

    public AndroidEmulatorLauncher(AppiumConfig appium)
    {
        _sdkPath = ResolveSdkPath(appium.AndroidSdkPath);
        _avdName = appium.AvdName;
        _adbPath = ResolveToolPath(_sdkPath, "platform-tools", "adb");
        _emulatorPath = ResolveToolPath(_sdkPath, "emulator", "emulator");
    }

    public async Task EnsureStartedAsync()
    {
        if (IsEmulatorRunning(_adbPath))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_avdName))
        {
            throw new InvalidOperationException(
                "No Android AVD configured. Set Appium:AvdName to the AVD to boot.");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _emulatorPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("-avd");
        startInfo.ArgumentList.Add(_avdName);

        _emulatorProcess = Process.Start(startInfo);
        if (_emulatorProcess is not null)
        {
            ProcessStreams.Drain(_emulatorProcess);
        }

        _started = true;

        await WaitForBootAsync(_adbPath);
    }

    public Task ShutdownIfStartedAsync()
    {
        if (_started)
        {
            try
            {
                _emulatorProcess?.Kill(true);
                _emulatorProcess?.WaitForExit();
            }
            catch (InvalidOperationException)
            {
                // Process already exited.
            }
        }

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(ShutdownIfStartedAsync());
    }

    private static string ResolveSdkPath(string configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return configuredPath.TrimEnd('\\', '/');
        }

        var fromEnv = Environment.GetEnvironmentVariable("ANDROID_HOME");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv.TrimEnd('\\', '/');
        }

        return string.Empty;
    }

    private static string ResolveToolPath(string sdkPath, string subDirectory, string toolName)
    {
        var toolFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? $"{toolName}.exe"
            : toolName;

        if (!string.IsNullOrWhiteSpace(sdkPath))
        {
            var fullPath = Path.Combine(sdkPath, subDirectory, toolFile);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return toolName;
    }

    private static bool IsEmulatorRunning(string adbPath)
    {
        var output = RunTool(adbPath, "devices");
        return output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Skip(1)
            .Any(line => line.Contains("emulator-", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task WaitForBootAsync(string adbPath)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(BootTimeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            var bootCompleted = RunTool(adbPath, "shell getprop sys.boot_completed");
            if (bootCompleted.Trim() == "1")
            {
                return;
            }

            await Task.Delay(PollIntervalMs);
        }

        throw new TimeoutException("Android emulator did not finish booting within the timeout.");
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

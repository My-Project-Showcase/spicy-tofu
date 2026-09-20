using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using Domain.Runtime.Environment.Configuration;

namespace Infrastructure.Automation.Mobile;

public sealed class AppiumServerLauncher : IAsyncDisposable
{
    private const int StartupWaitMs = 60_000;
    private const int PollIntervalMs = 1_000;

    private readonly Uri _serverUri;
    private readonly string _executable;
    private readonly HttpClient _httpClient = new();
    private Process? _serverProcess;
    private bool _started;

    public AppiumServerLauncher(AppiumConfig appium)
    {
        _serverUri = new Uri(appium.ServerUrl);
        _executable = appium.AppiumServerExecutable;
    }

    public async Task EnsureStartedAsync()
    {
        if (await IsServerRunningAsync())
        {
            return;
        }

        var startInfo = BuildServerStartInfo();
        _serverProcess = Process.Start(startInfo);
        if (_serverProcess is not null)
        {
            ProcessStreams.Drain(_serverProcess);
        }

        _started = true;

        await WaitForServerAsync();
    }

    public async Task ShutdownIfStartedAsync()
    {
        if (!_started)
        {
            return;
        }

        try
        {
            _serverProcess?.Kill(true);
            _serverProcess?.WaitForExit();
        }
        catch (InvalidOperationException)
        {
            // Server process already exited.
        }
    }

    public ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        return new ValueTask(ShutdownIfStartedAsync());
    }

    private async Task<bool> IsServerRunningAsync()
    {
        using var tcp = new TcpClient();
        try
        {
            await tcp.ConnectAsync(_serverUri.Host, _serverUri.Port);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private ProcessStartInfo BuildServerStartInfo()
    {
        var portArg = _serverUri.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var executableArg = _executable.Contains(' ') ? $"\"{_executable}\"" : _executable;

        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            && NeedsShellOnWindows(_executable))
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c \"{executableArg} --address {_serverUri.Host} --port {portArg}\"";
            return startInfo;
        }

        startInfo.FileName = _executable;
        startInfo.ArgumentList.Add("--address");
        startInfo.ArgumentList.Add(_serverUri.Host);
        startInfo.ArgumentList.Add("--port");
        startInfo.ArgumentList.Add(portArg);
        return startInfo;
    }

    private static bool NeedsShellOnWindows(string executable)
    {
        var extension = Path.GetExtension(executable);
        return extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".bat", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(extension);
    }

    private async Task WaitForServerAsync()
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(StartupWaitMs);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await _httpClient.GetAsync(_serverUri);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Server not ready yet.
            }

            await Task.Delay(PollIntervalMs);
        }

        throw new TimeoutException("Appium server did not become ready within the timeout.");
    }
}

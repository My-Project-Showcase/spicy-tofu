using System.Diagnostics;

namespace Infrastructure.Automation.Mobile;

internal static class ProcessStreams
{
    public static void Drain(Process process)
    {
        if (process.StandardOutput.BaseStream.CanRead)
        {
            _ = process.StandardOutput.BaseStream.CopyToAsync(Stream.Null);
        }

        if (process.StandardError.BaseStream.CanRead)
        {
            _ = process.StandardError.BaseStream.CopyToAsync(Stream.Null);
        }
    }
}

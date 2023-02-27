using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Common.Services.Shell;

public sealed class ShellLauncher : IShellLauncher
{
    private readonly string? _bashPath;
    private readonly string? _powerShellPath;

    public ShellLauncher(string? bashPath = null, string? powerShellPath = null)
    {
        _bashPath = bashPath;
        _powerShellPath = powerShellPath;
    }

    public async Task<Result<string>> RunBashAsync(string command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_bashPath))
        {
            var fault = new Fault("BashIsNotAvailable");
            return Result.Fail<string>(fault);
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            var fault = new Fault("BashIsNotAvailable");
            return Result.Fail<string>(fault);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var fault = new Fault("CommandExecutionAvailableOnlyOnLinux");
            return Result.Fail<string>(fault);
        }

        var escapedArgs = command.Replace("\"", "\\\"");

        return await RunShellCommand(command, $"-c \"{escapedArgs}\"", _bashPath, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Result<string>> RunShellCommand(string command, string arguments, string shellPath, CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shellPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        error = ExtendErrorWithFullInfo(error, command, arguments, shellPath);

        if (string.IsNullOrWhiteSpace(error))
        {
            var fault = Fault.Unknown.SetMessage(error);
            return Result.Fail<string>(fault);
        }

        return Result.Success(output);
    }

    public async Task<Result<string>> RunPowerShellAsync(string command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_powerShellPath))
        {
            var fault = new Fault("PowerShellIsNotAvailable");
            return Result.Fail<string>(fault);
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            var fault = new Fault("PowerShellCommandIsEmpty");
            return Result.Fail<string>(fault);
        }

        var escapedArgs = command.Replace("\"", "\\\"");

        return await RunShellCommand(command, $"& {escapedArgs}", _powerShellPath, cancellationToken).ConfigureAwait(false);
    }

    private static string ExtendErrorWithFullInfo(string error, string command, string arguments, string shellPath)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return error;
        }

        var sb = new StringBuilder(error).AppendLine()
            .Append(" - ShellPath: \"").Append(shellPath).Append("\"").AppendLine()
            .Append(" - Command: \"").Append(command).Append("\"").AppendLine()
            .Append(" - Arguments: \"").Append(arguments).AppendLine();

        return sb.ToString();
    }
}
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Models;
using Zs.Common.Services.Abstractions;

namespace Zs.Common.Services.Shell;

public sealed class ShellLauncher : IShellLauncher
{
    private readonly string _bashPath;
    private readonly string _powerShellPath;

    public ShellLauncher(string bashPath = null, string powerShellPath = null)
    {
        _bashPath = bashPath;
        _powerShellPath = powerShellPath;
    }

    public async Task<ServiceResult<string>> RunBashAsync(string command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_powerShellPath))
            return ServiceResult<string>.Error("Bash is not awailable");

        if (string.IsNullOrWhiteSpace(command))
            return ServiceResult<string>.Error("Bash command can not be empty");

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return ServiceResult<string>.Error("Command execution available only on Linux");

        var escapedArgs = command.Replace("\"", "\\\"");

        return await RunShellCommand(command, $"-c \"{escapedArgs}\"", _bashPath, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<string>> RunPowerShellAsync(string command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_powerShellPath))
            return ServiceResult<string>.Error("PowerShell is not awailable");

        if (string.IsNullOrWhiteSpace(command))
            return ServiceResult<string>.Error("PowerShell command can not be empty");

        var escapedArgs = command.Replace("\"", "\\\"");

        return await RunShellCommand(command, $"& {escapedArgs}", _powerShellPath, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ServiceResult<string>> RunShellCommand(string command, string arguments, string shellPath, CancellationToken cancellationToken)
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
        var output = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
        var error = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        error = ExtendErrorWithFullInfo(error, command, arguments, shellPath);

        return string.IsNullOrWhiteSpace(error)
            ? ServiceResult<string>.Success(output)
            : ServiceResult<string>.Error(error, output);
    }

    private static string ExtendErrorWithFullInfo(string error, string command, string arguments, string shellPath)
    {
        if (string.IsNullOrWhiteSpace(error))
            return error;

        var sb = new StringBuilder(error).AppendLine()
            .Append(" - ShellPath: \"").Append(shellPath).Append("\"").AppendLine()
            .Append(" - Command: \"").Append(command).Append("\"").AppendLine()
            .Append(" - Arguments: \"").Append(arguments).AppendLine();

        return sb.ToString();
    }
}
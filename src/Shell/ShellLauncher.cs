using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Extensions;
using Zs.Common.Models;

namespace Zs.Common.Services.Shell;

public static class ShellLauncher
{
    public static async Task<Result<string>> RunAsync(string shellPath, string command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(shellPath))
        {
            var fault = Faults.ShellNotFound(shellPath);
            return Result.Fail<string>(fault);
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            var fault = new Fault(FaultCodes.CommandMustNotBeEmpty);
            return Result.Fail<string>(fault);
        }

        var arguments = $"-c \"{command.Replace("\"", "\\\"")}\"";

        return await RunShellCommandAsync(shellPath, arguments, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Result<string>> RunShellCommandAsync(string shellPath, string arguments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var process = CreateProcess(shellPath, arguments);

        try
        {
            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            error = ExtendErrorWithFullInfo(error, shellPath, arguments);

            if (!string.IsNullOrWhiteSpace(error))
            {
                var fault = Fault.Unknown.WithMessage(error);
                return Result.Fail<string>(fault);
            }
            return Result.Success(output);
        }
        catch (Exception ex)
        {
            var message = $"{ex.GetType()}{Environment.NewLine}{shellPath} {arguments}";
            return Fault.Unknown.WithMessage(message);
        }
    }

    private static Process CreateProcess(string shellPath, string arguments)
    {
        return new Process
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
    }

    private static string ExtendErrorWithFullInfo(string error, string shellPath, string arguments)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return error;
        }

        var sb = new StringBuilder(error).AppendLine()
            .Append(" - ShellPath: \"").Append(shellPath).Append("\"").AppendLine()
            .Append(" - Arguments: \"").Append(arguments).AppendLine();

        return sb.ToString();
    }
}
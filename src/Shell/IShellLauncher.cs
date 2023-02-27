using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Models;

namespace Zs.Common.Services.Shell;

public interface IShellLauncher
{
    Task<Result<string>> RunBashAsync(string command, CancellationToken cancellationToken = default);
    Task<Result<string>> RunPowerShellAsync(string command, CancellationToken cancellationToken = default);
}
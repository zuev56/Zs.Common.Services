using System.Threading;
using System.Threading.Tasks;
using Zs.Common.Models;

namespace Zs.Common.Services.Abstractions;

public interface IShellLauncher
{
    Task<ServiceResult<string>> RunBashAsync(string command, CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> RunPowerShellAsync(string command, CancellationToken cancellationToken = default);
}

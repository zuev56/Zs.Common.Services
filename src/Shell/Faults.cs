using System.Collections.Generic;
using Zs.Common.Models;

namespace Zs.Common.Services.Shell;

public static class Faults
{
    public static Fault ShellNotFound(string path)
    {
        var context = new Dictionary<string, object>
        {
            [nameof(path)] = path
        };

        return new Fault(FaultCodes.ShellNotFound)
        {
            Context = context
        };
    }
}
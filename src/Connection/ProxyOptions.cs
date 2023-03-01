using System.ComponentModel.DataAnnotations;

namespace Zs.Common.Services.Connection;

public sealed class ProxyOptions
{
    public const string SectionName = "Proxy";

    [Required]
    public string Address { get; set; } = null!;

    public string? UserName { get; set; }

    public string? Password { get; set; }
}
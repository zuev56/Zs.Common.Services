using System;
using System.ComponentModel.DataAnnotations;

namespace Zs.Common.Services.Connection;

public sealed class ConnectionAnalyzerOptions
{
    public const string SectionName = "ConnectionAnalyzer";

    public bool UseProxy { get; set; }

    [Required]
    public string[] Urls { get; set; } = Array.Empty<string>();
}
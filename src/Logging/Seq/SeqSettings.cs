using System.ComponentModel.DataAnnotations;

namespace Zs.Common.Services.Logging.Seq;

public sealed class SeqSettings
{
    public const string SectionName = "Seq";

    [Required]
    public string Url { get; init; } = null!;

    [Required]
    public string Token { get; init; } = null!;
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Models;

namespace Zs.Common.Services.Scheduler;

public sealed class SqlJob : Job<string?>
{
    private readonly string _sqlQuery;
    private readonly QueryResultType _resultType;
    private readonly IDbClient _dbClient;

    // TODO: Try use fluent interface to create instanses
    public SqlJob(
        string sqlQuery,
        QueryResultType resultType,
        IDbClient dbClient,
        TimeSpan period,
        DateTime? startUtcDate = null,
        string? description = null,
        ILogger? logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period;
        _resultType = resultType;
        _sqlQuery = sqlQuery;
        _dbClient = dbClient;
        Description = description;
    }

    protected override async Task<Result<string?>> GetExecutionResult()
    {
        try
        {
            var queryResult = await _dbClient.GetQueryResultAsync(_sqlQuery, _resultType).ConfigureAwait(false);
            LastResult = Result.Success(queryResult);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, $"{nameof(GetExecutionResult)} error");
            var fault = new Fault("SqlJobExecutionError", $"Sql job '{Description}' execution error", Array.Empty<Fault>());
            LastResult = Result.Fail<string?>(fault);
        }
        return LastResult;
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zs.Common.Abstractions;
using Zs.Common.Enums;
using Zs.Common.Models;

namespace Zs.Common.Services.Scheduler;

/// <summary>
/// <see cref="Job"/> based on SQL script
/// </summary>
public sealed class SqlJob : Job<string>
{
    private readonly string _sqlQuery;
    private QueryResultType _resultType;
    private readonly IDbClient _dbClient;

    // TODO: Try use fluent interface to create instanses
    public SqlJob(
        string sqlQuery,
        QueryResultType resultType,
        IDbClient dbClient,
        TimeSpan period,
        DateTime? startUtcDate = null,
        string description = null,
        ILogger logger = null)
        : base(period, startUtcDate, logger)
    {
        Period = period != default ? period : throw new ArgumentException($"{nameof(period)} can't have default value");

        _resultType = resultType;
        _sqlQuery = sqlQuery ?? throw new ArgumentNullException(nameof(sqlQuery));
        _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));
        Description = description;
    }

    protected override async Task<IOperationResult<string>> GetExecutionResult()
    {
        try
        {
            var queryResult = await _dbClient.GetQueryResultAsync(_sqlQuery, _resultType).ConfigureAwait(false);
            LastResult = ServiceResult<string>.Success(queryResult);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"{nameof(GetExecutionResult)} error");
            LastResult = ServiceResult<string>.Error($"Sql job '{Description}' execution error");
        }
        return LastResult;
    }

}

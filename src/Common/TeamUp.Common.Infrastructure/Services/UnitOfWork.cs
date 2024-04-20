using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

using RailwayResult;
using RailwayResult.Errors;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Contracts.Errors;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Processing;

namespace TeamUp.Common.Infrastructure.Services;

internal sealed class UnitOfWork<TDatabaseContext, TModuleId> : IUnitOfWork<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	internal static readonly ConcurrencyError ConcurrencyError = new($"{typeof(TDatabaseContext).Name}Database.Concurrency.Conflict", "Multiple concurrent update requests have occurred.");
	internal static readonly ConflictError UniqueConstraintError = new($"{typeof(TDatabaseContext).Name}Database.Constraints.PropertyConflict", "Unique property conflict has occurred.");
	internal static readonly InternalError UnexpectedError = new($"{typeof(TDatabaseContext).Name}Database.InternalError", "Unexpected error have occurred.");

	private readonly TDatabaseContext _dbContext;
	private readonly DomainEventsDispatcher _domainEventsDispatcher;
	private readonly ILogger<UnitOfWork<TDatabaseContext, TModuleId>> _logger;

	public UnitOfWork(TDatabaseContext dbContext, DomainEventsDispatcher domainEventsDispatcher, ILogger<UnitOfWork<TDatabaseContext, TModuleId>> logger)
	{
		_dbContext = dbContext;
		_domainEventsDispatcher = domainEventsDispatcher;
		_logger = logger;
	}

	public async Task<Result> SaveChangesAsync(CancellationToken ct = default)
	{
		await _domainEventsDispatcher.DispatchDomainEventsAsync(_dbContext, ct);

		try
		{
			await _dbContext.SaveChangesAsync(ct);
			return Result.Success;
		}
		catch (DbUpdateConcurrencyException ex)
		{
			_logger.LogInformation("Database Concurrency Conflict: {msg}", ex.Message);
			return ConcurrencyError;
		}
		catch (DbUpdateException ex)
		{
			_logger.LogError(ex.InnerException, "Database Update Exception");

			if (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
			{
				return UniqueConstraintError;
			}

			return UnexpectedError;
		}
	}
}

using Microsoft.EntityFrameworkCore;

using Quartz;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing.Outbox;

public interface ICleanProcessedOutboxMessagesJob<TModuleId> : IJob where TModuleId : IModuleId;

[DisallowConcurrentExecution]
internal sealed class CleanProcessedOutboxMessagesJob<TDatabaseContext, TModuleId> : ICleanProcessedOutboxMessagesJob<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private readonly TDatabaseContext _dbContext;

	public CleanProcessedOutboxMessagesJob(TDatabaseContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _dbContext.Set<OutboxMessage>()
			.Where(msg => msg.ProcessedUtc != null)
			.ExecuteDeleteAsync(context.CancellationToken);
	}
}

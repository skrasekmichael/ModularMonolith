using Microsoft.EntityFrameworkCore;

using Quartz;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing.Inbox;

public interface ICleanProcessedInboxMessagesJob<TModuleId> : IJob where TModuleId : IModuleId;

[DisallowConcurrentExecution]
internal sealed class CleanProcessedInboxMessagesJob<TDatabaseContext, TModuleId> : ICleanProcessedInboxMessagesJob<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private readonly TDatabaseContext _dbContext;

	public CleanProcessedInboxMessagesJob(TDatabaseContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _dbContext.Set<InboxMessage>()
			.Where(msg => msg.ProcessedUtc != null)
			.ExecuteDeleteAsync(context.CancellationToken);
	}
}

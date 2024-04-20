using Microsoft.EntityFrameworkCore;

using Quartz;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing.Outbox;

public interface IProcessOutboxMessagesJob<TModuleId> : IJob where TModuleId : IModuleId;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxMessagesJob<TDatabaseContext, TModuleId> : IProcessOutboxMessagesJob<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private readonly TDatabaseContext _dbContext;
	private readonly OutboxConsumer _dispatcher;

	public ProcessOutboxMessagesJob(TDatabaseContext dbContext, OutboxConsumer dispatcher)
	{
		_dbContext = dbContext;
		_dispatcher = dispatcher;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _dispatcher.DispatchIntegrationEventsAsync(_dbContext, context.CancellationToken);
		await _dbContext.SaveChangesAsync(context.CancellationToken);
	}
}

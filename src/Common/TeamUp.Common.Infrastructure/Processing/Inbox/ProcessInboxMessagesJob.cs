using Microsoft.EntityFrameworkCore;

using Quartz;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Processing.Inbox;

public interface IProcessInboxMessagesJob<TModuleId> : IJob where TModuleId : IModuleId;

[DisallowConcurrentExecution]
internal sealed class ProcessInboxMessagesJob<TDatabaseContext, TModuleId> : IProcessInboxMessagesJob<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private readonly TDatabaseContext _dbContext;
	private readonly IInboxConsumer _inboxConsumer;

	public ProcessInboxMessagesJob(TDatabaseContext dbContext, IInboxConsumer inboxConsumer)
	{
		_dbContext = dbContext;
		_inboxConsumer = inboxConsumer;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		await _inboxConsumer.DispatchIntegrationEventsAsync(_dbContext, context.CancellationToken);
	}
}

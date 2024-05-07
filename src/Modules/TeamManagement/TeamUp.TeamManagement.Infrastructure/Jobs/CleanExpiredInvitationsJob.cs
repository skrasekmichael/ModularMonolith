using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;

namespace TeamUp.TeamManagement.Infrastructure.Jobs;

public interface ICleanExpiredInvitationsJob : IJob;

[DisallowConcurrentExecution]
internal sealed class CleanExpiredInvitationsJob : ICleanExpiredInvitationsJob
{
	private readonly TeamManagementDbContext _dbContext;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly ILogger<CleanExpiredInvitationsJob> _logger;

	public CleanExpiredInvitationsJob(TeamManagementDbContext dbContext, IDateTimeProvider dateTimeProvider, ILogger<CleanExpiredInvitationsJob> logger)
	{
		_dbContext = dbContext;
		_dateTimeProvider = dateTimeProvider;
		_logger = logger;
	}

	public Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("Cleaning expired invitations.");

		return _dbContext.Invitations
			.Where(Invitation.HasExpiredExpression(_dateTimeProvider.UtcNow))
			.ExecuteDeleteAsync(context.CancellationToken);
	}
}

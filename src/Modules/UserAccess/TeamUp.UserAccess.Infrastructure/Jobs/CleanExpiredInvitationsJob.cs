using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Quartz;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.UserAccess.Infrastructure.Jobs;

public interface ICleanExpiredAccountsJob : IJob;

[DisallowConcurrentExecution]
internal sealed class CleanExpiredAccountsJob : ICleanExpiredAccountsJob
{
	private readonly UserAccessDbContext _dbContext;
	private readonly IDateTimeProvider _dateTimeProvider;
	private readonly IUnitOfWork<UserAccessModuleId> _unitOfWork;
	private readonly ILogger<CleanExpiredAccountsJob> _logger;

	public CleanExpiredAccountsJob(UserAccessDbContext dbContext, IDateTimeProvider dateTimeProvider, IUnitOfWork<UserAccessModuleId> unitOfWork, ILogger<CleanExpiredAccountsJob> logger)
	{
		_dbContext = dbContext;
		_dateTimeProvider = dateTimeProvider;
		_unitOfWork = unitOfWork;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("Cleaning expired accounts.");

		var users = await _dbContext.Users
			.Where(User.AccountHasExpiredExpression(_dateTimeProvider.UtcNow))
			.ToListAsync(context.CancellationToken);

		foreach (var user in users)
		{
			user.Delete();
		}

		await _unitOfWork.SaveChangesAsync(context.CancellationToken);
	}
}

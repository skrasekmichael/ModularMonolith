using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using RailwayResult;
using RailwayResult.Errors;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.Common.Infrastructure.Processing;
using TeamUp.Common.Infrastructure.Services;
namespace TeamUp.Tests.EndToEnd.Mocks;

internal interface ICanCommit;

internal interface IBeforeCommit;

internal sealed class DelayedCommitUnitOfWork<TDatabaseContext, TModuleId> : IUnitOfWork<TModuleId>
	where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
	where TModuleId : IModuleId
{
	private readonly UnitOfWork<TDatabaseContext, TModuleId> _unitOfWork;
	private readonly DelayedCommitUnitOfWorkOptions _options;
	private readonly CallbackWithTimeout _canCommitCallback;
	private readonly CallbackWithTimeout _beforeCommitCallback;

	public DelayedCommitUnitOfWork(
		UnitOfWork<TDatabaseContext, TModuleId> unitOfWork,
		DelayedCommitUnitOfWorkOptions options,
		Owner<TModuleId, ICanCommit, CallbackWithTimeout> canCommitCallback,
		Owner<TModuleId, IBeforeCommit, CallbackWithTimeout> beforeCommitCallback)
	{
		_unitOfWork = unitOfWork;
		_options = options;
		_canCommitCallback = canCommitCallback.Service;
		_beforeCommitCallback = beforeCommitCallback.Service;
	}

	public async Task<Result> SaveChangesAsync(CancellationToken ct = default)
	{
		if (_options.IsDelayRequested)
		{
			_beforeCommitCallback.Invoke();
			if (!await _canCommitCallback.WaitForCallbackAsync())
			{
				return new InternalError("Tests.Error", "Callback Timeout");
			}
		}

		return await _unitOfWork.SaveChangesAsync(ct);
	}
}

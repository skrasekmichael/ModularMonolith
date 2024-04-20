using RailwayResult;

using TeamUp.Common.Contracts;

namespace TeamUp.Common.Application;

public interface IUnitOfWork<TModuleId> where TModuleId : IModuleId
{
	public Task<Result> SaveChangesAsync(CancellationToken ct = default);
}

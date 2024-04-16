using RailwayResult;

namespace TeamUp.Common.Application;

public interface IUnitOfWork
{
	public Task<Result> SaveChangesAsync(CancellationToken ct = default);
}

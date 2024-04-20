using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Contracts;
using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Services;

public interface IDbContextConfigurator
{
	public void Configure<TDatabaseContext, TModuleId>(DbContextOptionsBuilder optionsBuilder)
		where TDatabaseContext : DbContext, IDatabaseContext<TModuleId>
		where TModuleId : IModuleId;
}

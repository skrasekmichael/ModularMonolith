using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Infrastructure.Persistence;

namespace TeamUp.Common.Infrastructure.Services;

public interface IDbContextConfigurator
{
	public void Configure<TDatabaseContext>(DbContextOptionsBuilder optionsBuilder) where TDatabaseContext : DbContext, IDatabaseContext;
}

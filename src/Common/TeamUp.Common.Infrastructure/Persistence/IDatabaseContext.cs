using TeamUp.Common.Contracts;

namespace TeamUp.Common.Infrastructure.Persistence;

public interface IDatabaseContext
{
	public const string MIGRATIONS_HISTORY_TABLE = "__EFMigrationsHistory";
}

public interface IDatabaseContext<TModuleId> : IDatabaseContext where TModuleId : IModuleId;

namespace TeamUp.Common.Infrastructure.Persistence;

public interface IDatabaseContext
{
	public const string MIGRATIONS_HISTORY_TABLE = "__EFMigrationsHistory";
	public static abstract string ModuleName { get; }
}

using Microsoft.EntityFrameworkCore;

using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Domain;
using TeamUp.UserAccess.Infrastructure.Persistence;

namespace TeamUp.UserAccess.Infrastructure.Services;

internal sealed class UserAccessDbQueryContextFacade : IUserAccessQueryContext
{
	private readonly UserAccessDbContext _context;

	public UserAccessDbQueryContextFacade(UserAccessDbContext context)
	{
		_context = context;
	}

	public IQueryable<User> Users => _context.Users.AsNoTracking();
}

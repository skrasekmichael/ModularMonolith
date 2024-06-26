﻿using Microsoft.EntityFrameworkCore;

using TeamUp.Common.Application;
using TeamUp.UserAccess.Application.Abstractions;
using TeamUp.UserAccess.Contracts.GetAccountDetails;
using TeamUp.UserAccess.Domain;

namespace TeamUp.UserAccess.Application;

internal sealed class GetUserDetailsQueryHandler : IQueryHandler<GetUserDetailsQuery, AccountResponse>
{
	private readonly IUserAccessQueryContext _queryContext;

	public GetUserDetailsQueryHandler(IUserAccessQueryContext queryContext)
	{
		_queryContext = queryContext;
	}

	public async Task<Result<AccountResponse>> Handle(GetUserDetailsQuery query, CancellationToken ct)
	{
		var account = await _queryContext.Users
			.Where(user => user.Id == query.UserId)
			.Select(user => new AccountResponse
			{
				Email = user.Email,
				Name = user.Name,
				State = user.State
			})
			.FirstOrDefaultAsync(ct);

		return account.EnsureNotNull(UserErrors.UserNotFound);
	}
}

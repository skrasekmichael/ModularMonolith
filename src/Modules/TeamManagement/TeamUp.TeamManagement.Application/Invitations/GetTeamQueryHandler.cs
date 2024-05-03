using Microsoft.EntityFrameworkCore;

using RailwayResult;

using TeamUp.Common.Application;
using TeamUp.Common.Contracts;
using TeamUp.TeamManagement.Application;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.GetUserTeams;

namespace TeamUp.Application.Teams.GetUserTeams;

internal sealed class GetUserTeamsQueryHandler : IQueryHandler<GetUserTeamsQuery, Collection<TeamSlimResponse>>
{
	private readonly ITeamManagementQueryContext _appQueryContext;

	public GetUserTeamsQueryHandler(ITeamManagementQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<Collection<TeamSlimResponse>>> Handle(GetUserTeamsQuery query, CancellationToken ct)
	{
		var teams = await _appQueryContext.Teams
			.Where(team => team.Members.Any(member => member.UserId == query.InitiatorId))
			.Select(team => new TeamSlimResponse
			{
				TeamId = team.Id,
				Name = team.Name,
			})
			.ToListAsync(ct);

		return new Collection<TeamSlimResponse>(teams);
	}
}

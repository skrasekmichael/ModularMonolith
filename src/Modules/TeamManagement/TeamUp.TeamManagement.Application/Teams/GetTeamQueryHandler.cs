using Microsoft.EntityFrameworkCore;

using RailwayResult;
using RailwayResult.FunctionalExtensions;

using TeamUp.Common.Application;
using TeamUp.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Contracts.Teams.GetTeam;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.TeamManagement.Application.Teams;

internal sealed class GetTeamQueryHandler : IQueryHandler<GetTeamQuery, TeamResponse>
{
	private readonly ITeamManagementQueryContext _appQueryContext;

	public GetTeamQueryHandler(ITeamManagementQueryContext appQueryContext)
	{
		_appQueryContext = appQueryContext;
	}

	public async Task<Result<TeamResponse>> Handle(GetTeamQuery query, CancellationToken ct)
	{
		var team = await _appQueryContext.Teams
			.Where(team => team.Id == query.TeamId)
			.Select(team => new TeamResponse
			{
				Name = team.Name,
				Members = team.Members
					.Select(member => new TeamMemberResponse
					{
						Id = member.Id,
						UserId = member.UserId,
						Nickname = member.Nickname,
						Role = member.Role
					})
					.ToList()
					.AsReadOnly()
			})
			.FirstOrDefaultAsync(ct);

		return team
			.EnsureNotNull(TeamErrors.TeamNotFound)
			.Ensure(team => team.Members.Any(member => member.UserId == query.InitiatorId), TeamErrors.NotMemberOfTeam);
	}
}

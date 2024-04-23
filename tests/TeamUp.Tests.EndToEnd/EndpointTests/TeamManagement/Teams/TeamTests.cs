using TeamUp.TeamManagement.Domain.Aggregates.Teams;

namespace TeamUp.Tests.EndToEnd.EndpointTests.TeamManagement.Teams;

public abstract class TeamTests(AppFixture app) : BaseEndpointTests(app)
{
	protected static bool TeamContainsMemberWithSameRole(Team team, TeamMember member) => member.Role == team.Members.FirstOrDefault(m => m.Id == member.Id)?.Role;
}

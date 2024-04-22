using TeamUp.UserAccess.Domain;

namespace TeamUp.Tests.Common.DataGenerators.TeamManagement;

public static class UserAccessUserToTeamManagementUserExtensions
{
	public static TeamUp.TeamManagement.Domain.Aggregates.Users.User ToUserTM(this User user)
	{
		return new TeamUp.TeamManagement.Domain.Aggregates.Users.User(user.Id, user.Name, user.Email);
	}

	public static List<TeamUp.TeamManagement.Domain.Aggregates.Users.User> ToUsersTM(this IEnumerable<User> users)
	{
		return users.Select(user => user.ToUserTM()).ToList();
	}
}

using RailwayResult.Errors;

namespace TeamUp.TeamManagement.Domain.Aggregates.Users;

public static class UserErrors
{
	public static readonly NotFoundError UserNotFound = new("TeamManagement.Users.NotFound", "User not found.");
}

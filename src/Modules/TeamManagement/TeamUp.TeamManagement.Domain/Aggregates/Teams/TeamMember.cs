using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Teams;

public sealed class TeamMember : Entity<TeamMemberId>
{
	public UserId UserId { get; }

	public TeamId TeamId { get; }
	public Team Team { get; }

	public string Nickname { get; private set; }
	public TeamRole Role { get; private set; }
	public DateTimeOffset TimeStamp { get; }

#pragma warning disable CS8618 // EF Core constructor
	private TeamMember() : base() { }
#pragma warning restore CS8618

	internal TeamMember(
		TeamMemberId id,
		UserId userId,
		Team team,
		string nickname,
		TeamRole role,
		DateTimeOffset timeStamp) : base(id)
	{
		UserId = userId;
		TeamId = team.Id;
		Team = team;
		Nickname = nickname;
		Role = role;
		TimeStamp = timeStamp;
	}

	public override string ToString() => $"{Nickname} ({Role})";

	internal void UpdateNickname(string nickname)
	{
		Nickname = nickname;
	}

	public void UpdateRole(TeamRole role)
	{
		Role = role;
	}
}

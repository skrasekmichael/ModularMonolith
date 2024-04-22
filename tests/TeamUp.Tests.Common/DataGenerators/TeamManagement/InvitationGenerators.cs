global using InvitationGenerator = Bogus.Faker<TeamUp.TeamManagement.Domain.Aggregates.Invitations.Invitation>;

using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;
using TeamUp.Tests.Common.Extensions;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.Tests.Common.DataGenerators.TeamManagement;

public sealed class InvitationGenerators : BaseGenerator
{
	private static readonly PrivateBinder InvitationBinder = new(
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Invitations.Invitation.RecipientId).GetBackingField(),
		nameof(TeamUp.TeamManagement.Domain.Aggregates.Invitations.Invitation.TeamId).GetBackingField()
	);

	public static readonly InvitationGenerator Invitation = new InvitationGenerator(binder: InvitationBinder)
		.UsePrivateConstructor()
		.RuleFor(i => i.Id, f => InvitationId.New());

	public static Invitation GenerateInvitation(UserId userId, TeamId teamId, DateTime createdUtc)
	{
		return Invitation
			.RuleForBackingField(i => i.RecipientId, userId)
			.RuleForBackingField(i => i.TeamId, teamId)
			.RuleFor(i => i.CreatedUtc, createdUtc)
			.Generate();
	}

	public static List<Invitation> GenerateTeamInvitations(TeamId teamId, DateTime createdUtc, List<User> users)
	{
		return users.Select(user =>
			Invitation
				.RuleForBackingField(i => i.RecipientId, user.Id)
				.RuleForBackingField(i => i.TeamId, teamId)
				.RuleFor(i => i.CreatedUtc, createdUtc)
				.Generate()
		).ToList();
	}

	public static List<Invitation> GenerateUserInvitations(UserId userId, DateTime createdUtc, IEnumerable<Team> teams)
	{
		return teams.Select(team =>
			Invitation
				.RuleForBackingField(i => i.RecipientId, userId)
				.RuleForBackingField(i => i.TeamId, team.Id)
				.RuleFor(i => i.CreatedUtc, createdUtc)
				.Generate()
		).ToList();
	}
}

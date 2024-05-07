using System.Linq.Expressions;

using TeamUp.Common.Domain;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.UserAccess.Contracts;

namespace TeamUp.TeamManagement.Domain.Aggregates.Invitations;

public sealed class Invitation : AggregateRoot<Invitation, InvitationId>
{
	internal static readonly TimeSpan InvitationTTL = TimeSpan.FromDays(4);

	public UserId RecipientId { get; }
	public TeamId TeamId { get; }

	public DateTime CreatedUtc { get; init; }

#pragma warning disable CS8618 // EF Core constructor
	private Invitation() : base() { }
#pragma warning restore CS8618

	internal Invitation(InvitationId id, UserId recipientId, TeamId teamId, DateTime createdUtc) : base(id)
	{
		RecipientId = recipientId;
		TeamId = teamId;
		CreatedUtc = createdUtc;
	}

	public bool HasExpired(DateTime utcNow) => utcNow - CreatedUtc >= InvitationTTL;

	internal static Expression<Func<Invitation, bool>> HasExpiredExpression(DateTime utcNow)
	{
		return invitation => utcNow - invitation.CreatedUtc >= InvitationTTL;
	}
}

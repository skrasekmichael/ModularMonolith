using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Contracts.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Invitations;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Invitations;

internal sealed class InvitationConfiguration : BaseEntityConfiguration<Invitation, InvitationId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<Invitation> invitationEntityBuilder)
	{
		invitationEntityBuilder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(invitation => invitation.RecipientId);

		invitationEntityBuilder
			.HasOne<Team>()
			.WithMany()
			.HasForeignKey(invitation => invitation.TeamId);

		invitationEntityBuilder
			.HasIndex(invitation => invitation.RecipientId);

		invitationEntityBuilder
			.HasIndex(invitation => invitation.TeamId);

		invitationEntityBuilder
			.HasIndex(invitation => new { invitation.TeamId, invitation.RecipientId })
			.IsUnique();
	}
}

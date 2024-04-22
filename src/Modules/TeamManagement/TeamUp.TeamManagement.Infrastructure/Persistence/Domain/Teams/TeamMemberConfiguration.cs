using Microsoft.EntityFrameworkCore.Metadata.Builders;

using TeamUp.Common.Infrastructure.Persistence;
using TeamUp.TeamManagement.Contracts.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Teams;
using TeamUp.TeamManagement.Domain.Aggregates.Users;

namespace TeamUp.TeamManagement.Infrastructure.Persistence.Domain.Teams;

internal sealed class TeamMemberConfiguration : BaseEntityConfiguration<TeamMember, TeamMemberId>
{
	protected override void ConfigureEntity(EntityTypeBuilder<TeamMember> teamMemberEntityBuilder)
	{
		teamMemberEntityBuilder
			.HasOne<User>()
			.WithMany()
			.HasForeignKey(teamMember => teamMember.UserId);

		teamMemberEntityBuilder
			.HasOne(teamMember => teamMember.Team)
			.WithMany(team => team.Members)
			.HasForeignKey(teamMember => teamMember.TeamId);

		teamMemberEntityBuilder
			.Property(team => team.Nickname)
			.IsRequired()
			.HasMaxLength(255);

		teamMemberEntityBuilder
			.HasIndex(team => team.TeamId);

		teamMemberEntityBuilder
			.Property<uint>("RowVersion")
			.IsRowVersion();
	}
}

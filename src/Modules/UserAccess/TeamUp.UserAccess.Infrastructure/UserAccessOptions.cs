using System.ComponentModel.DataAnnotations;

using TeamUp.Common.Infrastructure.Options;

namespace TeamUp.UserAccess.Infrastructure;

internal sealed class UserAccessOptions : IAppOptions
{
	public static string SectionName => "Modules:UserAccess";

	[Required]
	public required HashingOptions Hashing { get; init; }

	public class HashingOptions
	{
		[Required]
		public required int Pbkdf2Iterations { get; set; }

		[Required]
		public required string Pepper { get; set; }
	}
}

using Bogus;

using TeamUp.Tests.Common.Extensions;
using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;
using TeamUp.UserAccess.Domain;

using Xunit;

namespace TeamUp.Tests.Common.DataGenerators;

public sealed class UserGenerators : BaseGenerator
{
	public static readonly Faker<RegisterUserRequest> ValidRegisterUserRequest = new Faker<RegisterUserRequest>()
		.RuleFor(r => r.Email, f => f.Internet.Email())
		.RuleFor(r => r.Name, f => f.Internet.UserName())
		.RuleFor(r => r.Password, GenerateValidPassword());

	public static readonly Faker<User> User = new Faker<User>()
		.UsePrivateConstructor()
		.RuleFor(u => u.Id, f => UserId.New())
		.RuleFor(u => u.Email, f => f.Internet.Email())
		.RuleFor(u => u.Name, f => f.Internet.UserName())
		.RuleFor(u => u.State, UserState.Activated)
		.RuleFor(u => u.Password, new Password());

	public static readonly Faker<User> DistinctUser = new Faker<User>()
		.UsePrivateConstructor()
		.RuleFor(u => u.Id, f => UserId.New())
		.RuleFor(u => u.Email, f => f.Internet.Email(uniqueSuffix: Guid.NewGuid().ToString()))
		.RuleFor(u => u.Name, f => f.Internet.UserName())
		.RuleFor(u => u.Password, new Password())
		.RuleFor(u => u.State, UserState.Activated);

	public static string GenerateValidPassword() => F.Internet.Password(10);

	public sealed class InvalidRegisterUserRequests : TheoryData<InvalidRequest<RegisterUserRequest>>
	{
		public InvalidRegisterUserRequests()
		{
			this.Add(x => x.Email, new RegisterUserRequest
			{
				Email = "invalid email",
				Name = F.Internet.UserName(),
				Password = GenerateValidPassword()
			});

			this.Add(x => x.Name, new RegisterUserRequest
			{
				Email = F.Internet.Email(),
				Name = "xx",
				Password = GenerateValidPassword()
			});

			this.Add(x => x.Name, new RegisterUserRequest
			{
				Email = F.Internet.Email(),
				Name = F.Random.AlphaNumeric(Constants.USERNAME_MAX_SIZE + 1),
				Password = GenerateValidPassword()
			});

			this.Add(x => x.Password, new RegisterUserRequest
			{
				Email = F.Internet.Email(),
				Name = F.Internet.UserName(),
				Password = ""
			});
		}
	}
}

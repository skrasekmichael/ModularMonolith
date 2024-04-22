using Bogus;

using TeamUp.UserAccess.Contracts;
using TeamUp.UserAccess.Domain;

namespace TeamUp.Tests.Common.DataGenerators.UserAccess;

public static class UserGeneratorExtensions
{
	public static Faker<User> WithStatus(this Faker<User> userGenerator, UserState state) => userGenerator.RuleFor(u => u.State, state);

	public static Faker<User> WithPassword(this Faker<User> userGenerator, Password password) => userGenerator.RuleFor(u => u.Password, password);
}

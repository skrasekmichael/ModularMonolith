using Bogus;

using TeamUp.Tests.Common.Extensions;

namespace TeamUp.Tests.Common.DataGenerators;

public abstract class BaseGenerator
{
	protected static Faker F => FakerExtensions.F;
}

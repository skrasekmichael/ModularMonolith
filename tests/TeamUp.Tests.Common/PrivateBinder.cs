using System.Reflection;

namespace TeamUp.Tests.Common;

public sealed class PrivateBinder : Bogus.Binder
{
	private readonly string[] _fields;

	public PrivateBinder(params string[] fields)
	{
		_fields = fields;
	}

	public override Dictionary<string, MemberInfo> GetMembers(Type type)
	{
		var members = base.GetMembers(type);

		var privateFields = type
			.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
			.Where(field => _fields.Contains(field.Name));

		foreach (var field in privateFields)
		{
			members.Add(field.Name, field);
		}

		return members;
	}
}

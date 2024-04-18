using MassTransit;

using TeamUp.Common.Application;
using TeamUp.Notifications.Contracts;
using TeamUp.UserAccess.Contracts.CreateUser;

namespace TeamUp.UserAccess.Application;

internal sealed class UserCreatedEventHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
	private readonly IRequestClient<SendEmailCommand> _client;

	public UserCreatedEventHandler(IRequestClient<SendEmailCommand> client)
	{
		_client = client;
	}

	public async Task<Result> Handle(UserCreatedIntegrationEvent integrationEvent, CancellationToken ct)
	{
		var command = new SendEmailCommand
		{
			Email = integrationEvent.Email,
			Subject = "Activation Email",
			Message = "Activate your account!"
		};

		var response = await _client.GetResponse<Result>(command, ct);
		return response.Message;
	}
}

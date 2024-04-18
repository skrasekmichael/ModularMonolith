using TeamUp.Bootstrapper;
using TeamUp.Bootstrapper.Middlewares;
using TeamUp.Common.Endpoints;
using TeamUp.Common.Infrastructure;
using TeamUp.Common.Infrastructure.Extensions;
using TeamUp.Notifications.Infrastructure;
using TeamUp.TeamManagement.Infrastructure;
using TeamUp.UserAccess.Endpoints;
using TeamUp.UserAccess.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "TEAMUP_");

builder.Services
	.AddHttpContextAccessor()
	.AddEndpointsApiExplorer()
	.AddRestApiVersioning()
	.AddSwagger()
	.AddSecurity();

var modules = builder.Services.AddInfrastructure(config =>
{
	config.AddModule<UserAccessModule>();
	config.AddModule<TeamManagementModule>();
	config.AddModule<NotificationsModule>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	await modules.MigrateAsync(app.Services);

	app.UseSwagger();
	app.UseSwaggerUI();

	app.UseMiddleware<RequestLoggingMiddleware>();
	app.UseMiddleware<ResponseLoggingMiddleware>();
}
else
{
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/_health");
app.MapEndpoints(group =>
{
	group.MapEndpointGroup<UserAccessEndpointGroup>();
	group.MapEndpointGroup<TeamManagementEndpointGroup>();
});

app.Run();

public sealed partial class Program;

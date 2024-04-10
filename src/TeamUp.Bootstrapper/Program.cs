using TeamUp.Bootstrapper;
using TeamUp.Bootstrapper.Middlewares;
using TeamUp.Common.Endpoints;
using TeamUp.UserAccess.Endpoints;
using TeamUp.UserAccess.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "TEAMUP_");

builder.Services
	.AddHttpContextAccessor()
	.AddEndpointsApiExplorer()
	.AddVersioning()
	.AddSwagger()
	.AddModule<UserAccessModule>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
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
	group.MapEndpointGroup<UserAccessEndpointGroup>("users");
});

app.Run();

public sealed partial class Program;

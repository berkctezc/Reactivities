using API.Extensions;
using API.Middleware;
using API.SignalR;
using Application.Activities;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API;

public class Startup(IConfiguration config)
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers(opt =>
			{
				var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
				opt.Filters.Add(new AuthorizeFilter(policy));
			})
			.AddFluentValidation(config => { config.RegisterValidatorsFromAssemblyContaining<Create>(); });
		services.AddApplicationServices(config)
			.AddIdentityServices(config);
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseMiddleware<ExceptionMiddleware>();

		if (env.IsDevelopment())
			app.UseSwagger()
				.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));

		// app.UseHttpsRedirection();

		app.UseAuthorization()
			.UseRouting()
			.UseCors("CorsPolicy")
			.UseAuthentication();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
			endpoints.MapHub<ChatHub>("/chat");
		});
	}
}
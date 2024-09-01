using Application.Activities;
using Application.Core;
using Application.Interfaces;
using Infrastructure.Photos;
using Infrastructure.Security;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Persistence;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
	{
		services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "API", Version = "v1"}); })
			.AddDbContext<DataContext>(opt => { opt.UseSqlite(config.GetConnectionString("DefaultConnection")); })
			.AddCors(opt =>
			{
				opt.AddPolicy("CorsPolicy", policy =>
				{
					policy
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials()
						.WithOrigins("http://localhost:3000");
				});
			});
		services.AddMediatR(typeof(List.Handler).Assembly)
			.AddAutoMapper(typeof(MappingProfiles).Assembly)
			.AddScoped<IUserAccessor, UserAccessor>()
			.AddScoped<IPhotoAccessor, PhotoAccessor>()
			.Configure<CloudinarySettings>(config.GetSection("Cloudinary"))
			.AddSignalR();

		return services;
	}
}
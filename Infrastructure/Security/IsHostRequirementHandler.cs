using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Security;

public class IsHostRequirementHandler(
	DataContext dbContext,
	IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<IsHostRequirement>
{
	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsHostRequirement requirement)
	{
		var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (userId == null) return Task.CompletedTask;

		var activityId = Guid.Parse(httpContextAccessor.HttpContext?.Request.RouteValues
			.SingleOrDefault(x => x.Key == "id").Value?.ToString());

		var attendee = dbContext.ActivityAttendees
			.AsNoTracking()
			.SingleOrDefaultAsync(x => x.AppUserId == userId && x.ActivityId == activityId)
			.Result;

		if (attendee == null) return Task.CompletedTask;

		if (attendee.IsHost) context.Succeed(requirement);

		return Task.CompletedTask;
	}
}
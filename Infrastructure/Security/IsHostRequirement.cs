using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Security;

public abstract class IsHostRequirement : IAuthorizationRequirement
{
}
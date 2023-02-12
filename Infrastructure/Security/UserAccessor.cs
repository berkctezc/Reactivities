﻿namespace Infrastructure.Security;

public class UserAccessor : IUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string GetUsername()
        => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
}
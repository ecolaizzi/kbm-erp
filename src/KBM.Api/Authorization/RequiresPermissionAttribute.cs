using KBM.Application.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KBM.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequiresPermissionAttribute(string permission) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserContext>();
        var permissions = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        if (!user.IsAuthenticated || user.UserId is null || user.CompanyId is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!await permissions.HasPermissionAsync(user.UserId.Value, user.CompanyId.Value, permission))
            context.Result = new ForbidResult();
    }
}

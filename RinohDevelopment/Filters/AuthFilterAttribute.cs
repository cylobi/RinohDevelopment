using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RinohDevelopment.Services;

namespace RinohDevelopment.Filters;

public class AuthFilterAttribute : TypeFilterAttribute
{
    public bool RequireAdmin { get; set; }

    public AuthFilterAttribute(bool requireAdmin = false) : base(typeof(AuthFilterImpl))
    {
        Arguments = new object[] { requireAdmin };
    }

    private class AuthFilterImpl : IAsyncActionFilter
    {
        private readonly IAuthService _authService;
        private readonly bool _requireAdmin;

        public AuthFilterImpl(IAuthService authService, bool requireAdmin)
        {
            _authService = authService;
            _requireAdmin = requireAdmin;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _authService.GetCurrentUserAsync(context.HttpContext);
            if (user == null)
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    context.Result = controller.RedirectToAction("Login", "Account");
                }
                return;
            }

            if (_requireAdmin && ! _authService.IsAdmin(user))
            {
                var controller = context.Controller as Controller;
                if (controller != null)
                {
                    context.Result = controller.RedirectToAction("AccessDenied", "Account");
                }
                return;
            }

            context.HttpContext.Items["CurrentUser"] = user;
            await next();
        }
    }
}
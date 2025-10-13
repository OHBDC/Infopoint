using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using InfoPoint.Data;

namespace InfoPoint.Authorization
{
    /// <summary>
    /// Authorization attribute to restrict access to HR management features.
    /// Checks if the current user's email is in the HRAuthorizedUsers table.
    /// </summary>
    public class HRAuthorizeAttribute : TypeFilterAttribute
    {
        public HRAuthorizeAttribute() : base(typeof(HRAuthorizeFilter))
        {
        }
    }

    public class HRAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _context;

        public HRAuthorizeFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // Get user's email from claims
            var email = user.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                context.Result = new ForbidResult();
                return;
            }

            // Check if user is in HRAuthorizedUsers table
            var isAuthorized = await _context.HRAuthorizedUsers
                .AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

            if (!isAuthorized)
            {
                context.Result = new ViewResult
                {
                    ViewName = "Unauthorized",
                    StatusCode = 403
                };
                return;
            }
        }
    }
}

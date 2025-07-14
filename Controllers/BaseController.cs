using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    public class BaseController : Controller
    {
        protected int? UserId => HttpContext.Session.GetInt32("UserId");
        protected string? Username => HttpContext.Session.GetString("Username");
        protected UserRole? UserRole => (UserRole?)HttpContext.Session.GetInt32("UserRole");

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (UserId == null)
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }
            
            ViewBag.Username = Username;
            ViewBag.UserRole = UserRole;
            base.OnActionExecuting(context);
        }

        protected bool CheckRole(UserRole requiredRole)
        {
            return UserRole == requiredRole;
        }

        protected bool CheckRoles(params UserRole[] requiredRoles)
        {
            return UserRole.HasValue && requiredRoles.Contains(UserRole.Value);
        }
    }
} 
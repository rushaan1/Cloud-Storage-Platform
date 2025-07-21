using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Cloud_Storage_Platform.Filters
{
    public class IdentifyUser : IAsyncResourceFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserIdentification _userIdentification;
        public IdentifyUser(UserManager<ApplicationUser> userManager, UserIdentification userIdentification) 
        {
            _userManager = userManager;
            _userIdentification = userIdentification;
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            string? uid = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(uid) && Guid.TryParse(uid, out var userId)) 
            {
                ApplicationUser? user = await _userManager.FindByIdAsync(userId.ToString());
                if (user != null) 
                {
                    _userIdentification.User = user;
                }
            }
            
            await next();
        }
    }
}

using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.DTO.AuthDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.ServiceContracts
{
    public interface IJwtService
    {
        AuthenticationResponse CreateJwtToken(ApplicationUser user);
        ClaimsPrincipal? GetClaimsPrincipalFromJwtToken(string? token);
    }
}

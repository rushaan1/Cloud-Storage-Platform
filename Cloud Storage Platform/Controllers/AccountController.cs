using Castle.Core.Internal;
using CloudStoragePlatform.Core.Domain.Entities;
using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.Domain.RepositoryContracts;
using CloudStoragePlatform.Core.DTO.AuthDTO;
using CloudStoragePlatform.Core.ServiceContracts;
using CloudStoragePlatform.Infrastructure.DbContext;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cloud_Storage_Platform.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IUserSessionsRepository _userSessionsRepository;
        private readonly IConfiguration _config;
        private readonly IBulkRetrievalService _retrievalService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService, IUserSessionsRepository userSessionsRepository, IConfiguration configuration, IBulkRetrievalService bulkRetrievalService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _userSessionsRepository = userSessionsRepository;
            _config = configuration;
            _retrievalService = bulkRetrievalService;
        }

        private void SetCookie(string name, string value, DateTimeOffset? expires, bool shouldExpire, bool httponly)
        {
            var options = new CookieOptions()
            {
                HttpOnly = httponly,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires
            };
            if (!shouldExpire) { options.Expires = null; }
            Response.Cookies.Append(name, value, options);
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApplicationUser>> PostRegister(RegisterDTO registerDTO)
        {
            if (ModelState.IsValid == false)
            {
                string errorMsg = string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMsg);
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                PersonName = registerDTO.PersonName,
                Country = registerDTO.Country
            };

            if (!string.IsNullOrEmpty(registerDTO.PhoneNumber)) 
            {
                user.PhoneNumber = registerDTO.PhoneNumber;
            }

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: registerDTO.RememberMe);

                AuthenticationResponse authenticationResponse = await ProcessAfterLogin(user.Email, registerDTO.RememberMe, true, user);

                return Ok(new { PersonName = authenticationResponse.PersonName, Email = authenticationResponse.Email });
            }
            else
            {
                string errorMsg = string.Join("|", result.Errors.Select(e => e.Description));
                return Problem(errorMsg);
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string email)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Ok(true);
            }
            else
            {
                return Ok(false);
            }
        }

        private async Task<AuthenticationResponse> ProcessAfterLogin(string email, bool rememberMe, bool isRegisteredNow, ApplicationUser? user = null) 
        {
            if (user == null) 
            {
                user = await _userManager.FindByEmailAsync(email);
            }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user!);

            if (!isRegisteredNow) 
            {
                // Remove expired sessions
                var sessions = await _userSessionsRepository.GetByUserIdAsync(user.Id);
                var expiredSessions = sessions.Where(s => s.RefreshTokenExpirationDateTime < DateTime.UtcNow).ToList();
                foreach (var expired in expiredSessions)
                {
                    await _userSessionsRepository.RemoveSessionAsync(expired);
                }
            }

            var session = new UserSession()
            {
                RefreshToken = authenticationResponse!.RefreshToken!,
                RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime,
                ApplicationUserId = user.Id,
                User = user
            };
            await _userSessionsRepository.AddSessionAsync(session);
            await _userSessionsRepository.SaveChangesAsync();

            SetCookie("access_token", authenticationResponse.Token!, authenticationResponse.Expiration, rememberMe, true);
            SetCookie("refresh_token", authenticationResponse.RefreshToken!, authenticationResponse.RefreshTokenExpirationDateTime, rememberMe, true);
            SetCookie("jwt_expiry", new DateTimeOffset(authenticationResponse.Expiration).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, rememberMe, false);
            SetCookie("refresh_expiry", new DateTimeOffset(authenticationResponse.RefreshTokenExpirationDateTime).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, rememberMe, false);
            return authenticationResponse;
        }

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginDTO loginDTO)
        {
            if (ModelState.IsValid == false)
            {
                string errorMsg = string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMsg);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: loginDTO.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                AuthenticationResponse authenticationResponse = await ProcessAfterLogin(loginDTO.Email, loginDTO.RememberMe, false);
                
                return Ok(new { PersonName = authenticationResponse.PersonName, Email = authenticationResponse.Email });
            }
            else
            {
                return Problem("Invalid email or password");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var sessionToRemove = await _userSessionsRepository.GetByRefreshTokenAsync(refreshToken);
                if (sessionToRemove != null)
                {
                    await _userSessionsRepository.RemoveSessionAsync(sessionToRemove);
                    await _userSessionsRepository.SaveChangesAsync();
                }
            }

            await _signInManager.SignOutAsync();

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            Response.Cookies.Delete("jwt_expiry");
            Response.Cookies.Delete("refresh_expiry");

            return NoContent();
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken) 
        {
            var setting = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new string[] { _config["Google_Auth_Client_ID"] }
            };
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, setting);
            
            if (payload == null) { return BadRequest(); }
            
            ApplicationUser? user = await _userManager.FindByEmailAsync(payload.Email);
            AuthenticationResponse authenticationResponse;
            
            if (user != null)
            {
                await _signInManager.SignInAsync(user, true);
                authenticationResponse = await ProcessAfterLogin(payload.Email, true, false, user);
            }
            else 
            {
                ApplicationUser createdUser = new ApplicationUser()
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    PersonName = payload.Name
                };

                IdentityResult iResult = await _userManager.CreateAsync(createdUser);
                authenticationResponse = await ProcessAfterLogin(createdUser.Email, true, true, createdUser);
            }
            
            return Ok(new { PersonName = authenticationResponse.PersonName, Email = authenticationResponse.Email });
        }

        [HttpPost("regenerate-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken([FromQuery] bool rememberMe)
        {
            string? refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken)) 
            {
                return BadRequest("No refresh token found");
            }
            var session = await _userSessionsRepository.GetByRefreshTokenAsync(refreshToken);
            if (session == null) 
            {
                return BadRequest("No matching session with refresh token");
            }

            if (session.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
            {
                return BadRequest("Expired refresh token");
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(session.ApplicationUserId.ToString());
            if (user == null)
            {
                return BadRequest("No matching user for session");
            }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

            SetCookie("access_token", authenticationResponse.Token!, authenticationResponse.Expiration, rememberMe, true);
            SetCookie("jwt_expiry", new DateTimeOffset(authenticationResponse.Expiration).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, rememberMe, false);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nRefreshed at "+DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")+" !\n");
            return Ok();
        }

        [HttpGet("account-details-analytics")]
        [Authorize]
        public async Task<IActionResult> GetAccountDetailsAndAnalytics()
        {
            // Get current user
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Unauthorized();
            }

            var analytics = await _retrievalService.GetUsageAnalytics();

            string createdAt = user.CreatedAt.HasValue
                ? user.CreatedAt.Value.ToString("dd-MM-yyyy")
                : "Not Available";
            string phone = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "N/A" : user.PhoneNumber;

            var result = new
            {
                analytics.TopExtensionsBySize,
                analytics.TopFilesBySize,
                analytics.TotalFolders,
                analytics.TotalFiles,
                analytics.FavoriteItems,
                analytics.ItemsShared,
                Email = user.Email,
                CreatedAt = createdAt,
                Country = user.Country,
                PhoneNumber = phone,
                PersonName = user.PersonName
            };
            return Ok(result);
        }

        [HttpPatch("update-account")]
        [Authorize]
        public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountDTO dto)
        {
            bool isValid = ModelState.IsValid;
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Unauthorized();

            bool updated = false;
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                user.Email = dto.Email;
                user.UserName = dto.Email;
                updated = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.FullName) && dto.FullName != user.PersonName)
            {
                user.PersonName = dto.FullName;
                updated = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.Country) && dto.Country != user.Country)
            {
                user.Country = dto.Country;
                updated = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = dto.PhoneNumber;
                updated = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest("Passwords do not match.");
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!result.Succeeded)
                    return BadRequest(string.Join("; ", result.Errors.Select(e => e.Description)));
                updated = true;
            }
            if (updated)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return Problem(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
            // Return updated info (same as analytics DTO)
            string createdAt = user.CreatedAt.HasValue ? user.CreatedAt.Value.ToString("dd-MM-yyyy") : "Not Available";
            string phone = string.IsNullOrWhiteSpace(user.PhoneNumber) ? "N/A" : user.PhoneNumber;
            var response = new
            {
                Email = user.Email,
                CreatedAt = createdAt,
                Country = user.Country,
                PhoneNumber = phone,
                PersonName = user.PersonName
            };
            return Ok(response);
        }

        [HttpGet("get-user")]
        [Authorize]
        public async Task<IActionResult> GetUser()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return Unauthorized();
            return Ok(new { personName = user.PersonName });
        }
    }
}

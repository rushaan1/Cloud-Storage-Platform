using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.DTO.AuthDTO;
using CloudStoragePlatform.Core.ServiceContracts;
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
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IJwtService _jwtService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
        }

        private void SetCookie(string name, string value, DateTimeOffset? expires, bool httponly = true)
        {
            Response.Cookies.Append(name, value, new CookieOptions()
            {
                HttpOnly = httponly,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires
            });
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
                await _signInManager.SignInAsync(user, isPersistent: true);

                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);

                user.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
                await _userManager.UpdateAsync(user);

                SetCookie("access_token", authenticationResponse.Token!, authenticationResponse.Expiration);
                SetCookie("refresh_token", authenticationResponse.RefreshToken!, authenticationResponse.RefreshTokenExpirationDateTime);
                SetCookie("jwt_expiry", new DateTimeOffset(authenticationResponse.Expiration).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, false);
                SetCookie("refresh_expiry", new DateTimeOffset(authenticationResponse.RefreshTokenExpirationDateTime).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, false);

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

        [HttpPost("login")]
        public async Task<IActionResult> PostLogin(LoginDTO loginDTO)
        {
            if (ModelState.IsValid == false)
            {
                string errorMsg = string.Join("|", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMsg);
            }

            var result = await _signInManager.PasswordSignInAsync(loginDTO.Email, loginDTO.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                ApplicationUser? user = await _userManager.FindByEmailAsync(loginDTO.Email);

                AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user!);

                user!.RefreshToken = authenticationResponse.RefreshToken;
                user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;
                await _userManager.UpdateAsync(user);

                SetCookie("access_token", authenticationResponse.Token!, authenticationResponse.Expiration);
                SetCookie("refresh_token", authenticationResponse.RefreshToken!, authenticationResponse.RefreshTokenExpirationDateTime);
                SetCookie("jwt_expiry", new DateTimeOffset(authenticationResponse.Expiration).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, false);
                SetCookie("refresh_expiry", new DateTimeOffset(authenticationResponse.RefreshTokenExpirationDateTime).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, false);
                
                return Ok(new { PersonName = authenticationResponse.PersonName, Email = authenticationResponse.Email });
            }
            else
            {
                return Problem("Invalid email or password");
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout()
        {
            await _signInManager.SignOutAsync();

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");

            return NoContent();
        }

        [HttpPost("regenerate-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken()
        {
            //if (regenerateTokenModel == null)
            //{
            //    return BadRequest("Inavlid client request");
            //}

            //ClaimsPrincipal? principal = _jwtService.GetClaimsPrincipalFromJwtToken(regenerateTokenModel.Token);
            //if (principal == null)
            //{
            //    return BadRequest("Inavlid JWT Token");
            //}

            //string? email = principal.FindFirstValue(ClaimTypes.Email);
            string? refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken)) 
            {
                return BadRequest("No refresh token found");
            }
            ApplicationUser? user = _userManager.Users.Where(u=>u.RefreshToken == refreshToken)?.First();
            if (user == null || user.RefreshTokenExpirationDateTime <= DateTime.UtcNow)
            {
                return BadRequest("Expired refresh token");
            }

            AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);
            user.RefreshToken = authenticationResponse.RefreshToken;
            user.RefreshTokenExpirationDateTime = authenticationResponse.RefreshTokenExpirationDateTime;

            await _userManager.UpdateAsync(user);

            SetCookie("access_token", authenticationResponse.Token!, authenticationResponse.Expiration);
            SetCookie("refresh_token", authenticationResponse.RefreshToken!, authenticationResponse.RefreshTokenExpirationDateTime);
            SetCookie("jwt_expiry", new DateTimeOffset(authenticationResponse.Expiration).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, false);
            SetCookie("refresh_expiry", new DateTimeOffset(authenticationResponse.RefreshTokenExpirationDateTime).ToUnixTimeSeconds().ToString(), authenticationResponse.RefreshTokenExpirationDateTime, false);
            Console.WriteLine("Refreshed!");
            return Ok();
        }
    }
}

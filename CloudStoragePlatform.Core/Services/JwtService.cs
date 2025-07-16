using CloudStoragePlatform.Core.Domain.IdentityEntites;
using CloudStoragePlatform.Core.DTO.AuthDTO;
using CloudStoragePlatform.Core.ServiceContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CloudStoragePlatform.Core.Services
{
    public class JwtService : IJwtService
    {
        public readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthenticationResponse CreateJwtToken(ApplicationUser user) 
        {
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT unique ID
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()!),
                new Claim(ClaimTypes.Name, user.PersonName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtCloudStorageWebApi"])); //secret
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken tokenGen = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], claims, expires: expiration, signingCredentials: signingCredentials);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            string token = handler.WriteToken(tokenGen);

            return new AuthenticationResponse()
            {
                Token = token,
                Email = user.Email,
                PersonName = user.PersonName,
                Expiration = expiration,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpirationDateTime = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["RefreshToken:EXPIRATION_MINUTES"]))
            };
        }

        public ClaimsPrincipal? GetClaimsPrincipalFromJwtToken(string? token) 
        {
            TokenValidationParameters validationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidIssuer = _configuration["Jwt:Issuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration!["JwtCloudStorageWebApi"]))
            };
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)) 
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }

        private string GenerateRefreshToken() 
        {
            byte[] bytes = new byte[64];
            var numberGen = RandomNumberGenerator.Create();
            numberGen.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}

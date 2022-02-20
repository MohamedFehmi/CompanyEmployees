using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Identity;
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

namespace CompanyEmployees.Utility
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        private User _user;

        public AuthenticationManager(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<bool> ValidateUser(UserForAuthenticationDTO userForAuthenticationDTO)
        {
            _user = await _userManager.FindByNameAsync(userForAuthenticationDTO.UserName);
            return (_user != null && await _userManager.CheckPasswordAsync(_user, userForAuthenticationDTO.Password));
        }

        public async Task<bool> ValidateUser(string username, string refreshToken)
        {
            _user = await _userManager.FindByNameAsync(username);
            return (_user != null && _user.RefreshToken == refreshToken && _user.RefreshTokenExpiryTime > DateTime.Now);
        }

        public async Task<string> CreateToken()
        {
            var signingCrednetials = GetSigningCredentials();//Create the Signature
            var claims = await GetClaims();//For the payload
            var tokenOptions = GenerateTokenOptions(signingCrednetials, claims);

            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        }

        public async Task<string> CreateRefreshToken(bool saveUserTokenExpiryDate)
        {
            //Create refresh token
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                
                var refreshToken = Convert.ToBase64String(randomNumber);

                //set the user refresh token and expiry and save changes
                _user.RefreshToken = refreshToken;
                if(saveUserTokenExpiryDate) _user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                
                await _userManager.UpdateAsync(_user);

                return refreshToken;
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token) 
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //Validate the audience and issuer depending on the use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"))),
                ValidateLifetime = false //for now we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            //Validate the token 
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims()
        {
            //Insert a user name as claim
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _user.UserName)
            };

            //Get the roles
            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCrednetials, List<Claim> claims)
        {
            var jwtSettings  = _configuration.GetSection("JwtSettings");

            var tokenOptions = new JwtSecurityToken
            (
                issuer: jwtSettings.GetSection("validIssuer").Value,
                audience: jwtSettings.GetSection("validAudience").Value,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.GetSection("expires").Value)),
                signingCredentials: signingCrednetials
            );

            return tokenOptions;
        }

        public async Task<bool> RevokeRefreshToken(UserForAuthenticationDTO userForAuthenticationDTO)
        {
            _user = await _userManager.FindByNameAsync(userForAuthenticationDTO.UserName);
            if (_user is null) return false;

            _user.RefreshToken = null;
            await _userManager.UpdateAsync(_user);

            return true;
        }
    }
}

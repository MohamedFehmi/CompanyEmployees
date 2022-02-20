using Entities.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IAuthenticationManager
    {
        Task<bool> ValidateUser(UserForAuthenticationDTO userForAuthenticationDTO);
        Task<bool> ValidateUser(string username, string refreshToken);
        Task<string> CreateToken();
        Task<string> CreateRefreshToken(bool saveUserTokenExpiryDate = false);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> RevokeRefreshToken(UserForAuthenticationDTO userForAuthenticationDTO);
    }
}

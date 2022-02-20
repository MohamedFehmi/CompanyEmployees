using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IAuthenticationManager _authManager;

        public AuthenticationController(ILoggerManager logger, IMapper mapper, UserManager<User> userManager, IAuthenticationManager authManager) 
        {
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _authManager = authManager;
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> RegisterUser([FromBody] UserForRegistrationDTO userForRegistrationDTO)
        {
            var user = _mapper.Map<User>(userForRegistrationDTO);

            var result = await _userManager.CreateAsync(user, userForRegistrationDTO.Password);
            if(!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState);
            }

            await _userManager.AddToRolesAsync(user, userForRegistrationDTO.Roles);

            return StatusCode(201);
        }

        [HttpPost("login")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> LoginUser([FromBody] UserForAuthenticationDTO userForAuthenticationDTO)
        {
            if(!await _authManager.ValidateUser(userForAuthenticationDTO))
            {
                _logger.LogError($"{nameof(LoginUser)}: Authentication failed. Wrong username or password");
                return Unauthorized();
            }
            
            return Ok(new { Token = await _authManager.CreateToken(), RefreshToken = await _authManager.CreateRefreshToken(true) });
        }

        [HttpPost("refresh")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> RefreshUserToken([FromBody] UserForAuthenticationDTO userForAuthenticationDTO)
        {
            string accessToken = userForAuthenticationDTO.AccessToken;
            string refreshToken = userForAuthenticationDTO.RefreshToken;

            var principal = _authManager.GetPrincipalFromExpiredToken(accessToken);
            var username = principal.Identity.Name;

            if (!await _authManager.ValidateUser(username, refreshToken))
            {
                _logger.LogError($"{nameof(RefreshUserToken)}: Refreshing the json web token has failed.");
                return BadRequest("Invalid client request.");
            }

            var newAccessToken = await _authManager.CreateToken();
            var newRefreshToken = await _authManager.CreateRefreshToken();

            return Ok(new { Token = newAccessToken, RefreshToken = newRefreshToken});
        }

        [HttpPost("revoke"), Authorize]
        public async Task<IActionResult> Revoke([FromBody] UserForAuthenticationDTO userForAuthenticationDTO)
        {
            if (!await _authManager.RevokeRefreshToken(userForAuthenticationDTO))
                return BadRequest();

            return NoContent();
        }

    }
}

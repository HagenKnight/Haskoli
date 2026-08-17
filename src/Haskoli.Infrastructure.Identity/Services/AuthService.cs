using Haskoli.Application.Contracts.ExternalServices;
using Haskoli.Application.Contracts.Identity;
using Haskoli.Application.Models;
using Haskoli.Application.Models.Identity;
using Haskoli.Domain.Exceptions.Api;
using Haskoli.Domain.Exceptions.Identity;
using Haskoli.Infrastructure.Identity.Constants;
using Haskoli.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Haskoli.Infrastructure.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<ApplicationUser> _logger;
        private readonly IEmailService _emailservice;

        public AuthService(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwtSettings,
            ILogger<ApplicationUser> logger,
            IEmailService emailservice
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _emailservice = emailservice;
        }

        public async Task<AuthResponse> Login(AuthRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogError($"La autenticación falló debido a un nombre de usuario o contraseña incorrecta.");
                throw new IdentityException($"La autenticación falló debido a un nombre de usuario o contraseña incorrecta.");
            }

            var resultado = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!resultado.Succeeded)
            {
                _logger.LogError($"La autenticación falló debido a un nombre de usuario o contraseña incorrecta.");
                throw new IdentityException($"La autenticación falló debido a un nombre de usuario o contraseña incorrecta.");
            }

            _logger.LogInformation($"Usuario {user.UserName} ha ingresado de forma exitosa.");

            var token = await GenerateToken(user);
            var authResponse = new AuthResponse
            {
                Id = user.Id,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Username = user.UserName
            };
            return authResponse;
        }

        public async Task<RegistrationResponse> Register(RegistrationRequest request)
        {
            // refactor values:
            request.Username = request.Username.ToLower();
            request.Email = request.Email.ToLower();

            var existingUser = await _userManager.FindByNameAsync(request.Username.ToLower());
            if (existingUser != null)
            {
                _logger.LogError($"El username {request.Username} ya fue tomado por otra cuenta.");
                throw new ApiException($"El username {request.Username} ya fue tomado por otra cuenta.");
            }

            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                _logger.LogError($"El email {request.Email} ya fue tomado por otra cuenta.");
                throw new ApiException($"El email {request.Email} ya fue tomado por otra cuenta.");
            }

            var user = new ApplicationUser
            {
                Email = request.Email,
                FirstName = request.Nombre,
                Lastname = request.Apellidos,
                UserName = request.Username,
                EmailConfirmed = true
            };


            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Operator");


                await SendEmail(request);

                var token = await GenerateToken(user);
                return new RegistrationResponse
                {
                    Email = user.Email,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId = user.Id,
                    Username = user.UserName
                };
            }
            _logger.LogCritical($"{result.Errors}");
            throw new IdentityException($"{result.Errors}");
        }

        private async Task<JwtSecurityToken> GenerateToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();
            foreach (var role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(CustomClaimTypes.Uid, user.Id)
            }.Union(userClaims).Union(roleClaims);

            //"saint-seiya"
            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        private async Task SendEmail(RegistrationRequest regRequest)
        {
            var email = new Email
            {
                To = regRequest.Email,
                Body = $"El usuario {regRequest.Nombre} {regRequest.Apellidos} se ha creado de forma correcta.",
                Subject = "Creación de usuario"
            };

            try
            {
                await _emailservice.SendEmail(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Errores al enviar el email de {regRequest.Email}");
            }

        }

    }
}

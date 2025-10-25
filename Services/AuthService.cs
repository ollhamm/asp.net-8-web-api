using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using aspnetcoreapi.DTOs;
using aspnetcoreapi.DTOs.Auth;
using aspnetcoreapi.Models;
using aspnetcoreapi.Repositories.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace aspnetcoreapi.Services
{
    public class AuthService
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;

        private readonly IConfiguration _config;
        public AuthService(IUserRepository users, IRoleRepository roles, IConfiguration config)
        {
            _users = users;
            _roles = roles;
            _config = config;
        }

        // SIGN UP METHOD
        public async Task<UserDto> SignUpAsync(SignUpUserRequest req, CancellationToken ct = default)
        {
            // Check if email already exists
            var existing = await _users.GetByEmailAsync(req.Email, ct);
            if (existing is not null)
                throw new InvalidOperationException("Email already exists.");

            // Validation role IDs
            var selectedRoles = await _roles.GetByIdsAsync(req.RoleIds, ct);
            var selectedRolesList = selectedRoles.ToList();
            if (req.RoleIds.Any() && selectedRolesList.Count != req.RoleIds.Count)
                throw new InvalidOperationException("One or more selected roles are invalid.");

            // Hash password
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = KeyDerivation.Pbkdf2(
                password: req.Password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);

            // Create user
            var user = new User
            {
                Name = req.Name,
                Email = req.Email,
                PasswordSalt = salt,
                PasswordHash = hash
            };

            var created = await _users.AddAsync(user, ct);

            // Assign roles to the user
            foreach (var role in selectedRolesList)
            {
                await _users.AssignRoleToUserAsync(created.Id, role.Id, ct);
            }

            // Return user dto
            return new UserDto
            {
                Id = created.Id,
                Name = created.Name!,
                Email = created.Email!,
                CreatedAt = created.CreatedAt,
                Roles = selectedRolesList.Select(r => r.Name!).ToList()
            };
        }

        // SIGN IN METHOD
        public async Task<SignInResponse> SignInAsync(SignInUserRequest req, CancellationToken ct = default)
        {
            // Verify user by email
            var user = await _users.GetByEmailAsync(req.Email, ct);
            if (user is null)
                throw new InvalidOperationException("Invalid email or password.");

            // Verify password
            var hash = KeyDerivation.Pbkdf2(
                password: req.Password,
                salt: user.PasswordSalt!,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);

            // Compare hashes
            if (!hash.SequenceEqual(user.PasswordHash!))
                throw new InvalidOperationException("Invalid email or password.");

            var userWithRoles = await _users.GetByIdWithRolesAsync(user.Id, ct);
            var roles = userWithRoles?.UserRoles.Select(ur => ur.Role.Name).Where(n => n != null)!.Cast<string>().ToList() ?? new();

            var (token, expiresAt) = GenerateJwt(user, roles);

            // Return response
            return new SignInResponse
            {
                AccessToken = token,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name!,
                    Email = user.Email!,
                    CreatedAt = user.CreatedAt,
                    Roles = roles
                }
            };
        }

        // LOGOUT METHOD
        public Task LogoutAsync(Guid userId, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        // JWT GENERATION METHOD
        private (string token, DateTime expiresAt) GenerateJwt(User user, List<string> roles)
        {
            var key = _config["Jwt:Key"]!;
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Name ?? string.Empty)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, expires);
        }

        // ASSIGN ROLE TO USER
        public Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default) =>
            _users.AssignRoleToUserAsync(userId, roleId, ct);
    }
}
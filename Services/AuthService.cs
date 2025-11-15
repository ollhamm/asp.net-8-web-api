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
        private readonly EmailServices _emailService = null!;
        public AuthService(IUserRepository users, IRoleRepository roles, IConfiguration config, EmailServices emailService)
        {
            _users = users;
            _roles = roles;
            _config = config;
            _emailService = emailService;
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


        // FORGOT PASSWORD METHOD
        public async Task<string> ForgotPasswordAsync(string email, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(email, ct);
            if (user is null)
                throw new InvalidOperationException("User not found.");

            // Generate reset token
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            user.ResetPasswordToken = token;
            user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _users.UpdateAsync(user, ct);

            // Create reset link
            var clientUrl = _config["AppSettings:ClientUrl"] ?? "http://localhost:3000";
            var resetLink = $"{clientUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

            // Send email
            var emailBody = $@"
                <h2>Reset Your Password</h2>
                <p>Hi {user.Name},</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>
            ";

            try
            {
                await _emailService.SendEmailAsync(email, "Reset Your Password", emailBody, ct);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send reset email. Please try again later.");
            }

            return token;
        }

        // RESET PASSWORD METHOD
        public async Task ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(email, ct);
            if (user is null || user.ResetPasswordToken != token)
                throw new InvalidOperationException("Invalid reset token.");

            if (user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                throw new InvalidOperationException("Reset token has expired.");

            // hash new password
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = KeyDerivation.Pbkdf2(
                password: newPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
            user.ResetPasswordToken = null;
            user.ResetPasswordTokenExpiry = null;

            await _users.UpdateAsync(user, ct);
        }



        // ASSIGN ROLE TO USER
        public Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken ct = default) =>
            _users.AssignRoleToUserAsync(userId, roleId, ct);
    }
}
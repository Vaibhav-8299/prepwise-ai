using Microsoft.EntityFrameworkCore;
using PrepWiseAPI.Data;
using PrepWiseAPI.DTOs.Auth;
using PrepWiseAPI.Helpers;
using PrepWiseAPI.Models;

namespace PrepWiseAPI.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, JwtHelper jwtHelper, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
            _config = config;
        }

        // ==================== REGISTER ====================
        public async Task<UserProfileDto> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                throw new Exception("Email already registered.");

            // Hash password with BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Create user (IsEmailVerified = false by default)
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = "Student",
                IsEmailVerified = false,
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate 6-digit OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            // Save OTP record
            var otpRecord = new OtpRecord
            {
                UserId = user.Id,
                OtpCode = otpCode,
                Purpose = "EmailVerify",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.OtpRecords.Add(otpRecord);
            await _context.SaveChangesAsync();

            // Send OTP email
            await _emailService.SendOtpEmailAsync(user.Email, otpCode, "EmailVerify");

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        // ==================== VERIFY EMAIL ====================
        public async Task<bool> VerifyEmailAsync(VerifyOtpDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new Exception("User not found.");

            if (user.IsEmailVerified)
                throw new Exception("Email is already verified.");

            // Find matching unused OTP
            var otpRecord = await _context.OtpRecords
                .Where(o => o.UserId == user.Id
                         && o.OtpCode == dto.OtpCode
                         && o.Purpose == "EmailVerify"
                         && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                throw new Exception("Invalid OTP code.");

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
                throw new Exception("OTP has expired. Please request a new one.");

            // Activate account
            user.IsEmailVerified = true;
            otpRecord.IsUsed = true;

            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== LOGIN ====================
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                throw new Exception("Invalid email or password.");

            if (!user.IsEmailVerified)
                throw new Exception("Please verify your email first.");

            if (user.IsBlocked)
                throw new Exception("Your account has been blocked. Contact admin.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid email or password.");

            // Generate JWT access token (15 min)
            var accessToken = _jwtHelper.GenerateAccessToken(user);

            // Generate refresh token (7 days) and save to DB
            var refreshTokenDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!);
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = _jwtHelper.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        // ==================== REFRESH TOKEN ====================
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if (storedToken == null)
                throw new Exception("Invalid refresh token.");

            if (storedToken.RevokedAt != null)
                throw new Exception("Refresh token has been revoked.");

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Refresh token has expired. Please login again.");

            if (storedToken.User.IsBlocked)
                throw new Exception("Your account has been blocked.");

            // Revoke old refresh token (token rotation)
            storedToken.RevokedAt = DateTime.UtcNow;

            // Generate new access token + new refresh token
            var newAccessToken = _jwtHelper.GenerateAccessToken(storedToken.User);
            var refreshTokenDays = int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]!);
            var newRefreshToken = new RefreshToken
            {
                UserId = storedToken.UserId,
                Token = _jwtHelper.GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                FullName = storedToken.User.FullName,
                Email = storedToken.User.Email,
                Role = storedToken.User.Role
            };
        }

        // ==================== LOGOUT (REVOKE TOKEN) ====================
        public async Task RevokeTokenAsync(RefreshTokenDto dto)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

            if (storedToken != null && storedToken.RevokedAt == null)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ==================== REVOKE ALL TOKENS (for block/password reset) ====================
        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // ==================== FORGOT PASSWORD ====================
        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new Exception("User not found.");

            // Generate 6-digit OTP
            var otpCode = new Random().Next(100000, 999999).ToString();

            var otpRecord = new OtpRecord
            {
                UserId = user.Id,
                OtpCode = otpCode,
                Purpose = "PasswordReset",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.OtpRecords.Add(otpRecord);
            await _context.SaveChangesAsync();

            // Send OTP email
            await _emailService.SendOtpEmailAsync(user.Email, otpCode, "PasswordReset");
        }

        // ==================== RESET PASSWORD ====================
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                throw new Exception("User not found.");

            // Find matching unused OTP
            var otpRecord = await _context.OtpRecords
                .Where(o => o.UserId == user.Id
                         && o.OtpCode == dto.OtpCode
                         && o.Purpose == "PasswordReset"
                         && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpRecord == null)
                throw new Exception("Invalid OTP code.");

            if (otpRecord.ExpiresAt < DateTime.UtcNow)
                throw new Exception("OTP has expired. Please request a new one.");

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            otpRecord.IsUsed = true;

            await _context.SaveChangesAsync();

            // Revoke all refresh tokens (force re-login with new password)
            await RevokeAllUserTokensAsync(user.Id);
        }

        // ==================== GET PROFILE ====================
        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

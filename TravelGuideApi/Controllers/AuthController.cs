using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TravelGuideApi.Data;
using TravelGuideApi.DTOs;
using TravelGuideApi.Helpers;
using TravelGuideApi.Models;

namespace TravelGuideApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TravelGuideContext _context;
        private readonly JwtService _jwtService;

        public AuthController(TravelGuideContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(UserRegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Email already exists"));
            }

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Username already exists"));
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = PasswordHasher.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var authResponse = new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Token = token,
                RefreshToken = refreshToken
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "User registered successfully"));
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Invalid credentials"));
            }

            if (!PasswordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Invalid credentials"));
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var authResponse = new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Token = token,
                RefreshToken = refreshToken
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Login successful"));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var refreshToken = await _context.RefreshTokens
                .Include(r => r.User)
                .SingleOrDefaultAsync(r => r.Token == refreshTokenDto.RefreshToken && !r.IsRevoked);

            if (refreshToken == null || refreshToken.Expires < DateTime.UtcNow)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.FailureResponse("Invalid or expired refresh token"));
            }

            var user = refreshToken.User;
            var newJwtToken = _jwtService.GenerateJwtToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            var authResponse = new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Token = newJwtToken,
                RefreshToken = newRefreshToken
            };

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(authResponse, "Token refreshed successfully"));
        }
    }
} 
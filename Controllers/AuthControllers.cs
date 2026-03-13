using Microsoft.AspNetCore.Mvc;
using Fm.Api.Data;
using Fm.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

namespace Fm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record SignupRequest(string Email, string Password, string? DisplayName);
    
    /// <summary>
    /// 회원가입
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("signup")]
    public async Task<IActionResult> Signup(SignupRequest request)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            return BadRequest("이미 존재하는 이메일입니다.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok("회원가입 성공");
    }

    /// <summary>
    /// 로그인
    /// </summary>
    /// <param name="Email"></param>
    /// <param name="Password"></param>
    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string AccessToken, int ExpiresInSeconds);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return Unauthorized("이메일 또는 비밀번호가 올바르지 않습니다.");

        var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!ok)
            return Unauthorized("이메일 또는 비밀번호가 올바르지 않습니다.");

        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"]!;
        var audience = jwt["Audience"]!;
        var accessMinutes = int.Parse(jwt["AccessMinutes"] ?? "30");

        var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new("uid", user.Id.ToString())
    };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(accessMinutes);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return Ok(new LoginResponse(accessToken, accessMinutes * 60));
    }

    /// <summary>
    /// 내정보
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userIdValue = User.FindFirst("uid")?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userIdValue))
            return Unauthorized();

        var userId = Guid.Parse(userIdValue);

        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId);

        if (user == null)
            return NotFound("사용자를 찾을 수 없습니다.");

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            displayName = user.DisplayName,
            profileImageUrl = user.ProfileImageUrl
        });
    }


}
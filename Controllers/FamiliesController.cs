using System.IdentityModel.Tokens.Jwt;
using Fm.Api.Data;
using Fm.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FamiliesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FamiliesController(AppDbContext db)
    {
        _db = db;
    }

    private Guid GetUserId()
    {
        var uid = User.FindFirst("uid")?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(uid))
            throw new UnauthorizedAccessException("사용자 정보를 찾을 수 없습니다.");

        return Guid.Parse(uid);
    }

    public record CreateFamilyRequest(string Name);

    [HttpPost]
    public async Task<IActionResult> Create(CreateFamilyRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("가족 이름을 입력하세요.");

        var userId = GetUserId();

        var family = new Family
        {
            Name = request.Name,
            CreatedByUserId = userId
        };

        _db.Families.Add(family);
        await _db.SaveChangesAsync();

        return Ok(family);
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var userId = GetUserId();

        var familyIds = await _db.FamilyMembers
            .Where(x => x.UserId == userId)
            .Select(x => x.FamilyId)
            .Distinct()
            .ToListAsync();

        var list = await _db.Families
            .Where(x => x.CreatedByUserId == userId || familyIds.Contains(x.Id))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(list);
    }
}
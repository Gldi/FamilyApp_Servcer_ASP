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
public class FamilyMembersController : ControllerBase
{
    private readonly AppDbContext _db;

    public FamilyMembersController(AppDbContext db)
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

    public record CreateFamilyMemberRequest(
        Guid FamilyId,
        string Name,
        string? Role,
        DateOnly? BirthDate,
        string? Gender,
        Guid? UserId
    );

    [HttpPost]
    public async Task<IActionResult> Create(CreateFamilyMemberRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("구성원 이름을 입력하세요.");

        var userId = GetUserId();

        var family = await _db.Families
            .SingleOrDefaultAsync(x => x.Id == request.FamilyId);

        if (family == null)
            return NotFound("가족 정보를 찾을 수 없습니다.");

        // 현재는 가족 생성자만 구성원 추가 가능하도록 단순 제어
        if (family.CreatedByUserId != userId)
            return Forbid();

        var member = new FamilyMember
        {
            FamilyId = request.FamilyId,
            Name = request.Name,
            Role = request.Role,
            BirthDate = request.BirthDate,
            Gender = request.Gender,
            UserId = request.UserId
        };

        _db.FamilyMembers.Add(member);
        await _db.SaveChangesAsync();

        return Ok(member);
    }

    [HttpGet("{familyId:guid}")]
    public async Task<IActionResult> GetList(Guid familyId)
    {
        var list = await _db.FamilyMembers
            .Where(x => x.FamilyId == familyId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return Ok(list);
    }
}
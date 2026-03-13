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
public class FamilyRelationshipsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FamilyRelationshipsController(AppDbContext db)
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

    public record CreateFamilyRelationshipRequest(
        Guid FamilyId,
        Guid FromMemberId,
        Guid ToMemberId,
        string RelationType
    );

    [HttpPost]
    public async Task<IActionResult> Create(CreateFamilyRelationshipRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RelationType))
            return BadRequest("관계 타입을 입력하세요.");

        if (request.FromMemberId == request.ToMemberId)
            return BadRequest("같은 사람끼리는 관계를 연결할 수 없습니다.");

        var userId = GetUserId();

        var family = await _db.Families
            .SingleOrDefaultAsync(x => x.Id == request.FamilyId);

        if (family == null)
            return NotFound("가족 정보를 찾을 수 없습니다.");

        if (family.CreatedByUserId != userId)
            return Forbid();

        var fromMember = await _db.FamilyMembers
            .SingleOrDefaultAsync(x => x.Id == request.FromMemberId && x.FamilyId == request.FamilyId);

        var toMember = await _db.FamilyMembers
            .SingleOrDefaultAsync(x => x.Id == request.ToMemberId && x.FamilyId == request.FamilyId);

        if (fromMember == null || toMember == null)
            return BadRequest("가족 구성원 정보가 올바르지 않습니다.");

        var exists = await _db.FamilyRelationships.AnyAsync(x =>
            x.FamilyId == request.FamilyId &&
            x.FromMemberId == request.FromMemberId &&
            x.ToMemberId == request.ToMemberId &&
            x.RelationType == request.RelationType);

        if (exists)
            return BadRequest("이미 같은 관계가 등록되어 있습니다.");

        var relationship = new FamilyRelationship
        {
            FamilyId = request.FamilyId,
            FromMemberId = request.FromMemberId,
            ToMemberId = request.ToMemberId,
            RelationType = request.RelationType
        };

        _db.FamilyRelationships.Add(relationship);
        await _db.SaveChangesAsync();

        return Ok(relationship);
    }

    [HttpGet("{familyId:guid}")]
    public async Task<IActionResult> GetList(Guid familyId)
    {
        var list = await _db.FamilyRelationships
            .Where(x => x.FamilyId == familyId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        return Ok(list);
    }
}
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
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SchedulesController(AppDbContext db)
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

    public record CreateScheduleRequest(
        Guid FamilyId,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt,
        bool IsAllDay
    );

    public record UpdateScheduleRequest(
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt,
    bool IsAllDay
);

    [HttpPost]
    public async Task<IActionResult> Create(CreateScheduleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("일정 제목을 입력하세요.");

        if (request.EndAt < request.StartAt)
            return BadRequest("종료 시간은 시작 시간보다 빠를 수 없습니다.");

        var userId = GetUserId();

        var family = await _db.Families
            .SingleOrDefaultAsync(x => x.Id == request.FamilyId);

        if (family == null)
            return NotFound("가족 정보를 찾을 수 없습니다.");

        var schedule = new Schedule
        {
            FamilyId = request.FamilyId,
            CreatedByUserId = userId,
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            IsAllDay = request.IsAllDay
        };

        _db.Schedules.Add(schedule);
        await _db.SaveChangesAsync();

        return Ok(schedule);
    }

    [HttpGet("{familyId:guid}")]
    public async Task<IActionResult> GetList(
        Guid familyId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var q = _db.Schedules
            .Where(x => x.FamilyId == familyId && !x.IsDeleted);

        if (from.HasValue)
            q = q.Where(x => x.EndAt >= from.Value);

        if (to.HasValue)
            q = q.Where(x => x.StartAt <= to.Value);

        var list = await q
            .OrderBy(x => x.StartAt)
            .ToListAsync();

        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var schedule = await _db.Schedules
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (schedule == null)
            return NotFound("일정을 찾을 수 없습니다.");

        // 지금은 작성자만 삭제 가능
        if (schedule.CreatedByUserId != userId)
            return Forbid();

        schedule.IsDeleted = true;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateScheduleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("일정 제목을 입력하세요.");

        if (request.EndAt < request.StartAt)
            return BadRequest("종료 시간은 시작 시간보다 빠를 수 없습니다.");

        var userId = GetUserId();

        var schedule = await _db.Schedules
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (schedule == null)
            return NotFound("일정을 찾을 수 없습니다.");

        // 현재는 작성자만 수정 가능
        if (schedule.CreatedByUserId != userId)
            return Forbid();

        schedule.Title = request.Title;
        schedule.Description = request.Description;
        schedule.StartAt = request.StartAt;
        schedule.EndAt = request.EndAt;
        schedule.IsAllDay = request.IsAllDay;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(schedule);
    }
}
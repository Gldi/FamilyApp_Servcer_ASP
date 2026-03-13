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
public class PostsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PostsController(AppDbContext db)
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

    public record CreatePostRequest(string Content);

    [HttpPost]
    public async Task<IActionResult> Create(CreatePostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("내용을 입력하세요.");

        var userId = GetUserId();

        var post = new Post
        {
            UserId = userId,
            Content = request.Content
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        return Ok(post);
    }

    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var list = await _db.Posts
            .Where(p => !p.IsDeleted)
            .Join(
                _db.Users,
                post => post.UserId,
                user => user.Id,
                (post, user) => new
                {
                    id = post.Id,
                    content = post.Content,
                    createdAt = post.CreatedAt,
                    updatedAt = post.UpdatedAt,
                    authorUserId = user.Id,
                    authorDisplayName = user.DisplayName,
                    authorProfileImageUrl = user.ProfileImageUrl
                }
            )
            .OrderByDescending(x => x.createdAt)
            .ToListAsync();

        return Ok(list);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();

        var post = await _db.Posts
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId && !x.IsDeleted);

        if (post == null)
            return NotFound("게시글을 찾을 수 없습니다.");

        post.IsDeleted = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
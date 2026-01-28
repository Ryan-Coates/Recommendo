using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommendo.Api.Data;
using Recommendo.Api.DTOs;

namespace Recommendo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly RecommendoContext _context;

    public AdminController(RecommendoContext context)
    {
        _context = context;
    }

    private async Task<bool> IsAdminUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users.FindAsync(userId);
        return user?.IsAdmin ?? false;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        if (!await IsAdminUser())
        {
            return Forbid();
        }

        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto(u.Id, u.Email, u.Username, u.CreatedAt, u.IsAdmin))
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        if (!await IsAdminUser())
        {
            return Forbid();
        }

        var totalUsers = await _context.Users.CountAsync();
        var totalRecommendations = await _context.Recommendations.CountAsync();
        var totalFriendships = await _context.Friendships.CountAsync(f => f.Status == Models.FriendshipStatus.Accepted);

        return Ok(new
        {
            totalUsers,
            totalRecommendations,
            totalFriendships
        });
    }
}

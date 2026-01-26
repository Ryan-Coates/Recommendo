using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendo.Api.DTOs;
using Recommendo.Api.Services;

namespace Recommendo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FriendDto>>> GetFriends()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var friends = await _friendService.GetFriendsAsync(userId);
        return Ok(friends);
    }

    [HttpPost("invite")]
    public async Task<ActionResult<InviteLinkDto>> GenerateInviteLink()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var inviteLink = await _friendService.GenerateInviteLinkAsync(userId);
        return Ok(inviteLink);
    }

    [HttpPost("invite/accept")]
    public async Task<ActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendService.AcceptInviteAsync(userId, request.Token);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired invite link" });
        }

        return Ok(new { message = "Friend added successfully" });
    }

    [HttpDelete("{friendId}")]
    public async Task<ActionResult> RemoveFriend(int friendId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendService.RemoveFriendAsync(userId, friendId);

        if (!success)
        {
            return NotFound(new { message = "Friend not found" });
        }

        return Ok(new { message = "Friend removed successfully" });
    }
}

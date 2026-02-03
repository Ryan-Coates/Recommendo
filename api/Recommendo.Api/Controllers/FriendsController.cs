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

    [HttpGet("pending")]
    public async Task<ActionResult<List<FriendDto>>> GetPendingRequests()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var requests = await _friendService.GetPendingRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<List<FriendDto>>> GetSentRequests()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var requests = await _friendService.GetSentRequestsAsync(userId);
        return Ok(requests);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<SearchUserDto>>> SearchUsers([FromQuery] string query)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var users = await _friendService.SearchUsersAsync(userId, query);
        return Ok(users);
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

        return Ok(new { message = "Friend request sent successfully" });
    }

    [HttpPost("request")]
    public async Task<ActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendService.SendFriendRequestAsync(userId, request.TargetUserId);

        if (!success)
        {
            return BadRequest(new { message = "Unable to send friend request" });
        }

        return Ok(new { message = "Friend request sent successfully" });
    }

    [HttpPost("request/respond")]
    public async Task<ActionResult> RespondToFriendRequest([FromBody] RespondToFriendRequestRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _friendService.RespondToFriendRequestAsync(userId, request.FriendshipId, request.Accept);

        if (!success)
        {
            return BadRequest(new { message = "Invalid friend request" });
        }

        return Ok(new { message = request.Accept ? "Friend request accepted" : "Friend request rejected" });
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

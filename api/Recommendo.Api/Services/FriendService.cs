using Microsoft.EntityFrameworkCore;
using Recommendo.Api.Data;
using Recommendo.Api.DTOs;
using Recommendo.Api.Models;

namespace Recommendo.Api.Services;

public interface IFriendService
{
    Task<InviteLinkDto> GenerateInviteLinkAsync(int userId);
    Task<bool> AcceptInviteAsync(int userId, string token);
    Task<List<FriendDto>> GetFriendsAsync(int userId);
    Task<bool> RemoveFriendAsync(int userId, int friendId);
}

public class FriendService : IFriendService
{
    private readonly RecommendoContext _context;

    public FriendService(RecommendoContext context)
    {
        _context = context;
    }

    public async Task<InviteLinkDto> GenerateInviteLinkAsync(int userId)
    {
        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var inviteLink = new InviteLink
        {
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt
        };

        _context.InviteLinks.Add(inviteLink);
        await _context.SaveChangesAsync();

        var inviteUrl = $"/invite/{token}";
        
        return new InviteLinkDto(token, inviteUrl, expiresAt);
    }

    public async Task<bool> AcceptInviteAsync(int userId, string token)
    {
        var inviteLink = await _context.InviteLinks
            .FirstOrDefaultAsync(il => il.Token == token && !il.Used && il.ExpiresAt > DateTime.UtcNow);

        if (inviteLink == null || inviteLink.UserId == userId)
        {
            return false;
        }

        // Check if already friends
        var existingFriendship = await _context.Friendships
            .AnyAsync(f => 
                (f.UserId == userId && f.FriendId == inviteLink.UserId) ||
                (f.UserId == inviteLink.UserId && f.FriendId == userId));

        if (existingFriendship)
        {
            return false;
        }

        // Create bidirectional friendship
        _context.Friendships.Add(new Friendship
        {
            UserId = userId,
            FriendId = inviteLink.UserId,
            Status = FriendshipStatus.Accepted
        });

        _context.Friendships.Add(new Friendship
        {
            UserId = inviteLink.UserId,
            FriendId = userId,
            Status = FriendshipStatus.Accepted
        });

        inviteLink.Used = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<FriendDto>> GetFriendsAsync(int userId)
    {
        var friendships = await _context.Friendships
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId && f.Status == FriendshipStatus.Accepted)
            .ToListAsync();

        return friendships.Select(f => new FriendDto(
            f.Id,
            f.Friend.Id,
            f.Friend.Username,
            f.Friend.Email,
            f.Status.ToString(),
            f.CreatedAt
        )).ToList();
    }

    public async Task<bool> RemoveFriendAsync(int userId, int friendId)
    {
        var friendships = await _context.Friendships
            .Where(f => 
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.UserId == friendId && f.FriendId == userId))
            .ToListAsync();

        if (!friendships.Any())
        {
            return false;
        }

        _context.Friendships.RemoveRange(friendships);
        await _context.SaveChangesAsync();

        return true;
    }
}

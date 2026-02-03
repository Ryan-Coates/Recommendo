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
    Task<List<FriendDto>> GetPendingRequestsAsync(int userId);
    Task<List<FriendDto>> GetSentRequestsAsync(int userId);
    Task<bool> RemoveFriendAsync(int userId, int friendId);
    Task<List<SearchUserDto>> SearchUsersAsync(int currentUserId, string searchTerm);
    Task<bool> SendFriendRequestAsync(int userId, int targetUserId);
    Task<bool> RespondToFriendRequestAsync(int userId, int friendshipId, bool accept);
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

        // Check if already friends or has pending request
        var existingFriendship = await _context.Friendships
            .AnyAsync(f => 
                (f.UserId == userId && f.FriendId == inviteLink.UserId) ||
                (f.UserId == inviteLink.UserId && f.FriendId == userId));

        if (existingFriendship)
        {
            return false;
        }

        // Create a pending friendship request from invite link recipient to invite creator
        _context.Friendships.Add(new Friendship
        {
            UserId = userId,
            FriendId = inviteLink.UserId,
            Status = FriendshipStatus.Pending
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

    public async Task<List<FriendDto>> GetPendingRequestsAsync(int userId)
    {
        // Get friend requests sent TO this user (where they are the FriendId)
        var pendingRequests = await _context.Friendships
            .Include(f => f.User)
            .Where(f => f.FriendId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();

        return pendingRequests.Select(f => new FriendDto(
            f.Id,
            f.User.Id,
            f.User.Username,
            f.User.Email,
            f.Status.ToString(),
            f.CreatedAt
        )).ToList();
    }

    public async Task<List<FriendDto>> GetSentRequestsAsync(int userId)
    {
        // Get friend requests sent BY this user (where they are the UserId)
        var sentRequests = await _context.Friendships
            .Include(f => f.Friend)
            .Where(f => f.UserId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();

        return sentRequests.Select(f => new FriendDto(
            f.Id,
            f.Friend.Id,
            f.Friend.Username,
            f.Friend.Email,
            f.Status.ToString(),
            f.CreatedAt
        )).ToList();
    }

    public async Task<List<SearchUserDto>> SearchUsersAsync(int currentUserId, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<SearchUserDto>();
        }

        var users = await _context.Users
            .Where(u => u.Id != currentUserId && 
                (u.Username.Contains(searchTerm) || u.Email.Contains(searchTerm)))
            .Take(20)
            .ToListAsync();

        var result = new List<SearchUserDto>();

        foreach (var user in users)
        {
            // Check friendship status
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.UserId == currentUserId && f.FriendId == user.Id) ||
                    (f.UserId == user.Id && f.FriendId == currentUserId));

            string friendshipStatus = "None";
            if (friendship != null)
            {
                if (friendship.Status == FriendshipStatus.Accepted)
                {
                    friendshipStatus = "Friends";
                }
                else if (friendship.UserId == currentUserId)
                {
                    friendshipStatus = "RequestSent";
                }
                else
                {
                    friendshipStatus = "RequestReceived";
                }
            }

            result.Add(new SearchUserDto(
                user.Id,
                user.Username,
                user.Email,
                friendshipStatus
            ));
        }

        return result;
    }

    public async Task<bool> SendFriendRequestAsync(int userId, int targetUserId)
    {
        if (userId == targetUserId)
        {
            return false;
        }

        // Check if target user exists
        var targetUser = await _context.Users.FindAsync(targetUserId);
        if (targetUser == null)
        {
            return false;
        }

        // Check if there's already a friendship or pending request
        var existingFriendship = await _context.Friendships
            .AnyAsync(f => 
                (f.UserId == userId && f.FriendId == targetUserId) ||
                (f.UserId == targetUserId && f.FriendId == userId));

        if (existingFriendship)
        {
            return false;
        }

        // Create pending friend request
        _context.Friendships.Add(new Friendship
        {
            UserId = userId,
            FriendId = targetUserId,
            Status = FriendshipStatus.Pending
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RespondToFriendRequestAsync(int userId, int friendshipId, bool accept)
    {
        // Find the friend request where userId is the FriendId (recipient)
        var friendRequest = await _context.Friendships
            .FirstOrDefaultAsync(f => f.Id == friendshipId && 
                f.FriendId == userId && 
                f.Status == FriendshipStatus.Pending);

        if (friendRequest == null)
        {
            return false;
        }

        if (accept)
        {
            // Accept: Update existing request and create reciprocal friendship
            friendRequest.Status = FriendshipStatus.Accepted;
            
            _context.Friendships.Add(new Friendship
            {
                UserId = userId,
                FriendId = friendRequest.UserId,
                Status = FriendshipStatus.Accepted
            });
        }
        else
        {
            // Reject: Remove the friend request
            _context.Friendships.Remove(friendRequest);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}

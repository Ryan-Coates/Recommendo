using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Recommendo.Api.Data;
using Recommendo.Api.Models;
using Recommendo.Api.Services;
using Xunit;

namespace Recommendo.Api.Tests.Services;

public class FriendServiceTests : IDisposable
{
    private readonly RecommendoContext _context;
    private readonly FriendService _friendService;

    public FriendServiceTests()
    {
        var options = new DbContextOptionsBuilder<RecommendoContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new RecommendoContext(options);
        _friendService = new FriendService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GenerateInviteLinkAsync_ShouldCreateValidInviteLink()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "testuser",
            PasswordHash = "hash"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _friendService.GenerateInviteLinkAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        var savedLink = await _context.InviteLinks
            .FirstOrDefaultAsync(il => il.Token == result.Token);
        savedLink.Should().NotBeNull();
        savedLink!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task SendFriendRequestAsync_ShouldCreatePendingFriendship()
    {
        // Arrange
        var user1 = new User { Id = 1, Email = "user1@test.com", Username = "user1", PasswordHash = "hash" };
        var user2 = new User { Id = 2, Email = "user2@test.com", Username = "user2", PasswordHash = "hash" };
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _friendService.SendFriendRequestAsync(user1.Id, user2.Id);

        // Assert
        result.Should().BeTrue();

        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => f.UserId == user1.Id && f.FriendId == user2.Id);
        friendship.Should().NotBeNull();
        friendship!.Status.Should().Be(FriendshipStatus.Pending);
    }

    [Fact]
    public async Task SendFriendRequestAsync_ShouldReturnFalse_WhenUsersAreSame()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", Username = "test", PasswordHash = "hash" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _friendService.SendFriendRequestAsync(user.Id, user.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RespondToFriendRequestAsync_AcceptShouldCreateBidirectionalFriendship()
    {
        // Arrange
        var user1 = new User { Id = 1, Email = "user1@test.com", Username = "user1", PasswordHash = "hash" };
        var user2 = new User { Id = 2, Email = "user2@test.com", Username = "user2", PasswordHash = "hash" };
        _context.Users.AddRange(user1, user2);

        var friendRequest = new Friendship
        {
            Id = 1,
            UserId = user1.Id,
            FriendId = user2.Id,
            Status = FriendshipStatus.Pending
        };
        _context.Friendships.Add(friendRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _friendService.RespondToFriendRequestAsync(user2.Id, friendRequest.Id, true);

        // Assert
        result.Should().BeTrue();

        var friendships = await _context.Friendships
            .Where(f => (f.UserId == user1.Id && f.FriendId == user2.Id) ||
                       (f.UserId == user2.Id && f.FriendId == user1.Id))
            .ToListAsync();

        friendships.Should().HaveCount(2);
        friendships.Should().OnlyContain(f => f.Status == FriendshipStatus.Accepted);
    }

    [Fact]
    public async Task RespondToFriendRequestAsync_RejectShouldRemoveFriendship()
    {
        // Arrange
        var user1 = new User { Id = 1, Email = "user1@test.com", Username = "user1", PasswordHash = "hash" };
        var user2 = new User { Id = 2, Email = "user2@test.com", Username = "user2", PasswordHash = "hash" };
        _context.Users.AddRange(user1, user2);

        var friendRequest = new Friendship
        {
            Id = 1,
            UserId = user1.Id,
            FriendId = user2.Id,
            Status = FriendshipStatus.Pending
        };
        _context.Friendships.Add(friendRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _friendService.RespondToFriendRequestAsync(user2.Id, friendRequest.Id, false);

        // Assert
        result.Should().BeTrue();

        var friendship = await _context.Friendships.FindAsync(friendRequest.Id);
        friendship.Should().BeNull();
    }

    [Fact]
    public async Task SearchUsersAsync_ShouldReturnUsersMatchingSearchTerm()
    {
        // Arrange
        var currentUser = new User { Id = 1, Email = "current@test.com", Username = "current", PasswordHash = "hash" };
        var user1 = new User { Id = 2, Email = "john@test.com", Username = "john", PasswordHash = "hash" };
        var user2 = new User { Id = 3, Email = "jane@test.com", Username = "jane", PasswordHash = "hash" };
        var user3 = new User { Id = 4, Email = "bob@test.com", Username = "bob", PasswordHash = "hash" };
        _context.Users.AddRange(currentUser, user1, user2, user3);
        await _context.SaveChangesAsync();

        // Act
        var results = await _friendService.SearchUsersAsync(currentUser.Id, "jo");

        // Assert
        results.Should().HaveCount(1);
        results.First().Username.Should().Be("john");
    }

    [Fact]
    public async Task GetPendingRequestsAsync_ShouldReturnRequestsSentToUser()
    {
        // Arrange
        var user1 = new User { Id = 1, Email = "user1@test.com", Username = "user1", PasswordHash = "hash" };
        var user2 = new User { Id = 2, Email = "user2@test.com", Username = "user2", PasswordHash = "hash" };
        _context.Users.AddRange(user1, user2);

        var friendRequest = new Friendship
        {
            UserId = user1.Id,
            FriendId = user2.Id,
            Status = FriendshipStatus.Pending
        };
        _context.Friendships.Add(friendRequest);
        await _context.SaveChangesAsync();

        // Act
        var results = await _friendService.GetPendingRequestsAsync(user2.Id);

        // Assert
        results.Should().HaveCount(1);
        results.First().UserId.Should().Be(user1.Id);
    }

    [Fact]
    public async Task GetFriendsAsync_ShouldReturnOnlyAcceptedFriendships()
    {
        // Arrange
        var user1 = new User { Id = 1, Email = "user1@test.com", Username = "user1", PasswordHash = "hash" };
        var user2 = new User { Id = 2, Email = "user2@test.com", Username = "user2", PasswordHash = "hash" };
        var user3 = new User { Id = 3, Email = "user3@test.com", Username = "user3", PasswordHash = "hash" };
        _context.Users.AddRange(user1, user2, user3);

        _context.Friendships.AddRange(
            new Friendship { UserId = user1.Id, FriendId = user2.Id, Status = FriendshipStatus.Accepted },
            new Friendship { UserId = user1.Id, FriendId = user3.Id, Status = FriendshipStatus.Pending }
        );
        await _context.SaveChangesAsync();

        // Act
        var results = await _friendService.GetFriendsAsync(user1.Id);

        // Assert
        results.Should().HaveCount(1);
        results.First().UserId.Should().Be(user2.Id);
    }
}

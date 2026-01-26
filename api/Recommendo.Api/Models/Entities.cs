namespace Recommendo.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<Friendship> FriendshipsInitiated { get; set; } = new List<Friendship>();
    public ICollection<Friendship> FriendshipsReceived { get; set; } = new List<Friendship>();
    public ICollection<InviteLink> InviteLinks { get; set; } = new List<InviteLink>();
    public ICollection<Recommendation> RecommendationsCreated { get; set; } = new List<Recommendation>();
    public ICollection<Recommendation> RecommendationsReceived { get; set; } = new List<Recommendation>();
}

public class Friendship
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public FriendshipStatus Status { get; set; } = FriendshipStatus.Accepted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
    public User Friend { get; set; } = null!;
}

public enum FriendshipStatus
{
    Pending,
    Accepted,
    Rejected
}

public class InviteLink
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User User { get; set; } = null!;
}

public class Recommendation
{
    public int Id { get; set; }
    public int CreatedByUserId { get; set; }
    public int RecommendedToUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public RecommendationType Type { get; set; }
    public string? Description { get; set; }
    public string? ExternalId { get; set; }
    public RecommendationStatus Status { get; set; } = RecommendationStatus.Unseen;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    public User CreatedByUser { get; set; } = null!;
    public User RecommendedToUser { get; set; } = null!;
    public ICollection<RecommendationNote> Notes { get; set; } = new List<RecommendationNote>();
}

public enum RecommendationType
{
    Movie,
    Book,
    Game,
    TvShow,
    Podcast,
    Music,
    Other
}

public enum RecommendationStatus
{
    Unseen,
    Watched,
    InProgress
}

public class RecommendationNote
{
    public int Id { get; set; }
    public int RecommendationId { get; set; }
    public int UserId { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Recommendation Recommendation { get; set; } = null!;
    public User User { get; set; } = null!;
}

namespace Recommendo.Api.DTOs;

// Auth DTOs
public record RegisterRequest(string Email, string Username, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(int Id, string Email, string Username, string Token, bool IsAdmin);

// User DTOs
public record UserDto(int Id, string Email, string Username, DateTime CreatedAt, bool IsAdmin);

// Friend DTOs
public record FriendDto(int Id, int UserId, string Username, string Email, string Status, DateTime CreatedAt);
public record InviteLinkDto(string Token, string InviteUrl, DateTime ExpiresAt);
public record AcceptInviteRequest(string Token);

// Recommendation DTOs
public record CreateRecommendationRequest(
    string Title,
    string Type,
    string? Description,
    string? ExternalId,
    List<int>? RecommendedToUserIds
);

public record UpdateRecommendationRequest(string Status);

public record RecommendationDto(
    int Id,
    int CreatedByUserId,
    string CreatedByUsername,
    int RecommendedToUserId,
    string RecommendedToUsername,
    string Title,
    string Type,
    string? Description,
    string? ExternalId,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

// Note DTOs
public record CreateNoteRequest(string Note);
public record NoteDto(int Id, int UserId, string Username, string Note, DateTime CreatedAt);

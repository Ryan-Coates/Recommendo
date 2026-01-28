using Microsoft.EntityFrameworkCore;
using Recommendo.Api.Data;
using Recommendo.Api.DTOs;
using Recommendo.Api.Models;

namespace Recommendo.Api.Services;

public interface IRecommendationService
{
    Task<List<RecommendationDto>> GetRecommendationsAsync(int userId, string? type, int? friendId);
    Task<RecommendationDto?> GetRecommendationByIdAsync(int id, int userId);
    Task<List<RecommendationDto>> CreateRecommendationAsync(int createdByUserId, CreateRecommendationRequest request);
    Task<bool> UpdateRecommendationStatusAsync(int id, int userId, string status);
    Task<bool> DeleteRecommendationAsync(int id, int userId);
}

public class RecommendationService : IRecommendationService
{
    private readonly RecommendoContext _context;

    public RecommendationService(RecommendoContext context)
    {
        _context = context;
    }

    public async Task<List<RecommendationDto>> GetRecommendationsAsync(int userId, string? type, int? friendId)
    {
        // Show recommendations the user received OR created
        var query = _context.Recommendations
            .Include(r => r.CreatedByUser)
            .Include(r => r.RecommendedToUser)
            .Where(r => r.RecommendedToUserId == userId || r.CreatedByUserId == userId);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<RecommendationType>(type, true, out var recType))
        {
            query = query.Where(r => r.Type == recType);
        }

        if (friendId.HasValue)
        {
            query = query.Where(r => r.CreatedByUserId == friendId.Value);
        }

        var recommendations = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return recommendations.Select(MapToDto).ToList();
    }

    public async Task<RecommendationDto?> GetRecommendationByIdAsync(int id, int userId)
    {
        var recommendation = await _context.Recommendations
            .Include(r => r.CreatedByUser)
            .Include(r => r.RecommendedToUser)
            .FirstOrDefaultAsync(r => r.Id == id && (r.CreatedByUserId == userId || r.RecommendedToUserId == userId));

        return recommendation == null ? null : MapToDto(recommendation);
    }

    public async Task<List<RecommendationDto>> CreateRecommendationAsync(int createdByUserId, CreateRecommendationRequest request)
    {
        if (!Enum.TryParse<RecommendationType>(request.Type, true, out var recType))
        {
            throw new ArgumentException("Invalid recommendation type");
        }

        var recommendations = new List<Recommendation>();

        // If no specific users, create recommendations for all friends
        var targetUserIds = request.RecommendedToUserIds;
        if (targetUserIds == null || targetUserIds.Count == 0)
        {
            // Get all friends of the current user
            var friendIds = await _context.Friendships
                .Where(f => (f.UserId == createdByUserId || f.FriendId == createdByUserId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => f.UserId == createdByUserId ? f.FriendId : f.UserId)
                .ToListAsync();
            
            // If no friends, create a recommendation to yourself (personal list)
            targetUserIds = friendIds.Count > 0 ? friendIds : new List<int> { createdByUserId };
        }

        foreach (var recommendedToUserId in targetUserIds)
        {
            var recommendation = new Recommendation
            {
                CreatedByUserId = createdByUserId,
                RecommendedToUserId = recommendedToUserId,
                Title = request.Title,
                Type = recType,
                Description = request.Description,
                ExternalId = request.ExternalId,
                Status = RecommendationStatus.Unseen
            };

            _context.Recommendations.Add(recommendation);
            recommendations.Add(recommendation);
        }

        await _context.SaveChangesAsync();

        // Reload with navigation properties
        var ids = recommendations.Select(r => r.Id).ToList();
        var loadedRecommendations = await _context.Recommendations
            .Include(r => r.CreatedByUser)
            .Include(r => r.RecommendedToUser)
            .Where(r => ids.Contains(r.Id))
            .ToListAsync();

        return loadedRecommendations.Select(MapToDto).ToList();
    }

    public async Task<bool> UpdateRecommendationStatusAsync(int id, int userId, string status)
    {
        if (!Enum.TryParse<RecommendationStatus>(status, true, out var recStatus))
        {
            return false;
        }

        var recommendation = await _context.Recommendations
            .FirstOrDefaultAsync(r => r.Id == id && r.RecommendedToUserId == userId);

        if (recommendation == null)
        {
            return false;
        }

        recommendation.Status = recStatus;

        if (recStatus == RecommendationStatus.Watched)
        {
            recommendation.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRecommendationAsync(int id, int userId)
    {
        var recommendation = await _context.Recommendations
            .FirstOrDefaultAsync(r => r.Id == id && (r.CreatedByUserId == userId || r.RecommendedToUserId == userId));

        if (recommendation == null)
        {
            return false;
        }

        _context.Recommendations.Remove(recommendation);
        await _context.SaveChangesAsync();
        return true;
    }

    private static RecommendationDto MapToDto(Recommendation rec) => new(
        rec.Id,
        rec.CreatedByUserId,
        rec.CreatedByUser.Username,
        rec.RecommendedToUserId,
        rec.RecommendedToUser.Username,
        rec.Title,
        rec.Type.ToString(),
        rec.Description,
        rec.ExternalId,
        rec.Status.ToString(),
        rec.CreatedAt,
        rec.CompletedAt
    );
}

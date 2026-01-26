using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recommendo.Api.DTOs;
using Recommendo.Api.Services;

namespace Recommendo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RecommendationDto>>> GetRecommendations(
        [FromQuery] string? type = null,
        [FromQuery] int? friendId = null)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recommendations = await _recommendationService.GetRecommendationsAsync(userId, type, friendId);
        return Ok(recommendations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecommendationDto>> GetRecommendation(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var recommendation = await _recommendationService.GetRecommendationByIdAsync(id, userId);

        if (recommendation == null)
        {
            return NotFound();
        }

        return Ok(recommendation);
    }

    [HttpPost]
    public async Task<ActionResult<List<RecommendationDto>>> CreateRecommendation([FromBody] CreateRecommendationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var recommendations = await _recommendationService.CreateRecommendationAsync(userId, request);
            return Ok(recommendations);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateRecommendationStatus(int id, [FromBody] UpdateRecommendationRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _recommendationService.UpdateRecommendationStatusAsync(id, userId, request.Status);

        if (!success)
        {
            return BadRequest(new { message = "Invalid status or recommendation not found" });
        }

        return Ok(new { message = "Recommendation updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRecommendation(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var success = await _recommendationService.DeleteRecommendationAsync(id, userId);

        if (!success)
        {
            return NotFound(new { message = "Recommendation not found" });
        }

        return Ok(new { message = "Recommendation deleted successfully" });
    }

    [HttpGet("types")]
    public ActionResult<List<string>> GetRecommendationTypes()
    {
        var types = Enum.GetNames(typeof(Models.RecommendationType));
        return Ok(types);
    }
}

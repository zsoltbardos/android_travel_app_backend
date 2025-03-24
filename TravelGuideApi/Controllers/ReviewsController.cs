using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TravelGuideApi.Data;
using TravelGuideApi.DTOs;
using TravelGuideApi.Models;

namespace TravelGuideApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public ReviewsController(TravelGuideContext context)
        {
            _context = context;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ReviewDto>>>> GetReviews([FromQuery] PaginationParams paginationParams)
        {
            var query = _context.Reviews
                .Include(r => r.Destination)
                .AsQueryable();

            // Apply sorting
            query = paginationParams.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(r => EF.Property<object>(r, paginationParams.SortBy ?? "Id"))
                : query.OrderBy(r => EF.Property<object>(r, paginationParams.SortBy ?? "Id"));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

            var reviews = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                DestinationId = r.DestinationId,
                UserId = r.UserId,
                ReviewerName = r.ReviewerName,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                ReviewDate = r.ReviewDate,
                DestinationName = r.Destination?.Name
            }).ToList();

            var paginatedResponse = new PaginatedResponse<ReviewDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = totalPages,
                HasPrevious = paginationParams.PageNumber > 1,
                HasNext = paginationParams.PageNumber < totalPages
            };

            return Ok(ApiResponse<PaginatedResponse<ReviewDto>>.SuccessResponse(paginatedResponse));
        }

        // GET: api/Reviews/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> GetReview(int id)
        {
            var review = await _context.Reviews
                .Include(r => r.Destination)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound(ApiResponse<ReviewDto>.FailureResponse("Review not found"));
            }

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                DestinationId = review.DestinationId,
                UserId = review.UserId,
                ReviewerName = review.ReviewerName,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                ReviewDate = review.ReviewDate,
                DestinationName = review.Destination?.Name
            };

            return Ok(ApiResponse<ReviewDto>.SuccessResponse(reviewDto));
        }

        // GET: api/Destinations/{destinationId}/Reviews
        [HttpGet("/api/Destinations/{destinationId}/Reviews")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ReviewDto>>>> GetReviewsForDestination(
            int destinationId, 
            [FromQuery] PaginationParams paginationParams)
        {
            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null)
            {
                return NotFound(ApiResponse<PaginatedResponse<ReviewDto>>.FailureResponse("Destination not found"));
            }

            var query = _context.Reviews
                .Where(r => r.DestinationId == destinationId)
                .AsQueryable();

            // Apply sorting
            query = paginationParams.SortOrder.ToLower() == "desc"
                ? query.OrderByDescending(r => EF.Property<object>(r, paginationParams.SortBy ?? "Id"))
                : query.OrderBy(r => EF.Property<object>(r, paginationParams.SortBy ?? "Id"));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

            var reviews = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var reviewDtos = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                DestinationId = r.DestinationId,
                UserId = r.UserId,
                ReviewerName = r.ReviewerName,
                ReviewText = r.ReviewText,
                Rating = r.Rating,
                ReviewDate = r.ReviewDate,
                DestinationName = destination.Name
            }).ToList();

            var paginatedResponse = new PaginatedResponse<ReviewDto>
            {
                Items = reviewDtos,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = totalPages,
                HasPrevious = paginationParams.PageNumber > 1,
                HasNext = paginationParams.PageNumber < totalPages
            };

            return Ok(ApiResponse<PaginatedResponse<ReviewDto>>.SuccessResponse(paginatedResponse));
        }

        // POST: api/Destinations/{destinationId}/Reviews
        [HttpPost("/api/Destinations/{destinationId}/Reviews")]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> AddReviewToDestination(
            int destinationId, 
            ReviewCreateDto reviewDto)
        {
            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination == null)
            {
                return NotFound(ApiResponse<ReviewDto>.FailureResponse("Destination not found"));
            }

            int? userId = null;
            // If user is authenticated, get user ID from claims
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int claimUserId))
                {
                    userId = claimUserId;
                }
            }

            var review = new Review
            {
                DestinationId = destinationId,
                UserId = userId,
                ReviewerName = userId.HasValue ? null : reviewDto.ReviewerName, // Use reviewer name only for anonymous reviews
                ReviewText = reviewDto.ReviewText,
                Rating = reviewDto.Rating,
                ReviewDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Update average rating for destination
            var averageRating = await _context.Reviews
                .Where(r => r.DestinationId == destinationId)
                .AverageAsync(r => r.Rating);
            
            destination.AverageRating = averageRating;
            await _context.SaveChangesAsync();

            var createdReviewDto = new ReviewDto
            {
                Id = review.Id,
                DestinationId = review.DestinationId,
                UserId = review.UserId,
                ReviewerName = review.ReviewerName,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                ReviewDate = review.ReviewDate,
                DestinationName = destination.Name
            };

            return CreatedAtAction(nameof(GetReview), new { id = review.Id }, 
                ApiResponse<ReviewDto>.SuccessResponse(createdReviewDto, "Review added successfully"));
        }

        // PUT: api/Reviews/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ReviewDto>>> UpdateReview(int id, ReviewUpdateDto reviewDto)
        {
            var review = await _context.Reviews
                .Include(r => r.Destination)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound(ApiResponse<ReviewDto>.FailureResponse("Review not found"));
            }

            // Check if user has permission to update the review
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != review.UserId.ToString())
            {
                return Forbid();
            }

            review.ReviewText = reviewDto.ReviewText;
            review.Rating = reviewDto.Rating;

            _context.Entry(review).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Update average rating for destination
                var averageRating = await _context.Reviews
                    .Where(r => r.DestinationId == review.DestinationId)
                    .AverageAsync(r => r.Rating);
                
                var destination = await _context.Destinations.FindAsync(review.DestinationId);
                if (destination != null)
                {
                    destination.AverageRating = averageRating;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(id))
                {
                    return NotFound(ApiResponse<ReviewDto>.FailureResponse("Review not found"));
                }
                else
                {
                    throw;
                }
            }

            var updatedReviewDto = new ReviewDto
            {
                Id = review.Id,
                DestinationId = review.DestinationId,
                UserId = review.UserId,
                ReviewerName = review.ReviewerName,
                ReviewText = review.ReviewText,
                Rating = review.Rating,
                ReviewDate = review.ReviewDate,
                DestinationName = review.Destination?.Name
            };

            return Ok(ApiResponse<ReviewDto>.SuccessResponse(updatedReviewDto, "Review updated successfully"));
        }

        // DELETE: api/Reviews/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(ApiResponse<bool>.FailureResponse("Review not found"));
            }

            // Check if user has permission to delete the review
            if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != review.UserId.ToString())
            {
                return Forbid();
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            // Update average rating for destination
            var destinationId = review.DestinationId;
            var destination = await _context.Destinations.FindAsync(destinationId);
            if (destination != null)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.DestinationId == destinationId)
                    .ToListAsync();
                
                if (reviews.Any())
                {
                    destination.AverageRating = reviews.Average(r => r.Rating);
                }
                else
                {
                    destination.AverageRating = 0;
                }
                
                await _context.SaveChangesAsync();
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Review deleted successfully"));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
} 
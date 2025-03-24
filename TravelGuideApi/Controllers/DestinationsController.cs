using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TravelGuideApi.Data;
using TravelGuideApi.DTOs;
using TravelGuideApi.Models;

namespace TravelGuideApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationsController : ControllerBase
    {
        private readonly TravelGuideContext _context;

        public DestinationsController(TravelGuideContext context)
        {
            _context = context;
        }

        // GET: api/Destinations
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DestinationDto>>>> GetDestinations([FromQuery] PaginationParams paginationParams)
        {
            var query = _context.Destinations.AsQueryable();

            // Apply sorting in a safe way
            if (string.IsNullOrEmpty(paginationParams.SortBy) || paginationParams.SortBy.ToLower() == "id")
            {
                // Default sorting by Id
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.Id) 
                    : query.OrderBy(d => d.Id);
            }
            else if (paginationParams.SortBy.ToLower() == "name")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.Name) 
                    : query.OrderBy(d => d.Name);
            }
            else if (paginationParams.SortBy.ToLower() == "averagerating")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.AverageRating) 
                    : query.OrderBy(d => d.AverageRating);
            }
            else if (paginationParams.SortBy.ToLower() == "createdat")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.CreatedAt) 
                    : query.OrderBy(d => d.CreatedAt);
            }
            else if (paginationParams.SortBy.ToLower() == "updatedat")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.UpdatedAt) 
                    : query.OrderBy(d => d.UpdatedAt);
            }
            else
            {
                // Default to Id if an invalid sort field is provided
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.Id) 
                    : query.OrderBy(d => d.Id);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

            var destinations = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var destinationDtos = destinations.Select(d => new DestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Location = d.Location,
                AverageRating = d.AverageRating,
                PointsOfInterest = string.IsNullOrEmpty(d.PointsOfInterest) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(d.PointsOfInterest),
                ImageUrls = string.IsNullOrEmpty(d.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(d.ImageUrls),
                CoverImageUrl = d.CoverImageUrl,
                Region = d.Region,
                Category = d.Category,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            var paginatedResponse = new PaginatedResponse<DestinationDto>
            {
                Items = destinationDtos,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = totalPages,
                HasPrevious = paginationParams.PageNumber > 1,
                HasNext = paginationParams.PageNumber < totalPages
            };

            return Ok(ApiResponse<PaginatedResponse<DestinationDto>>.SuccessResponse(paginatedResponse));
        }

        // GET: api/Destinations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DestinationDto>>> GetDestination(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);

            if (destination == null)
            {
                return NotFound(ApiResponse<DestinationDto>.FailureResponse("Destination not found"));
            }

            var destinationDto = new DestinationDto
            {
                Id = destination.Id,
                Name = destination.Name,
                Description = destination.Description,
                Location = destination.Location,
                AverageRating = destination.AverageRating,
                PointsOfInterest = string.IsNullOrEmpty(destination.PointsOfInterest) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(destination.PointsOfInterest),
                ImageUrls = string.IsNullOrEmpty(destination.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(destination.ImageUrls),
                CoverImageUrl = destination.CoverImageUrl,
                Region = destination.Region,
                Category = destination.Category,
                CreatedAt = destination.CreatedAt,
                UpdatedAt = destination.UpdatedAt
            };

            return Ok(ApiResponse<DestinationDto>.SuccessResponse(destinationDto));
        }

        // POST: api/Destinations
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DestinationDto>>> CreateDestination(DestinationCreateDto destinationDto)
        {
            var destination = new Destination
            {
                Name = destinationDto.Name,
                Description = destinationDto.Description,
                Location = destinationDto.Location,
                PointsOfInterest = JsonSerializer.Serialize(destinationDto.PointsOfInterest ?? new List<string>()),
                ImageUrls = JsonSerializer.Serialize(destinationDto.ImageUrls ?? new List<string>()),
                CoverImageUrl = destinationDto.CoverImageUrl,
                Region = destinationDto.Region,
                Category = destinationDto.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                AverageRating = 0
            };

            _context.Destinations.Add(destination);
            await _context.SaveChangesAsync();

            var createdDestinationDto = new DestinationDto
            {
                Id = destination.Id,
                Name = destination.Name,
                Description = destination.Description,
                Location = destination.Location,
                AverageRating = destination.AverageRating,
                PointsOfInterest = destinationDto.PointsOfInterest ?? new List<string>(),
                ImageUrls = destinationDto.ImageUrls ?? new List<string>(),
                CoverImageUrl = destination.CoverImageUrl,
                Region = destination.Region,
                Category = destination.Category,
                CreatedAt = destination.CreatedAt,
                UpdatedAt = destination.UpdatedAt
            };

            return CreatedAtAction(nameof(GetDestination), new { id = destination.Id }, 
                ApiResponse<DestinationDto>.SuccessResponse(createdDestinationDto, "Destination created successfully"));
        }

        // PUT: api/Destinations/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DestinationDto>>> UpdateDestination(int id, DestinationUpdateDto destinationDto)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
            {
                return NotFound(ApiResponse<DestinationDto>.FailureResponse("Destination not found"));
            }

            destination.Name = destinationDto.Name ?? destination.Name;
            destination.Description = destinationDto.Description ?? destination.Description;
            destination.Location = destinationDto.Location ?? destination.Location;
            
            if (destinationDto.PointsOfInterest != null)
            {
                destination.PointsOfInterest = JsonSerializer.Serialize(destinationDto.PointsOfInterest);
            }
            
            if (destinationDto.ImageUrls != null)
            {
                destination.ImageUrls = JsonSerializer.Serialize(destinationDto.ImageUrls);
            }
            
            destination.CoverImageUrl = destinationDto.CoverImageUrl ?? destination.CoverImageUrl;
            destination.Region = destinationDto.Region ?? destination.Region;
            destination.Category = destinationDto.Category ?? destination.Category;
            destination.UpdatedAt = DateTime.UtcNow;

            _context.Entry(destination).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DestinationExists(id))
                {
                    return NotFound(ApiResponse<DestinationDto>.FailureResponse("Destination not found"));
                }
                else
                {
                    throw;
                }
            }

            var updatedDestinationDto = new DestinationDto
            {
                Id = destination.Id,
                Name = destination.Name,
                Description = destination.Description,
                Location = destination.Location,
                AverageRating = destination.AverageRating,
                PointsOfInterest = string.IsNullOrEmpty(destination.PointsOfInterest) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(destination.PointsOfInterest),
                ImageUrls = string.IsNullOrEmpty(destination.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(destination.ImageUrls),
                CoverImageUrl = destination.CoverImageUrl,
                Region = destination.Region,
                Category = destination.Category,
                CreatedAt = destination.CreatedAt,
                UpdatedAt = destination.UpdatedAt
            };

            return Ok(ApiResponse<DestinationDto>.SuccessResponse(updatedDestinationDto, "Destination updated successfully"));
        }

        // DELETE: api/Destinations/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteDestination(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
            {
                return NotFound(ApiResponse<bool>.FailureResponse("Destination not found"));
            }

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Destination deleted successfully"));
        }

        // GET: api/Destinations/search
        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<DestinationDto>>>> SearchDestinations(
            [FromQuery] string q = "", 
            [FromQuery] string region = "", 
            [FromQuery] string category = "", 
            [FromQuery] float? minRating = null,
            [FromQuery] PaginationParams paginationParams = null)
        {
            if (paginationParams == null)
            {
                paginationParams = new PaginationParams();
            }

            var query = _context.Destinations.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(d => d.Name.Contains(q) || d.Description.Contains(q));
            }

            if (!string.IsNullOrEmpty(region))
            {
                query = query.Where(d => d.Region == region);
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(d => d.Category == category);
            }

            if (minRating.HasValue)
            {
                query = query.Where(d => d.AverageRating >= minRating.Value);
            }

            // Apply sorting in a safe way
            if (string.IsNullOrEmpty(paginationParams.SortBy) || paginationParams.SortBy.ToLower() == "id")
            {
                // Default sorting by Id
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.Id) 
                    : query.OrderBy(d => d.Id);
            }
            else if (paginationParams.SortBy.ToLower() == "name")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.Name) 
                    : query.OrderBy(d => d.Name);
            }
            else if (paginationParams.SortBy.ToLower() == "averagerating")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.AverageRating) 
                    : query.OrderBy(d => d.AverageRating);
            }
            else if (paginationParams.SortBy.ToLower() == "createdat")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.CreatedAt) 
                    : query.OrderBy(d => d.CreatedAt);
            }
            else if (paginationParams.SortBy.ToLower() == "updatedat")
            {
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.UpdatedAt) 
                    : query.OrderBy(d => d.UpdatedAt);
            }
            else
            {
                // Default to Id if an invalid sort field is provided
                query = paginationParams.SortOrder.ToLower() == "desc" 
                    ? query.OrderByDescending(d => d.Id) 
                    : query.OrderBy(d => d.Id);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)paginationParams.PageSize);

            var destinations = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            var destinationDtos = destinations.Select(d => new DestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Location = d.Location,
                AverageRating = d.AverageRating,
                PointsOfInterest = string.IsNullOrEmpty(d.PointsOfInterest) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(d.PointsOfInterest),
                ImageUrls = string.IsNullOrEmpty(d.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(d.ImageUrls),
                CoverImageUrl = d.CoverImageUrl,
                Region = d.Region,
                Category = d.Category,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            var paginatedResponse = new PaginatedResponse<DestinationDto>
            {
                Items = destinationDtos,
                TotalCount = totalCount,
                PageNumber = paginationParams.PageNumber,
                PageSize = paginationParams.PageSize,
                TotalPages = totalPages,
                HasPrevious = paginationParams.PageNumber > 1,
                HasNext = paginationParams.PageNumber < totalPages
            };

            return Ok(ApiResponse<PaginatedResponse<DestinationDto>>.SuccessResponse(paginatedResponse));
        }

        // GET: api/Destinations/popular
        [HttpGet("popular")]
        public async Task<ActionResult<ApiResponse<List<DestinationDto>>>> GetPopularDestinations([FromQuery] int count = 5)
        {
            var destinations = await _context.Destinations
                .OrderByDescending(d => d.AverageRating)
                .Take(count)
                .ToListAsync();

            var destinationDtos = destinations.Select(d => new DestinationDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Location = d.Location,
                AverageRating = d.AverageRating,
                PointsOfInterest = string.IsNullOrEmpty(d.PointsOfInterest) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(d.PointsOfInterest),
                ImageUrls = string.IsNullOrEmpty(d.ImageUrls) 
                    ? new List<string>() 
                    : JsonSerializer.Deserialize<List<string>>(d.ImageUrls),
                CoverImageUrl = d.CoverImageUrl,
                Region = d.Region,
                Category = d.Category,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();

            return Ok(ApiResponse<List<DestinationDto>>.SuccessResponse(destinationDtos));
        }

        // GET: api/Destinations/categories
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetCategories()
        {
            var categories = await _context.Destinations
                .Select(d => d.Category)
                .Distinct()
                .Where(c => !string.IsNullOrEmpty(c))
                .ToListAsync();

            return Ok(ApiResponse<List<string>>.SuccessResponse(categories));
        }

        // GET: api/Destinations/regions
        [HttpGet("regions")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetRegions()
        {
            var regions = await _context.Destinations
                .Select(d => d.Region)
                .Distinct()
                .Where(r => !string.IsNullOrEmpty(r))
                .ToListAsync();

            return Ok(ApiResponse<List<string>>.SuccessResponse(regions));
        }

        private bool DestinationExists(int id)
        {
            return _context.Destinations.Any(e => e.Id == id);
        }
    }
} 
using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Features.Properties.Commands.AddPropertyImages;
using Baytology.Application.Features.Properties.Commands.CreateAgentReview;
using Baytology.Application.Features.Properties.Commands.CreateProperty;
using Baytology.Application.Features.Properties.Commands.DeleteProperty;
using Baytology.Application.Features.Properties.Commands.RecordPropertyView;
using Baytology.Application.Features.Properties.Commands.SaveProperty;
using Baytology.Application.Features.Properties.Commands.UnsaveProperty;
using Baytology.Application.Features.Properties.Commands.UpdateProperty;
using Baytology.Application.Common.Models;
using Baytology.Application.Features.Properties.Dtos;
using Baytology.Application.Features.Properties.Queries.GetProperties;
using Baytology.Application.Features.Properties.Queries.GetPropertyById;
using Baytology.Application.Features.Properties.Queries.GetPropertySavedState;
using Baytology.Application.Features.Properties.Queries.GetSavedProperties;
using Baytology.Contracts.Common;
using Baytology.Contracts.Requests.Properties;
using Baytology.Contracts.Responses.Properties;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
public class PropertiesController(ISender sender) : ApiController
{
    [HttpGet]
    [EndpointSummary("Get paginated list of properties with filters")]
    [ProducesResponseType(typeof(PaginatedList<PropertyListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription(
        "Returns a paginated list of properties. Supports filtering by city, district, property type, listing type, price range, bedrooms, and pagination.")]
    [EndpointName("GetProperties")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetProperties([FromQuery] GetPropertiesRequest request, CancellationToken ct)
    {
        var query = new GetPropertiesQuery(
            City: request.City,
            District: request.District,
            PropertyType: request.PropertyType?.ToString(),
            ListingType: request.ListingType?.ToString(),
            MinPrice: request.MinPrice,
            MaxPrice: request.MaxPrice,
            MinArea: request.MinArea,
            MaxArea: request.MaxArea,
            MinBedrooms: request.MinBedrooms,
            MaxBedrooms: request.MaxBedrooms,
            AgentUserId: request.AgentUserId,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get property by ID")]
    [ProducesResponseType(typeof(PropertyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Returns full property details by property id, including images and amenity when available.")]
    [EndpointName("GetPropertyById")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetProperty(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPropertyByIdQuery(id), ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("saved")]
    [Authorize]
    [EndpointSummary("Get saved properties for the current user")]
    [ProducesResponseType(typeof(PaginatedList<PropertyListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [EndpointDescription("Returns the authenticated user's saved properties as a paginated list.")]
    [EndpointName("GetSavedProperties")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetSavedProperties([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetSavedPropertiesQuery(userId, pageRequest.PageNumber, pageRequest.PageSize), ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [Authorize(Roles = "Agent")]
    [EndpointSummary("Create a new property listing")]
    [ProducesResponseType(typeof(CreatePropertyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Creates a new property listing for the authenticated agent. AgentUserId is taken from the JWT.")]
    [EndpointName("CreateProperty")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateProperty([FromBody] CreatePropertyRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new CreatePropertyCommand(
            userId,
            request.Title,
            request.Description,
            (Baytology.Domain.Common.Enums.PropertyType)request.PropertyType,
            (Baytology.Domain.Common.Enums.ListingType)request.ListingType,
            request.Price,
            request.Area,
            request.Bedrooms,
            request.Bathrooms,
            request.Floor,
            request.TotalFloors,
            request.AddressLine,
            request.City,
            request.District,
            request.ZipCode,
            request.Latitude,
            request.Longitude,
            request.HasParking,
            request.HasPool,
            request.HasGym,
            request.HasElevator,
            request.HasSecurity,
            request.HasBalcony,
            request.HasGarden,
            request.HasCentralAC,
            (Baytology.Domain.Common.Enums.FurnishingStatus)request.FurnishingStatus,
            request.ViewType is null
                ? null
                : (Baytology.Domain.Common.Enums.ViewType)request.ViewType,
            request.ImageUrls);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(id => CreatedAtAction(nameof(GetProperty), new { id }, new CreatePropertyResponse(id)), Problem);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Agent")]
    [EndpointSummary("Update a property listing")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] UpdatePropertyRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new UpdatePropertyCommand(
            id,
            userId,
            request.Title,
            request.Description,
            (Baytology.Domain.Common.Enums.PropertyType)request.PropertyType,
            (Baytology.Domain.Common.Enums.ListingType)request.ListingType,
            request.Price,
            request.Area,
            request.Bedrooms,
            request.Bathrooms,
            request.Floor,
            request.TotalFloors,
            request.AddressLine,
            request.City,
            request.District,
            request.ZipCode,
            request.Latitude,
            request.Longitude,
            request.IsFeatured,
            request.HasParking,
            request.HasPool,
            request.HasGym,
            request.HasElevator,
            request.HasSecurity,
            request.HasBalcony,
            request.HasGarden,
            request.HasCentralAC,
            (Baytology.Domain.Common.Enums.FurnishingStatus)request.FurnishingStatus,
            request.ViewType is null
                ? null
                : (Baytology.Domain.Common.Enums.ViewType)request.ViewType);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Agent")]
    [EndpointSummary("Delete a property listing")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProperty(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new DeletePropertyCommand(id, userId);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = "Agent")]
    [EndpointSummary("Add images to a property")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddImages(Guid id, [FromBody] AddPropertyImagesRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new AddPropertyImagesCommand(id, userId, request.ImageUrls);
        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("{id:guid}/save")]
    [Authorize]
    [EndpointSummary("Save property to favorites")]
    [ProducesResponseType(typeof(SavePropertyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Saves a property to the current user's favorites. If the property is already saved, returns 409 Conflict.")]
    [EndpointName("SaveProperty")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> SaveProperty(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new SavePropertyCommand(userId, id), ct);
        return result.Match(r => Ok(new SavePropertyResponse(r)), Problem);
    }

    [HttpGet("{id:guid}/save")]
    [Authorize]
    [EndpointSummary("Check whether a property is saved by the current user")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Returns true when the authenticated user already has this property in favorites.")]
    [EndpointName("GetPropertySavedState")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetPropertySavedState(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new GetPropertySavedStateQuery(userId, id), ct);
        return result.Match(isSaved => Ok(isSaved), Problem);
    }

    [HttpDelete("{id:guid}/save")]
    [Authorize]
    [EndpointSummary("Remove property from favorites")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Removes a property from the current user's favorites.")]
    [EndpointName("UnsaveProperty")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> UnsaveProperty(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await sender.Send(new UnsavePropertyCommand(userId, id), ct);
        return result.Match(_ => Ok(), Problem);
    }

    [HttpPost("{id:guid}/view")]
    [EndpointSummary("Record a property view")]
    [ProducesResponseType(typeof(RecordPropertyViewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Records a property view for analytics.")]
    [EndpointName("RecordPropertyView")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> RecordView(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await sender.Send(new RecordPropertyViewCommand(id, userId, ip), ct);
        return result.Match(r => Ok(new RecordPropertyViewResponse(r)), Problem);
    }

    [HttpPost("reviews")]
    [Authorize]
    [EndpointSummary("Create an agent review")]
    [ProducesResponseType(typeof(CreateAgentReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Creates an agent review by the authenticated user. Rating must be between 1 and 5.")]
    [EndpointName("CreateAgentReview")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateReview([FromBody] CreateAgentReviewRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var commandToSend = new CreateAgentReviewCommand(
            request.AgentUserId,
            userId,
            request.PropertyId,
            request.Rating,
            request.Comment);
        var result = await sender.Send(commandToSend, ct);
        return result.Match(id => Ok(new CreateAgentReviewResponse(id)), Problem);
    }
}

using System.Security.Claims;

using Asp.Versioning;

using Baytology.Application.Common.Models;
using Baytology.Application.Features.Bookings.Commands.CreateBooking;
using Baytology.Application.Features.Bookings.Commands.UpdateBookingStatus;
using Baytology.Application.Features.Bookings.Dtos;
using Baytology.Application.Features.Bookings.Queries.GetBookingById;
using Baytology.Application.Features.Bookings.Queries.GetBookings;
using Baytology.Contracts.Common;
using Baytology.Contracts.Requests.Bookings;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[ApiVersion("1")]
[Authorize]
public sealed class BookingsController(ISender sender) : ApiController
{
    [HttpGet]
    [EndpointSummary("Get all bookings for the current user")]
    [ProducesResponseType(typeof(PaginatedList<BookingListItemDto>), StatusCodes.Status200OK)]
    [EndpointDescription("Returns a paginated list of bookings where the current user is either the buyer or the agent.")]
    [EndpointName("GetBookings")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetBookings([FromQuery] PageRequest pageRequest, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetBookingsQuery(userId, pageRequest.PageNumber, pageRequest.PageSize);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get booking by id")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointDescription("Returns the details of a specific booking.")]
    [EndpointName("GetBookingById")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> GetBookingById(Guid id, CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var query = new GetBookingByIdQuery(id, userId);
        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    [HttpPost]
    [EndpointSummary("Create a new booking and start payment")]
    [ProducesResponseType(typeof(CreateBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Creates a new booking for the authenticated user and starts the payment flow (escrow).")]
    [EndpointName("CreateBooking")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CreateBookingCommand(
            request.PropertyId,
            userId,
            request.StartDate,
            request.EndDate,
            request.Amount,
            request.CommissionRate,
            request.Currency,
            request.PayerEmail,
            request.PayerName,
            request.PayerPhone);

        var result = await sender.Send(command, ct);
        return result.Match(
            response => CreatedAtAction(nameof(GetBookingById), new { id = response.BookingId }, response),
            Problem);
    }

    [HttpPatch("{bookingId:guid}/status")]
    [EndpointSummary("Update booking status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Updates the booking status. The assigned agent can confirm or cancel, and the booking creator can cancel pending bookings.")]
    [EndpointName("UpdateBookingStatus")]
    [MapToApiVersion("1")]
    public async Task<IActionResult> UpdateStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UpdateBookingStatusCommand(
            bookingId,
            userId,
            (Baytology.Domain.Common.Enums.BookingStatus)request.Status);

        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }
}

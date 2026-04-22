using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Results;
using Baytology.Domain.Payments;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.Payments.Commands.RequestRefund;

public record RequestRefundCommand(Guid PaymentId, string RequestedBy, string Reason, decimal Amount)
    : IRequest<Result<Guid>>;

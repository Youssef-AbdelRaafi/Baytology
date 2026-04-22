using Baytology.Application.Common.Caching;
using Baytology.Application.Features.UserProfiles.Dtos;
using Baytology.Domain.Common.Results;
using Baytology.Domain.UserProfiles;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Baytology.Application.Features.UserProfiles.Queries.GetUserProfile;

public record GetUserProfileQuery(string UserId) : IRequest<Result<UserProfileDto>>;

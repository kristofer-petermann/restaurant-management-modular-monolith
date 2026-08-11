using MediatR;

namespace Shared.Contracts.Identity;

public sealed record GetUserDisplayNameQuery(Guid UserId) : IRequest<UserDisplayNameResponse?>;

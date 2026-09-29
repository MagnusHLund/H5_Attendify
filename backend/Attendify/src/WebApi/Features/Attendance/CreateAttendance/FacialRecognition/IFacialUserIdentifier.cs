using Attendify.Common.Domain.Users;

public interface IFacialUserIdentifier
{
    Task<UserId> IdentifyUserAsync(IFormFile image, CancellationToken cancellationToken);
}
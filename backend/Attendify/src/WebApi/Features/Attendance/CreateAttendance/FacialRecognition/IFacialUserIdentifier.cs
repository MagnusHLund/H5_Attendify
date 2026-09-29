using Attendify.Common.Domain.Users;

public interface IFacialUserIdentifier
{
    Task<UserId> IdentifyUserAsync(IFormFile base64Image, CancellationToken cancellationToken);
}
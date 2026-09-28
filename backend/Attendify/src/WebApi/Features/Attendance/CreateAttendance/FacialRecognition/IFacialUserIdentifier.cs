using Attendify.Common.Domain.Users;

public interface IFacialUserIdentifier
{
    Task<UserId> IdentifyUserAsync(string base64Image, CancellationToken cancellationToken);
}
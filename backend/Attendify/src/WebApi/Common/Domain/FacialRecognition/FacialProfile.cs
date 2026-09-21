using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.FacialRecognition;

public sealed class FacialProfile : AggregateRoot<int>
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public ICollection<FacialEmbedding> FacialEmbeddings { get; } = [];

    private FacialProfile() { }

    public static FacialProfile Create(User user) =>
        new()
        {
            User = user,
        };
}

using Attendify.Common.Domain.Base;
using Attendify.Common.Domain.Users;

namespace Attendify.Common.Domain.Schools;

public sealed class School : AggregateRoot<Guid>
{
    public const int NameMaxLength = 256;

    public string Name
    {
        get;
        set
        {
            ThrowIfNullOrWhiteSpace(value, nameof(Name));
            ThrowIfGreaterThan(value.Length, NameMaxLength, nameof(Name));
            field = value;
        }
    } = null!;

    public ICollection<User> Users { get; } = [];

    private School() { }
}

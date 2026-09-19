using Attendify.Common.Domain.Base;

namespace Attendify.Common.Domain.EducationalInstitute;

[ValueObject<Guid>]
public readonly partial struct EducationalInstituteId;

public class EducationalInstitute : AggregateRoot<EducationalInstituteId>
{
    public string Name { get; set; } = null!;
}

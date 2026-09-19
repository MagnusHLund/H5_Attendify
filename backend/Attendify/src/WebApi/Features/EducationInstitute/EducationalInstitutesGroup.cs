namespace Attendify.Features.EducationInstitute;

public sealed class EducationalInstitutesGroup : Group
{
    public EducationalInstitutesGroup()
    {
        Configure(
            "educational-institutes",
            ep => ep.Description(x => x.ProducesProblemDetails(500))
        );
    }
}

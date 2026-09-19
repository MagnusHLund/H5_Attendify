namespace Attendify.Features.EducationInstitute.GetEducationalInstitutes;

public sealed class GetEducationalInstitutesSummary : Summary<GetEducationalInstitutesEndpoint>
{
    public GetEducationalInstitutesSummary()
    {
        Summary = "Gets all educational institutes";
        Description = "Retrieves all educational institutes available for student registration.";

        Response<IReadOnlyList<GetEducationalInstitutesResponse>>(
            200,
            "The available educational institutes."
        );
    }
}

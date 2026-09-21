namespace Attendify.Features.EducationInstitute.GetEducationalInstitutes;

public sealed class GetEducationalInstitutesEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<IReadOnlyList<GetEducationalInstitutesResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<EducationalInstitutesGroup>();
        AllowAnonymous();
        Description(x => x.WithName("GetAllEducationalInstitutes"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        IReadOnlyList<GetEducationalInstitutesResponse> educationalInstitutes = await dbContext
            .EducationalInstitutes.OrderBy(institute => institute.Name)
            .Select(institute => new GetEducationalInstitutesResponse(institute.Id, institute.Name))
            .ToListAsync(ct);

        await Send.OkAsync(educationalInstitutes, ct);
    }
}

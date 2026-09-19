namespace Attendify.Features.EducationInstitute.GetEducationalInstitutes;

public sealed class GetEducationalInstitutesEndpoint(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<IReadOnlyList<GetEducationalInstitutesResponse>>
{
    public override void Configure()
    {
        Get("/");
        Group<EducationalInstitutesGroup>();
        Description(x => x.WithName("GetAllEducationalInstitutes"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        IReadOnlyList<GetEducationalInstitutesResponse> educationalInstitutes = await dbContext
            .EducationalInstitutes.Select(x => new GetEducationalInstitutesResponse(
                x.Id.Value,
                x.Name
            ))
            .ToListAsync(ct);

        await Send.OkAsync(educationalInstitutes, ct);
    }
}

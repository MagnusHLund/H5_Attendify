using System.Net;
using System.Net.Http.Json;
using Attendify.Common.Domain.EducationalInstitute;
using Attendify.Features.EducationInstitute.GetEducationalInstitutes;
using Attendify.IntegrationTests.Common;

namespace Attendify.IntegrationTests;

public sealed class EducationalInstituteEndpointTests(TestingDatabaseFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task GetEducationalInstitutes_ReturnsInstitutesInNameOrder()
    {
        EducationalInstitute zulu = EducationalInstitute.Create("Zulu Academy");
        zulu.SetCreated(TimeProvider.System, null);
        EducationalInstitute alpha = EducationalInstitute.Create("Alpha College");
        alpha.SetCreated(TimeProvider.System, null);
        await AddRangeAsync(new[] { zulu, alpha });

        using HttpClient client = GetAnonymousClient();
        using HttpResponseMessage response = await client.GetAsync(
            "/api/educational-institutes",
            CancellationToken
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        GetEducationalInstitutesResponse[]? institutes = await response.Content
            .ReadFromJsonAsync<GetEducationalInstitutesResponse[]>(CancellationToken);
        Assert.NotNull(institutes);
        Assert.Equal(new[] { "Alpha College", "Zulu Academy" }, institutes.Select(item => item.Name));
    }
}

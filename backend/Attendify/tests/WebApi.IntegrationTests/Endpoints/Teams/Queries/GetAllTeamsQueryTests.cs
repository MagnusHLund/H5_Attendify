using FastEndpoints;
using Attendify.Features.Teams.GetAllTeams;
using Attendify.IntegrationTests.Common;
using Attendify.IntegrationTests.Common.Factories;

namespace Attendify.IntegrationTests.Endpoints.Teams.Queries;

public class GetAllTeamsQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Query_ShouldReturnAllTeams()
    {
        // Arrange
        const int entityCount = 10;
        var entities = TeamFactory.Generate(entityCount);
        await AddRangeAsync(entities);
        var client = GetAnonymousClient();

        // Act
        var result = await client.GETAsync<GetAllTeamsEndpoint, GetAllTeamsResponse>();

        // Assert
        result.Response.IsSuccessStatusCode.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Teams.Should().HaveCount(entityCount);

        var firstTeam = result.Result.Teams.First();
        firstTeam.Id.Should().NotBeEmpty();
        firstTeam.Name.Should().NotBeEmpty();
    }
}
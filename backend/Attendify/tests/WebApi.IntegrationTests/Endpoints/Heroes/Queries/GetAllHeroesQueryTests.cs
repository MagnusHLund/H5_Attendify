using FastEndpoints;
using Attendify.Features.Heroes.GetAllHeroes;
using Attendify.IntegrationTests.Common;
using Attendify.IntegrationTests.Common.Factories;

namespace Attendify.IntegrationTests.Endpoints.Heroes.Queries;

public class GetAllHeroesQueryTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Query_ShouldReturnAllHeroes()
    {
        // Arrange
        const int entityCount = 10;
        var entities = HeroFactory.Generate(entityCount);
        await AddRangeAsync(entities);
        var client = GetAnonymousClient();

        // Act
        var result = await client.GETAsync<GetAllHeroesEndpoint, GetAllHeroesResponse>();

        // Assert
        result.Response.IsSuccessStatusCode.Should().BeTrue();
        result.Result.Should().NotBeNull();
        result.Result!.Heroes.Should().HaveCount(entityCount);
    }
}
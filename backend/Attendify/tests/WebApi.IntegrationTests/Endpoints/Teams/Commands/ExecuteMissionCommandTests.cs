using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Attendify.Common.Domain.Teams;
using Attendify.Features.Teams.ExecuteMission;
using Attendify.IntegrationTests.Common;
using Attendify.IntegrationTests.Common.Factories;
using System.Net;

namespace Attendify.IntegrationTests.Endpoints.Teams.Commands;

public class ExecuteMissionCommandTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_ShouldExecuteMission()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        team.AddHero(hero);
        await AddAsync(team);
        var cmd = new ExecuteMissionRequest(team.Id.Value, "Save the world");
        var client = GetAnonymousClient();

        // Act
        var result = await client.POSTAsync<ExecuteMissionEndpoint, ExecuteMissionRequest>(cmd);

        // Assert
        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(TeamSpec.ById(team.Id))
            .FirstOrDefaultAsync(CancellationToken);
        var mission = updatedTeam!.Missions.First();

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        updatedTeam!.Missions.Should().HaveCount(1);
        updatedTeam.Status.Should().Be(TeamStatus.OnMission);
        mission.Status.Should().Be(MissionStatus.InProgress);
    }
}
using Ardalis.Specification.EntityFrameworkCore;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Attendify.Common.Domain.Heroes;
using Attendify.Common.Domain.Teams;
using Attendify.Features.Heroes.UpdateHero;
using Attendify.IntegrationTests.Common;
using Attendify.IntegrationTests.Common.Factories;
using Attendify.IntegrationTests.Common.Utilities;
using System.Net;

namespace Attendify.IntegrationTests.Endpoints.Teams.Events;

public class UpdatePowerLevelEventTests(TestingDatabaseFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task Command_UpdatePowerOnTeam()
    {
        // Arrange
        var hero = HeroFactory.Generate();
        var team = TeamFactory.Generate();
        List<Power> powers = [new Power("Strength", 10)];
        hero.UpdatePowers(powers);
        team.AddHero(hero);
        await AddAsync(team);
        powers.Add(new Power("Speed", 5));
        var powerDtos = powers.Select(p => new UpdateHeroRequest.HeroPowerDto(p.Name, p.PowerLevel));
        var cmd = new UpdateHeroRequest(hero.Name, hero.Alias, hero.Id.Value, powerDtos);
        var client = GetAnonymousClient();

        // Act
        var result = await client.PUTAsync<UpdateHeroEndpoint, UpdateHeroRequest>(cmd);

        // Assert
        await Wait.ForEventualConsistency();
        var updatedTeam = await GetQueryable<Team>()
            .WithSpecification(TeamSpec.ById(team.Id))
            .FirstOrDefaultAsync(CancellationToken);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        updatedTeam!.TotalPowerLevel.Should().Be(15);
    }
}
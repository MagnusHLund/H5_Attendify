using System.Net;
using System.Net.Http.Json;
using Attendify.Common.Domain.EducationalInstitute;
using Attendify.Common.Domain.Users;
using Attendify.Common.Persistence;
using Attendify.Common.Services;
using Attendify.Features.Auth.LoginWithPassword;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Attendify.IntegrationTests.Common;

/// <summary>
/// Integration tests inherit from this to access helper classes
/// </summary>
[Collection<TestingDatabaseFixtureCollection>]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private readonly TestingDatabaseFixture _fixture;
    private readonly ApplicationDbContext _dbContext;

    protected IntegrationTestBase(TestingDatabaseFixture fixture)
    {
        _fixture = fixture;
        _scope = _fixture.CreateScope();
        _dbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    /// <summary>
    /// Setup for each test
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        await _fixture.TestSetup();
    }

    protected async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        await _dbContext.AddAsync(entity, CancellationToken);
        await _dbContext.SaveChangesAsync(CancellationToken);
    }

    protected async Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities)
        where TEntity : class
    {
        await _dbContext.AddRangeAsync(entities, CancellationToken);
        await _dbContext.SaveChangesAsync(CancellationToken);
    }

    protected HttpClient GetAnonymousClient() => _fixture.CreateClient();

    protected TService GetService<TService>()
        where TService : notnull
    {
        return _scope.ServiceProvider.GetRequiredService<TService>();
    }

    protected async Task<HttpClient> CreateAuthenticatedStudentClientAsync(
        string email = "student@example.com",
        string password = "correct horse battery",
        string studentId = "student-123",
        string educationalInstituteName = "Integration test school"
    )
    {
        EducationalInstitute institute = EducationalInstitute.Create(educationalInstituteName);
        institute.SetCreated(TimeProvider.System, null);
        await AddAsync(institute);

        User student = User.Create(
            institute.Id,
            email,
            "temporary-password-hash",
            GetService<IStudentIdProtector>().Protect(studentId)
        );
        student.UpdatePassword(GetService<IPasswordHasher<User>>().HashPassword(student, password));
        student.SetCreated(TimeProvider.System, null);
        await AddAsync(student);

        HttpClient client = GetAnonymousClient();
        using HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginWithPasswordRequest { Email = student.Email, Password = password },
            CancellationToken
        );
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.StartsWith("AccessToken=", StringComparison.Ordinal));
        Assert.Contains(response.Headers.GetValues("Set-Cookie"), cookie =>
            cookie.StartsWith("RefreshToken=", StringComparison.Ordinal));

        return client;
    }

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask DisposeAsync()
    {
        _scope.Dispose();
        return ValueTask.CompletedTask;
    }
}
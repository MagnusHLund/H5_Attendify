using System.Reflection;

namespace Attendify.ArchitectureTests.Common;

public abstract class TestBase
{
    protected const string DomainAssemblyName = "Domain";
    protected const string CommandsAssemblyName = "Commands";
    protected const string QueriesAssemblyName = "Queries";
    
    protected static readonly Assembly RootAssembly = typeof(Attendify.Program).Assembly;
}
using Mono.Cecil;

namespace Attendify.ArchitectureTests.Common;

public class IsNotEnumRule : ICustomRule
{
    public bool MeetsRule(TypeDefinition type) => !type.IsEnum;
}
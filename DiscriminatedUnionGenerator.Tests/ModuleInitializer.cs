using System.Runtime.CompilerServices;
using VerifyTests;

namespace DiscriminatedUnionGenerator.Tests
{
    internal static class ModuleInitializer
    {
        [ModuleInitializer]
        public static void Init()
        {
            VerifySourceGenerators.Enable();
        }
    }
}

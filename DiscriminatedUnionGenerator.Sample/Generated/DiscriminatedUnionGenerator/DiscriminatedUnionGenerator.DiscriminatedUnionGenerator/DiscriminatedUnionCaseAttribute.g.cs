#nullable enable

namespace DiscriminatedUnionGenerator
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true)]
    internal sealed class DiscriminatedUnionCaseAttribute : System.Attribute
    {
        public DiscriminatedUnionCaseAttribute(System.Type type)
        {
            Type = type;
        }

        public DiscriminatedUnionCaseAttribute(System.Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public System.Type Type { get; }

        public string? Name { get; }
    }
}
#nullable enable

namespace DiscriminatedUnionGenerator
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true)]
    public class DiscriminatedUnionCaseAttribute : System.Attribute
    {
        public DiscriminatedUnionCaseAttribute(Type type)
        {
            Type = type;
        }

        public DiscriminatedUnionCaseAttribute(Type type, string? name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; }

        public string? Name { get; }
    }
}
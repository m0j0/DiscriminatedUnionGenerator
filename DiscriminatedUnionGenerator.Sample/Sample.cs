namespace DiscriminatedUnionGenerator.Sample;

public sealed record NotFound;

public sealed record Duplicate(string Data);

public readonly record struct Success;

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
[DiscriminatedUnionCase(typeof(Success))]
public sealed partial class SampleClass
{
}

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
[DiscriminatedUnionCase(typeof(Success))]
public sealed partial record SampleRecord;

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
[DiscriminatedUnionCase(typeof(Success))]
public readonly partial struct SampleStruct
{
}

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
[DiscriminatedUnionCase(typeof(Success))]
public readonly partial record struct SampleStructRecord;

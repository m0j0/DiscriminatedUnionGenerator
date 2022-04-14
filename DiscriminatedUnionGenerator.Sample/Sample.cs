namespace DiscriminatedUnionGenerator.Sample;

public sealed record NotFound;

public sealed record Duplicate(string Data);

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
public sealed partial class SampleClass
{
}

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
public sealed partial record SampleRecord
{
}

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
public readonly partial struct SampleStruct
{
}

[DiscriminatedUnionCase(typeof(NotFound))]
[DiscriminatedUnionCase(typeof(string), "Str")]
[DiscriminatedUnionCase(typeof(Duplicate))]
[DiscriminatedUnionCase(typeof(int), "Integ")]
public readonly partial record struct SampleStructRecord;

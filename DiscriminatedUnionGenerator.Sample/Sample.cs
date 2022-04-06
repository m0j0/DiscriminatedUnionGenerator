using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscriminatedUnionGenerator;

namespace DiscriminatedUnionGenerator.Sample
{
    [DiscriminatedUnionCase(typeof(NotFound))]
    [DiscriminatedUnionCase(typeof(object), "Test")]
    [DiscriminatedUnionCase(typeof(Duplicate))]
    public sealed partial class Sample
    {
    }

    public sealed record NotFound;
    public sealed record Duplicate(string Data);
}

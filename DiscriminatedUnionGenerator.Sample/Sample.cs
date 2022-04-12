using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscriminatedUnionGenerator;

namespace DiscriminatedUnionGenerator.Sample
{
    [DiscriminatedUnionCase(typeof(NotFound))]
    [DiscriminatedUnionCase(typeof(object), "Test2")] 
    [DiscriminatedUnionCase(typeof(Duplicate))]
    //[DiscriminatedUnionCase(typeof(Duplicate), "Duplicate2")]
    public sealed partial class Sample
    {
    }

    public sealed record NotFound;
    public sealed record Duplicate(string Data);
} 

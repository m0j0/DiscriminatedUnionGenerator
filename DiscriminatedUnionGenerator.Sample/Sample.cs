using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscriminatedUnionGenerator;

namespace DiscriminatedUnionGenerator.Sample
{
    [DiscriminatedUnionCase(typeof(object))]
    [DiscriminatedUnionCase(typeof(object), "Test")]
    public sealed class Sample
    {
    }
}

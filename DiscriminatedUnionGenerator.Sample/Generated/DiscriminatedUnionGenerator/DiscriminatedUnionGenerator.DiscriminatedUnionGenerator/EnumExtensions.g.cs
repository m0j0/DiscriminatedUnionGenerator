
namespace DiscriminatedUnionGenerator.Sample
{
    public static partial class EnumExtensions
    {
                public static string ToStringFast(this DiscriminatedUnionGenerator.Sample.Sample value)
                    => value switch
                    {
                    _ => value.ToString(),
                };

    }
}
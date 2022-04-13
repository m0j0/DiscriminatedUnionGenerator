namespace DiscriminatedUnionGenerator.Sample
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var sample = new Sample(new Duplicate("DUPL "));
            var str = sample.Tag switch
            {
                Sample.Case.NotFound => "notFound",
                Sample.Case.Str => sample.AsStr,
                Sample.Case.Duplicate => sample.AsDuplicate.Data,
                _ => "Unknown"
            };
            Console.WriteLine(str);
        }
    }
}

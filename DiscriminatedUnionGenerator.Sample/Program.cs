namespace DiscriminatedUnionGenerator.Sample
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var sample = new Sample(new Duplicate("DUPL"));
            var str = sample.Value switch
            {
                NotFound notFound => "notFound",
                string s => s,
                Duplicate duplicate => duplicate.Data,
                _ => "Unknown"
            };
            Console.WriteLine(str);
        }
    }
}

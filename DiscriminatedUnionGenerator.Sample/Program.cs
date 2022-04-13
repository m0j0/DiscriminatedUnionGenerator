namespace DiscriminatedUnionGenerator.Sample
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var sample = CreateSample();
            var str = sample.Tag switch
            {
                Sample.Case.NotFound => "notFound",
                Sample.Case.Str => sample.AsStr,
                Sample.Case.Duplicate => sample.AsDuplicate.Data,
                _ => "Unknown"
            };
            Console.WriteLine(str);

            var sample2 = new Sample("text");
            var strFromSample = (string) sample2;
            Console.WriteLine(strFromSample);
        }

        private static Sample CreateSample()
        {
            return new Duplicate("DUPL  ");
        }
    }
}

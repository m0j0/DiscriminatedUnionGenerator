namespace DiscriminatedUnionGenerator.Sample
{
    internal static class Program
    {
        public static async Task Main(string[] args)
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

            Console.WriteLine(sample.Match(notFound => "not found",
                    st => st,
                    duplicate => duplicate.Data
                )
            );
            Console.WriteLine(await sample.MatchAsync(notFound => Task.FromResult("not found"),
                    Task.FromResult,
                    async duplicate =>
                    {
                        await Task.Delay(1);
                        return duplicate.Data;
                    })
            );

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

namespace DiscriminatedUnionGenerator.Sample
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var sample = CreateSample();
            var str = sample.Tag switch
            {
                SampleClass.Case.NotFound => "notFound",
                SampleClass.Case.Str => sample.AsStr,
                SampleClass.Case.Duplicate => sample.AsDuplicate.Data,
                _ => "Unknown"
            };
            Console.WriteLine(str);

            Console.WriteLine(sample.Match(notFound => "not found",
                    st => st,
                    duplicate => duplicate.Data,
                    integ => integ.ToString(),
                    success => success.ToString()
                )
            );
            Console.WriteLine(await sample.MatchAsync(notFound => Task.FromResult("not found"),
                    Task.FromResult,
                    async duplicate =>
                    {
                        await Task.Delay(1);
                        return duplicate.Data;
                    },
                    integ => Task.FromResult(integ.ToString()),
                    async success =>
                    {
                        await Task.Delay(1);
                        return "success";
                    })
            );

            var sample2 = new SampleRecord("text");
            var strFromSample = (string) sample2;
            Console.WriteLine(strFromSample);
        }

        private static SampleClass CreateSample()
        {
            return new Duplicate("DUPL  ");
        }
    }
}

#nullable enable

namespace DiscriminatedUnionGenerator.Sample
{
    partial class Sample
    {
        private readonly int _tag;
        private readonly DiscriminatedUnionGenerator.Sample.NotFound? _case0;
        private readonly string? _case1;
        private readonly DiscriminatedUnionGenerator.Sample.Duplicate? _case2;

        public Sample(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = 0;
            _case0 = notFound;
        }

        public Sample(string str)
        {
            _tag = 1;
            _case1 = str;
        }

        public Sample(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = 2;
            _case2 = duplicate;
        }

        public bool IsNotFound => _tag == 0;
        public bool IsStr => _tag == 1;
        public bool IsDuplicate => _tag == 2;

        public DiscriminatedUnionGenerator.Sample.NotFound AsNotFound => _tag == 0 ? _case0! : throw new InvalidOperationException();
        public string AsStr => _tag == 1 ? _case1! : throw new InvalidOperationException();
        public DiscriminatedUnionGenerator.Sample.Duplicate AsDuplicate => _tag == 2 ? _case2! : throw new InvalidOperationException();

        public object Value => _tag switch
        {
            0 => _case0!,
            1 => _case1!,
            2 => _case2!,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}



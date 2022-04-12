#nullable enable

namespace DiscriminatedUnionGenerator.Sample
{
    partial class Sample
    {
        private readonly int _tag;
        private readonly DiscriminatedUnionGenerator.Sample.NotFound? _case0;
        private readonly object? _case1;
        private readonly DiscriminatedUnionGenerator.Sample.Duplicate? _case2;

        public Sample(DiscriminatedUnionGenerator.Sample.NotFound NotFound)
        {
            _tag = 0;
            _case0 = NotFound;
        }

        public Sample(object Test2)
        {
            _tag = 1;
            _case1 = Test2;
        }

        public Sample(DiscriminatedUnionGenerator.Sample.Duplicate Duplicate)
        {
            _tag = 2;
            _case2 = Duplicate;
        }

        public bool IsNotFound => _tag == 0;
        public bool IsTest2 => _tag == 1;
        public bool IsDuplicate => _tag == 2;

        public DiscriminatedUnionGenerator.Sample.NotFound AsNotFound => _tag == 0 ? _case0! : throw new InvalidOperationException();
        public object AsTest2 => _tag == 1 ? _case1! : throw new InvalidOperationException();
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



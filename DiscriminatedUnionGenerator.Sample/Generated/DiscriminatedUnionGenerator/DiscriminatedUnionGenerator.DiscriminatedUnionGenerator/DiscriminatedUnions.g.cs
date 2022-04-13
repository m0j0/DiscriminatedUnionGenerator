#nullable enable

namespace DiscriminatedUnionGenerator.Sample
{
    partial class Sample
    {
        public enum Case
        {
            NotFound = 1,
            Test2 = 2,
            Str = 3,
            Duplicate = 4,
        }

        private readonly Case _tag;
        private readonly DiscriminatedUnionGenerator.Sample.NotFound? _case1;
        private readonly object? _case2;
        private readonly string? _case3;
        private readonly DiscriminatedUnionGenerator.Sample.Duplicate? _case4;

        public Sample(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _case1 = notFound;
        }

        public Sample(object test2)
        {
            _tag = Case.Test2;
            _case2 = test2;
        }

        public Sample(string str)
        {
            _tag = Case.Str;
            _case3 = str;
        }

        public Sample(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _case4 = duplicate;
        }

        public bool IsNotFound => _tag == Case.NotFound;
        public bool IsTest2 => _tag == Case.Test2;
        public bool IsStr => _tag == Case.Str;
        public bool IsDuplicate => _tag == Case.Duplicate;

        public DiscriminatedUnionGenerator.Sample.NotFound AsNotFound => _tag == Case.NotFound ? _case1! : throw new InvalidOperationException();
        public object AsTest2 => _tag == Case.Test2 ? _case2! : throw new InvalidOperationException();
        public string AsStr => _tag == Case.Str ? _case3! : throw new InvalidOperationException();
        public DiscriminatedUnionGenerator.Sample.Duplicate AsDuplicate => _tag == Case.Duplicate ? _case4! : throw new InvalidOperationException();

        public Case Tag => _tag;
    }
}



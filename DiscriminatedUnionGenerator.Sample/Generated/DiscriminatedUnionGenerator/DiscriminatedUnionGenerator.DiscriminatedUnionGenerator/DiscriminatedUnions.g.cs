#nullable enable

namespace DiscriminatedUnionGenerator.Sample
{
    partial class Sample
    {
        public enum Case
        {
            NotFound = 1,
            Str = 2,
            Duplicate = 3,
        }

        private readonly Case _tag;
        private readonly DiscriminatedUnionGenerator.Sample.NotFound? _case1;
        private readonly string? _case2;
        private readonly DiscriminatedUnionGenerator.Sample.Duplicate? _case3;

        public Sample(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _case1 = notFound;
        }

        public Sample(string str)
        {
            _tag = Case.Str;
            _case2 = str;
        }

        public Sample(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _case3 = duplicate;
        }

        public bool IsNotFound => _tag == Case.NotFound;
        public bool IsStr => _tag == Case.Str;
        public bool IsDuplicate => _tag == Case.Duplicate;

        public DiscriminatedUnionGenerator.Sample.NotFound AsNotFound => _tag == Case.NotFound ? _case1! : throw new System.InvalidOperationException();
        public string AsStr => _tag == Case.Str ? _case2! : throw new System.InvalidOperationException();
        public DiscriminatedUnionGenerator.Sample.Duplicate AsDuplicate => _tag == Case.Duplicate ? _case3! : throw new System.InvalidOperationException();

        public Case Tag => _tag;

        public static implicit operator Sample(DiscriminatedUnionGenerator.Sample.NotFound notFound) => new Sample(notFound);
        public static implicit operator Sample(string str) => new Sample(str);
        public static implicit operator Sample(DiscriminatedUnionGenerator.Sample.Duplicate duplicate) => new Sample(duplicate);

        public static explicit operator DiscriminatedUnionGenerator.Sample.NotFound(Sample sample) => sample.AsNotFound;
        public static explicit operator string(Sample sample) => sample.AsStr;
        public static explicit operator DiscriminatedUnionGenerator.Sample.Duplicate(Sample sample) => sample.AsDuplicate;
    }
}



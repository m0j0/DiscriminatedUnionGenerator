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
        private readonly DiscriminatedUnionGenerator.Sample.NotFound? _caseNotFound;
        private readonly string? _caseStr;
        private readonly DiscriminatedUnionGenerator.Sample.Duplicate? _caseDuplicate;

        public Sample(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _caseNotFound = notFound;
        }

        public Sample(string str)
        {
            _tag = Case.Str;
            _caseStr = str;
        }

        public Sample(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _caseDuplicate = duplicate;
        }

        public bool IsNotFound => _tag == Case.NotFound;
        public bool IsStr => _tag == Case.Str;
        public bool IsDuplicate => _tag == Case.Duplicate;

        public DiscriminatedUnionGenerator.Sample.NotFound AsNotFound => _tag == Case.NotFound ? _caseNotFound! : throw new System.InvalidOperationException();
        public string AsStr => _tag == Case.Str ? _caseStr! : throw new System.InvalidOperationException();
        public DiscriminatedUnionGenerator.Sample.Duplicate AsDuplicate => _tag == Case.Duplicate ? _caseDuplicate! : throw new System.InvalidOperationException();

        public Case Tag => _tag;

        public TResult Match<TResult>(
            System.Func<DiscriminatedUnionGenerator.Sample.NotFound, TResult> funcNotFound,
            System.Func<string, TResult> funcStr,
            System.Func<DiscriminatedUnionGenerator.Sample.Duplicate, TResult> funcDuplicate)
        {
            return _tag switch
            {
                Case.NotFound => funcNotFound(_caseNotFound!),
                Case.Str => funcStr(_caseStr!),
                Case.Duplicate => funcDuplicate(_caseDuplicate!),
                _ => throw new System.InvalidOperationException()
            };
        }

        public async System.Threading.Tasks.Task<TResult> MatchAsync<TResult>(
            System.Func<DiscriminatedUnionGenerator.Sample.NotFound, System.Threading.Tasks.Task<TResult>> funcNotFound,
            System.Func<string, System.Threading.Tasks.Task<TResult>> funcStr,
            System.Func<DiscriminatedUnionGenerator.Sample.Duplicate, System.Threading.Tasks.Task<TResult>> funcDuplicate)
        {
            return _tag switch
            {
                Case.NotFound => await funcNotFound(_caseNotFound!).ConfigureAwait(false),
                Case.Str => await funcStr(_caseStr!).ConfigureAwait(false),
                Case.Duplicate => await funcDuplicate(_caseDuplicate!).ConfigureAwait(false),
                _ => throw new System.InvalidOperationException()
            };
        }

        public static implicit operator Sample(DiscriminatedUnionGenerator.Sample.NotFound notFound) => new Sample(notFound);
        public static implicit operator Sample(string str) => new Sample(str);
        public static implicit operator Sample(DiscriminatedUnionGenerator.Sample.Duplicate duplicate) => new Sample(duplicate);

        public static explicit operator DiscriminatedUnionGenerator.Sample.NotFound(Sample sample) => sample.AsNotFound;
        public static explicit operator string(Sample sample) => sample.AsStr;
        public static explicit operator DiscriminatedUnionGenerator.Sample.Duplicate(Sample sample) => sample.AsDuplicate;
    }
}



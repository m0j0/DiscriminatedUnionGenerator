#nullable enable

namespace DiscriminatedUnionGenerator.Sample
{
    partial class SampleClass
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

        public SampleClass(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _caseNotFound = notFound;
            _caseStr = null;
            _caseDuplicate = null;
        }

        public SampleClass(string str)
        {
            _tag = Case.Str;
            _caseNotFound = null;
            _caseStr = str;
            _caseDuplicate = null;
        }

        public SampleClass(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _caseNotFound = null;
            _caseStr = null;
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

        public static implicit operator SampleClass(DiscriminatedUnionGenerator.Sample.NotFound notFound) => new SampleClass(notFound);
        public static implicit operator SampleClass(string str) => new SampleClass(str);
        public static implicit operator SampleClass(DiscriminatedUnionGenerator.Sample.Duplicate duplicate) => new SampleClass(duplicate);

        public static explicit operator DiscriminatedUnionGenerator.Sample.NotFound(SampleClass sampleClass) => sampleClass.AsNotFound;
        public static explicit operator string(SampleClass sampleClass) => sampleClass.AsStr;
        public static explicit operator DiscriminatedUnionGenerator.Sample.Duplicate(SampleClass sampleClass) => sampleClass.AsDuplicate;
    }
}


namespace DiscriminatedUnionGenerator.Sample
{
    partial record SampleRecord
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

        public SampleRecord(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _caseNotFound = notFound;
            _caseStr = null;
            _caseDuplicate = null;
        }

        public SampleRecord(string str)
        {
            _tag = Case.Str;
            _caseNotFound = null;
            _caseStr = str;
            _caseDuplicate = null;
        }

        public SampleRecord(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _caseNotFound = null;
            _caseStr = null;
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

        public static implicit operator SampleRecord(DiscriminatedUnionGenerator.Sample.NotFound notFound) => new SampleRecord(notFound);
        public static implicit operator SampleRecord(string str) => new SampleRecord(str);
        public static implicit operator SampleRecord(DiscriminatedUnionGenerator.Sample.Duplicate duplicate) => new SampleRecord(duplicate);

        public static explicit operator DiscriminatedUnionGenerator.Sample.NotFound(SampleRecord sampleRecord) => sampleRecord.AsNotFound;
        public static explicit operator string(SampleRecord sampleRecord) => sampleRecord.AsStr;
        public static explicit operator DiscriminatedUnionGenerator.Sample.Duplicate(SampleRecord sampleRecord) => sampleRecord.AsDuplicate;
    }
}


namespace DiscriminatedUnionGenerator.Sample
{
    partial struct SampleStruct
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

        public SampleStruct(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _caseNotFound = notFound;
            _caseStr = null;
            _caseDuplicate = null;
        }

        public SampleStruct(string str)
        {
            _tag = Case.Str;
            _caseNotFound = null;
            _caseStr = str;
            _caseDuplicate = null;
        }

        public SampleStruct(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _caseNotFound = null;
            _caseStr = null;
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

        public static implicit operator SampleStruct(DiscriminatedUnionGenerator.Sample.NotFound notFound) => new SampleStruct(notFound);
        public static implicit operator SampleStruct(string str) => new SampleStruct(str);
        public static implicit operator SampleStruct(DiscriminatedUnionGenerator.Sample.Duplicate duplicate) => new SampleStruct(duplicate);

        public static explicit operator DiscriminatedUnionGenerator.Sample.NotFound(SampleStruct sampleStruct) => sampleStruct.AsNotFound;
        public static explicit operator string(SampleStruct sampleStruct) => sampleStruct.AsStr;
        public static explicit operator DiscriminatedUnionGenerator.Sample.Duplicate(SampleStruct sampleStruct) => sampleStruct.AsDuplicate;
    }
}


namespace DiscriminatedUnionGenerator.Sample
{
    partial record struct SampleStructRecord
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

        public SampleStructRecord(DiscriminatedUnionGenerator.Sample.NotFound notFound)
        {
            _tag = Case.NotFound;
            _caseNotFound = notFound;
            _caseStr = null;
            _caseDuplicate = null;
        }

        public SampleStructRecord(string str)
        {
            _tag = Case.Str;
            _caseNotFound = null;
            _caseStr = str;
            _caseDuplicate = null;
        }

        public SampleStructRecord(DiscriminatedUnionGenerator.Sample.Duplicate duplicate)
        {
            _tag = Case.Duplicate;
            _caseNotFound = null;
            _caseStr = null;
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

        public static implicit operator SampleStructRecord(DiscriminatedUnionGenerator.Sample.NotFound notFound) => new SampleStructRecord(notFound);
        public static implicit operator SampleStructRecord(string str) => new SampleStructRecord(str);
        public static implicit operator SampleStructRecord(DiscriminatedUnionGenerator.Sample.Duplicate duplicate) => new SampleStructRecord(duplicate);

        public static explicit operator DiscriminatedUnionGenerator.Sample.NotFound(SampleStructRecord sampleStructRecord) => sampleStructRecord.AsNotFound;
        public static explicit operator string(SampleStructRecord sampleStructRecord) => sampleStructRecord.AsStr;
        public static explicit operator DiscriminatedUnionGenerator.Sample.Duplicate(SampleStructRecord sampleStructRecord) => sampleStructRecord.AsDuplicate;
    }
}



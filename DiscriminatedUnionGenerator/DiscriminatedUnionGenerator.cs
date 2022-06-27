using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace DiscriminatedUnionGenerator
{
    [Generator]
    public class DiscriminatedUnionGenerator : IIncrementalGenerator
    {
        private const string CaseAttributeName = "DiscriminatedUnionGenerator.DiscriminatedUnionCaseAttribute";

        private const string Attribute = @"#nullable enable

namespace DiscriminatedUnionGenerator
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct, AllowMultiple = true)]
    internal sealed class DiscriminatedUnionCaseAttribute : System.Attribute
    {
        public DiscriminatedUnionCaseAttribute(System.Type type)
        {
            Type = type;
        }

        public DiscriminatedUnionCaseAttribute(System.Type type, string name)
        {
            Type = type;
            Name = name;
        }

        public System.Type Type { get; }

        public string? Name { get; }
    }
}";
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "DiscriminatedUnionCaseAttribute.g.cs",
                SourceText.From(Attribute, Encoding.UTF8)));
            
            var classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select classes with attributes
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
                .Where(static m => m is not null)!; // filter out attributed classes that we don't care about

            // Combine the selected classes with the `Compilation`
            var compilationAndEnums
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            // Generate the source using the compilation and classes
            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0 ||
                   node is RecordDeclarationSyntax r && r.AttributeLists.Count > 0 ||
                   node is StructDeclarationSyntax s && s.AttributeLists.Count > 0;
        }

        private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            var classDeclarationSyntax = (TypeDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        // weird, we couldn't get the symbol, ignore it
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    // Is the attribute the [EnumExtensions] attribute?
                    if (fullName == CaseAttributeName)
                    {
                        // return the enum
                        return classDeclarationSyntax;
                    }
                }
            }

            // we didn't find the attribute we were looking for
            return null;
        }

        private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            var distinctClasses = classes.Distinct();

            // Convert each EnumDeclarationSyntax to an UnionsToGenerate
            var enumsToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);

            // If there were errors in the EnumDeclarationSyntax, we won't create an
            // UnionsToGenerate for it, so make sure we have something to generate
            if (enumsToGenerate.Count > 0)
            {
                // generate the source code and add it to the output
                string result = GenerateExtensionClass(enumsToGenerate);
                context.AddSource("DiscriminatedUnions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }

        private static IReadOnlyList<UnionToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<TypeDeclarationSyntax> classes, CancellationToken ct)
        {
            // Get the semantic representation of our marker attribute 
            INamedTypeSymbol? caseAttribute = compilation.GetTypeByMetadataName(CaseAttributeName);

            if (caseAttribute == null)
            {
                // If this is null, the compilation couldn't find the marker attribute type
                // which suggests there's something very wrong! Bail out..
                return Array.Empty<UnionToGenerate>();
            }

            var unionsToGenerates = new List<UnionToGenerate>();

            foreach (var classDeclarationSyntax in classes)
            {
                // stop if we're asked to
                ct.ThrowIfCancellationRequested();

                // Get the semantic representation of the enum syntax
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                if (ModelExtensions.GetDeclaredSymbol(semanticModel, classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                {
                    // something went wrong, bail out
                    continue;
                }

                UnionKind kind;
                switch (CSharpExtensions.Kind(classDeclarationSyntax))
                {
                    case SyntaxKind.RecordStructDeclaration:
                        kind = UnionKind.StructRecord;
                        break;
                    case SyntaxKind.RecordDeclaration:
                        kind = UnionKind.Record;
                        break;
                    case SyntaxKind.ClassDeclaration:
                        kind = UnionKind.Class;
                        break;
                    case SyntaxKind.StructDeclaration:
                        kind = UnionKind.Struct;
                        break;
                    default:
                        continue;
                }


                var cases = new List<CaseData>();

                foreach (AttributeData attributeData in classSymbol.GetAttributes())
                {
                    if (!caseAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
                    {
                        // This isn't the [EnumExtensions] attribute
                        continue;
                    }

                    // This is the right attribute, check the constructor arguments
                    if (!attributeData.ConstructorArguments.IsEmpty)
                    {
                        ImmutableArray<TypedConstant> args = attributeData.ConstructorArguments;

                        // make sure we don't have any errors
                        foreach (TypedConstant arg in args)
                        {
                            if (arg.Kind == TypedConstantKind.Error)
                            {
                                // have an error, so don't try and do any generation
                                break;
                            }
                        }

                        if (args.Length == 1)
                        {
                            var type = (INamedTypeSymbol)args[0].Value!;
                            cases.Add(new CaseData(type.IsValueType, type.ToString(), type.Name));
                        }
                        else if (args.Length == 2)
                        {
                            var type = (INamedTypeSymbol)args[0].Value!;
                            var name = args[1].Value?.ToString() ?? "NONAME";
                            cases.Add(new CaseData(type.IsValueType, type.ToString(), name));
                        }
                    }
                }


                unionsToGenerates.Add(new UnionToGenerate(kind, classSymbol.ContainingNamespace?.ToString(), classSymbol.Name, cases));
            }

            return unionsToGenerates;
        }

        public static string GenerateExtensionClass(IReadOnlyList<UnionToGenerate> unionsToGenerate)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            foreach (var union in unionsToGenerate)
            {
                if (!string.IsNullOrWhiteSpace(union.TypeNamespace))
                {
                    sb.AppendLine($@"namespace {union.TypeNamespace}
{{");
                }

                static string KindToString(UnionKind kind)
                {
                    switch (kind)
                    {
                        case UnionKind.Class:
                            return "class";
                        case UnionKind.Record:
                            return "record";
                        case UnionKind.Struct:
                            return "struct";
                        case UnionKind.StructRecord:
                            return "record struct";
                        default:
                            throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                    }
                }

                static string GetValueTypeAppendix(CaseData caseData)
                {
                    return caseData.IsValueType ? ".Value" : "";
                }


                sb.AppendLine($@"    partial {KindToString(union.Kind)} {union.TypeName}
    {{");

                #region enum

                sb.AppendLine($"        public enum Case");
                sb.AppendLine("        {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    sb.AppendLine($"            {union.Cases[i].Name} = {i + 1},");
                }
                sb.AppendLine("        }");
                sb.AppendLine();

                #endregion

                #region fields

                sb.AppendLine($"        private readonly Case _tag;");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    sb.AppendLine($"        private readonly {union.Cases[i].Type}? _case{union.Cases[i].Name};");
                }
                sb.AppendLine();

                #endregion

                #region ctors

                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"        public {union.TypeName}({caseData.Type} {caseData.LoweredName})");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            _tag = Case.{caseData.Name};");
                    for (var j = 0; j < union.Cases.Count; j++)
                    {
                        if (i == j)
                        {
                            sb.Append($"            _case{union.Cases[j].Name} = {caseData.LoweredName}");
                            sb.AppendLine(caseData.IsValueType
                                ? ";"
                                : $" ?? throw new ArgumentNullException(\"{caseData.LoweredName}\");");
                        }
                        else
                        {
                            sb.AppendLine($"            _case{union.Cases[j].Name} = null;");
                        }
                    }
                    sb.AppendLine("        }");

                    if (i != union.Cases.Count - 1)
                    {
                        sb.AppendLine();
                    }
                }
                sb.AppendLine();

                #endregion

                #region is

                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"        public bool Is{caseData.Name} => _tag == Case.{caseData.Name};");
                }
                sb.AppendLine();

                #endregion

                #region as

                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"        public {caseData.Type} As{caseData.Name} => _tag == Case.{caseData.Name} ? _case{union.Cases[i].Name}!{GetValueTypeAppendix(caseData)} : throw new System.InvalidOperationException();");
                }
                sb.AppendLine();

                #endregion

                #region tag

                sb.AppendLine($"        public Case Tag => _tag;");
                sb.AppendLine();

                #endregion

                #region switch

                sb.AppendLine("        public void Switch(");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.Append($"            System.Action<{caseData.Type}>? action{caseData.Name}");
                    sb.AppendLine(i == union.Cases.Count - 1 ? ")" : ",");
                }
                sb.AppendLine("        {");
                sb.AppendLine("            switch (_tag)");
                sb.AppendLine("            {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"                case Case.{caseData.Name}:"); 
                    sb.AppendLine($"                    action{caseData.Name}?.Invoke(_case{caseData.Name}!{GetValueTypeAppendix(caseData)});");
                    sb.AppendLine("                    break;");
                }
                sb.AppendLine("                default:");
                sb.AppendLine("                    throw new System.InvalidOperationException();");
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();

                #endregion

                #region switch async

                sb.AppendLine("        public async System.Threading.Tasks.Task SwitchAsync(");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.Append($"            System.Func<{caseData.Type}, System.Threading.Tasks.Task>? action{caseData.Name}");
                    sb.AppendLine(i == union.Cases.Count - 1 ? ")" : ",");
                }
                sb.AppendLine("        {");
                sb.AppendLine("            switch (_tag)");
                sb.AppendLine("            {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"                case Case.{caseData.Name}:");
                    var actionName = $"action{caseData.Name}";
                    sb.AppendLine($"                    if ({actionName} != null) await {actionName}(_case{caseData.Name}!{GetValueTypeAppendix(caseData)}).ConfigureAwait(false);");
                    sb.AppendLine("                    break;");
                }
                sb.AppendLine("                default:");
                sb.AppendLine("                    throw new System.InvalidOperationException();");
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();

                #endregion

                #region match

                sb.AppendLine("        public TResult Match<TResult>(");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.Append($"            System.Func<{caseData.Type}, TResult> func{caseData.Name}");
                    sb.AppendLine(i == union.Cases.Count - 1 ? ")" : ",");
                }
                sb.AppendLine("        {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var funcName = $"func{union.Cases[i].Name}";
                    sb.AppendLine($"            if ({funcName} == null) throw new ArgumentNullException(\"{funcName}\");");
                }
                sb.AppendLine();
                sb.AppendLine("            return _tag switch");
                sb.AppendLine("            {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"                Case.{caseData.Name} => func{caseData.Name}(_case{caseData.Name}!{GetValueTypeAppendix(caseData)}),");
                }
                sb.AppendLine("                _ => throw new System.InvalidOperationException()");
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();

                #endregion

                #region match async

                sb.AppendLine("        public async System.Threading.Tasks.Task<TResult> MatchAsync<TResult>(");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.Append($"            System.Func<{caseData.Type}, System.Threading.Tasks.Task<TResult>> func{caseData.Name}");
                    sb.AppendLine(i == union.Cases.Count - 1 ? ")" : ",");
                }
                sb.AppendLine("        {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var funcName = $"func{union.Cases[i].Name}";
                    sb.AppendLine($"            if ({funcName} == null) throw new ArgumentNullException(\"{funcName}\");");
                }
                sb.AppendLine();
                sb.AppendLine("            return _tag switch");
                sb.AppendLine("            {");
                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"                Case.{caseData.Name} => await func{caseData.Name}(_case{caseData.Name}!{GetValueTypeAppendix(caseData)}).ConfigureAwait(false),");
                }
                sb.AppendLine("                _ => throw new System.InvalidOperationException()");
                sb.AppendLine("            };");
                sb.AppendLine("        }");
                sb.AppendLine();

                #endregion

                #region implicit

                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"        public static implicit operator {union.TypeName}({caseData.Type} {caseData.LoweredName}) => new {union.TypeName}({caseData.LoweredName});");
                }
                sb.AppendLine();

                #endregion

                #region explicit

                for (var i = 0; i < union.Cases.Count; i++)
                {
                    var caseData = union.Cases[i];
                    sb.AppendLine($"        public static explicit operator {caseData.Type}({union.TypeName} {union.LoweredTypeName}) => {union.LoweredTypeName}.As{caseData.Name};");
                }

                #endregion

                sb.AppendLine("    }");

                if (!string.IsNullOrWhiteSpace(union.TypeNamespace))
                {
                    sb.AppendLine("}");
                }

                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public enum UnionKind
    {
        Class,
        Record,
        Struct,
        StructRecord
    }

    public readonly struct UnionToGenerate
    {
        public UnionToGenerate(UnionKind kind, string? typeNamespace, string typeName, List<CaseData> cases)
        {
            Kind = kind;
            TypeNamespace = typeNamespace;
            TypeName = typeName;
            LoweredTypeName = Extensions.FirstCharToLowerCase(typeName);
            Cases = cases;
        }

        public UnionKind Kind { get; }

        public string? TypeNamespace { get; }

        public string TypeName { get; }

        public string LoweredTypeName { get; }

        public List<CaseData> Cases { get; }
    }

    public readonly struct CaseData
    {
        public CaseData(bool isValueType, string type, string name)
        {
            IsValueType = isValueType;
            Type = type;
            Name = name;
            LoweredName = Extensions.FirstCharToLowerCase(name);
        }

        public bool IsValueType { get; }

        public string Type { get; }

        public string Name { get; }

        public string LoweredName { get; }
    }

    public static class Extensions
    {
        public static string FirstCharToLowerCase(string str)
        {
            if (!string.IsNullOrEmpty(str) && char.IsUpper(str[0]))
            {
                return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str.Substring(1);
            }

            return str;
        }
    }
}

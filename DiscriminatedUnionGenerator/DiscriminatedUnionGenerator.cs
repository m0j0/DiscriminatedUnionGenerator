using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
            
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select classes with attributes
                    transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
                .Where(static m => m is not null)!; // filter out attributed classes that we don't care about

            // Combine the selected classes with the `Compilation`
            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            // Generate the source using the compilation and classes
            context.RegisterSourceOutput(compilationAndEnums,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;
        }

        private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
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

        private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
            IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();

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

        private static IReadOnlyList<UnionToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, CancellationToken ct)
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

            foreach (ClassDeclarationSyntax classDeclarationSyntax in classes)
            {
                // stop if we're asked to
                ct.ThrowIfCancellationRequested();

                // Get the semantic representation of the enum syntax
                SemanticModel semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                {
                    // something went wrong, bail out
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
                            var type = (INamedTypeSymbol)args[0].Value;
                            cases.Add(new CaseData(type.ToString(), type.Name));
                        }
                        else if (args.Length == 2)
                        {
                            var type = args[0].Value?.ToString() ?? "NOTYPE";
                            var name = args[1].Value?.ToString() ?? "NONAME";
                            cases.Add(new CaseData(type, name));
                        }
                    }
                }


                unionsToGenerates.Add(new UnionToGenerate(classSymbol.ContainingNamespace?.ToString(), classSymbol.Name, cases));
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

                sb.AppendLine($@"    partial class {union.TypeName}
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
                    sb.AppendLine($"        private readonly {union.Cases[i].Type}? _case{i + 1};");
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
                    sb.AppendLine($"            _case{i + 1} = {caseData.LoweredName};");
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
                    sb.AppendLine($"        public {caseData.Type} As{caseData.Name} => _tag == Case.{caseData.Name} ? _case{i + 1}! : throw new InvalidOperationException();");
                }
                sb.AppendLine();

                #endregion

                #region tag

                sb.AppendLine($"        public Case Tag => _tag;");
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


    public readonly struct UnionToGenerate
    {
        public UnionToGenerate(string? typeNamespace, string typeName, List<CaseData> cases)
        {
            TypeNamespace = typeNamespace;
            TypeName = typeName;
            LoweredTypeName = Extensions.FirstCharToLowerCase(typeName);
            Cases = cases;
        }

        public string? TypeNamespace { get; }

        public string TypeName { get; }

        public string LoweredTypeName { get; }

        public List<CaseData> Cases { get; }
    }

    public readonly struct CaseData
    {
        public CaseData(string type, string name)
        {
            Type = type;
            Name = name;
            LoweredName = Extensions.FirstCharToLowerCase(name);
        }

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

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
        public DiscriminatedUnionCaseAttribute(Type type)
        {
            Type = type;
        }

        public DiscriminatedUnionCaseAttribute(Type type, string? name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; }

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
            List<UnionsToGenerate> enumsToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);

            // If there were errors in the EnumDeclarationSyntax, we won't create an
            // UnionsToGenerate for it, so make sure we have something to generate
            if (enumsToGenerate.Count > 0)
            {
                // generate the source code and add it to the output
                string result = GenerateExtensionClass(enumsToGenerate);
                context.AddSource("DiscriminatedUnions.g.cs", SourceText.From(result, Encoding.UTF8));
            }
        }

        static List<UnionsToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> enums, CancellationToken ct)
        {
            // Create a list to hold our output
            var enumsToGenerate = new List<UnionsToGenerate>();
            // Get the semantic representation of our marker attribute 
            INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName(CaseAttributeName);

            if (enumAttribute == null)
            {
                // If this is null, the compilation couldn't find the marker attribute type
                // which suggests there's something very wrong! Bail out..
                return enumsToGenerate;
            }

            foreach (ClassDeclarationSyntax enumDeclarationSyntax in enums)
            {
                // stop if we're asked to
                ct.ThrowIfCancellationRequested();

                // Get the semantic representation of the enum syntax
                SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);
                if (semanticModel.GetDeclaredSymbol(enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
                {
                    // something went wrong, bail out
                    continue;
                }

                // Get the full type name of the enum e.g. Colour, 
                // or OuterClass<T>.Colour if it was nested in a generic type (for example)
                string enumName = enumSymbol.ToString();

                // Get all the members in the enum
                ImmutableArray<ISymbol> enumMembers = enumSymbol.GetMembers();
                var members = new List<string>(enumMembers.Length);

                // Get all the fields from the enum, and add their name to the list
                foreach (ISymbol member in enumMembers)
                {
                    if (member is IFieldSymbol field && field.ConstantValue is not null)
                    {
                        members.Add(member.Name);
                    }
                }

                // Create an UnionsToGenerate for use in the generation phase
                enumsToGenerate.Add(new UnionsToGenerate(enumSymbol.ContainingNamespace?.ToString(), enumSymbol.Name, new List<CaseData>()));
            }

            return enumsToGenerate;
        }

        public static string GenerateExtensionClass(List<UnionsToGenerate> unionsToGenerate)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            foreach (var unionToGenerate in unionsToGenerate)
            {
                if (!string.IsNullOrWhiteSpace(unionToGenerate.TypeNamespace))
                {
                    sb.AppendLine($@"namespace {unionToGenerate.TypeNamespace}
{{");
                }

                sb.AppendLine($@"    partial class {unionToGenerate.TypeName}
    {{");

                foreach (var caseData in unionToGenerate.Cases)
                {
                    sb.AppendLine($"        public {caseData.Type.FullName} {caseData.Name ?? caseData.Type.FullName} {{ get; }}");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");

                if (!string.IsNullOrWhiteSpace(unionToGenerate.TypeNamespace))
                {
                    sb.AppendLine("}");
                }

                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }


    public readonly struct UnionsToGenerate
    {
        public UnionsToGenerate(string? typeNamespace, string typeName, List<CaseData> cases)
        {
            TypeNamespace = typeNamespace;
            TypeName = typeName;
            Cases = cases;
        }

        public string? TypeNamespace { get; }

        public string TypeName { get; }

        public List<CaseData> Cases { get; }
    }

    public readonly struct CaseData
    {
        public CaseData(Type type, string? name)
        {
            Type = type;
            Name = name;
        }

        public Type Type { get; }

        public string? Name { get; }
    }
}

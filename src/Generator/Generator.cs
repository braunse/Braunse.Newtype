using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Braunse.Newtype.Generator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interestingDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: Parser.IsNewtypeCandidate,
                transform: Parser.ToNewtypeCandidateDeclOrNull
            )
            .Where(v => v is not null);

        var interestingDeclarationsAndCompilation = interestingDeclarations.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(interestingDeclarationsAndCompilation, static (spc, tup) => Generate(spc, tup.Left!, tup.Right));
    }

    private static void Generate(SourceProductionContext spc, BaseTypeDeclarationSyntax syntax, Compilation compilation)
    {
        if (Parser.ToNewtypeCandidate(syntax, compilation, ref spc) is not { } cand) return;
        spc.AddSource($"{cand.FullName}.Newtype.cs", SourceGenerator.GenerateSource(cand));
    }
}
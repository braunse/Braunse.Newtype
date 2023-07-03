using System;
using System.Threading;
using Braunse.Newtype.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Braunse.Newtype.Generator;

public static class Parser
{
    private static readonly string NewtypeAttributeFullName = // typeof(NewtypeAttribute).FullName;
        "Braunse.Newtype.NewtypeAttribute";

    public static bool IsNewtypeCandidate(SyntaxNode node, CancellationToken _)
    {
        return node switch
        {
            StructDeclarationSyntax {AttributeLists.Count: > 0} => true,
            RecordDeclarationSyntax {AttributeLists.Count: > 0} => true,
            ClassDeclarationSyntax {AttributeLists.Count: > 0} => true,
            _ => false
        };
    }

    public static BaseTypeDeclarationSyntax? ToNewtypeCandidateDeclOrNull(GeneratorSyntaxContext context,
        CancellationToken _)
    {
        var baseDecl = (BaseTypeDeclarationSyntax) context.Node;

        AttributeSyntax newtypeAttributeSyntax;
        foreach (var attributeList in baseDecl.AttributeLists)
        {
            foreach (var attributeSyntax in attributeList.Attributes)
            {
                var attributeSymbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);
                if (attributeSymbolInfo.Symbol is not IMethodSymbol attributeConstructorSymbol) continue;

                var attributeTypeSymbol = attributeConstructorSymbol.ContainingType;
                if (!string.Equals(NewtypeAttributeFullName, attributeTypeSymbol.ToDisplayString(),
                        StringComparison.InvariantCulture))
                    continue;

                return baseDecl;
            }
        }

        // We didn't find a Newtype(...) attribute
        return null;
    }

    public static NewtypeCandidate? ToNewtypeCandidate(BaseTypeDeclarationSyntax syn, Compilation compilation,
        ref SourceProductionContext context)
    {
        if (compilation.GetTypeByMetadataName(NewtypeAttributeFullName) is not { } newtypeAttributeSymbol)
        {
            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InternalError,
                syn.GetLocation(),
                $"Could not find class named '{NewtypeAttributeFullName}' in compilation, make sure to reference Braunse.Newtype.Runtime"));
            return null;
        }

        var model = compilation.GetSemanticModel(syn.SyntaxTree);
        var declaredSymbol = model.GetDeclaredSymbol(syn);
        if (declaredSymbol is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InternalError,
                syn.GetLocation(),
                $"Could not find a symbol that is declared here"));
            return null;
        }

        AttributeData newtypeAttributeData;
        INamedTypeSymbol innerType;
        foreach (var attributeData in declaredSymbol.GetAttributes())
        {
            var x = 1;
            if (!SymbolEqualityComparer.Default.Equals(newtypeAttributeSymbol, attributeData.AttributeClass))
                continue;

            if (attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol innerTypeCand)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InnerTypeNotNamed, syn.GetLocation(),
                    attributeData.ConstructorArguments[0].Value));
                return null;
            }

            innerType = innerTypeCand;
            newtypeAttributeData = attributeData;
            goto FOUND_ATTRIBUTE;
        }

        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.InternalError, syn.GetLocation(),
            $"Could not find the [NewtypeAttribute] for this declaration"));
        return null;

        FOUND_ATTRIBUTE:
        var ns = ComputeNamespace(syn);
        var name = syn.Identifier.Text;
        var candidate = new NewtypeCandidate(syn, innerType, ns, name);

        foreach (var namedArgument in newtypeAttributeData.NamedArguments)
        {
            switch (namedArgument.Key)
            {
                case nameof(NewtypeAttribute.Convertible):
                    candidate.Convertible = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.Opaque):
                    candidate.Opaque = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.ImplicitWrapUnwrap):
                    candidate.ImplicitWrapUnwrap = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.ImplicitWrap):
                    candidate.ImplicitWrap = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.ImplicitUnwrap):
                    candidate.ImplciitUnwrap = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.Equatable):
                    candidate.Equatable = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.Comparable):
                    candidate.Comparable = (bool) namedArgument.Value.Value!;
                    break;
                
                case nameof(NewtypeAttribute.SqlType):
                    if (namedArgument.Value.Value is not INamedTypeSymbol namedSqlType)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.SqlTypeNotNamed, syn.GetLocation(),
                            namedArgument.Value.Value));
                    }
                    else
                    {
                        candidate.SqlType = namedSqlType;
                    }

                    break;
            }
        }

        return candidate;
    }

    private static string? ComputeNamespace(BaseTypeDeclarationSyntax syn)
    {
        SyntaxNode node;
        for (node = syn;
             node is not null &&
             node is not FileScopedNamespaceDeclarationSyntax &&
             node is not NamespaceDeclarationSyntax;
             node = node.Parent)
            ;

        if (node is not BaseNamespaceDeclarationSyntax nsNode) return null;
        var ns = nsNode.Name.ToString();

        while (nsNode.Parent is BaseNamespaceDeclarationSyntax parentNode)
        {
            ns = $"{parentNode.Name}.{ns}";
            nsNode = parentNode;
        }

        return ns;
    }
}
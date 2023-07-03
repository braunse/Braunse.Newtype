using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Braunse.Newtype.Generator;

public record NewtypeCandidate
{
    public NewtypeCandidate(BaseTypeDeclarationSyntax syntax, INamedTypeSymbol inner, string? ns, string name)
    {
        Syntax = syntax;
        Inner = inner;
        Namespace = ns;
        Name = name;

        Kind = syntax switch
        {
            StructDeclarationSyntax => DeclKind.Struct,
            RecordDeclarationSyntax => DeclKind.Record,
            ClassDeclarationSyntax => DeclKind.Class,
            _ => throw new ArgumentOutOfRangeException(nameof(syntax),
                "This should never happen, we check the type in IsNewtypeCandidate")
        };
    }

    public string? Namespace { get; }
    public string Name { get; }
    public string FullName => $"{Namespace}{(Namespace != null ? "." : "")}{Name}";
    public BaseTypeDeclarationSyntax Syntax { get; }
    public INamedTypeSymbol Inner { get; }
    public DeclKind Kind { get; }
    public bool Convertible { get; set; } = true;
    public bool Opaque { get; set; }
    public bool ImplicitWrapUnwrap { get; set; }
    public bool ImplicitWrap { get; set; }
    public bool ImplciitUnwrap { get; set; }
    public bool Equatable { get; set; }
    public bool Comparable { get; set; }
    public INamedTypeSymbol? SqlType { get; set; }
    public bool EffectiveEquatable => Equatable || Comparable;
    public bool EffectiveImplicitWrap => ImplicitWrapUnwrap || ImplicitWrap;
    public bool EffectiveImplicitUnwrap => ImplicitWrapUnwrap || ImplciitUnwrap;
};
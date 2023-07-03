using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Braunse.Newtype.Generator;

public static class SourceGenerator
{
    public static string GenerateSource(NewtypeCandidate candidate)
    {
        var b = new StringBuilder();

        if (candidate.Namespace is { } ns)
        {
            b.Append($"namespace {ns} {{\n");
        }

        var vis = candidate.Syntax.Modifiers.FirstOrDefault(m => m.Kind() switch
        {
            SyntaxKind.PublicKeyword => true,
            SyntaxKind.ProtectedKeyword => true,
            SyntaxKind.InternalKeyword => true,
            SyntaxKind.PrivateKeyword => true,
            _ => false
        });
        var kindKw = candidate.Kind switch
        {
            DeclKind.Class => "class",
            DeclKind.Struct => "struct",
            DeclKind.Record => "record",
            _ => throw new ArgumentOutOfRangeException("candidate.Kind")
        };

        var inner = candidate.Inner.ToDisplayString();
        
        var interfaceList = new List<string>();
        if(candidate.Convertible) interfaceList.Add($"Braunse.Newtype.IWrapper<{candidate.Name}, {inner}>");
        if (candidate.EffectiveEquatable) interfaceList.Add($"System.IEquatable<{candidate.Name}>");
        if (candidate.Comparable) interfaceList.Add($"System.IComparable<{candidate.Name}>");
        var interfaces = string.Join(", ", interfaceList);
        var intfColon = interfaceList.Count > 0 ? ":" : "";

        b.Append($"{vis} partial {kindKw} {candidate.Name} {intfColon} {interfaces} {{\n");

        b.Append($"\tpublic {candidate.Name}({inner} value) {{ Value = value; }}\n");
        b.Append($"\tpublic {inner} Value {{ get; }}\n");

        if (candidate.EffectiveImplicitWrap)
            b.Append($"\tpublic static implicit operator {candidate.Name}({inner} value) => new(value);\n");

        if (candidate.EffectiveImplicitUnwrap)
            b.Append($"\tpublic static implicit operator {inner}({candidate.Name} value) => value.Value;\n");

        if (candidate.Convertible)
        {
            b.Append($"\tpublic {inner} Unwrap() => Value;\n")
                .Append(
                    $"\tpublic static {candidate.Name} Wrap({inner} value) => new(value);\n");
        }

        if (candidate.Kind != DeclKind.Record)
        {
            b.Append($"\tpublic override int GetHashCode() => Value.GetHashCode();\n")
                .Append($"\tpublic override string ToString() => Value.ToString();\n");
        }
        
        if (candidate.EffectiveEquatable)
        {
            b.Append(
                $"\tbool System.IEquatable<{candidate.Name}>.Equals({candidate.Name} other) => Value.Equals(other.Value);\n");
            if (candidate.Kind != DeclKind.Record)
            {
                b.Append(
                    $"\tpublic override bool Equals(object? other) => other is {candidate.Name} o && ((IEquatable<{candidate.Name}>)this).Equals(o);\n");
            }

            b.Append($"\tpublic static bool operator ==({candidate.Name} a, {candidate.Name} b) => a.Equals(b);\n")
                .Append($"\tpublic static bool operator !=({candidate.Name} a, {candidate.Name} b) => !(a == b);\n");
        }
        else
        {
            b.Append(
                $"\tpublic override bool Equals(object? other) => other is {candidate.Name} o && Value.Equals(o.Value);\n");
        }

        if (candidate.Comparable)
        {
            b.Append($"\tint System.IComparable<{candidate.Name}>.CompareTo({candidate.Name} other) => Value.CompareTo(other.Value);\n")
                .Append(
                    $"\tpublic static bool operator<({candidate.Name} a, {candidate.Name} b) => ((IComparable<{candidate.Name}>)a).CompareTo(b) < 0;\n")
                .Append(
                    $"\tpublic static bool operator<=({candidate.Name} a, {candidate.Name} b) => ((IComparable<{candidate.Name}>)a).CompareTo(b) <= 0;\n")
                .Append(
                    $"\tpublic static bool operator>({candidate.Name} a, {candidate.Name} b) => ((IComparable<{candidate.Name}>)a).CompareTo(b) > 0;\n")
                .Append(
                    $"\tpublic static bool operator>=({candidate.Name} a, {candidate.Name} b) => ((IComparable<{candidate.Name}>)a).CompareTo(b) >= 0;\n");
        }

        b.Append("}\n");

        if (candidate.Namespace is not null) b.Append("}\n");

        return b.ToString();
    }
}
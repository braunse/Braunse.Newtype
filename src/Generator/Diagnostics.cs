using Microsoft.CodeAnalysis;

namespace Braunse.Newtype.Runtime;

public static class Diagnostics
{
    public static class Categories
    {
        public static readonly string Internal = "Internal";
        public static readonly string Types = "Types";
    }

    public static readonly DiagnosticDescriptor InternalError = new("NTY0001", "Internal Error",
        "Internal Error: {0}", Categories.Internal,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor InnerTypeNotNamed = new("NTY0002", "Inner type must be a named type",
        "The inner type {0} is not a named type", Categories.Types,
        DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor SqlTypeNotNamed = new("NTY0003", "SqlType must be a named type",
        "The given SqlType {0} is not a named type", Categories.Types,
        DiagnosticSeverity.Error, true);
}
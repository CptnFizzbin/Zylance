using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// TODO: Create SourceGenerator to create a JSON serialization context for all protobuf types

// Suppress IL2026 warnings for MessageSerializer usage
// All types used with MessageSerializer are protobuf-generated types which are safe for JSON serialization
[assembly: UnconditionalSuppressMessage(
    "Trimming",
    "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
    Justification =
        "MessageSerializer is only used with protobuf-generated message types, which have stable JSON serialization contracts.",
    Scope = "namespaceanddescendants",
    Target = "~N:Zylance.Core")]

// Suppress IL2062 warnings for RequestRouter reflection
// Protobuf message types discovered via reflection always have public constructors
[assembly: UnconditionalSuppressMessage(
    "Trimming",
    "IL2062:Value passed to parameter of method can not be statically determined",
    Justification =
        "RequestRouter uses reflection on protobuf message types which are guaranteed to have public constructors.",
    Scope = "member",
    Target =
        "~M:Zylance.Core.Interfaces.RequestRouter.RegisterTypedHandler(System.Object,System.Reflection.MethodInfo,System.String)")]

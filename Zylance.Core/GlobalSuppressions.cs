﻿using System.Diagnostics.CodeAnalysis;

// Suppress IL2026 warnings for Entity Framework Core validation attributes
// EF Core preserves these attributes through its metadata system and is trim-safe
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
    Justification = "Entity Framework Core preserves validation attributes through its metadata system.",
    Scope = "namespaceanddescendants",
    Target = "~N:Zylance.Core.Entities")]

// Suppress IL2026 warnings for MessageSerializer usage
// All types used with MessageSerializer are protobuf-generated types which are safe for JSON serialization
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
    Justification = "MessageSerializer is only used with protobuf-generated message types, which have stable JSON serialization contracts.",
    Scope = "namespaceanddescendants",
    Target = "~N:Zylance.Core")]


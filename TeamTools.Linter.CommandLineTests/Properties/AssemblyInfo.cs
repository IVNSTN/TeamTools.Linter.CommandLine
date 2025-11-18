[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Structure", "CA1016:Mark assemblies with assembly version", Justification = "Tests are not published and don't need versioning")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "Irrelevant for tests")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "In test classes some method implementations may look static but they are not")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("CodeSmell", "RCS1202:Avoid NullReferenceException", Justification = "Just let test fail")]

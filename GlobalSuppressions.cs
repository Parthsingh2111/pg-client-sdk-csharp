using System.Diagnostics.CodeAnalysis;

// Global suppressions for nullable reference type warnings
[assembly: SuppressMessage("Nullable", "CS8625:Cannot convert null literal to non-nullable reference type", Justification = "SDK designed to work without nullable reference types for broader compatibility")]
[assembly: SuppressMessage("Nullable", "CS8601:Possible null reference assignment", Justification = "SDK designed to work without nullable reference types for broader compatibility")]
[assembly: SuppressMessage("Nullable", "CS8602:Dereference of a possibly null reference", Justification = "SDK designed to work without nullable reference types for broader compatibility")]
[assembly: SuppressMessage("Nullable", "CS8603:Possible null reference return", Justification = "SDK designed to work without nullable reference types for broader compatibility")]
[assembly: SuppressMessage("Nullable", "CS8604:Possible null reference argument", Justification = "SDK designed to work without nullable reference types for broader compatibility")] 